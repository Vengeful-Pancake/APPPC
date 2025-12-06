using APPPC.Control;
using APPPC.KHSX_Helpers;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using unvell.ReoGrid;
using unvell.ReoGrid.CellTypes;
using unvell.ReoGrid.Data;
using unvell.ReoGrid.DataFormat;
using unvell.ReoGrid.Events;
using unvell.ReoGrid.IO;

namespace APPPC.Functions
{
    public partial class KHSX_panel : UserControl
    {
        private string _tab3MachineId = null;
        private DateTime _tab3StartDate;
        private ComboBox? _tab4KaEditingCombo;
        private DataTable _tab4Table;
        private DataTable allWorkorders;
        private string _tab4MachineId = null;
        private string _tab4DefaultKa = null;

        // ===== ReoGrid (Tab3) =====
        private ReoGridControl _tab3Grid;
        private Worksheet _tab3Sheet;
        private string _tab3ExcelWorkingPath;
        private readonly string _tab3ExcelArchiveDir = @"\\dongau-nas\KHSX\DA\KHSXmayExcel";
        private PartialGrid? _cutRowsBuffer;
        private int _cutRowsCount;
        private bool _hasCutRows;

        private readonly Color _tabGreen = Color.FromArgb(0x18, 0x9F, 0x40);
        private readonly Color _tabGreenActive;
        private List<Button> _tabButtons;

        private Button _btnFillMachineShown;          // Tab2 autofill button
        private Button _btnImportExcel;               // Import Excel button

        // chips UI + state
        private FlowLayoutPanel _filterBar;
        private sealed class Chip
        {
            public string Display { get; init; } = "";
            public string Column { get; init; } = "";
            public string Value { get; init; } = "";
            public Button Button { get; init; }
        }
        private readonly List<Chip> _chips = new();

        // Column map (Tab3)
        private readonly (string header, string field, bool editable, string? fmt)[] _tab3ColMap = new[] {
            ("LSX",            "LSX",            false, null),
            ("Mã SP",          "product_code",   false, null),
            ("Tên Sản Phẩm",   "product_name",   false, null),
            ("Số lượng",       "production_qty", false, "#,##0"),
            ("Số ĐH",          "order_name",     false, null),
            ("Lịch Nhận Tuần", "desire",         false, "dd/MM/yyyy"),
            ("Ngày KH",        "RawDate",        true,  "dd/MM/yyyy"),
            ("Ca",             "ka",             true,  null),
            ("Thứ tự",         "sequence",       true,  "0"),
            ("Th.gian cần (h)","time_needed",    true,  "0.00"),
            ("Giờ bắt đầu",    "start_time",     false, "dd/MM/yyyy HH:mm"),
            ("Giờ kết thúc",   "finish_time",    false, "dd/MM/yyyy HH:mm"),
            ("Ghi chú",        "note",           true,  null),
        };

        public int modetab { get; set; }
        private readonly string[] _kaOptions = { "Bình thường", "Ca 1", "Ca 2", "Nghỉ", "Ca 1 dài", "2 Ca", "Ca 2 dài", "Ca dài", "3 Ka", "Ca dài 10h", "Ka 4H", "Ca dài 11h" };
        private bool _tab4ApplyingKa = false;
        private const string DefaultShiftName = "Bình thường";
        private readonly Dictionary<string, string> _searchMap = new(StringComparer.OrdinalIgnoreCase);

