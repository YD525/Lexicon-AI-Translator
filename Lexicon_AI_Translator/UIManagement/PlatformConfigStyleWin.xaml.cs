using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LexTranslator.UIManage;
using PhoenixEngine.PlatformManagement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace LexTranslator.UIManagement
{
    /// <summary>
    /// Interaction logic for PlatformConfigStyleWin.xaml
    /// </summary>
    public partial class PlatformConfigStyleWin : Window
    {
        public PlatformConfigStyleWin()
        {
            InitializeComponent();
        }

        public Border GenCloudAIConfig(string PlatformName, string Document, bool IsSystemNode, List<string> Keys, string Model, CustomPlatformType CustomType, List<string> Models)
        {
            Border GetPlatformBorder = UIHelper.CloneElement(CloudAIConfig);

            Grid GetChildGrid = GetPlatformBorder.Child as Grid;
            Grid Header = GetChildGrid.Children[0] as Grid;
            Grid Body = GetChildGrid.Children[1] as Grid;
            StackPanel GetTittlePanel = (Header.Children[0] as Border).Child as StackPanel;
            StackPanel ControlPanel = (Header.Children[1] as Grid).Children[0] as StackPanel;

            Label TittleLab = GetTittlePanel.Children[0] as Label;
            TittleLab.Content = PlatformName;

            Ellipse GetShape = GetTittlePanel.Children[1] as Ellipse;
            GetShape.Fill = new SolidColorBrush(DeFine.NodeStyleWin.GetNodeColor(CustomType));

            if (Document.Length == 0)
            {
                (ControlPanel.Children[0] as Label).Visibility = Visibility.Collapsed;
            }
            else
            {
                (ControlPanel.Children[0] as Label).Content = Document;
            }

            if (IsSystemNode)
            {
                (ControlPanel.Children[1] as Border).Visibility = Visibility.Collapsed;
            }
            else
            {
                (ControlPanel.Children[1] as Border).Visibility = Visibility.Visible;
            }

            TextBox GetModelTextBox = ((Body.Children[0] as Grid).Children[0] as StackPanel).Children[5] as TextBox;
            ComboBox GetModels = ((Body.Children[0] as Grid).Children[0] as StackPanel).Children[6] as ComboBox;
            GetModels.Items.Clear();

            foreach (var GetModel in Models)
            {
                GetModels.Items.Add(GetModel);
            }

            GetModelTextBox.Text = Model;

            ListBox GetKeys = ((Body.Children[0] as Grid).Children[1] as ScrollViewer).Content as ListBox;
            GetKeys.Items.Clear();

            for (int i = 0; i < Keys.Count; i++)
            {
                GetKeys.Items.Add(Keys[i]);
            }

            return GetPlatformBorder;
        }

        public Border GenLocalAIConfig(string PlatformName, string Document, bool IsSystemNode, List<string> Keys, string Model, CustomPlatformType CustomType)
        {
            Border GetPlatformBorder = UIHelper.CloneElement(CloudAIConfig);

            Grid GetChildGrid = GetPlatformBorder.Child as Grid;
            Grid Header = GetChildGrid.Children[0] as Grid;
            Grid Body = GetChildGrid.Children[1] as Grid;
            StackPanel GetTittlePanel = (Header.Children[0] as Border).Child as StackPanel;
            StackPanel ControlPanel = (Header.Children[1] as Grid).Children[0] as StackPanel;

            Label TittleLab = GetTittlePanel.Children[0] as Label;
            TittleLab.Content = PlatformName;

            Ellipse GetShape = GetTittlePanel.Children[1] as Ellipse;
            GetShape.Fill = new SolidColorBrush(DeFine.NodeStyleWin.GetNodeColor(CustomType));

            if (Document.Length == 0)
            {
                (ControlPanel.Children[0] as Label).Visibility = Visibility.Collapsed;
            }
            else
            {
                (ControlPanel.Children[0] as Label).Content = Document;
            }

            if (IsSystemNode)
            {
                (ControlPanel.Children[1] as Border).Visibility = Visibility.Collapsed;
            }
            else
            {
                (ControlPanel.Children[1] as Border).Visibility = Visibility.Visible;
            }

            Label GetModelLab = ((Body.Children[0] as Grid).Children[0] as StackPanel).Children[5] as Label;

            GetModelLab.Content = Model;

            ListBox GetKeys = ((Body.Children[0] as Grid).Children[1] as ScrollViewer).Content as ListBox;
            GetKeys.Items.Clear();

            for (int i = 0; i < Keys.Count; i++)
            {
                GetKeys.Items.Add(Keys[i]);
            }

            return GetPlatformBorder;
        }


        public Border GenTraditionalConfig(string PlatformName, string Document, bool IsSystemNode, List<string> Keys,CustomPlatformType CustomType)
        {
            Border GetPlatformBorder = UIHelper.CloneElement(CloudAIConfig);

            Grid GetChildGrid = GetPlatformBorder.Child as Grid;
            Grid Header = GetChildGrid.Children[0] as Grid;
            Grid Body = GetChildGrid.Children[1] as Grid;
            StackPanel GetTittlePanel = (Header.Children[0] as Border).Child as StackPanel;
            StackPanel ControlPanel = (Header.Children[1] as Grid).Children[0] as StackPanel;

            Label TittleLab = GetTittlePanel.Children[0] as Label;
            TittleLab.Content = PlatformName;

            Ellipse GetShape = GetTittlePanel.Children[1] as Ellipse;
            GetShape.Fill = new SolidColorBrush(DeFine.NodeStyleWin.GetNodeColor(CustomType));

            if (Document.Length == 0)
            {
                (ControlPanel.Children[0] as Label).Visibility = Visibility.Collapsed;
            }
            else
            {
                (ControlPanel.Children[0] as Label).Content = Document;
            }

            if (IsSystemNode)
            {
                (ControlPanel.Children[1] as Border).Visibility = Visibility.Collapsed;
            }
            else
            {
                (ControlPanel.Children[1] as Border).Visibility = Visibility.Visible;
            }

            ListBox GetKeys = ((Body.Children[0] as Grid).Children[1] as ScrollViewer).Content as ListBox;
            GetKeys.Items.Clear();

            for (int i = 0; i < Keys.Count; i++)
            {
                GetKeys.Items.Add(Keys[i]);
            }

            return GetPlatformBorder;
        }
    }
}
