using System;
using System.ComponentModel;
using System.Windows;
using PhoenixTranslator.ApplicationLayer;

namespace PhoenixTranslator.UIManagement.Preview
{
    /// <summary>
    /// Hosts preview workflow navigation while retaining the complete legacy workspace as a fallback.
    /// </summary>
    public partial class PreviewShellWindow : Window
    {
        private PhoenixGui _legacyWorkspace;

        /// <summary>
        /// Creates the preview application shell in its no-project state.
        /// </summary>
        public PreviewShellWindow()
        {
            InitializeComponent();
            DataContext = new PreviewShellViewModel(OpenLegacyWorkspace);
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
            DeFine.CloseAny();
        }
    }
}
