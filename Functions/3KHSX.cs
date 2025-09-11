using APPPC.Control;
using APPPC.KHSX_Helpers;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text;

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

        public int modetab { get; set; }
        private readonly string[] _kaOptions = { "Bình thường", "Ca 1", "Ca 2", "Nghỉ", "Ca 1 dài", "2 Ca", "Ca 2 dài", "Ca dài", "3 Ka", "Ca dài 10h", "Ka 4H", "Ca dài 11h" };
        private bool _tab4ApplyingKa = false;
        private static string EscapeLike(string s) => s?.Replace("'", "''") ?? "";
        private static string CombineFilters(params string[] parts) => string.Join(" AND ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        private string? currentOrderKeyForTab2 = null;
        private static string HashDate(DateTime d) => $"#{d.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}#";
        private readonly Dictionary<string, (TimeSpan start, TimeSpan? end)> _shifts = new()
        {
            ["Bình thường"] = (TimeSpan.Parse("07:30"), TimeSpan.Parse("15:30")),
            ["Ca 1"] = (TimeSpan.Parse("06:00"), TimeSpan.Parse("13:30")),
            ["Ca 2"] = (TimeSpan.Parse("14:00"), TimeSpan.Parse("21:30")),
            ["Nghỉ"] = (TimeSpan.Parse("00:00"), TimeSpan.Parse("00:00")),
            ["Ca 1 dài"] = (TimeSpan.Parse("06:00"), TimeSpan.Parse("17:00")),
            ["2 Ca"] = (TimeSpan.Parse("06:00"), TimeSpan.Parse("21:00")),
            ["Ca 2 dài"] = (TimeSpan.Parse("18:00"), TimeSpan.Parse("05:00")),
            ["Ca dài"] = (TimeSpan.Parse("00:01"), TimeSpan.Parse("23:59")),
            ["3 Ka"] = (TimeSpan.Parse("00:01"), TimeSpan.Parse("23:59")),
            ["Ca dài 10h"] = (TimeSpan.Parse("06:00"), TimeSpan.Parse("16:00")),
            ["Ka 4H"] = (TimeSpan.Parse("08:00"), TimeSpan.Parse("12:00")),
            ["Ca dài 11h"] = (TimeSpan.Parse("06:00"), TimeSpan.Parse("17:00")),
        };
        private readonly Dictionary<string, List<string>> categoryKeywords = new Dictionary<string, List<string>>
        {
            { "In", new List<string> { "In", "Flexo", "Offset", "Máy In" } },
            { "Bế", new List<string> { "Bế", "Cắt", "Diecut" } },
            { "Dán", new List<string> { "Dán", "Dập ghim", "Dán keo" } },
            { "Tráng", new List<string> { "Tráng", "Phủ" } },
            { "Gỡ", new List<string> { "Gỡ", "Tháo", "Tách" } }
        };

        private static string NormalizeLsxKey(object value)
        {
            return (value?.ToString() ?? "")
                .Trim()
                .Replace(" ", "")
                .Replace("\u200B", "");
        }

        private const string DefaultShiftName = "Bình thường";

        private readonly Dictionary<string, string> _searchMap = new(StringComparer.OrdinalIgnoreCase);

        private (DateTime? start, DateTime? end) ComputeKaWindow(DateTime day, string? ka)
        {
            if (string.IsNullOrWhiteSpace(ka)) return (null, null);
            var key = NormalizeKaKey(ka);
            if (key == "Nghỉ") return (null, null);      // day off = no times

            var shift = _shifts[key];
            var start = day.Date + shift.start;
            DateTime end = shift.end.HasValue ? day.Date + shift.end.Value : start.AddHours(24);
            if (shift.end.HasValue && shift.end.Value <= shift.start) end = end.AddDays(1);
            return (start, end);
        }

        private void RecalcRowTimes(DataRow r)
        {
            if (r == null) return;
            var d = r.Field<DateTime>("DateOnly");
            var ka = r.Field<string?>("ka");
            var (st, en) = ComputeKaWindow(d, ka);
            r["start_time"] = (object?)st ?? DBNull.Value;
            r["end_time"] = (object?)en ?? DBNull.Value;
        }

        private void UpdateStartFinishForRow(DataRow r)
        {
            bool planned = r.Table.Columns.Contains("Date") && r.Field<bool?>("Date") == true;
            if (!planned) { r["start_time"] = DBNull.Value; r["finish_time"] = DBNull.Value; return; }

            // base date is RawDate (already >= _tab3StartDate)
            var baseDate = r.Table.Columns.Contains("RawDate") && r["RawDate"] != DBNull.Value
                ? ((DateTime)r["RawDate"]).Date
                : _tab3StartDate;

            var ka = (r.Table.Columns.Contains("ka") ? r["ka"] : r.Table.Columns.Contains("ka_combo") ? r["ka_combo"] : null)?.ToString() ?? "";
            if (!_shifts.TryGetValue(ka, out var sh)) sh = (TimeSpan.Zero, null);

            var start = baseDate + sh.start;
            r["start_time"] = start;

            double hours = 0;
            if (r.Table.Columns.Contains("time_needed") && r["time_needed"] != DBNull.Value)
                double.TryParse(r["time_needed"].ToString(), out hours);

            r["finish_time"] = start.AddHours(hours);
        }

        public KHSX_panel()
        {
            InitializeComponent();
            KHSX_panel_SizeChanged(null, null);
            this.Dock = DockStyle.Fill;

            From.Format = DateTimePickerFormat.Custom;
            From.CustomFormat = "dd/MM/yyyy";
            To.Format = DateTimePickerFormat.Custom;
            To.CustomFormat = "dd/MM/yyyy";

            // Default_Ka combobox: fixed list, user must choose from it
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
            // after dataGridView2.DataSource = _tab4Table; and column setup
            dataGridView2.AllowUserToAddRows = false;   // ⛔ no blank last row
            dataGridView2.AllowUserToDeleteRows = false;

            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dataGridView2, new object[] { true });
            RefreshSearcherItems();
        }

        private void RefreshSearcherItems()
        {
            if (dataGridView2.Columns.Count == 0) return;

            searcher.BeginUpdate();
            try
            {
                searcher.Items.Clear();
                _searchMap.Clear();

                // Use DisplayIndex so the list order matches the grid visually
                foreach (DataGridViewColumn c in dataGridView2.Columns
                             .Cast<DataGridViewColumn>()
                             .OrderBy(col => col.DisplayIndex))
                {
                    if (!c.Visible) continue;                 // only show visible columns
                    var display = string.IsNullOrWhiteSpace(c.HeaderText) ? c.Name : c.HeaderText;
                    var dataProp = string.IsNullOrWhiteSpace(c.DataPropertyName) ? c.Name : c.DataPropertyName;

                    if (!_searchMap.ContainsKey(display))
                    {
                        _searchMap[display] = dataProp;
                        searcher.Items.Add(display);
                    }
                }

                // Pick a sensible default if possible
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

        private string? MapToOrderMapKey(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return null;

            // Normalize
            label = label.Trim();

            // OrderMap keys you defined: "Bế","Dán","Tráng Màng","In"
            // Our DateSorter categories use e.g. "Tráng" (not "Tráng Màng")
            if (label.Equals("Tráng", StringComparison.OrdinalIgnoreCase) ||
                label.Equals("Tráng Màng", StringComparison.OrdinalIgnoreCase) ||
                label.IndexOf("Tráng", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Phủ", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Tráng Màng";

            if (label.IndexOf("Bế", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Cắt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Diecut", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Bế";

            if (label.IndexOf("Dán", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Dập ghim", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("keo", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Dán";

            if (label.IndexOf("In", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Flexo", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0 ||
                label.IndexOf("Máy In", StringComparison.OrdinalIgnoreCase) >= 0)
                return "In";

            return null;
        }

        private string? InferOrderKeyFromWorkorderName(string? workorderName)
        {
            if (string.IsNullOrWhiteSpace(workorderName)) return null;

            foreach (var kv in categoryKeywords)
            {
                if (kv.Value.Any(k => workorderName.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return MapToOrderMapKey(kv.Key);
                }
            }
            return null;
        }

        private void ArrangeButtonsRight(params System.Windows.Forms.Control[] buttons)

        {
            int rightMargin = 10;
            int spacing = 5;
            int x = this.Width - rightMargin;
            int y = buttons[0].Top;

            for (int i = buttons.Length - 1; i >= 0; i--)
            {
                var btn = buttons[i];
                x -= btn.Width;
                btn.Location = new Point(x, y);
                x -= spacing;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow != null && modetab == 3)
            {
                var lsx = dataGridView2.CurrentRow.Cells["lsx"].Value?.ToString();
                if (!string.IsNullOrEmpty(lsx))
                    WorkorderService.ProductionPlan(lsx, dataGridView2);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (dataGridView2.DataSource is not DataView dv) return;

            string baseFilter = BuildBaseFilter();
            string f = textBox1.Text.Trim();

            string textFilter = string.Empty;

            if (!string.IsNullOrEmpty(f))
            {
                if (searcher.SelectedItem != null &&
                    _searchMap.TryGetValue(searcher.SelectedItem.ToString(), out var col))
                {
                    // Convert to string so this works for text, numbers, and dates
                    textFilter = $"CONVERT([{col}], 'System.String') LIKE '%{EscapeLike(f)}%'";
                }
                else
                {
                    // Fallback (old behavior)
                    textFilter = $"(order_name LIKE '%{EscapeLike(f)}%' OR lsx LIKE '%{EscapeLike(f)}%')";
                }
            }

            dv.RowFilter = CombineFilters(baseFilter, textFilter);
        }

        private void btnSaveYeuCau_Click(object sender, EventArgs e)
        {
            if (modetab == 1)
            {
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.IsNewRow) continue;
                    string lsx = row.Cells["lsx"]?.Value?.ToString();

                    string date1 = row.Cells["desire"].Value.ToString();
                    string date2 = row.Cells["checker"].Value.ToString();
                    string extra = row.Cells["extra"].Value.ToString();
                    string ghichu = row.Cells["ghichu"]?.Value?.ToString();

                    try
                    {
                        SQL.SaveYeuCauDates(lsx, date1, date2, extra, ghichu);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi lưu LSX {lsx}: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                MessageBox.Show("Đã lưu các ngày thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else if (modetab == 2)
            {
                int ok = 0, skip = 0;
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.IsNewRow) continue;

                    string lsx = row.Cells["lsx"]?.Value?.ToString()?.Trim();
                    string machine = row.Cells["machine"]?.Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(lsx) || string.IsNullOrEmpty(machine)) { skip++; continue; }

                    try
                    {
                        SQL.SaveWorkorderMachine(lsx, machine);
                        ok++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi lưu máy cho LSX {lsx}: {ex.Message}",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                MessageBox.Show($"Đã lưu máy cho {ok} dòng. Bỏ qua {skip} dòng thiếu dữ liệu.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else if (modetab == 3)
            {
                if (string.IsNullOrEmpty(_tab3MachineId)) { MessageBox.Show("Vui lòng chọn máy trong DateSorter.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (dataGridView2.DataSource is not DataTable dt3) { MessageBox.Show("Không có dữ liệu để lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                try
                {
                    SQL.SavePlanUsingCheckbox(_tab3MachineId, From.Value.Date, dt3);
                    MessageBox.Show("Đã lưu kế hoạch thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTab3PlanGrid();
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi lưu kế hoạch: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }

            else if (modetab == 4)
            {
                if (_tab4Table == null || _tab4Table.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu Ca/Ngày để lưu.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try
                {
                    int affected = SQL.SaveShiftDays(_tab4Table); // upsert by Date
                    MessageBox.Show($"Đã lưu {affected} dòng Ca/Ngày vào bảng Shift.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTab4PlanGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi lưu Ca/Ngày: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


        }

        private void chkCopyToAll_Click(object sender, EventArgs e)
        {
            if (modetab == 4)
            {
                if (string.IsNullOrWhiteSpace(_tab4DefaultKa))
                {
                    MessageBox.Show("Vui lòng chọn Ca mặc định ở ô 'Default Ka' trước.",
                        "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (_tab4Table == null || _tab4Table.Rows.Count == 0) return;

                var from = From.Value.Date;
                var to = To.Value.Date;

                foreach (DataRow r in _tab4Table.Rows)
                {
                    var d = r.Field<DateTime>("DateOnly").Date;
                    if (d < from || d > to) continue;

                    var kaToApply = (d.DayOfWeek == DayOfWeek.Sunday) ? "Nghỉ" : _tab4DefaultKa;
                    r["ka"] = kaToApply;
                    r["Machine"] = _tab4MachineId ?? "";
                    RecalcRowTimes(r);
                }


                // repaint styles (Nghỉ/Sunday grey)
                foreach (DataGridViewRow gr in dataGridView2.Rows)
                    ApplyTab4RowStyle(gr);

                dataGridView2.Refresh();
                MessageBox.Show("Đã áp dụng Ca mặc định cho toàn bộ ngày trong khoảng đã chọn.",
                    "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ====== original (Tab1/2) copy behavior ======
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

        private void Sequence_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void MachineCell_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow digits and backspace only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void DateSorter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DateSorter.SelectedItem == null) return;

            if (modetab == 3)
            {
                _tab3MachineId = DateSorter.SelectedItem.ToString();
                LoadTab3PlanGrid();
                return;
            }

            if (dataGridView2.DataSource is not DataView dv) return;

            string baseFilter = BuildBaseFilter();

            if (modetab == 1)
            {
                string selected = DateSorter.SelectedItem.ToString();
                string datePart = selected.Split('(')[0].Trim();
                if (DateTime.TryParseExact(datePart, "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out DateTime d))
                {
                    string dayFilter = $"desire >= {HashDate(d)} AND desire < {HashDate(d.AddDays(1))}";

                    dv.RowFilter = CombineFilters(baseFilter, dayFilter);
                }
                return;
            }

            string raw = DateSorter.SelectedItem.ToString();
            string selectedCategory = raw.Split('(')[0].Trim();

            currentOrderKeyForTab2 = MapToOrderMapKey(selectedCategory);

            string catFilter = "";
            if (categoryKeywords.TryGetValue(selectedCategory, out var list))
            {
                var ors = list.Select(k => $"workorder_name LIKE '%{EscapeLike(k)}%'");
                catFilter = "(" + string.Join(" OR ", ors) + ")";
            }
            else
            {
                catFilter = $"workorder_name LIKE '%{EscapeLike(selectedCategory)}%'";
                currentOrderKeyForTab2 = MapToOrderMapKey(selectedCategory)
                                         ?? InferOrderKeyFromWorkorderName(selectedCategory);
            }

            dv.RowFilter = CombineFilters(baseFilter, catFilter);
        }

        private void From_ValueChanged(object sender, EventArgs e)
        {
            if (modetab == 4) { LoadTab4PlanGrid(); return; }
            if (modetab == 3) { _tab3StartDate = From.Value.Date; LoadTab3PlanGrid(); return; }
            if (dataGridView2.DataSource is not DataView dv) return;

            string baseFilter = BuildBaseFilter();
            DateTime from = From.Value.Date;
            DateTime to = To.Value.Date;

            string range = $"desire >= {HashDate(from)} AND desire < {HashDate(to.AddDays(1))}";

            dv.RowFilter = CombineFilters(baseFilter, range);
        }

        private void To_ValueChanged(object sender, EventArgs e)
        {
            if (modetab == 4) { LoadTab4PlanGrid(); return; }
            if (dataGridView2.DataSource is not DataView dv) return;

            string baseFilter = BuildBaseFilter();
            DateTime from = From.Value.Date;
            DateTime to = To.Value.Date;

            string range = $"desire >= {HashDate(from)} AND desire < {HashDate(to.AddDays(1))}";

            dv.RowFilter = CombineFilters(baseFilter, range);
        }

        private void ShowEmpty_CheckedChanged(object sender, EventArgs e)
        {
            allWorkorders = WorkorderService.LoadPendingWorkorders(dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);
            RefreshSearcherItems();
        }

        private void ShowOld_CheckedChanged(object sender, EventArgs e)
        {
            allWorkorders = WorkorderService.LoadPendingWorkorders(dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);
            RefreshSearcherItems();
        }

        private void KHSX_panel_SizeChanged(object sender, EventArgs e)
        {
            int rightMargin = 5;
            int bottomMargin = 10;
            dataGridView2.Width = this.Width - dataGridView2.Location.X - rightMargin;
            dataGridView2.Height = this.Height - dataGridView2.Location.Y - bottomMargin;
            ArrangeButtonsRight(From, To, DateSorter, chkCopyToAll, button1, btnSaveYeuCau);
        }

        private void ApplyPlannedRowStyle(DataGridViewRow row)
        {
            try
            {
                if (row?.Cells["Date"] == null) return;
                bool planned = row.Cells["Date"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Date"].Value);
                row.DefaultCellStyle.BackColor = planned ? Color.LightBlue : SystemColors.Window;
            }
            catch (Exception)
            {
            }
        }

        private void RecomputeSequenceAndSort()
        {
            if (dataGridView2.DataSource is not DataTable dt) return;

            var planned = dt.AsEnumerable()
                .Where(r => r.Field<bool?>("Date") == true)
                .ToList();

            // guarantee all have a start_time to sort by
            foreach (var r in planned)
            {
                if (r["start_time"] == DBNull.Value)
                {
                    var day = (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date;
                    var ka = (r["ka"]?.ToString() ?? DefaultShiftName);
                    var (s, _) = ShiftWindow(day, ka);
                    r["start_time"] = s;
                }
            }

            // assign sequence per day using start_time asc
            foreach (var g in planned.GroupBy(r => ((DateTime)r["start_time"]).Date))
            {
                short seq = 1;
                foreach (var r in g.OrderBy(r => r.Field<DateTime>("start_time"))
                                   .ThenBy(r => r.Field<string?>("LSX") ?? string.Empty))
                {
                    r["sequence"] = seq++;
                }
            }

            // clear sequence for non-planned rows
            foreach (var r in dt.AsEnumerable().Where(r => !(r.Field<bool?>("Date") ?? false)))
                r["sequence"] = DBNull.Value;

            // keep the grid aligned with what we calculated
            if (dataGridView2.Columns.Contains("start_time"))
                dataGridView2.Sort(dataGridView2.Columns["start_time"], ListSortDirection.Ascending);
        }

        private static double GetHours(DataRow r)
        {
            if (r.Table.Columns.Contains("time_needed") && r["time_needed"] != DBNull.Value &&
                double.TryParse(r["time_needed"]?.ToString(), out var h)) return h;
            return 0d;
        }

        private string BuildBaseFilter()
        {
            var list = new List<string>();
            if (!ShowEmpty.Checked)
            {
                list.Add("(NOT (lsx IS NULL OR lsx='') AND " +
                         " NOT (product_code IS NULL OR product_code='') AND " +
                         " NOT (product_name IS NULL OR product_name='') AND " +
                         " NOT (order_name IS NULL OR order_name=''))");
                list.Add("can_sx > 0");   // keep your empty filter
            }

            // ✅ correct DateTime literal
            list.Add($"date_planned_start >= {HashDate(DateTime.Today.AddYears(-1))}");

            return string.Join(" AND ", list);
        }

        private DataTable BuildTab4Table(DateTime from, DateTime to)
        {
            var dt = new DataTable();
            dt.Columns.Add("DateOnly", typeof(DateTime));
            dt.Columns.Add("ka", typeof(string));
            dt.Columns.Add("start_time", typeof(DateTime));
            dt.Columns.Add("end_time", typeof(DateTime));
            dt.Columns.Add("note", typeof(string));
            dt.Columns.Add("Machine", typeof(string));

            // NEW: pull existing rows for this machine & range
            var existing = SQL.LoadShiftDays(_tab4MachineId ?? "", from, to);

            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                var r = dt.NewRow();
                r["DateOnly"] = d;
                r["Machine"] = _tab4MachineId ?? "";

                if (existing.TryGetValue(d, out var val))
                {
                    // Normalize to your defined keys (“Ca 2 dài”, etc.)
                    var normalized = NormalizeKaKey(val.Shift);
                    if (!string.IsNullOrWhiteSpace(normalized)) r["ka"] = normalized; else r["ka"] = DBNull.Value;

                    if (!string.IsNullOrWhiteSpace(val.Note)) r["note"] = val.Note; else r["note"] = DBNull.Value;
                }
                else
                {
                    r["ka"] = DBNull.Value;
                    r["note"] = DBNull.Value;
                }

                dt.Rows.Add(r);
                RecalcRowTimes(r); // compute start/end from ka (or leave blank if ka is null)
            }
            return dt;
        }

        private void CompactPlanOrder(DataTable dt)
        {
            var planned = dt.AsEnumerable()
                .Where(r => r.Field<bool?>("Date") == true)
                .OrderBy(r => r.Field<int?>("plan_order") ?? int.MaxValue)
                .ThenBy(r => r.Field<string?>("LSX") ?? string.Empty)
                .ToList();

            int i = 1;
            foreach (var r in planned) r["plan_order"] = i++;
        }

        private int NextPlanOrder(DataTable dt)
        {
            return dt.AsEnumerable().Count(r => r.Field<bool?>("Date") == true) + 1;
        }

        private DateTime AlignToShiftStart(DateTime t, string ka, int addOffOnce = 0, bool applySkip = false)
        {
            var (s, e) = ShiftWindow(t.Date, ka);
            if (t < s) return s;
            if (t >= e)
            {
                var next = t.Date.AddDays(1 + (applySkip ? addOffOnce : 0));
                return ShiftWindow(next, ka).start;
            }
            return t;
        }

        private DateTime FinishAcrossShifts(DateTime start, double hours, string ka, int skipDays)
        {
            bool firstJump = true;
            DateTime cursor = start;
            while (hours > 1e-9)
            {
                var (dayStart, dayEnd) = ShiftWindow(cursor.Date, ka);
                if (cursor < dayStart) cursor = dayStart;
                if (cursor >= dayEnd)
                {
                    var add = 1 + (firstJump ? Math.Max(0, skipDays) : 0);
                    cursor = ShiftWindow(cursor.Date.AddDays(add), ka).start;
                    firstJump = false;
                    continue;
                }

                var room = (dayEnd - cursor).TotalHours;
                if (hours <= room)
                {
                    return cursor.AddHours(hours);
                }

                hours -= room;
                var add2 = 1 + (firstJump ? Math.Max(0, skipDays) : 0);
                cursor = ShiftWindow(cursor.Date.AddDays(add2), ka).start;
                firstJump = false;
            }

            return cursor;
        }

        private (DateTime start, DateTime end) ShiftWindow(DateTime day, string? ka)
        {
            var key = string.IsNullOrWhiteSpace(ka) ? DefaultShiftName : ka!;
            if (!_shifts.TryGetValue(key, out var s)) s = _shifts[DefaultShiftName];

            var start = day.Date + s.start;
            // if no end configured, treat as 24h
            var end = s.end.HasValue ? (day.Date + s.end.Value) : start.AddHours(24);
            // night shift crosses midnight
            if (s.end.HasValue && s.end.Value <= s.start) end = end.AddDays(1);
            return (start, end);
        }

        private void ApplyShiftToDay(DataTable dt, DateTime day, string ka)
        {
            foreach (var r in dt.AsEnumerable()
                                .Where(r => r.Field<DateTime?>("RawDate")?.Date == day.Date))
                r["ka"] = string.IsNullOrWhiteSpace(ka) ? DefaultShiftName : ka;
        }

        private string? GetSavedDayShift(IEnumerable<DataRow> rows, DateTime day)
        {
            return rows
                .Where(x => ((x.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date == day.Date))
                .OrderBy(x => x.Field<short?>("sequence") ?? short.MaxValue)   // seq=1 first
                .Select(x => (x["ka"]?.ToString() ?? "").Trim())
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }

        private void RecalculateSchedule(DataTable dt)
        {
            var rows = dt.AsEnumerable()
                .Where(r => r.Field<bool?>("Date") == true)
                .OrderBy(r => r.Field<int?>("plan_order") ?? int.MaxValue)
                .ThenBy(r => r.Field<string?>("LSX") ?? string.Empty)
                .ToList();

            // ✅ Nothing planned yet → clear any stale values and exit (avoids Min/Max on empty).
            if (rows.Count == 0)
            {
                // Guarded clears in case columns exist
                bool hasSeq = dt.Columns.Contains("sequence");
                bool hasST = dt.Columns.Contains("start_time");
                bool hasFT = dt.Columns.Contains("finish_time");
                foreach (DataRow r in dt.Rows)
                {
                    if (hasSeq) r["sequence"] = DBNull.Value;
                    if (hasST) r["start_time"] = DBNull.Value;
                    if (hasFT) r["finish_time"] = DBNull.Value;
                }
                return;
            }

            // ✅ Only compute the range after we know there is at least one planned row
            var rangeStart = rows.Min(r => (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date);
            var rangeEnd = rows.Max(r => (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date);

            // Your existing preload of DB shifts (from Tab4)
            var dbShiftMap = SQL.LoadShiftDays(_tab3MachineId ?? "", rangeStart, rangeEnd);

            // helper: lấy ca cho 1 ngày, nếu không có trong DB thì dùng Bình thường
            string ShiftFor(DateTime d)
            {
                if (dbShiftMap.TryGetValue(d.Date, out var s) && !string.IsNullOrWhiteSpace(s.Shift))
                    return NormalizeKaKey(s.Shift);
                return DefaultShiftName; // "Bình thường"
            }

            // helper: đưa thời điểm về slot làm việc đầu tiên ≥ t (bỏ qua ngày Nghỉ)
            DateTime AlignToFirstWorkingSlot(DateTime t)
            {
                DateTime d = t.Date;
                while (ShiftFor(d) == "Nghỉ") d = d.AddDays(1);            // skip off-day(s)
                var (st, _) = ShiftWindow(d, ShiftFor(d));
                return (t <= st) ? st : t;
            }

            // helper: chạy qua lịch theo từng ngày (skip ngày Nghỉ) cho đến khi hết giờ
            DateTime FinishAcrossCalendar(DateTime start, double hours)
            {
                DateTime cur = AlignToFirstWorkingSlot(start);
                while (hours > 1e-9)
                {
                    var day = cur.Date;
                    var ka = ShiftFor(day);
                    if (ka == "Nghỉ") { cur = AlignToFirstWorkingSlot(day.AddDays(1)); continue; }

                    var (st, en) = ShiftWindow(day, ka);
                    if (cur < st) cur = st;
                    if (cur >= en) { cur = AlignToFirstWorkingSlot(day.AddDays(1)); continue; }

                    var room = (en - cur).TotalHours;
                    if (hours <= room) return cur.AddHours(hours);

                    hours -= room;
                    cur = AlignToFirstWorkingSlot(day.AddDays(1));
                }
                return cur;
            }

            DateTime? lastFinish = null;
            DateTime? prevStartDay = null;
            int seq = 1;

            foreach (var r in rows)
            {
                var hours = GetHours(r);
                var baseDay = (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date;

                // tìm ngày làm việc đầu tiên ≥ baseDay
                var seed = AlignToFirstWorkingSlot(baseDay);
                var start = (lastFinish == null) ? seed : AlignToFirstWorkingSlot(lastFinish.Value);

                if (prevStartDay == null || start.Date != prevStartDay.Value.Date)
                {
                    seq = 1;
                    prevStartDay = start.Date;
                    ApplyShiftToDay(dt, start.Date, ShiftFor(start.Date)); // ghi Ka của ngày thực tế
                }

                var finish = FinishAcrossCalendar(start, hours);

                r["RawDate"] = start.Date;
                r["ka"] = ShiftFor(start.Date);
                r["sequence"] = (short)seq;
                r["start_time"] = start;
                r["finish_time"] = finish;

                lastFinish = finish;
                seq++;
            }

            if (dataGridView2.Columns.Contains("start_time"))
                dataGridView2.Sort(dataGridView2.Columns["start_time"], ListSortDirection.Ascending);

        }


        private static string StripDiacritics(string s)
        {
            var formD = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.ToLowerInvariant(ch));
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private string NormalizeKaKey(string? ka)
        {
            if (string.IsNullOrWhiteSpace(ka)) return DefaultShiftName;

            static string Canon(string s)
            {
                // remove diacritics, spaces and punctuation; lowercase
                var formD = s.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(formD.Length);
                foreach (var ch in formD)
                {
                    var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (cat == UnicodeCategory.NonSpacingMark) continue;
                    if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_' || ch == '/' || ch == '.') continue;
                    sb.Append(char.ToLowerInvariant(ch));
                }
                return sb.ToString().Normalize(NormalizationForm.FormC);
            }

            var t = Canon(ka);

            // 1) exact match against our defined keys (after canonicalization)
            foreach (var k in _shifts.Keys)
                if (Canon(k) == t) return k;

            // 2) tolerant aliases commonly stored in SQL
            var alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["binhthuong"] = "Bình thường",
                ["bt"] = "Bình thường",

                ["ca1"] = "Ca 1",
                ["1ca"] = "Ca 1",

                ["ca2"] = "Ca 2",
                ["2shift"] = "2 Ca",
                ["2ca"] = "2 Ca",

                ["cadai"] = "Ca dài",
                ["ca1dai"] = "Ca 1 dài",
                ["ca2dai"] = "Ca 2 dài",

                ["3ka"] = "3 Ka",
                ["3k"] = "3 Ka",
                ["ca3"] = "3 Ka",

                ["ka4h"] = "Ka 4H",
                ["4h"] = "Ka 4H",

                ["ca10h"] = "Ca dài 10h",
                ["ca11h"] = "Ca dài 11h",

                ["nghi"] = "Nghỉ",
            };

            if (alias.TryGetValue(t, out var std)) return std;

            return DefaultShiftName;
        }


        private void LoadTab4PlanGrid()
        {
            modetab = 4;

            // show both pickers and machine picker
            From.Visible = true;
            To.Visible = true;
            DateSorter.Visible = true;                         // <— show machine list
            DateSorter.Items.Clear();
            foreach (var m in SQL.LoadMachineListForPlan()) DateSorter.Items.Add(m);
            if (DateSorter.Items.Count > 0)
            {
                if (DateSorter.SelectedIndex < 0) DateSorter.SelectedIndex = 0;
                _tab4MachineId = DateSorter.SelectedItem?.ToString();
            }

            _tab4Table = BuildTab4Table(From.Value.Date, To.Value.Date);

            // grid columns (same as before) ...
            dataGridView2.DataSource = _tab4Table;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.Columns.Clear();

            var colDate = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "DateOnly",
                Name = "DateOnly",
                HeaderText = "Ngày",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM/yyyy" }
            };
            dataGridView2.Columns.Add(colDate);

            var colKa = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "ka",
                Name = "ka",
                HeaderText = "Ca",
                FlatStyle = FlatStyle.Flat
            };
            colKa.Items.AddRange(_kaOptions);
            dataGridView2.Columns.Add(colKa);

            var colSt = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "start_time",
                Name = "start_time",
                HeaderText = "Giờ bắt đầu",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" }
            };
            var colEn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "end_time",
                Name = "end_time",
                HeaderText = "Giờ kết thúc",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" }
            };
            dataGridView2.Columns.Add(colSt);
            dataGridView2.Columns.Add(colEn);

            var colNote = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "note",
                Name = "note",
                HeaderText = "Ghi chú"
            };
            dataGridView2.Columns.Add(colNote);

            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // style/handlers
            dataGridView2.RowPrePaint -= dataGridView2_RowPrePaint_Tab4;
            dataGridView2.RowPrePaint += dataGridView2_RowPrePaint_Tab4;

            dataGridView2.CellValueChanged -= dataGridView2_CellValueChanged_Tab4;
            dataGridView2.CellValueChanged += dataGridView2_CellValueChanged_Tab4;

            dataGridView2.CurrentCellDirtyStateChanged -= dataGridView2_CurrentCellDirtyStateChanged_Tab4Commit;
            dataGridView2.CurrentCellDirtyStateChanged += dataGridView2_CurrentCellDirtyStateChanged_Tab4Commit;

            dataGridView2.EditingControlShowing -= dataGridView2_EditingControlShowing_Tab4;
            dataGridView2.EditingControlShowing += dataGridView2_EditingControlShowing_Tab4;

            if (_tab4Table != null)
            {
                _tab4Table.ColumnChanged -= _tab4Table_ColumnChanged;
                _tab4Table.ColumnChanged += _tab4Table_ColumnChanged;
            }

            dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void LoadTab3PlanGrid()
        {
            try
            {
                if (modetab != 3 || string.IsNullOrEmpty(_tab3MachineId)) return;

                // ---------- helpers ----------
                string NormalizeTextLocal(string? s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return "";
                    string noDia = StripDiacritics(s);
                    var compact = System.Text.RegularExpressions.Regex.Replace(noDia, @"\s+", " ").Trim();
                    return compact.ToLowerInvariant();
                }
                string? DetectCategoryFromMachineIdLocal(string? machineId)
                {
                    if (string.IsNullOrWhiteSpace(machineId)) return null;
                    foreach (var kv in categoryKeywords)
                        if (kv.Value.Any(k => machineId.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                            return kv.Key;
                    return null;
                }
                double? ResolveStepHoursLocal(
                    string lsx, string? stepHint, string? machineId,
                    Dictionary<string, List<(string Step, double Hours)>> allSteps)
                {
                    if (!allSteps.TryGetValue(lsx, out var list) || list.Count == 0) return null;

                    if (!string.IsNullOrWhiteSpace(stepHint))
                    {
                        var hintN = NormalizeTextLocal(stepHint);
                        var exact = list.FirstOrDefault(t => NormalizeTextLocal(t.Step) == hintN);
                        if (!string.IsNullOrEmpty(exact.Step)) return exact.Hours;

                        var contains = list.FirstOrDefault(t =>
                        {
                            var stepN = NormalizeTextLocal(t.Step);
                            return stepN.Contains(hintN) || hintN.Contains(stepN);
                        });
                        if (!string.IsNullOrEmpty(contains.Step)) return contains.Hours;
                    }
                    var cat = DetectCategoryFromMachineIdLocal(machineId);
                    if (!string.IsNullOrEmpty(cat) && categoryKeywords.TryGetValue(cat, out var keys))
                    {
                        var match = list.FirstOrDefault(t => keys.Any(k => t.Step.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0));
                        if (!string.IsNullOrEmpty(match.Step)) return match.Hours;
                    }
                    var nz = list.FirstOrDefault(t => t.Hours > 0);
                    if (!string.IsNullOrEmpty(nz.Step)) return nz.Hours;

                    var mx = list.OrderByDescending(t => t.Hours).FirstOrDefault();
                    return !string.IsNullOrEmpty(mx.Step) ? mx.Hours : (double?)null;
                }
                // --------------------------------

                var dt = SQL.LoadPlanForMachineFromDate(_tab3MachineId, _tab3StartDate);

                // add/prepare expected columns
                if (!dt.Columns.Contains("Date")) dt.Columns.Add("Date", typeof(bool));
                foreach (DataRow r in dt.Rows)
                {
                    bool planned = r["RawDate"] != DBNull.Value && ((DateTime)r["RawDate"]).Date >= _tab3StartDate;
                    r["Date"] = planned;
                }
                if (!dt.Columns.Contains("product_name")) dt.Columns.Add("product_name", typeof(string));
                if (!dt.Columns.Contains("product_code")) dt.Columns.Add("product_code", typeof(string));
                if (!dt.Columns.Contains("production_qty")) dt.Columns.Add("production_qty", typeof(object));
                if (!dt.Columns.Contains("order_name")) dt.Columns.Add("order_name", typeof(string));
                if (!dt.Columns.Contains("desire")) dt.Columns.Add("desire", typeof(DateTime));
                if (!dt.Columns.Contains("note")) dt.Columns.Add("note", typeof(string));
                if (!dt.Columns.Contains("time_needed")) dt.Columns.Add("time_needed", typeof(double));
                if (!dt.Columns.Contains("start_time")) dt.Columns.Add("start_time", typeof(DateTime));
                if (!dt.Columns.Contains("finish_time")) dt.Columns.Add("finish_time", typeof(DateTime));
                if (!dt.Columns.Contains("ka")) dt.Columns.Add("ka", typeof(string));
                if (!dt.Columns.Contains("plan_order")) dt.Columns.Add("plan_order", typeof(int));

                // stable plan_order seed
                var plannedAtLoad = dt.AsEnumerable()
                    .Where(r => r.Field<bool?>("Date") == true)
                    .OrderBy(r => r.Field<DateTime?>("RawDate") ?? DateTime.MaxValue)
                    .ThenBy(r => r.Field<short?>("sequence") ?? short.MaxValue)
                    .ThenBy(r => r.Field<string?>("LSX") ?? string.Empty)
                    .ToList();
                int seed = 1; foreach (var r in plannedAtLoad) r["plan_order"] = seed++;

                // try to fill meta from cached Tab1/2 data
                if (allWorkorders != null && allWorkorders.Columns.Contains("lsx"))
                {
                    var idx = allWorkorders.AsEnumerable()
                        .GroupBy(r => NormalizeLsxKey(r["lsx"]))
                        .ToDictionary(g => g.Key, g => g.First());

                    foreach (DataRow r in dt.Rows)
                    {
                        var key = NormalizeLsxKey(r["LSX"]);
                        if (idx.TryGetValue(key, out var src))
                        {
                            r["product_name"] = src.Table.Columns.Contains("product_name") ? src["product_name"] : DBNull.Value;
                            r["product_code"] = src.Table.Columns.Contains("product_code") ? src["product_code"] : DBNull.Value;
                            r["production_qty"] = src.Table.Columns.Contains("production_qty") ? src["production_qty"] : DBNull.Value;
                            r["order_name"] = src.Table.Columns.Contains("order_name") ? src["order_name"] : DBNull.Value;
                            r["desire"] = src.Table.Columns.Contains("desire") ? src["desire"] : DBNull.Value;
                        }
                    }
                }

                // fetch any still-missing meta from Postgres
                var toFetch = dt.AsEnumerable()
                    .Where(r => string.IsNullOrWhiteSpace(r["product_name"]?.ToString())
                             && string.IsNullOrWhiteSpace(r["product_code"]?.ToString())
                             && (r["production_qty"] == DBNull.Value || string.IsNullOrWhiteSpace(r["production_qty"]?.ToString())))
                    .Select(r => (r["LSX"]?.ToString() ?? "").Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (toFetch.Length > 0)
                {
                    using var conn = new Npgsql.NpgsqlConnection(SQL.PostGreSQLConnectionString);
                    conn.Open();
                    const string metaSql = @"
WITH base AS (
  SELECT mp.id AS production_id, mp.sophieu AS lsx, mp.product_id, mp.product_qty AS mo_qty
  FROM mrp_production mp
),
pick AS (
  SELECT sm.production_id,
         MIN(COALESCE(sm.date_expected, sm.date, sp.date_done))::date AS giao_date,
         SUM(sm.product_uom_qty) AS move_qty,
         MIN(sp.origin) AS so_name
  FROM stock_move sm
  LEFT JOIN stock_picking sp ON sp.id = sm.picking_id
  WHERE sm.production_id IS NOT NULL AND (sm.state IS NULL OR sm.state <> 'cancel')
  GROUP BY sm.production_id
),
sol AS (
  SELECT so.name AS so_name, sol.product_id, SUM(sol.product_uom_qty) AS so_line_qty
  FROM sale_order_line sol
  JOIN sale_order so ON so.id = sol.order_id
  GROUP BY so.name, sol.product_id
),
done AS (
  SELECT wo.production_id, COALESCE(SUM(wo.qty_produced), 0) AS qty_done
  FROM mrp_workorder wo
  GROUP BY wo.production_id
),
ranked AS (
  SELECT
    mp.sophieu AS lsx,
    r.name     AS routing_name,
    COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
    CAST(wo.date_planned_start AS date) AS date_planned_start,
    pick.so_name AS order_name,
    COALESCE(sol.so_line_qty, pick.move_qty, 0) AS production_qty,
    pick.giao_date AS date_planned_finished,
    GREATEST(COALESCE(sol.so_line_qty, pick.move_qty, 0) - COALESCE(d.qty_done, 0), 0) AS can_sx,
    ROW_NUMBER() OVER (
      PARTITION BY mp.sophieu
      ORDER BY CASE wo.state WHEN 'ready' THEN 0 WHEN 'pending' THEN 1 ELSE 2 END,
               wo.date_planned_start DESC
    ) AS rn
  FROM mrp_production_data pd
  JOIN mrp_workorder  wo ON pd.production_id = wo.production_id
  JOIN mrp_production mp ON pd.production_id = mp.id
  JOIN mrp_routing    r  ON mp.routing_id    = r.id
  LEFT JOIN base  b   ON b.production_id  = mp.id
  LEFT JOIN pick  pick ON pick.production_id = mp.id
  LEFT JOIN sol   sol  ON sol.so_name = pick.so_name AND sol.product_id = b.product_id
  LEFT JOIN done  d    ON d.production_id = mp.id
  WHERE wo.state <> 'cancel'
)
SELECT
  lsx,
  split_part(routing_name,' ',1) AS product_code,
  ltrim(routing_name, split_part(routing_name,' ',1)) AS product_name,
  order_name,
  production_qty,
  date_planned_finished
FROM ranked
WHERE rn = 1 AND lsx = ANY(@lsx);";
                    using var cmd = new Npgsql.NpgsqlCommand(metaSql, conn);
                    cmd.Parameters.Add("@lsx", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text).Value = toFetch;
                    using var rd = cmd.ExecuteReader();
                    var meta = new Dictionary<string, (string code, string name, string order, object qty, DateTime? giao)>(StringComparer.OrdinalIgnoreCase);
                    while (rd.Read())
                    {
                        var lsx = (rd["lsx"]?.ToString() ?? "").Trim();
                        var code = (rd["product_code"]?.ToString() ?? "").Trim();
                        var name = (rd["product_name"]?.ToString() ?? "").Trim();
                        var ord = (rd["order_name"]?.ToString() ?? "").Trim();
                        var qty = rd["production_qty"] ?? DBNull.Value;
                        DateTime? giao = rd.IsDBNull(rd.GetOrdinal("date_planned_finished")) ? (DateTime?)null : Convert.ToDateTime(rd["date_planned_finished"]);
                        meta[lsx] = (code, name, ord, qty, giao);
                    }
                    foreach (DataRow r in dt.Rows)
                    {
                        var lsx = (r["LSX"]?.ToString() ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(lsx)) continue;
                        if (meta.TryGetValue(lsx, out var m))
                        {
                            if (string.IsNullOrWhiteSpace(r["product_code"]?.ToString())) r["product_code"] = m.code;
                            if (string.IsNullOrWhiteSpace(r["product_name"]?.ToString())) r["product_name"] = m.name;
                            if (string.IsNullOrWhiteSpace(r["order_name"]?.ToString())) r["order_name"] = m.order;
                            if (r["production_qty"] == DBNull.Value || string.IsNullOrWhiteSpace(r["production_qty"]?.ToString()))
                                r["production_qty"] = m.qty;
                            if (dt.Columns.Contains("desire") && r["desire"] == DBNull.Value && m.giao.HasValue)
                                r["desire"] = m.giao.Value;
                        }
                    }
                }

                // pick Ka from Shift table (by machine), fallback Tab4, else default
                DateTime minDay = plannedAtLoad.Count > 0
                    ? plannedAtLoad.Min(r => (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date)
                    : _tab3StartDate;
                DateTime maxDay = plannedAtLoad.Count > 0
                    ? plannedAtLoad.Max(r => (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date)
                    : _tab3StartDate;

                var dbShiftsByDate = SQL.LoadShiftDays(_tab3MachineId ?? "", minDay, maxDay)
                                      .GroupBy(kv => kv.Key.Date)
                                      .ToDictionary(g => g.Key, g => g.Last().Value);

                foreach (DataRow r in dt.Rows)
                {
                    if (r.Field<bool?>("Date") != true) continue;

                    var day = (r.Field<DateTime?>("RawDate") ?? _tab3StartDate).Date;
                    bool set = false;

                    if (dbShiftsByDate.TryGetValue(day, out var v) && !string.IsNullOrWhiteSpace(v.Shift))
                    {
                        r["ka"] = NormalizeKaKey(v.Shift);
                        set = true;
                    }
                    if (!set && _tab4Table != null &&
                        string.Equals(_tab4MachineId, _tab3MachineId, StringComparison.OrdinalIgnoreCase))
                    {
                        var row4 = _tab4Table.AsEnumerable().FirstOrDefault(x => x.Field<DateTime>("DateOnly").Date == day);
                        var ka4 = row4?.Field<string?>("ka");
                        if (!string.IsNullOrWhiteSpace(ka4))
                        {
                            r["ka"] = NormalizeKaKey(ka4);
                            set = true;
                        }
                    }
                    if (!set && string.IsNullOrWhiteSpace(r["ka"]?.ToString()))
                        r["ka"] = DefaultShiftName;
                }

                // build time_needed from mrp_workorder.sogio_can
                var lsxList = dt.AsEnumerable()
                                .Select(r => (r["LSX"]?.ToString() ?? "").Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

                var allSteps = new Dictionary<string, List<(string Step, double Hours)>>(StringComparer.OrdinalIgnoreCase);
                if (lsxList.Count > 0)
                {
                    using var conn = new Npgsql.NpgsqlConnection(SQL.PostGreSQLConnectionString);
                    conn.Open();
                    const string sql = @"
SELECT mp.sophieu AS lsx,
       COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
       COALESCE(wo.sogio_can, 0) AS sogio_can
FROM mrp_workorder wo
JOIN mrp_production mp ON mp.id = wo.production_id
WHERE mp.sophieu = ANY(@lsx);";
                    using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
                    cmd.Parameters.Add("@lsx", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text).Value = lsxList.ToArray();
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read())
                    {
                        var lsx = (rd["lsx"]?.ToString() ?? "").Trim();
                        var step = (rd["workorder_name"]?.ToString() ?? "Thành Phẩm").Trim();
                        var hrs = Convert.ToDouble(rd["sogio_can"] ?? 0d);
                        if (!allSteps.TryGetValue(lsx, out var list)) { list = new(); allSteps[lsx] = list; }
                        list.Add((step, hrs));
                    }
                }

                foreach (DataRow r in dt.Rows)
                {
                    var lsx = (r["LSX"]?.ToString() ?? "").Trim();
                    string? hint = dt.Columns.Contains("workorder_name") ? r["workorder_name"]?.ToString() : null;
                    var hours = ResolveStepHoursLocal(lsx, hint, _tab3MachineId, allSteps);
                    if (hours.HasValue) r["time_needed"] = hours.Value;
                    UpdateStartFinishForRow(r);
                }

                // bind grid
                dataGridView2.DataSource = dt;
                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                if (dataGridView2.Columns.Contains("product_name"))
                {
                    var c = dataGridView2.Columns["product_name"];
                    c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    c.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                }
                foreach (DataGridViewColumn c in dataGridView2.Columns)
                    if (c.Name != "product_name") c.FillWeight = Math.Max(60, c.FillWeight);

                if (dataGridView2.Columns.Contains("LSX")) dataGridView2.Columns["LSX"].HeaderText = "LSX";
                if (dataGridView2.Columns.Contains("RawDate")) dataGridView2.Columns["RawDate"].HeaderText = "Ngày KH";
                if (dataGridView2.Columns.Contains("sequence")) dataGridView2.Columns["sequence"].HeaderText = "Thứ tự";
                if (dataGridView2.Columns.Contains("product_name")) dataGridView2.Columns["product_name"].HeaderText = "Tên Sản Phẩm";
                if (dataGridView2.Columns.Contains("product_code")) dataGridView2.Columns["product_code"].HeaderText = "Mã SP";
                if (dataGridView2.Columns.Contains("production_qty")) dataGridView2.Columns["production_qty"].HeaderText = "Số lượng";
                if (dataGridView2.Columns.Contains("order_name")) dataGridView2.Columns["order_name"].HeaderText = "Số ĐH";
                if (dataGridView2.Columns.Contains("note")) dataGridView2.Columns["note"].HeaderText = "Ghi chú";
                if (dataGridView2.Columns.Contains("time_needed")) dataGridView2.Columns["time_needed"].HeaderText = "Th.gian cần (h)";
                if (dataGridView2.Columns.Contains("start_time")) dataGridView2.Columns["start_time"].HeaderText = "Giờ bắt đầu";
                if (dataGridView2.Columns.Contains("finish_time")) dataGridView2.Columns["finish_time"].HeaderText = "Giờ kết thúc";
                if (dataGridView2.Columns.Contains("desire"))
                {
                    dataGridView2.Columns["desire"].HeaderText = "Lịch nhận tuần";
                    dataGridView2.Columns["desire"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                if (dataGridView2.Columns.Contains("start_time"))
                    dataGridView2.Columns["start_time"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                if (dataGridView2.Columns.Contains("finish_time"))
                    dataGridView2.Columns["finish_time"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                foreach (DataGridViewColumn c in dataGridView2.Columns) c.ReadOnly = true;
                if (dataGridView2.Columns.Contains("Date")) dataGridView2.Columns["Date"].ReadOnly = false;
                if (dataGridView2.Columns.Contains("note")) dataGridView2.Columns["note"].ReadOnly = false;

                foreach (DataGridViewRow row in dataGridView2.Rows) ApplyPlannedRowStyle(row);

                RecomputeSequenceAndSort();
                RecalculateSchedule(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }



        private void Default_Ka_SelectedIndexChanged(object sender, EventArgs e)
        {
            _tab4DefaultKa = NormalizeKaKey((sender as ComboBox)?.SelectedItem?.ToString());
        }

        private void _tab4Table_ColumnChanged(object? sender, DataColumnChangeEventArgs e)
        {
            if (modetab != 4) return;
            if (_tab4ApplyingKa) return;                 // avoid re-entrancy
            if (e.Column.ColumnName == "ka" && e.Row?.Table != null)
                RecalcRowTimes(e.Row);
        }

        private void KaEditingCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (modetab != 4) return;
            if (_tab4ApplyingKa) return; // guard
            if (sender is not ComboBox cb) return;
            if (dataGridView2.CurrentCell is not DataGridViewComboBoxCell) return;

            _tab4ApplyingKa = true;
            int rowIndex = dataGridView2.CurrentCell.RowIndex;

            // Defer the update so the ComboBox finishes its own change first
            BeginInvoke(new Action(() =>
            {
                try
                {
                    var drv = dataGridView2.Rows[rowIndex].DataBoundItem as DataRowView;
                    if (drv != null)
                    {
                        var normalized = NormalizeKaKey(cb.SelectedItem?.ToString());
                        drv["ka"] = normalized;              // write to data source once

                        RecalcRowTimes(drv.Row);             // recompute times now
                        ApplyTab4RowStyle(dataGridView2.Rows[rowIndex]);
                    }

                    dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    dataGridView2.EndEdit();
                    dataGridView2.InvalidateRow(rowIndex);   // immediate repaint
                }
                finally
                {
                    _tab4ApplyingKa = false;
                }
            }));
        }

        private void ApplyTab4RowStyle(DataGridViewRow gridRow)
        {
            if (gridRow?.DataBoundItem is not DataRowView drv) return;

            bool isSunday = drv.Row.Field<DateTime>("DateOnly").DayOfWeek == DayOfWeek.Sunday;
            string ka = drv.Row.Field<string?>("ka") ?? "";
            bool isNghi = NormalizeKaKey(ka) == "Nghỉ";

            // Priority: Nghỉ > Sunday > normal
            if (isNghi) gridRow.DefaultCellStyle.BackColor = Color.Gainsboro;
            else if (isSunday) gridRow.DefaultCellStyle.BackColor = Color.Gainsboro;
            else gridRow.DefaultCellStyle.BackColor = SystemColors.Window;
        }

        void ClearGrid(DataGridView grid, bool removeColumns = false)
        {
            grid.SuspendLayout();
            try
            {
                if (grid.DataSource is DataView dv && dv.Table != null) dv.Table.Clear();
                else if (grid.DataSource is DataTable dt) dt.Clear();
                else if (grid.DataSource is BindingSource bs)
                {
                    if (bs.List is DataView v && v.Table != null) v.Table.Clear();
                    else if (bs.List is DataTable t) t.Clear();
                    else bs.Clear();
                }
                else if (grid.DataSource != null) grid.DataSource = null;
                else grid.Rows.Clear();

                if (removeColumns) grid.Columns.Clear();
                grid.ClearSelection();
            }
            finally { grid.ResumeLayout(); }
        }

        // NEW: fetch sogio_can per LSX + step (workorder_name)
        public static Dictionary<(string Lsx, string Step), double> LoadStepTimePerLsxAndStep(IEnumerable<(string Lsx, string Step)> keys)
        {
            var wanted = keys
                .Where(k => !string.IsNullOrWhiteSpace(k.Lsx) && !string.IsNullOrWhiteSpace(k.Step))
                .Distinct()
                .ToList();

            var result = new Dictionary<(string, string), double>(
                new ValueTupleComparer<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase));

            if (wanted.Count == 0) return result;

            var lsxArr = wanted.Select(k => k.Lsx.Trim()).Distinct().ToArray();

            using var conn = new NpgsqlConnection(SQL.PostGreSQLConnectionString);
            conn.Open();
            const string sql = @"
        SELECT mp.sophieu AS lsx,
               COALESCE(NULLIF(wo.routing_equip_name,''),'Thành Phẩm') AS workorder_name,
               COALESCE(wo.sogio_can, 0) AS sogio_can
        FROM mrp_workorder wo
        JOIN mrp_production mp ON mp.id = wo.production_id
        WHERE mp.sophieu = ANY(@lsx);";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.Add("@lsx", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text).Value = lsxArr;

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                var lsx = (rd["lsx"]?.ToString() ?? "").Trim();
                var step = (rd["workorder_name"]?.ToString() ?? "Thành Phẩm").Trim();
                var hours = Convert.ToDouble(rd["sogio_can"] ?? 0d);
                result[(lsx, step)] = hours;
            }

            // keep only the pairs the grid actually shows (defensive)
            var filtered = new Dictionary<(string, string), double>(
                new ValueTupleComparer<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase));
            foreach (var p in wanted)
                if (result.TryGetValue((p.Lsx.Trim(), p.Step.Trim()), out var h)) filtered[(p.Lsx.Trim(), p.Step.Trim())] = h;

            return filtered;
        }

        // helper to get case-insensitive tuple keys
        private sealed class ValueTupleComparer<T1, T2> : IEqualityComparer<(T1, T2)>
        {
            private readonly IEqualityComparer<T1> _c1;
            private readonly IEqualityComparer<T2> _c2;
            public ValueTupleComparer(IEqualityComparer<T1> c1, IEqualityComparer<T2> c2) { _c1 = c1; _c2 = c2; }
            public bool Equals((T1, T2) x, (T1, T2) y) => _c1.Equals(x.Item1, y.Item1) && _c2.Equals(x.Item2, y.Item2);
            public int GetHashCode((T1, T2) obj) => HashCode.Combine(_c1.GetHashCode(obj.Item1), _c2.GetHashCode(obj.Item2));
        }


        private void dataGridView2_CurrentCellDirtyStateChanged_Tab4Commit(object? sender, EventArgs e)
        {
            if (modetab == 4 && dataGridView2.IsCurrentCellDirty)
                dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGridView2_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var colName = dataGridView2.Columns[e.ColumnIndex].Name;
            if (colName != "machine") return;

            var cell = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var raw = cell.Value?.ToString()?.Trim();

            if (string.IsNullOrEmpty(raw)) return;

            if (!int.TryParse(raw, out int index) || index <= 0)
            {
                // Not a positive integer → leave as user entered
                return;
            }

            // Prefer the DateSorter-selected category
            string? orderKey = currentOrderKeyForTab2;

            // If none selected or not mappable, infer from workorder_name
            if (string.IsNullOrEmpty(orderKey))
            {
                var woName = dataGridView2.Rows[e.RowIndex].Cells["workorder_name"]?.Value?.ToString();
                orderKey = InferOrderKeyFromWorkorderName(woName);
            }

            // If still no key, do nothing (keep the number)
            if (string.IsNullOrEmpty(orderKey))
                return;

            // Map the number to a machine name using OrderMap
            string mapped = WorkorderService.OrderMap(orderKey, index);

            // Write back mapped name
            cell.Value = mapped;
        }

        private void dataGridView2_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridView2.CurrentCell == null) return;
            var colName = dataGridView2.Columns[dataGridView2.CurrentCell.ColumnIndex].Name;

            if (e.Control is TextBox tb)
            {
                // clear old
                tb.KeyPress -= MachineCell_KeyPress;
                tb.KeyPress -= Sequence_KeyPress;

                if (colName == "machine") tb.KeyPress += MachineCell_KeyPress; // your Tab-2 mapping (digits only)
                if (colName == "sequence") tb.KeyPress += Sequence_KeyPress;    // Tab-3: digits only
            }
        }

        private void dataGridView2_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dataGridView2.IsCurrentCellDirty)
                dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGridView2_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (modetab != 3) return;
            foreach (DataGridViewRow row in dataGridView2.Rows) ApplyPlannedRowStyle(row);
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView2_RowPrePaint_Tab4(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (modetab != 4 || e.RowIndex < 0) return;
            ApplyTab4RowStyle(dataGridView2.Rows[e.RowIndex]);
        }

        private void dataGridView2_EditingControlShowing_Tab4(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (modetab != 4) return;

            if (_tab4KaEditingCombo != null)
                _tab4KaEditingCombo.SelectedIndexChanged -= KaEditingCombo_SelectedIndexChanged;

            if (dataGridView2.CurrentCell is DataGridViewComboBoxCell &&
                dataGridView2.Columns[dataGridView2.CurrentCell.ColumnIndex].Name == "ka" &&
                e.Control is ComboBox combo)
            {
                _tab4KaEditingCombo = combo;
                _tab4KaEditingCombo.DropDownStyle = ComboBoxStyle.DropDownList; // important
                _tab4KaEditingCombo.SelectedIndexChanged += KaEditingCombo_SelectedIndexChanged;
            }
        }

        private void dataGridView2_CellValueChanged_Tab4(object? sender, DataGridViewCellEventArgs e)
        {
            if (modetab != 4 || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dataGridView2.Columns[e.ColumnIndex].Name == "ka")
            {
                var drv = dataGridView2.Rows[e.RowIndex].DataBoundItem as DataRowView;
                if (drv != null) RecalcRowTimes(drv.Row);
                ApplyTab4RowStyle(dataGridView2.Rows[e.RowIndex]);
                dataGridView2.InvalidateRow(e.RowIndex);
            }
        }

        private void dataGridView2_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (modetab != 3 || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dataGridView2.Columns[e.ColumnIndex].Name;
            var dv = dataGridView2.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (dv == null) return;
            var row = dv.Row;

            if (col == "Date")
            {
                bool planned = row.Field<bool?>("Date") == true;
                var dt = (DataTable)dataGridView2.DataSource;

                if (planned)
                {
                    if (row["RawDate"] == DBNull.Value || ((DateTime)row["RawDate"]).Date < _tab3StartDate)
                        row["RawDate"] = _tab3StartDate;

                    if (string.IsNullOrWhiteSpace(row["ka"]?.ToString()))
                    {
                        var baseDay = (row["RawDate"] != DBNull.Value ? ((DateTime)row["RawDate"]).Date : _tab3StartDate);
                        var one = SQL.LoadShiftDays(_tab3MachineId ?? "", baseDay, baseDay);
                        if (one.TryGetValue(baseDay, out var v) && !string.IsNullOrWhiteSpace(v.Shift))
                            row["ka"] = NormalizeKaKey(v.Shift);
                        else
                            row["ka"] = DefaultShiftName;
                    }

                    row["plan_order"] = NextPlanOrder(dt);

                    row["start_time"] = DBNull.Value;
                    row["finish_time"] = DBNull.Value;
                }
                else
                {
                    row["sequence"] = DBNull.Value;
                    row["RawDate"] = DBNull.Value;
                    row["start_time"] = DBNull.Value;
                    row["finish_time"] = DBNull.Value;
                    row["plan_order"] = DBNull.Value;

                    CompactPlanOrder(dt);
                }

                ApplyPlannedRowStyle(dataGridView2.Rows[e.RowIndex]);

                RecalculateSchedule(dt);
                RecomputeSequenceAndSort();
                return;
            }



        }

        private void tab1_Click(object sender, EventArgs e)
        {
            ClearGrid(dataGridView2, removeColumns: true);
            textBox1.Clear();
            modetab = 1;
            label1.Text = "Lịch nhận tuần";
            button1.Visible = true;
            Default_Ka.Visible = false;
            ShowEmpty.Text = "Hiển thị LSX trống";
            chkCopyToAll.Visible = true;
            chkCopyToAll.Text = "Copy vào Số ĐH tương tự";
            allWorkorders = WorkorderService.LoadPendingWorkorders(dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);
            RefreshSearcherItems();
        }

        private void tab2_Click(object sender, EventArgs e)
        {
            ClearGrid(dataGridView2, removeColumns: true);
            modetab = 2;
            textBox1.Clear();
            label1.Text = "KHSX theo công đoạn";
            Default_Ka.Visible = false;
            button1.Visible = true;
            chkCopyToAll.Visible = false;
            ShowEmpty.Text = "Hiển thị LSX chưa có kế hoạch";
            allWorkorders = WorkorderService.LoadPendingWorkorders(dataGridView1, dataGridView2, ShowEmpty, ShowOld, DateSorter, modetab);

            // user will type quickly into the machine column
            dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;

            // Sort any way you like; this is harmless for mapping logic
            if (dataGridView2.DataSource is DataView dv)
                dv.Sort = "workorder_name ASC";

            // (Re)populate categories for the number→name mapping
            From.Visible = false;
            To.Visible = false;
            DateSorter.Items.Clear();

            var categorizedCounts = new Dictionary<string, int>();
            var uncategorizedCounts = new Dictionary<string, int>();
            foreach (var row in allWorkorders.AsEnumerable())
            {
                string? name = row.Field<string?>("workorder_name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                bool matched = false;
                foreach (var category in categoryKeywords)
                {
                    if (category.Value.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (!categorizedCounts.ContainsKey(category.Key)) categorizedCounts[category.Key] = 0;
                        categorizedCounts[category.Key]++; matched = true; break;
                    }
                }
                if (!matched)
                {
                    if (!uncategorizedCounts.ContainsKey(name)) uncategorizedCounts[name] = 0;
                    uncategorizedCounts[name]++;
                }
            }
            foreach (var kv in categorizedCounts.OrderByDescending(c => c.Value))
                DateSorter.Items.Add($"{kv.Key} ({kv.Value})");
            foreach (var kv in uncategorizedCounts.OrderByDescending(c => c.Value))
                DateSorter.Items.Add($"{kv.Key} ({kv.Value})");

            DateSorter.SelectedIndex = 0;
            DateSorter.Text = DateSorter.SelectedText;
            currentOrderKeyForTab2 = null;


            dataGridView2.EditingControlShowing -= dataGridView2_EditingControlShowing;
            dataGridView2.EditingControlShowing += dataGridView2_EditingControlShowing;
            dataGridView2.CellEndEdit -= dataGridView2_CellEndEdit;
            dataGridView2.CellEndEdit += dataGridView2_CellEndEdit;
            RefreshSearcherItems();
        }

        private void tab3_Click(object sender, EventArgs e)
        {
            ClearGrid(dataGridView2, removeColumns: true);
            modetab = 3;
            textBox1.Clear();
            label1.Text = "KHSX máy";
            Default_Ka.Visible = false;
            button1.Visible = false;
            chkCopyToAll.Visible = false;
            From.Visible = true;
            To.Visible = false;

            DateSorter.Visible = true;
            DateSorter.Items.Clear();
            foreach (var m in SQL.LoadMachineListForPlan()) DateSorter.Items.Add(m);
            if (DateSorter.Items.Count > 0)
            {
                DateSorter.SelectedIndex = 0;                   // machine pick
                _tab3MachineId = DateSorter.SelectedItem.ToString();
            }

            _tab3StartDate = From.Value.Date;

            dataGridView2.CurrentCellDirtyStateChanged -= dataGridView2_CurrentCellDirtyStateChanged;
            dataGridView2.CurrentCellDirtyStateChanged += dataGridView2_CurrentCellDirtyStateChanged;
            dataGridView2.CellValueChanged -= dataGridView2_CellValueChanged;
            dataGridView2.CellValueChanged += dataGridView2_CellValueChanged;
            dataGridView2.DataBindingComplete -= dataGridView2_DataBindingComplete;
            dataGridView2.DataBindingComplete += dataGridView2_DataBindingComplete;

            LoadTab3PlanGrid();
            RefreshSearcherItems();
        }

        private void tab4_Click(object sender, EventArgs e)
        {
            modetab = 4;
            ClearGrid(dataGridView2, removeColumns: true);
            label1.Text = "Ka làm việc";
            textBox1.Clear();
            button1.Visible = false;
            Default_Ka.Visible = true;
            chkCopyToAll.Visible = true;
            chkCopyToAll.Text = "Áp Dụng";
            LoadTab4PlanGrid();
            RefreshSearcherItems();
        }

        private void searcher_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-run the search with the new column
            textBox1_TextChanged(sender, e);
        }
    }
}