        private static string EscapeLike(string s) => s?.Replace("'", "''") ?? "";
        private static string CombineFilters(params string[] parts) => string.Join(" AND ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        private string? currentOrderKeyForTab2 = null;
        private static string HashDate(DateTime d) => $"#{d.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}#";

        // ===== Excel helpers for import =====
        private static string NormKey(string? s) =>
            (s ?? string.Empty).Trim().Replace(" ", "").Replace("\u200B", "");

        private static string Canon(string s)
        {
            var formD = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.ToLowerInvariant(ch));
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static int FindHeaderColumn(IXLWorksheet ws, params string[] names)
        {
            var header = ws.FirstRowUsed();
            if (header == null) return -1;
            var targets = names.Select(Canon).ToArray();

            foreach (var cell in header.CellsUsed())
            {
                var h = Canon(cell.GetString().Trim());
                if (targets.Any(t => h.Contains(t)))
                    return cell.Address.ColumnNumber;
            }
            return -1;
        }

        private static DateTime? ParseCellDate(IXLCell cell)
        {
            if (cell == null) return null;

            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime().Date;

            if (cell.TryGetValue<double>(out var oa))
            {
                try { return DateTime.FromOADate(oa).Date; } catch { }
            }

            var s = cell.GetString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            string[] fmts = {
                "dd/MM/yyyy","d/M/yyyy","dd-MM-yyyy","d-M-yyyy",
                "yyyy-MM-dd","M/d/yyyy","MM/dd/yyyy"
            };
            if (DateTime.TryParseExact(s, fmts,
                CultureInfo.GetCultureInfo("vi-VN"),
                DateTimeStyles.None, out var dt))
                return dt.Date;

            if (DateTime.TryParse(s, out var auto))
                return auto.Date;

            return null;
        }

        private sealed class ExcelTriple
        {
            public DateTime? LichTuan { get; set; }    // → desire
            public DateTime? NgayYC { get; set; }      // → checker
            public DateTime? DieuChinh { get; set; }   // → extra
        }

        private static Dictionary<string, ExcelTriple> ReadNgayYcDieuChinh_AllSheets(string filePath)
        {
            var map = new Dictionary<string, ExcelTriple>(StringComparer.OrdinalIgnoreCase);
            using var wb = new XLWorkbook(filePath);

            foreach (var ws in wb.Worksheets)
            {
                int colLsx = FindHeaderColumn(ws, "lsx", "lenh sx", "so phieu", "sophieu", "lệnh sx", "Lệnh SX");
                if (colLsx < 1) continue;

                int colLichTuan = FindHeaderColumn(ws, "lịch tuần", "lich tuan", "lich tuan (du kien)", "Ngày Nhận", "ngay nhan", "Ngay Nhan", "ngày nhận");
                int colYC = FindHeaderColumn(ws, "ngày y/c", "ngay y/c", "ngay yc", "y/c", "yc");
                int colDC = FindHeaderColumn(ws, "điều chỉnh", "dieu chinh", "điều-chỉnh");

                if (colLichTuan < 1 && colYC < 1 && colDC < 1) continue;

                var headerRow = ws.FirstRowUsed().RowNumber();
                foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() > headerRow))
                {
                    var key = NormKey(row.Cell(colLsx).GetString());
                    if (string.IsNullOrEmpty(key)) continue;

                    var triple = map.ContainsKey(key) ? map[key] : new ExcelTriple();

                    if (colLichTuan > 0) triple.LichTuan = ParseCellDate(row.Cell(colLichTuan)) ?? triple.LichTuan;
                    if (colYC > 0) triple.NgayYC = ParseCellDate(row.Cell(colYC)) ?? triple.NgayYC;
                    if (colDC > 0) triple.DieuChinh = ParseCellDate(row.Cell(colDC)) ?? triple.DieuChinh;

                    map[key] = triple;
                }
            }
            return map;
        }

