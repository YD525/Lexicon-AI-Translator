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
        private readonly PreviewHistoryUpdateViewModel _historyUpdateViewModel;

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
            _historyUpdateViewModel = new PreviewHistoryUpdateViewModel(
                shellViewModel,
                _translationWorkspaceViewModel,
                new PreviewProjectComparisonService(),
                new PreviewRevisionHistoryStore(),
                SelectPreviousRevision,
                PreviewTranslationProject.Open,
                ConfirmConflictReuse,
                ConfirmBulkReuse,
                OpenLegacyWorkspace);
            DataContext = shellViewModel;
            TranslationWorkspace.DataContext = _translationWorkspaceViewModel;
            ReviewQualityWorkspace.DataContext = _reviewQualityViewModel;
            HistoryUpdateWorkspace.DataContext = _historyUpdateViewModel;
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

        private static string SelectPreviousRevision()
        {
            var dialog = new OpenFileDialog
            {
                Title = PreviewMessageCatalog.Get("Update_Compare_Title"),
                Filter = PreviewMessageCatalog.Get("Workspace_Project_Filter"),
                CheckFileExists = true,
                Multiselect = false
            };
            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }

        private bool ConfirmConflictReuse(int entryCount)
        {
            return MessageBox.Show(
                this,
                PreviewMessageCatalog.Format("Update_Conflict_Confirmation", entryCount),
                PreviewMessageCatalog.Get("Update_Conflict_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
        }

        private bool ConfirmBulkReuse(int entryCount)
        {
            return MessageBox.Show(
                this,
                PreviewMessageCatalog.Format("Update_ReuseVisible_Confirmation", entryCount),
                PreviewMessageCatalog.Get("Update_ReuseVisible_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
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
            _historyUpdateViewModel.Dispose();
            _reviewQualityViewModel.Dispose();
            _translationWorkspaceViewModel.Dispose();
            DeFine.CloseAny();
        }
    }
}
