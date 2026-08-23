using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PhoenixTranslator.SkyrimManagement;
using PhoenixTranslator.UIManage;

namespace PhoenixTranslator.UIManagement.Main
{
    /// <summary>
    /// Interaction logic for ModFileDialog.xaml
    /// </summary>
    public partial class ModFileDialog : Window
    {
        public BlockListView ModView = null;

        public ModFileDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ModView == null)
            {
                ModView = new BlockListView(CModView, 500);


                ModView.ExColStyles.Add(new ExColStyle(1, GridUnitType.Star));
                ModView.ExColStyles.Add(new ExColStyle(1, GridUnitType.Star));
                ModView.ExColStyles.Add(new ExColStyle(1, GridUnitType.Star));
                ModView.ExColStyles.Add(new ExColStyle(1, GridUnitType.Star));
                ModView.ExColStyles.Add(new ExColStyle(1, GridUnitType.Star));
            }
        }

        public void UPDateMods(List<SkyrimMod> Mods)
        {
            int ColumnLength = 5;

            int CurrentLength = 5;

            List<SkyrimMod> ModBlocks = new List<SkyrimMod>();

            foreach (var GetMod in Mods)
            {
                if (CurrentLength > 0)
                {
                    CurrentLength--;
                    ModBlocks.Add(GetMod);
                }

                if (CurrentLength == 0)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        ModView.AddRow(UIHelper.CreateModLine(ModBlocks).ToArray());
                    }));

                    ModBlocks.Clear();
                    CurrentLength = ColumnLength;
                }
            }

            if (ModBlocks.Count > 0)
            {
                for (int i = 0; i < CurrentLength; i++)
                {
                    ModBlocks.Add(new SkyrimMod());
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    ModView.AddRow(UIHelper.CreateModLine(ModBlocks).ToArray());
                }));
            }

        }

        private void Path_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchStr_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Close_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public bool IsLeftMouseDown = false;

        private void WinHead_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsLeftMouseDown = true;
            }

            if (IsLeftMouseDown)
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.DragMove();
                    }));

                    IsLeftMouseDown = false;
                }
                catch { }
            }
        }

       
    }
}
