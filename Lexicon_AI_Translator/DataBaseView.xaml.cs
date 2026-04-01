using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PhoenixEngine;
using PhoenixEngine.ADO;

namespace LexTranslator
{
    public partial class DataBaseView : Window
    {
        private string _TableName;

        private Dictionary<int, long> _RowIds = new Dictionary<int, long>();

        public void QueryFirst(string Sql)
        {
            SqlOrder.Text = Sql;
            RunQuery(SqlOrder.Text);
        }
        public DataBaseView()
        {
            InitializeComponent();
        }

        // ── Update table name display ─────────────────────────────
        private void SetTableName(string Name)
        {
            _TableName = Name;
            TableNameBlock.Text = Name;
            TableNameRun.Text = Name;
            Title = $"DataBaseView — {Name}";
        }

        // ── Query button click ────────────────────────────────────
        private void QueryDataBase(object Sender, RoutedEventArgs E)
        {
            string Sql = SqlOrder.Text?.Trim();
            if (string.IsNullOrEmpty(Sql)) return;

            string Parsed = ParseTableName(Sql);
            if (!string.IsNullOrEmpty(Parsed))
                SetTableName(Parsed);

            RunQuery(Sql);
        }

        // ── Insert button: add empty row ──────────────────────────
        private void InsertRow(object Sender, RoutedEventArgs E)
        {

        }

