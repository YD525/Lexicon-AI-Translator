using System.Windows.Controls;

namespace PhoenixTranslator.UIManagement.Preview
{
    /// <summary>
    /// Presents the keyboard-first preview translation workspace.
    /// </summary>
    public partial class PreviewTranslationWorkspaceView : UserControl
    {
        /// <summary>
        /// Creates the preview translation workspace view.
        /// </summary>
        public PreviewTranslationWorkspaceView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Moves keyboard focus to the workflow search field after shell navigation.
        /// </summary>
        internal void FocusInitialControl()
        {
            SearchBox.Focus();
        }
    }
}
