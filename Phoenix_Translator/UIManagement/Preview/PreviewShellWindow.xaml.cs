using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using PhoenixTranslator.ApplicationLayer;

namespace PhoenixTranslator.UIManagement.Preview
{
    /// <summary>
    /// Hosts preview workflow navigation while retaining the complete legacy workspace as a fallback.
    /// </summary>
    public partial class PreviewShellWindow : Window
    {
        private PhoenixGui _legacyWorkspace;
        private readonly PreviewTranslationWorkspaceViewModel _translationWorkspaceViewModel;
        private readonly PreviewReviewQualityViewModel _reviewQualityViewModel;

        /// <summary>
        /// Creates the preview application shell in its no-project state.
        /// </summary>
        public PreviewShellWindow()
        {
            InitializeComponent();
            var shellViewModel = new PreviewShellViewModel(OpenLegacyWorkspace);
            _translationWorkspaceViewModel = new PreviewTranslationWorkspaceViewModel(
                SelectPreviewProject,
                OpenLegacyWorkspace,
                shellViewModel,
                PreviewTranslationProject.Open);
            _reviewQualityViewModel = new PreviewReviewQualityViewModel(
                shellViewModel,
                _translationWorkspaceViewModel,
                new PreviewQualityAnalyzer(),
                new PreviewReviewStateStore(),
                ConfirmBulkApproval,
                OpenLegacyWorkspace);
            DataContext = shellViewModel;
            TranslationWorkspace.DataContext = _translationWorkspaceViewModel;
            ReviewQualityWorkspace.DataContext = _reviewQualityViewModel;
        }

        private bool ConfirmBulkApproval(int entryCount)
        {
            return MessageBox.Show(
                this,
                PreviewMessageCatalog.Format("Review_ApproveScope_Confirmation", entryCount),
                PreviewMessageCatalog.Get("Review_ApproveScope_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
        }

        private static string SelectPreviewProject()
        {
            var dialog = new OpenFileDialog
            {
                Title = PreviewMessageCatalog.Get("Workspace_Open_Title"),
                Filter = PreviewMessageCatalog.Get("Workspace_Project_Filter"),
                CheckFileExists = true,
                Multiselect = false
            };
            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }

        private void OpenLegacyWorkspace()
        {
            if (_legacyWorkspace == null)
            {
                _legacyWorkspace = new PhoenixGui();
                _legacyWorkspace.Closed += LegacyWorkspaceClosed;
                _legacyWorkspace.Show();
            }

            if (_legacyWorkspace.WindowState == WindowState.Minimized)
            {
                _legacyWorkspace.WindowState = WindowState.Normal;
            }

            _legacyWorkspace.Activate();
        }

        private void LegacyWorkspaceClosed(object sender, EventArgs e)
        {
            _legacyWorkspace.Closed -= LegacyWorkspaceClosed;
            _legacyWorkspace = null;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _reviewQualityViewModel.Dispose();
            _translationWorkspaceViewModel.Dispose();
            DeFine.CloseAny();
        }
    }
}