        public string ExtractTableName(string Sql)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Sql))
                    return string.Empty;

                Sql = Sql.Trim();

                string Pattern = @"(?i)^\s*(?:SELECT\s+.*?\s+FROM|INSERT\s+INTO|UPDATE|DELETE\s+FROM)\s+([`""\[]?)(\w+)\1";

                Match Match = Regex.Match(Sql, Pattern);
                if (Match.Success)
                {
                    string TableName = Match.Groups[2].Value;
                    SetTableName(TableName);
                    return TableName;
                }

                SetTableName(string.Empty);
                return string.Empty;
            }
            catch 
            {
                SetTableName(string.Empty);
                return string.Empty;
            }
        }

        private void RunQuery(string UserSql)
        {
            try
            {
                ExtractTableName(UserSql);
                SetStatus("Querying...", true);
                _RowIds.Clear();

                string RewrittenSql = InjectRowid(UserSql);

                List<Dictionary<string, object>> Rows =
                    Phoenix.LocalDB.P_ExecuteQuery(RewrittenSql);

                DataTable Table = ToDataTable(Rows);

                // Cache rowid per row index, then hide the rowid column from view
                for (int I = 0; I < Table.Rows.Count; I++)
                {
                    if (Table.Columns.Contains("Rowid") &&
                        long.TryParse(Table.Rows[I]["Rowid"]?.ToString(), out long Rid))
                        _RowIds[I] = Rid;
                }

                BuildColumns(Table);

                MainGrid.BeginningEdit -= OnBeginningEdit;
                MainGrid.CellEditEnding -= OnCellEditEnding;
                MainGrid.BeginningEdit += OnBeginningEdit;
                MainGrid.CellEditEnding += OnCellEditEnding;

                MainGrid.ItemsSource = Table.DefaultView;

                RowCountRun.Text = Table.Rows.Count.ToString();
                ColCountRun.Text = (Table.Columns.Count - 1).ToString(); // exclude rowid
                SetStatus($"OK · {Table.Rows.Count} rows", true);
            }
            catch (Exception Ex)
            {
                SetStatus($"Error: {Ex.Message}", false);
            }
        }

        private string InjectRowid(string Sql)
        {
            if (Regex.IsMatch(Sql, @"\browid\b", RegexOptions.IgnoreCase))
                return Sql;

            var Match = Regex.Match(Sql,
                @"^(\s*SELECT\s+)(.*?)(\s+FROM\s+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!Match.Success) return Sql;

            string Keyword = Match.Groups[1].Value;
            string Columns = Match.Groups[2].Value;
            string FromClause = Match.Groups[3].Value;
            string Rest = Sql.Substring(Match.Length);

            return $"{Keyword}Rowid, {Columns.Trim()}{FromClause}{Rest}";
        }

        private Dictionary<(int, string), string> _EditSnapshots
        = new Dictionary<(int, string), string>();
        private void OnBeginningEdit(object Sender, DataGridBeginningEditEventArgs E)
        {
            string ColName = E.Column.Header?.ToString();
            if (string.IsNullOrEmpty(ColName) || ColName == "Rowid") return;

            int RowIndex = E.Row.GetIndex();
            DataRowView Drv = E.Row.Item as DataRowView;
            if (Drv == null) return;

            string OldValue = Drv.Row[ColName]?.ToString() ?? "";
            _EditSnapshots[(RowIndex, ColName)] = OldValue;
        }
        private TextBox FindChildTextBox(DependencyObject Parent)
        {
            if (Parent == null) return null;
            int Count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(Parent);
            for (int I = 0; I < Count; I++)
            {
                var Child = System.Windows.Media.VisualTreeHelper.GetChild(Parent, I);
                if (Child is TextBox Tb) return Tb;
                var Found = FindChildTextBox(Child);
                if (Found != null) return Found;
            }
            return null;
        }

        private void OnCellEditEnding(object Sender, DataGridCellEditEndingEventArgs E)
        {
            if (E.EditAction != DataGridEditAction.Commit) return;

            string EditedColumn = E.Column.Header?.ToString();
            if (string.IsNullOrEmpty(EditedColumn) || EditedColumn == "Rowid") return;

            int RowIndex = E.Row.GetIndex();
            if (!_RowIds.TryGetValue(RowIndex, out long Rowid))
            {
                SetStatus("UPDATE skipped: rowid not found for this row", false);
                return;
            }

            string NewValue = "";
            if (E.EditingElement is TextBox Tb)
                NewValue = Tb.Text;
            else
                NewValue = FindChildTextBox(E.EditingElement)?.Text ?? "";

            // Use pre-cached snapshot, not DataRowView (which is already updated)
            string OldValue = "";
            _EditSnapshots.TryGetValue((RowIndex, EditedColumn), out OldValue);
            _EditSnapshots.Remove((RowIndex, EditedColumn));

            if (NewValue == OldValue) return;

            string UpdateSql =
                $"UPDATE {Quote(_TableName)} " +
                $"SET {Quote(EditedColumn)} = {SqlVal(NewValue)} " +
                $"WHERE rowid = {Rowid};";

            try
            {
                Phoenix.LocalDB.P_ExecuteQuery(UpdateSql);
                SetStatus($"Updated [{EditedColumn}] = \"{NewValue}\"  (rowid={Rowid})", true);
            }
            catch (Exception Ex)
            {
                SetStatus($"UPDATE failed: {Ex.Message}", false);
            }
        }

        // ── Wrap identifier in double quotes ──────────────────────
        private string Quote(string Name) => $"\"{Name}\"";

        // ── Escape and quote a SQL string value ───────────────────
        private string SqlVal(string Value)
        {
            if (Value == null || Value == "(null)") return "NULL";
            return "'" + Value.Replace("'", "''") + "'";
        }

        // ── List<Dictionary<string,object>> → DataTable ───────────
        private DataTable ToDataTable(List<Dictionary<string, object>> Rows)
        {
            var Table = new DataTable();
            if (Rows == null || Rows.Count == 0) return Table;

            foreach (var Key in Rows[0].Keys)
                Table.Columns.Add(Key, typeof(string));

            foreach (var Row in Rows)
            {
                DataRow Dr = Table.NewRow();
                foreach (var Key in Row.Keys)
                {
                    var Value = Row[Key];

                    if (Value != null && (Key.Equals("Source")
                        ||
                        Key.Equals("Result")))
                    {
                        // Auto Decode
                        Value = SQLSafeCodec.Decode(Value.ToString());
                    }

                    Dr[Key] = Value == null ? "(null)" : Value.ToString();
                }

                Table.Rows.Add(Dr);
            }

            return Table;
        }

        // ── Build DataGrid columns dynamically ────────────────────
        private void BuildColumns(DataTable Table)
        {
            MainGrid.Columns.Clear();

            foreach (DataColumn Col in Table.Columns)
            {
                bool IsMultiLine = Col.ColumnName == "Source" || Col.ColumnName == "Result";

                DataGridColumn GridCol;

                if (IsMultiLine)
                {
                    // Template column — display TextBlock, edit multiline TextBox
                    var DisplayTemplate = new DataTemplate();
                    var DisplayFactory = new FrameworkElementFactory(typeof(TextBlock));
                    DisplayFactory.SetBinding(TextBlock.TextProperty, new Binding($"[{Col.ColumnName}]"));
                    DisplayFactory.SetValue(TextBlock.PaddingProperty, new Thickness(10, 0, 10, 0));
                    DisplayFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                    DisplayFactory.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Consolas"));
                    DisplayFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(212, 212, 212)));
                    DisplayFactory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
                    DisplayTemplate.VisualTree = DisplayFactory;

                    var EditTemplate = new DataTemplate();
                    var EditFactory = new FrameworkElementFactory(typeof(TextBox));
                    EditFactory.SetBinding(TextBox.TextProperty, new Binding($"[{Col.ColumnName}]")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                    });
                    EditFactory.SetValue(TextBox.AcceptsReturnProperty, true);
                    EditFactory.SetValue(TextBox.TextWrappingProperty, TextWrapping.Wrap);
                    EditFactory.SetValue(TextBox.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                    EditFactory.SetValue(TextBox.MinHeightProperty, 80d);
                    EditFactory.SetValue(TextBox.MaxHeightProperty, 200d);
                    EditFactory.SetValue(TextBox.BackgroundProperty, new SolidColorBrush(Color.FromRgb(13, 31, 48)));
                    EditFactory.SetValue(TextBox.ForegroundProperty, new SolidColorBrush(Colors.White));
                    EditFactory.SetValue(TextBox.BorderThicknessProperty, new Thickness(0));
                    EditFactory.SetValue(TextBox.CaretBrushProperty, new SolidColorBrush(Color.FromRgb(11, 116, 209)));
                    EditFactory.SetValue(TextBox.FontFamilyProperty, new FontFamily("Consolas"));
                    EditFactory.SetValue(TextBox.FontSizeProperty, 12d);
                    EditFactory.SetValue(TextBox.PaddingProperty, new Thickness(10, 6, 10, 6));
                    EditFactory.SetValue(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Top);
                    EditTemplate.VisualTree = EditFactory;

                    var TemplateCol = new DataGridTemplateColumn
                    {
                        Header = Col.ColumnName,
                        CellTemplate = DisplayTemplate,
                        CellEditingTemplate = EditTemplate,
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                        CanUserSort = false
                    };
                    GridCol = TemplateCol;
                }
                else
                {
                    // Standard single-line text column
                    var TextCol = new DataGridTextColumn
                    {
                        Header = Col.ColumnName,
                        Binding = new Binding($"[{Col.ColumnName}]"),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                        ElementStyle = MakeTextBlockStyle(Col.ColumnName),
                        EditingElementStyle = MakeEditBoxStyle(),
                        CanUserSort = false
                    };
                    GridCol = TextCol;
                }

                MainGrid.Columns.Add(GridCol);
            }
        }

        // ── Cell display style (color by column name convention) ──
        private Style MakeTextBlockStyle(string ColumnName)
        {
            var Style = new Style(typeof(TextBlock));
            var Name = ColumnName.ToLower();

            SolidColorBrush Fg;
            if (Name == "id" || Name.EndsWith("id") || Name.EndsWith("_id"))
                Fg = new SolidColorBrush(Color.FromRgb(74, 163, 240));   // Blue — primary key
            else if (Name.Contains("time") || Name.Contains("date") || Name.EndsWith("at"))
                Fg = new SolidColorBrush(Color.FromRgb(102, 102, 102));  // Gray — timestamp
            else
                Fg = new SolidColorBrush(Color.FromRgb(212, 212, 212));  // Default

            Style.Setters.Add(new Setter(TextBlock.ForegroundProperty, Fg));
            Style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(10, 0, 10, 0)));
            Style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            Style.Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Consolas")));

            var NullTrigger = new DataTrigger { Binding = new Binding("."), Value = "(null)" };
            NullTrigger.Setters.Add(new Setter(TextBlock.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(85, 85, 85))));
            NullTrigger.Setters.Add(new Setter(TextBlock.FontStyleProperty, FontStyles.Italic));
            Style.Triggers.Add(NullTrigger);

            return Style;
        }

        // ── Cell editing TextBox style ────────────────────────────
        private Style MakeEditBoxStyle()
        {
            var Style = new Style(typeof(TextBox));
            Style.Setters.Add(new Setter(TextBox.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(13, 31, 48))));
            Style.Setters.Add(new Setter(TextBox.ForegroundProperty,
                new SolidColorBrush(Colors.White)));
            Style.Setters.Add(new Setter(TextBox.BorderThicknessProperty, new Thickness(0)));
            Style.Setters.Add(new Setter(TextBox.CaretBrushProperty,
                new SolidColorBrush(Color.FromRgb(11, 116, 209))));
            Style.Setters.Add(new Setter(TextBox.FontFamilyProperty, new FontFamily("Consolas")));
            Style.Setters.Add(new Setter(TextBox.FontSizeProperty, 12d));
            Style.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(10, 0, 10, 0)));
            return Style;
        }

        // ── Extract table name from SQL (FROM keyword) ────────────
        private string ParseTableName(string Sql)
        {
            try
            {
                var Match = Regex.Match(Sql,
                    @"\bFROM\s+([`""\[]?[\w]+[`""\]]?)",
                    RegexOptions.IgnoreCase);
                if (!Match.Success) return null;

                // Strip any quoting characters
                return Match.Groups[1].Value.Trim('`', '"', '[', ']');
            }
            catch { return null; }
        }

        // ── Update status bar message ─────────────────────────────
        private void SetStatus(string Msg, bool Ok)
        {
            StatusMsg.Text = Msg;
            StatusMsg.Foreground = Ok
                ? new SolidColorBrush(Color.FromRgb(126, 200, 160))
                : new SolidColorBrush(Color.FromRgb(226, 75, 74));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DeFine.CloseDataBaseView();
        }
    }
}
