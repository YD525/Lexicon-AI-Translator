using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LexTranslator.UIManage;
using PhoenixEngine.PlatformManagement;

namespace LexTranslator.UIManagement
{
    /// <summary>
    /// Interaction logic for NodeStyleWin.xaml
    /// </summary>
    public partial class NodeStyleWin : Window
    {
        public NodeStyleWin()
        {
            InitializeComponent();
        }

        public void SetHeaderTagEnableInFo(Grid Header, string InFo)
        {
            if (Header.Children[1] is Border)
            {
                Label GetInFoControlHandle = ((Border)Header.Children[1]).Child as Label;
                GetInFoControlHandle.Content = InFo;
            }
        }

        public Grid GenNodeTree(string Tittle)
        {
            Grid NewHeaderTag = UIHelper.CloneElement(HeaderTag);
            Label GetTittle = NewHeaderTag.Children[0] as Label;
            GetTittle.Content = Tittle;
            return NewHeaderTag;
        }

        public Grid GenMainNodeTree(string Tittle)
        {
            Grid NewHeaderTag = UIHelper.CloneElement(MainHeaderTag);
            Label GetTittle = NewHeaderTag.Children[0]  as Label;
            GetTittle.Content = Tittle;
            return NewHeaderTag;
        }

        public Color GetNodeColor(CustomPlatformType Type)
        {
            switch (Type)
            {
                case CustomPlatformType.CloudAI:
                    return Color.FromRgb(11, 116, 209);
                case CustomPlatformType.LocalAI:
                    return Color.FromRgb(5, 190, 218);
                case CustomPlatformType.Traditional:
                    return Color.FromRgb(210, 2, 120);
            }

            return Color.FromRgb(5, 190, 218);
        }

        public void SetNodeEnable(Grid NodeGrid,bool Enable)
        {
            Grid GetMask = NodeGrid.Children[0] as Grid;

            if (Enable)
            {
                GetMask.Visibility = Visibility.Visible;
            }
            else
            {
                GetMask.Visibility = Visibility.Collapsed;
            }
        }

        public ContentControl GetNodeLight(Grid NodeGrid)
        {
            Grid NodeBody = NodeGrid.Children[1] as Grid;

            return NodeBody.Children[1] as ContentControl;
        }

        public Grid GenNode(string PlatformName, CustomPlatformType Type,bool Enable)
        {
            Grid NodeGrid = UIHelper.CloneElement(Node);
            Grid GetMask = NodeGrid.Children[0] as Grid;

            Border GetEnableBtn = (GetMask.Children[1] as Grid).Children[0] as Border;

            GetEnableBtn.PreviewMouseDown += GetEnableBtn_PreviewMouseDown;

            if (Enable)
            {
                GetMask.Visibility = Visibility.Collapsed;
            }

            Grid NodeBody = NodeGrid.Children[1] as Grid;

            StackPanel GetStackPanel = NodeBody.Children[0] as StackPanel;
            Grid GetColorGrid = GetStackPanel.Children[0] as Grid;

            GetColorGrid.Background = new SolidColorBrush(GetNodeColor(Type));

            Label GetTittle = GetStackPanel.Children[1] as Label;
            GetTittle.Content = PlatformName;

            return NodeGrid;
        }

        private void GetEnableBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
          
        }

        public Grid GenEmptyNode(CustomPlatformType Type)
        {
            Grid EmptyNodeGrid = UIHelper.CloneElement(EmptyNode);

            Border GetAddBtn = EmptyNodeGrid.Children[0] as Border;
            GetAddBtn.Tag = Type;

            GetAddBtn.PreviewMouseDown += GetAddBtn_PreviewMouseDown;

            return EmptyNodeGrid;
        }

        private void GetAddBtn_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border)
            { 
               Border GetBtnHandle = (Border)sender;
               CustomPlatformType GetType = (CustomPlatformType)GetBtnHandle.Tag;

                CustomWizard NCustomWizard = new CustomWizard();
                NCustomWizard.Owner = DeFine.WorkingWin;
                NCustomWizard.Show();
                NCustomWizard.SelectPlatformType(GetType);
            }
        }
    }
}
