using System.Windows;
using System.Windows.Controls;
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
        public CustomPlatformType GetHeaderTagType(Grid Header)
        {
            return (CustomPlatformType)(Header.Children[0] as Label).Tag;
        }
        public Grid GenNodeTree(string Tittle, CustomPlatformType Type)
        {
            Grid NewHeaderTag = UIHelper.CloneElement(HeaderTag);
            Label GetTittle = (NewHeaderTag.Children[0] as Border).Child as Label;
            GetTittle.Content = Tittle;
            GetTittle.Tag = Type;
            return NewHeaderTag;
        }

        public Grid GenMainNodeTree(string Tittle, CustomPlatformType Type)
        {
            Grid NewHeaderTag = UIHelper.CloneElement(MainHeaderTag);
            Label GetTittle = (NewHeaderTag.Children[1] as Border).Child as Label;
            GetTittle.Content = Tittle;
            GetTittle.Tag = Type;
            return NewHeaderTag;
        }

        public Color GetNodeColor(CustomPlatformType Type)
        {
            switch (Type)
            {
                case CustomPlatformType.CloudAI:
                    return Color.FromRgb(0, 0, 0);
                case CustomPlatformType.LocalAI:
                    return Color.FromRgb(0, 0, 0);
                case CustomPlatformType.Traditional:
                    return Color.FromRgb(0, 0, 0);
            }

            return Color.FromRgb(0, 0, 0);
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

        public Grid GenNode(string PlatformName, CustomPlatformType Type)
        {
            Grid NodeGrid = UIHelper.CloneElement(Node);
            Grid GetMask = NodeGrid.Children[0] as Grid;
            Grid NodeBody = NodeGrid.Children[1] as Grid;

            StackPanel GetStackPanel = NodeBody.Children[0] as StackPanel;
            Grid GetColorGrid = GetStackPanel.Children[0] as Grid;

            GetColorGrid.Background = new SolidColorBrush(GetNodeColor(Type));

            Label GetTittle = GetStackPanel.Children[1] as Label;
            GetTittle.Content = PlatformName;

            return NodeGrid;
        }

        public Grid GenEmptyNode()
        {
            Grid EmptyNodeGrid = UIHelper.CloneElement(EmptyNode);
            return EmptyNodeGrid;
        }
    }
}
