using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private readonly PreviewShellViewModel _shellViewModel;
        private readonly PreviewTranslationWorkspaceViewModel _translationWorkspaceViewModel;
        private readonly PreviewReviewQualityViewModel _reviewQualityViewModel;
        private readonly PreviewHistoryUpdateViewModel _historyUpdateViewModel;
        private readonly PreviewSettingsViewModel _settingsViewModel;

        /// <summary>
        /// Creates the preview application shell in its no-project state.
        /// </summary>
        public PreviewShellWindow()
        {
            InitializeComponent();
            _shellViewModel = new PreviewShellViewModel(OpenLegacyWorkspace);
            _translationWorkspaceViewModel = new PreviewTranslationWorkspaceViewModel(
                SelectPreviewProject,
                OpenLegacyWorkspace,
                _shellViewModel,
                PreviewTranslationProject.Open);
            _reviewQualityViewModel = new PreviewReviewQualityViewModel(
                _shellViewModel,
                _translationWorkspaceViewModel,
                new PreviewQualityAnalyzer(),
                new PreviewReviewStateStore(),
                ConfirmBulkApproval,
                OpenLegacyWorkspace);
            _historyUpdateViewModel = new PreviewHistoryUpdateViewModel(
                _shellViewModel,
                _translationWorkspaceViewModel,
                new PreviewProjectComparisonService(),
                new PreviewRevisionHistoryStore(),
                SelectPreviousRevision,
                PreviewTranslationProject.Open,
                ConfirmConflictReuse,
                ConfirmBulkReuse,
                OpenLegacyWorkspace);
            _settingsViewModel = new PreviewSettingsViewModel(
                _shellViewModel,
                new LegacyPreviewSettingsStore(),
                ConfirmSettingsReset,
                ConfirmSettingsDiscard,
                OpenLegacyWorkspace);
            DataContext = _shellViewModel;
            TranslationWorkspace.DataContext = _translationWorkspaceViewModel;
            ReviewQualityWorkspace.DataContext = _reviewQualityViewModel;
            HistoryUpdateWorkspace.DataContext = _historyUpdateViewModel;
            SettingsWorkspace.DataContext = _settingsViewModel;
            _shellViewModel.PropertyChanged += ShellViewModelPropertyChanged;
        }

        private bool ConfirmBulkApproval(int entryCount)
        {
            return ShowConfirmation(
                this,
                PreviewMessageCatalog.Format("Review_ApproveScope_Confirmation", entryCount),
                PreviewMessageCatalog.Get("Review_ApproveScope_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
        }

        private string SelectPreviewProject()
        {
            IInputElement previousFocus = Keyboard.FocusedElement;
            var dialog = new OpenFileDialog
            {
                Title = PreviewMessageCatalog.Get("Workspace_Open_Title"),
                Filter = PreviewMessageCatalog.Get("Workspace_Project_Filter"),
                CheckFileExists = true,
                Multiselect = false
            };
            bool? result = dialog.ShowDialog(this);
            RestoreFocus(previousFocus);
            return result == true ? dialog.FileName : string.Empty;
        }

        private string SelectPreviousRevision()
        {
            IInputElement previousFocus = Keyboard.FocusedElement;
            var dialog = new OpenFileDialog
            {
                Title = PreviewMessageCatalog.Get("Update_Compare_Title"),
                Filter = PreviewMessageCatalog.Get("Workspace_Project_Filter"),
                CheckFileExists = true,
                Multiselect = false
            };
            bool? result = dialog.ShowDialog(this);
            RestoreFocus(previousFocus);
            return result == true ? dialog.FileName : string.Empty;
        }

        private bool ConfirmConflictReuse(int entryCount)
        {
            return ShowConfirmation(
                this,
                PreviewMessageCatalog.Format("Update_Conflict_Confirmation", entryCount),
                PreviewMessageCatalog.Get("Update_Conflict_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
        }

        private bool ConfirmBulkReuse(int entryCount)
        {
            return ShowConfirmation(
                this,
                PreviewMessageCatalog.Format("Update_ReuseVisible_Confirmation", entryCount),
                PreviewMessageCatalog.Get("Update_ReuseVisible_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
        }

        private bool ConfirmSettingsReset()
        {
            return ShowConfirmation(
                this,
                PreviewMessageCatalog.Get("Settings_Reset_Confirmation"),
                PreviewMessageCatalog.Get("Settings_Reset_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel) == MessageBoxResult.OK;
        }

        private bool ConfirmSettingsDiscard()
        {
            return ShowConfirmation(
                this,
                PreviewMessageCatalog.Get("Settings_Discard_Confirmation"),
                PreviewMessageCatalog.Get("Settings_Discard_ConfirmationTitle"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
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

        private MessageBoxResult ShowConfirmation(
            Window owner,
            string message,
            string title,
            MessageBoxButton buttons,
            MessageBoxImage image,
            MessageBoxResult defaultResult)
        {
            IInputElement previousFocus = Keyboard.FocusedElement;
            MessageBoxResult result = MessageBox.Show(owner, message, title, buttons, image, defaultResult);
            RestoreFocus(previousFocus);
            return result;
        }

        private void RestoreFocus(IInputElement previousFocus)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Input,
                new Action(() => previousFocus?.Focus()));
        }

        private void ShellViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PreviewShellViewModel.CurrentDestination))
            {
                return;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(FocusCurrentWorkflow));
        }

        private void FocusCurrentWorkflow()
        {
            switch (_shellViewModel.CurrentDestination)
            {
                case PreviewShellDestination.Translate:
                    TranslationWorkspace.FocusInitialControl();
                    break;
                case PreviewShellDestination.Review:
                case PreviewShellDestination.Quality:
                    ReviewQualityWorkspace.FocusInitialControl();
                    break;
                case PreviewShellDestination.History:
                case PreviewShellDestination.ProjectUpdate:
                    HistoryUpdateWorkspace.FocusInitialControl();
                    break;
                case PreviewShellDestination.Settings:
                    SettingsWorkspace.FocusInitialControl();
                    break;
                default:
                    PrimaryNavigation.Focus();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FocusCurrentWorkflow();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F1)
            {
                return;
            }

            ShowKeyboardHelp();
            e.Handled = true;
        }

        private void KeyboardHelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShowKeyboardHelp();
        }

        private void ShowKeyboardHelp()
        {
            IInputElement previousFocus = Keyboard.FocusedElement;
            MessageBox.Show(
                this,
                PreviewMessageCatalog.Get("Accessibility_KeyboardHelp_Content"),
                PreviewMessageCatalog.Get("Accessibility_KeyboardHelp_Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Information,
                MessageBoxResult.OK);
            RestoreFocus(previousFocus);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_settingsViewModel.TryDiscardForClose())
            {
                e.Cancel = true;
                return;
            }

            _settingsViewModel.Dispose();
            _shellViewModel.PropertyChanged -= ShellViewModelPropertyChanged;
            _historyUpdateViewModel.Dispose();
            _reviewQualityViewModel.Dispose();
            _translationWorkspaceViewModel.Dispose();
            DeFine.CloseAny();
        }
    }
}