        // ===== Date parsing + coloring =====
        private static bool TryGetCellDate(object value, out DateOnly d)
        {
            d = default;
            if (value == null) return false;

            if (value is DateTime dt)
            {
                d = DateOnly.FromDateTime(dt.Date);
                return true;
            }

            var s = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return false;

            string[] fmts = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(s, fmts, CultureInfo.GetCultureInfo("vi-VN"),
                                       DateTimeStyles.None, out var parsed) ||
                DateTime.TryParse(s, out parsed))
            {
                d = DateOnly.FromDateTime(parsed.Date);
                return true;
            }
            return false;
        }

        private void ApplyDateStatusColorsForRow(DataGridViewRow row)
        {
            if (row?.DataGridView == null || row.IsNewRow) return;

            var desireCell = row.Cells["desire"];
            var checkerCell = row.Cells["checker"];
            var extraCell = row.Cells["extra"];

            // reset to default
            desireCell.Style.BackColor = SystemColors.Window;
            checkerCell.Style.BackColor = SystemColors.Window;
            extraCell.Style.BackColor = SystemColors.Window;

            var today = DateOnly.FromDateTime(DateTime.Today);

            // any value in "extra" -> green
            if (extraCell?.Value != null && !string.IsNullOrWhiteSpace(extraCell.Value.ToString()))
                extraCell.Style.BackColor = Color.LightGreen;

            bool hasDesire = TryGetCellDate(desireCell?.Value, out var dDesire);
            bool hasChecker = TryGetCellDate(checkerCell?.Value, out var dChecker);

            // flag PAST dates only (before today)
            if (hasDesire && dDesire < today) desireCell.Style.BackColor = Color.LightCoral;
            if (hasChecker && dChecker < today) checkerCell.Style.BackColor = Color.LightCoral;

            // checker later than desire -> checker red
            if (hasDesire && hasChecker && dChecker > dDesire)
                checkerCell.Style.BackColor = Color.LightCoral;
        }

        private void ApplyDateStatusColors()
        {
            if (dataGridView2 == null) return;
            foreach (DataGridViewRow r in dataGridView2.Rows)
                ApplyDateStatusColorsForRow(r);
            dataGridView2.Invalidate();
        }

        // ===== Excel import =====
        private void ImportYeuCauFromExcel_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn file Excel (chứa LSX + Lịch tuần/Ngày Y/C/Điều chỉnh)",
                Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
                Multiselect = false
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            Dictionary<string, ExcelTriple> dict;
            try { dict = ReadNgayYcDieuChinh_AllSheets(ofd.FileName); }
            catch (Exception ex)
            {
                MessageBox.Show($"Không đọc được Excel: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dict.Count == 0)
            {
                MessageBox.Show("Không tìm thấy dữ liệu hợp lệ (cần có LSX và ít nhất một trong ba: Lịch tuần / Ngày Y/C / Điều chỉnh).",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dataGridView2.Columns["lsx"] == null)
            {
                MessageBox.Show("Bảng hiện tại không có cột LSX.", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ensure target columns exist
            if (dataGridView2.Columns["desire"] == null)
                dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "desire", HeaderText = "Lịch Nhận Tuần" });
            if (dataGridView2.Columns["checker"] == null)
                dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "checker", HeaderText = "Ngày SX Phản Hồi" });
            if (dataGridView2.Columns["extra"] == null)
                dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "extra", HeaderText = "Ngày Giao Hàng" });

            // Ask the GRID which columns are editable for THIS user
            bool canDesire = WorkorderService.ColumnIsEditable(dataGridView2, "desire");
            bool canChecker = WorkorderService.ColumnIsEditable(dataGridView2, "checker");
            bool canExtra = WorkorderService.ColumnIsEditable(dataGridView2, "extra");

            int updated = 0, notFound = 0, skippedByPerm = 0;
            dataGridView2.SuspendLayout();
            try
            {
                foreach (DataGridViewRow r in dataGridView2.Rows)
                {
                    if (r.IsNewRow) continue;
                    var k = NormKey(r.Cells["lsx"]?.Value?.ToString());
                    if (string.IsNullOrEmpty(k)) { notFound++; continue; }
                    if (!dict.TryGetValue(k, out var triple)) { notFound++; continue; }

                    bool wrote = false;

                    if (triple.LichTuan.HasValue)
                    {
                        if (canDesire) { r.Cells["desire"].Value = triple.LichTuan.Value; wrote = true; }
                        else skippedByPerm++;
                    }
                    if (triple.NgayYC.HasValue)
                    {
                        if (canChecker) { r.Cells["checker"].Value = triple.NgayYC.Value; wrote = true; }
                        else skippedByPerm++;
                    }
                    if (triple.DieuChinh.HasValue)
                    {
                        if (canExtra) { r.Cells["extra"].Value = triple.DieuChinh.Value; wrote = true; }
                        else skippedByPerm++;
                    }

                    if (wrote) updated++;
                }
            }
            finally { dataGridView2.ResumeLayout(); }

            ApplyDateStatusColors();
            MessageBox.Show(
                $"Đã cập nhật {updated} dòng theo LSX.\n" +
                $"Không khớp: {notFound}.\n" +
                (skippedByPerm > 0 ? $"Bỏ qua do quyền hạn: {skippedByPerm}." : ""),
                "Nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // ===== Init & layout =====
        public KHSX_panel()
        {
            InitializeComponent();
            _tabGreenActive = ControlPaint.Light(_tabGreen, 0.45f);
            _tabButtons = new List<Button> { tab1 };


            foreach (var b in _tabButtons)
            {
                if (b == null) continue;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.UseVisualStyleBackColor = false;
                b.BackColor = _tabGreen;
                b.ForeColor = Color.White;
            }

            this.Dock = DockStyle.Fill;

            From.Format = DateTimePickerFormat.Custom; From.CustomFormat = "dd/MM/yyyy";
            To.Format = DateTimePickerFormat.Custom; To.CustomFormat = "dd/MM/yyyy";

            Default_Ka.DropDownStyle = ComboBoxStyle.DropDownList;
            Default_Ka.Items.Clear();
            Default_Ka.Items.AddRange(_kaOptions);

            button1.Visible = true;
            Default_Ka.Visible = false;
            chkCopyToAll.Visible = true;
            ShowEmpty.Text = "Hiển thị LSX trống";

            modetab = 1;
            allWorkorders = WorkorderService.LoadPendingWorkorders(
                dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);

            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;


            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dataGridView2, new object[] { true });

            RefreshSearcherItems();

            _filterBar = new FlowLayoutPanel
            {
                Name = "filterBar",
                AutoSize = true,
                WrapContents = true,
                Location = new Point(textBox1.Left, textBox1.Bottom + 4),
                Width = Math.Max(200, dataGridView2.Width / 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(_filterBar);

            DateSorter.DropDownStyle = ComboBoxStyle.DropDownList;
            _filterBar.Dock = DockStyle.Top;
            _filterBar.BringToFront();

            // Create Import Excel button (above Copy button)
            _btnImportExcel = new Button
            {
                Name = "btnImportExcel",
                Text = "Nhập Excel",
                AutoSize = true
            };
            _btnImportExcel.Click += ImportYeuCauFromExcel_Click;
            Controls.Add(_btnImportExcel);

            // hook coloring events
            dataGridView2.CellEndEdit += DataGridView2_CellEndEdit;
            dataGridView2.CurrentCellDirtyStateChanged += DataGridView2_CurrentCellDirtyStateChanged;
            dataGridView2.DataBindingComplete += DataGridView2_DataBindingComplete;

            // Initial layout
            KHSX_panel_SizeChanged(null, null);

            // chips: press Enter to add
            textBox1.KeyDown += textBox1_KeyDown_AddChipOnEnter;
        }

        private void LayoutGridsUnderHeader()
        {
            int headerBottom = new[] {
                textBox1?.Bottom ?? 0,
                searcher?.Bottom ?? 0,
                ShowEmpty?.Bottom ?? 0,
                ShowOld?.Bottom ?? 0,
                From.Visible ? From.Bottom : 0,
                To.Visible ? To.Bottom : 0,
                DateSorter.Visible ? DateSorter.Bottom : 0,
                Default_Ka.Visible ? Default_Ka.Bottom : 0,
                _btnImportExcel?.Bottom ?? 0,
                chkCopyToAll.Visible ? chkCopyToAll.Bottom : 0,
                button1.Visible ? button1.Bottom : 0,
                btnSaveYeuCau?.Bottom ?? 0,
                _filterBar?.Bottom ?? 0
            }.Max() + 8;

            int rightMargin = 5, bottomMargin = 10;
            int left = dataGridView2.Left;

            dataGridView2.Top = headerBottom;
            dataGridView2.Left = left;
            dataGridView2.Width = this.Width - left - rightMargin;
            dataGridView2.Height = this.Height - dataGridView2.Top - bottomMargin;
            dataGridView2.SendToBack();

            if (_tab3Grid != null)
            {
                _tab3Grid.Top = dataGridView2.Top;
                _tab3Grid.Left = dataGridView2.Left;
                _tab3Grid.Width = dataGridView2.Width;
                _tab3Grid.Height = dataGridView2.Height;
                _tab3Grid.SendToBack();
            }
        }

        private void KHSX_panel_SizeChanged(object sender, EventArgs e)
        {
            ArrangeButtonsRight(From, To, DateSorter, _btnFillMachineShown, chkCopyToAll, button1, btnSaveYeuCau);

            // Place Import Excel directly above Copy button
            if (_btnImportExcel != null && chkCopyToAll != null && !_btnImportExcel.IsDisposed)
            {
                _btnImportExcel.Left = chkCopyToAll.Left;
                _btnImportExcel.Top = Math.Max(0, chkCopyToAll.Top - _btnImportExcel.Height - 4);
            }

            LayoutGridsUnderHeader();
        }

        private void ArrangeButtonsRight(params System.Windows.Forms.Control[] buttons)
        {
            if (buttons == null || buttons.Length == 0) return;
            var list = buttons.Where(b => b != null && !b.IsDisposed).ToList();
            if (list.Count == 0) return;

            int rightMargin = 10;
            int spacing = 5;
            int x = this.ClientSize.Width - rightMargin;

            var refCtrl = list.FirstOrDefault(c => c.Visible) ?? list[0];
            int y = refCtrl.Top;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var btn = list[i];
                if (btn == null || btn.IsDisposed) continue;

                x -= btn.Width;
                btn.Location = new Point(x, y);
                x -= spacing;
            }
        }

        // ===== Chips & filtering =====
        private void textBox1_KeyDown_AddChipOnEnter(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true; e.SuppressKeyPress = true;
            AddFilterChipFromInputs();
        }

        private void AddFilterChipFromInputs()
        {
            var term = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(term)) return;

            var display = searcher.SelectedItem?.ToString() ?? "";
            if (!_searchMap.TryGetValue(display, out var col) || string.IsNullOrWhiteSpace(col)) return;

            var btn = new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlatStyle = FlatStyle.Standard,
                Text = $"[{display}]: {term}",
                Margin = new Padding(3)
            };

            var chip = new Chip { Display = display, Column = col, Value = term, Button = btn };
            btn.Tag = chip;
            btn.Click += (s, _) => { RemoveChip((Button)s); };

            _chips.Add(chip);
            _filterBar.Controls.Add(btn);

            textBox1.Clear();
            ApplyAllFilters();
        }

        private void RemoveChip(Button chipButton)
        {
            if (chipButton?.Tag is Chip chip)
            {
                _chips.Remove(chip);
                _filterBar.Controls.Remove(chipButton);
                chipButton.Dispose();
                ApplyAllFilters();
            }
        }

        private static string QuoteForRowFilter(string s) =>
            s?.Replace("'", "''").Replace("[", "[[]").Replace("]", "[]]") ?? string.Empty;

        private void FilterTab2ByMachine(string selected)
        {
            if (dataGridView2.DataSource is not DataView dv || dv.Table == null) return;
            if (!dv.Table.Columns.Contains("machine")) { ApplyAllFilters(); return; }

            string extra;
            if (string.Equals(selected, "Chung", StringComparison.OrdinalIgnoreCase))
                extra = "([machine] IS NULL OR [machine] = '')";
            else
                extra = $"[machine] = '{QuoteForRowFilter(selected.Trim())}'";

            ApplyAllFilters(extra);
        }

        private string BuildChipsFilterExpression(DataView dv)
        {
            if (_chips.Count == 0) return string.Empty;
            var parts = new List<string>();
            foreach (var ch in _chips)
            {
                if (dv.Table?.Columns.Contains(ch.Column) != true) continue;
                parts.Add($"CONVERT([{ch.Column}], 'System.String') LIKE '%{EscapeLike(ch.Value)}%'");
            }
            return string.Join(" AND ", parts);
        }

        private void ApplyAllFilters(string extra = "")
        {
            if (dataGridView2.DataSource is not DataView dv) return;

            string baseFilter = BuildBaseFilter();
            string chips = BuildChipsFilterExpression(dv);
            var expr = CombineFilters(baseFilter, chips, extra);

            try { dv.RowFilter = expr; }
            catch (System.Data.SyntaxErrorException)
            {
                try { dv.RowFilter = CombineFilters(baseFilter, chips); }
                catch { dv.RowFilter = string.Empty; }
                System.Diagnostics.Debug.WriteLine("RowFilter syntax error. Full expr:");
                System.Diagnostics.Debug.WriteLine(expr);
            }
        }

        private void RefreshSearcherItems()
        {
            if (dataGridView2.Columns.Count == 0) return;

            searcher.BeginUpdate();
            try
            {
                searcher.Items.Clear();
                _searchMap.Clear();

                foreach (DataGridViewColumn c in dataGridView2.Columns.Cast<DataGridViewColumn>().OrderBy(col => col.DisplayIndex))
                {
                    if (!c.Visible) continue;
                    var display = string.IsNullOrWhiteSpace(c.HeaderText) ? c.Name : c.HeaderText;
                    var dataProp = string.IsNullOrWhiteSpace(c.DataPropertyName) ? c.Name : c.DataPropertyName;

                    if (!_searchMap.ContainsKey(display))
                    {
                        _searchMap[display] = dataProp;
                        searcher.Items.Add(display);
                    }
                }

                var preferred = new[] { "LSX", "Số ĐH", "Tên sản phẩm", "Mã SP" };
                foreach (var p in preferred)
                {
                    int i = searcher.Items.IndexOf(p);
                    if (i >= 0) { searcher.SelectedIndex = i; return; }
                }
                if (searcher.Items.Count > 0) searcher.SelectedIndex = 0;
            }
            finally { searcher.EndUpdate(); }
        }

        private string? PickExistingColumn(DataTable table, params string[] candidates)
        {
            foreach (var c in candidates)
                if (table.Columns.Contains(c)) return c;
            return null;
        }

        private string BuildBaseFilter()
        {
            if (dataGridView2.DataSource is not DataView dv || dv.Table is null)
                return string.Empty;

            var t = dv.Table;
            var parts = new List<string>();

            if (!ShowEmpty.Checked)
            {
                var nonEmpty = new List<string>();
                if (t.Columns.Contains("lsx")) nonEmpty.Add("(NOT (lsx IS NULL OR lsx=''))");
                if (t.Columns.Contains("product_code")) nonEmpty.Add("(NOT (product_code IS NULL OR product_code=''))");
                if (t.Columns.Contains("product_name")) nonEmpty.Add("(NOT (product_name IS NULL OR product_name=''))");
                if (t.Columns.Contains("order_name")) nonEmpty.Add("(NOT (order_name IS NULL OR order_name=''))");
                if (nonEmpty.Count > 0) parts.Add(string.Join(" AND ", nonEmpty));

                if (t.Columns.Contains("can_sx")) parts.Add("can_sx > 0");
            }

            var dateCol = PickExistingColumn(t, "date_planned_start", "desire", "date_planned_finished");
            if (!string.IsNullOrEmpty(dateCol))
                parts.Add($"{dateCol} >= {HashDate(DateTime.Today.AddYears(-1))}");

            return string.Join(" AND ", parts);
        }

        // ===== Date coloring: triggers =====
        private void DataGridView2_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = dataGridView2.Columns[e.ColumnIndex].Name;
            if (col == "desire" || col == "checker" || col == "extra")
                ApplyDateStatusColorsForRow(dataGridView2.Rows[e.RowIndex]);
        }

        private void DataGridView2_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dataGridView2.IsCurrentCellDirty)
                dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DataGridView2_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            ApplyDateStatusColors();
        }

        // ===== Save / copy / misc =====
        private void btnSaveYeuCau_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataGridViewRow row in dataGridView2.Rows)
                        {
                            if (row.IsNewRow) continue;

                            string lsx = row.Cells["lsx"]?.Value?.ToString();

                            string date1 = row.Cells["desire"].Value?.ToString();
                            string date2 = row.Cells["checker"].Value?.ToString();
                            string date3 = row.Cells["extra"].Value?.ToString();
                            string ghichu = row.Cells["ghichu"]?.Value?.ToString();

                            SQL.SaveYeuCauDates(lsx, date1, date2, date3, ghichu);
                        }
                        transaction.Commit();
                        // Immediately hide rows that already have Ngày Giao Hàng
                        ExcludeDeliveredRows();
                        MessageBox.Show("Đã lưu các ngày thành công.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Hide any row that has a real date in column 'extra'
        private void ExcludeDeliveredRows()
        {
            if (dataGridView2?.DataSource is DataView dv && dv.Table != null)
            {
                var t = dv.Table;
                var toRemove = t.AsEnumerable()
                                .Where(r => HasRealDate(r["extra"]))
                                .ToList();
                foreach (var r in toRemove) t.Rows.Remove(r);
            }
            else
            {
                // fallback: remove from grid
                foreach (DataGridViewRow r in dataGridView2.Rows.Cast<DataGridViewRow>().ToList())
                {
                    if (r.IsNewRow) continue;
                    if (HasRealDate(r.Cells["extra"]?.Value))
                        dataGridView2.Rows.Remove(r);
                }
            }
        }

        // shared parser (treat 31/12/1899 etc. as 'no date')
        private static bool HasRealDate(object v)
        {
            if (v == null || v == DBNull.Value) return false;
            if (v is DateTime dd) return dd.Year > 1901;
            var s = v.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return false;
            string[] fmts = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(s, fmts, CultureInfo.GetCultureInfo("vi-VN"),
                                       DateTimeStyles.None, out var d) ||
                DateTime.TryParse(s, out d))
                return d.Year > 1901;
            return false;
        }


        private void chkCopyToAll_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null || dataGridView2.CurrentCell == null)
            {
                MessageBox.Show("Vui lòng chọn một ô cần sao chép.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string columnName = dataGridView2.Columns[dataGridView2.CurrentCell.ColumnIndex].Name;
            if (columnName != "desire" && columnName != "checker" && columnName != "extra")
            {
                MessageBox.Show("Chỉ có thể sao chép các cột Ngày BH Yêu Cầu, Ngày SX Phản Hồi, hoặc Ngày Giao Hàng.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var value = dataGridView2.CurrentCell.Value;
            string orderName = dataGridView2.CurrentRow.Cells["order_name"]?.Value?.ToString();
            if (string.IsNullOrEmpty(orderName))
            {
                MessageBox.Show("Không tìm thấy Số ĐH để sao chép.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.IsNewRow || row == dataGridView2.CurrentRow) continue;
                if (row.Cells["order_name"]?.Value?.ToString() == orderName)
                    row.Cells[columnName].Value = value;
            }
            MessageBox.Show($"Đã sao chép giá trị '{value}' từ cột '{dataGridView2.Columns[columnName].HeaderText}' cho các dòng cùng Số ĐH.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DateSorter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modetab == 2)
            {
                var sel = DateSorter.SelectedItem?.ToString() ?? "Chung";
                FilterTab2ByMachine(sel);
                return;
            }

            if (dataGridView2.DataSource is not DataView dv || dv.Table == null) { ApplyAllFilters(); return; }
            string filterCol = DateSorter.Tag as string;
            if (string.IsNullOrEmpty(filterCol) || !dv.Table.Columns.Contains(filterCol)) { ApplyAllFilters(); return; }
            if (DateSorter.SelectedIndex < 0 || DateSorter.SelectedItem == null) { ApplyAllFilters(); return; }

            var token = DateSorter.SelectedItem.ToString();
            var datePart = token.Split(' ')[0];
            if (!DateTime.TryParseExact(datePart, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"),
                                        DateTimeStyles.None, out var d)) { ApplyAllFilters(); return; }

            var from = d.Date; var to = from.AddDays(1);
            string dateFilter = string.Format(CultureInfo.InvariantCulture,
                "{0} >= #{1:MM/dd/yyyy}# AND {0} < #{2:MM/dd/yyyy}#", filterCol, from, to);
            ApplyAllFilters(dateFilter);
        }

        private void From_ValueChanged(object sender, EventArgs e)
        {
            if (dataGridView2.DataSource is not DataView dv || dv.Table is null) return;

            var dateCol = PickExistingColumn(dv.Table, "desire", "date_planned_start", "date_planned_finished");
            if (string.IsNullOrEmpty(dateCol)) { ApplyAllFilters(); return; }

            DateTime from = From.Value.Date, to = To.Value.Date.AddDays(1);
            string range = $"{dateCol} >= {HashDate(from)} AND {dateCol} < {HashDate(to)}";
            ApplyAllFilters(range);
        }

        private void To_ValueChanged(object sender, EventArgs e) => From_ValueChanged(sender, e);

        private void ShowEmpty_CheckedChanged(object sender, EventArgs e)
        {
            allWorkorders = WorkorderService.LoadPendingWorkorders(dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);
            RefreshSearcherItems();
            if (modetab == 2) FilterTab2ByMachine(DateSorter.SelectedItem?.ToString() ?? "Chung");
            else ApplyAllFilters();
        }

        private void ShowOld_CheckedChanged(object sender, EventArgs e)
        {
            allWorkorders = WorkorderService.LoadPendingWorkorders(dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);
            RefreshSearcherItems();
            if (modetab == 2) FilterTab2ByMachine(DateSorter.SelectedItem?.ToString() ?? "Chung");
            else ApplyAllFilters();
        }

        private void searcher_SelectedIndexChanged(object sender, EventArgs e) => textBox1_TextChanged(sender, e);
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void tab1_Click(object sender, EventArgs e)
        {
            dataGridView2.SuspendLayout();
            try
            {
                if (dataGridView2.DataSource is DataView dv && dv.Table != null) dv.Table.Clear();
                dataGridView2.Columns.Clear();
            }
            finally { dataGridView2.ResumeLayout(); }

            MarkActiveTab(tab1);
            modetab = 1;
            textBox1.Clear();
            label1.Text = "Lịch nhận tuần";
            button1.Visible = true;
            Default_Ka.Visible = false;
            ShowEmpty.Text = "Hiển thị LSX trống";
            chkCopyToAll.Visible = true;
            chkCopyToAll.Text = "Copy vào Số ĐH tương tự";

            allWorkorders = WorkorderService.LoadPendingWorkorders(
                dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);
            RefreshSearcherItems();
            LayoutGridsUnderHeader();
        }

        // Highlight the active tab button and reset the rest
        private void MarkActiveTab(Button active)
        {
            if (_tabButtons != null)
            {
                foreach (var b in _tabButtons)
                {
                    if (b == null || b.IsDisposed) continue;
                    b.BackColor = _tabGreen;
                    b.ForeColor = Color.White;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    b.UseVisualStyleBackColor = false;
                }
            }

            if (active != null && !active.IsDisposed)
            {
                active.FlatStyle = FlatStyle.Flat;
                active.FlatAppearance.BorderSize = 0;
                active.UseVisualStyleBackColor = false;
                active.BackColor = _tabGreenActive;
                active.ForeColor = Color.White;

                if (_tabButtons != null && !_tabButtons.Contains(active))
                    _tabButtons.Add(active);
            }
        }

        private void tab2_Click(object sender, EventArgs e) { }
        private void tab3_Click(object sender, EventArgs e) { }
        private void tab4_Click(object sender, EventArgs e) { }
        private void tab5_Click(object sender, EventArgs e) { MarkActiveTab(tab5); }
    }
}
