using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using PhoenixEngine.Engine.ADO;
using LexTranslator.UIManagement;

namespace LexTranslator
{
    public partial class HistoryWindow : Window
    {
        private int _FileUniqueKey;
        private List<HistoryRecord> _AllRecords = new List<HistoryRecord>();
        private List<HistoryRecord> _FilteredRecords = new List<HistoryRecord>();

        private TranslateView _Owner = null;
        public HistoryWindow(TranslateView Owner, int FileUniqueKey)
        {
            InitializeComponent();
            _FileUniqueKey = FileUniqueKey;

            this.Owner = Application.Current.MainWindow;
            this._Owner = Owner;

            if (!this.Resources.Contains("CurrentStatusConverter"))
            {
                this.Resources.Add("CurrentStatusConverter", new CurrentStatusConverter());
            }

            this.PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void Window_Loaded(object Sender, RoutedEventArgs E)
        {
            LoadHistoryData();
        }

        private void Window_Closing(object Sender, System.ComponentModel.CancelEventArgs E)
        {
            this._Owner.CurrentHistory = null;
        }

        private void Window_PreviewKeyDown(object Sender, KeyEventArgs E)
        {
            if (E.Key == Key.Escape)
                this.Close();
            else if (E.Key == Key.F5)
                RefreshData();
            else if (E.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
                E.Handled = true;
            }
        }

        private void LoadHistoryData()
        {
            try
            {
                StatusText.Text = "Loading history records...";
                var RawItems = HistoryDBCache.GetHistoryItems(_FileUniqueKey,(int)_Owner.Mod.P_Translator.To);

                var List = RawItems
                    .OrderBy(x => x.Rowid)
                    .Select((Item, Idx) => new HistoryRecord
                    {
                        Index = Idx + 1,
                        FileUniqueKey = Item.FileUniqueKey,
                        Rowid = Item.Rowid,
                        Key = Item.Key,
                        To = Item.To,
                        CurrentText = Item.CurrentText,
                        IsCurrent = Item.IsCurrent,
                        Time = Item.Time,
                        RangeID = Item.RangeID
                    })
                    .ToList();

                _AllRecords = List;
                ApplyFilter();
                StatusText.Text = $"Loaded {_FilteredRecords.Count} records (total: {_AllRecords.Count})";

                if (_FilteredRecords.Any(R => R.IsCurrent == 1))
                {
                    var Current = _FilteredRecords.First(R => R.IsCurrent == 1);
                    ScrollToItem(Current);
                }
            }
            catch (Exception Ex)
            {
                StatusText.Text = $"Error: {Ex.Message}";
                MessageBox.Show($"Failed to load history:\n{Ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshData()
        {
            LoadHistoryData();
        }

        private void ApplyFilter()
        {
            string Keyword = SearchBox.Text?.Trim() ?? string.Empty;

            var Query = _AllRecords.AsEnumerable();

            if (!string.IsNullOrEmpty(Keyword))
            {
                Query = Query.Where(R =>
                    (R.PreviousText?.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                    (R.CurrentText?.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                );
            }

            _FilteredRecords = Query.ToList();

            HistoryGrid.ItemsSource = null;
            HistoryGrid.ItemsSource = _FilteredRecords;

            if (string.IsNullOrEmpty(Keyword))
                StatusText.Text = $"Loaded {_FilteredRecords.Count} records (total: {_AllRecords.Count})";
            else
                StatusText.Text = $"Found {_FilteredRecords.Count} records matching '{Keyword}' (total: {_AllRecords.Count})";
        }

        private void ScrollToItem(HistoryRecord Item)
        {
            try
            {
                int Index = _FilteredRecords.IndexOf(Item);
                if (Index >= 0)
                {
                    var Row = HistoryGrid.ItemContainerGenerator.ContainerFromIndex(Index) as DataGridRow;
                    if (Row != null)
                        Row.BringIntoView();
                    else
                        HistoryGrid.ScrollIntoView(Item);
                }
            }
            catch { }
        }

        private void SearchBox_TextChanged(object Sender, TextChangedEventArgs E)
        {
            ApplyFilter();
        }

        private void SearchClick(object Sender, MouseButtonEventArgs E)
        {
            ApplyFilter();
        }
        private void RefreshClick(object Sender, MouseButtonEventArgs E)
        {
            RefreshData();
        }

        private void ClearAllClick(object Sender, MouseButtonEventArgs E)
        {
            if (_AllRecords.Count == 0)
            {
                StatusText.Text = "No records to clear.";
                return;
            }

            if (MessageBox.Show($"Delete all {_AllRecords.Count} history records?\nThis cannot be undone.",
                                "Confirm Clear All",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    StatusText.Text = "Clearing all records...";
                    if (HistoryDBCache.ClearHistory(_FileUniqueKey))
                    {
                        _AllRecords.Clear();
                        _FilteredRecords.Clear();
                        HistoryGrid.ItemsSource = null;
                        HistoryGrid.ItemsSource = _FilteredRecords;
                        StatusText.Text = "All history records cleared.";
                    }
                    else
                        StatusText.Text = "Failed to clear history.";
                }
                catch (Exception Ex)
                {
                    StatusText.Text = $"Error: {Ex.Message}";
                    MessageBox.Show($"Clear failed:\n{Ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void CloseClick(object Sender, MouseButtonEventArgs E) => this.Close();

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

    public class HistoryRecord
    {
        public int Index { get; set; }
        public int FileUniqueKey { get; set; }
        public int Rowid { get; set; }
        public string Key { get; set; }
        public int To { get; set; }
        public string PreviousText { get; set; }
        public string CurrentText { get; set; }
        public int IsCurrent { get; set; }
        public DateTime Time { get; set; }
        public string RangeID { get; set; }
    }

    public class CurrentStatusConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, System.Globalization.CultureInfo Culture)
        {
            return (Value is int V && V == 1) ? "Current" : "History";
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, System.Globalization.CultureInfo Culture)
        {
            throw new NotImplementedException();
        }
    }
}