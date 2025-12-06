// File: APPPC/Functions/chamcong.cs
using System;
using System.Collections.Generic;
using System.ComponentModel; // ListSortDirection
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using APPPC.Control;
using Microsoft.Data.SqlClient;

// Alias to keep signature change localized
using WFControl = System.Windows.Forms.Control;

namespace APPPC.Functions
{
    public partial class chamcong : UserControl
    {
        public static int grd { get; set; }

        // small DTO for ComboBox binding
        private sealed class UserOption
        {
            public string Display { get; set; } = "";
            public string Value { get; set; } = "";
        }

        // --- helper: robust numeric parse for MSNV (non-numeric -> int.MaxValue so they sink to bottom) ---
        private static int ParseMSNV(string? s) => int.TryParse(s, out var n) ? n : int.MaxValue;

        // --- numeric conversion for grid cell values (used by SortCompare) ---
        private static int ToInt(object? v)
        {
            if (v == null) return int.MaxValue;
            var s = v.ToString();
            return int.TryParse(s, out var n) ? n : int.MaxValue;
        }

        // --- extract first name (last word) from Vietnamese full name ---
        private static string ExtractFirstName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;

            var parts = fullName.Trim()
                                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return (parts.Length == 0) ? string.Empty : parts[parts.Length - 1];
        }


        // --- hide STT, CheckIn, CheckOut by Name or HeaderText (VN) ---
        private static void HideCols(DataGridView grid)
        {
            if (grid == null) return;

            // Hide STT by name then fallback to index 0
            if (grid.Columns.Contains("STT"))
                grid.Columns["STT"].Visible = false;
            else if (grid.Columns.Count > 0)
                grid.Columns[0].Visible = false;

            foreach (DataGridViewColumn col in grid.Columns)
            {
                var name = col.Name?.Trim() ?? "";
                var header = col.HeaderText?.Trim() ?? "";

                if (name.Equals("CheckIn", StringComparison.OrdinalIgnoreCase) ||
                    header.Equals("Giờ vào", StringComparison.OrdinalIgnoreCase))
                    col.Visible = false;

                if (name.Equals("CheckOut", StringComparison.OrdinalIgnoreCase) ||
                    header.Equals("Giờ ra", StringComparison.OrdinalIgnoreCase))
                    col.Visible = false;
            }
        }

        // --- enforce numeric sort on MSNV column (name "MSNV" or index 1) ---
        private void DataGridView_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid == null) return;

            bool isMsnvCol =
                string.Equals(e.Column.Name, "MSNV", StringComparison.OrdinalIgnoreCase) ||
                (e.Column.Index == 1);

            if (isMsnvCol)
            {
                int a = ToInt(e.CellValue1);
                int b = ToInt(e.CellValue2);
                e.SortResult = a.CompareTo(b);
                e.Handled = true; // prevent default string sort
            }
        }

        public chamcong()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            // Date format
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd/MM/yyyy";

            // Permissions (safe parsing)
            int qh = int.TryParse(Session.CurrentUser?.Quyenhan, out var qtmp) ? qtmp : 0;
            int msnvMe = int.TryParse(Session.CurrentUser?.Msnv, out var mtmp) ? mtmp : 0;
            btnExportNS.Visible = (qh > 4) || (msnvMe == 35);

            // Label + grid positioning
            dataGridView2.Location = new Point(12, 178);
            label1.Text = "Bảng chấm công ngày:";

            // Ensure users are loaded once, then bind combo safely
            EnsureUsersLoaded();
            BindUserCombo();

            comboBox1.Visible = false;
            dataGridView2.Visible = false;

            // Numeric sort handler for both grids
            dataGridView1.SortCompare += DataGridView_SortCompare;
            dataGridView2.SortCompare += DataGridView_SortCompare;

            // Limit UI based on permission
            if (qh < 4)
            {
                tab1.Enabled = false;
                tab2.Enabled = false;
                tab3.Enabled = false;
                tab4.Enabled = false;
                tab5.Enabled = false;
                btnSave.Enabled = false;
                tab4_Click(this, EventArgs.Empty);
            }

            grd = 1;
            grid(grd);
        }

        private static void EnsureUsersLoaded()
        {
            if (Session.User_s == null || Session.User_s.Count == 0)
            {
                // Choose your source. GetUsers() already filters out resigned users.
                Session.User_s = SQL.GetUsers();
                if (Session.User_s == null) Session.User_s = new List<SQL.User>();
            }
        }

        private void BindUserCombo()
        {
            var items = Session.User_s
                .Where(u => u != null)
                .OrderBy(u => ParseMSNV(u.Msnv)) // numeric MSNV sort
                .Select(u => new UserOption
                {
                    Display = $"{u.Msnv} - {u.Hoten}",
                    Value = u.Msnv
                })
                .ToList();

            comboBox1.DisplayMember = nameof(UserOption.Display);
            comboBox1.ValueMember = nameof(UserOption.Value);
            comboBox1.DataSource = items;

            // default to current user if present
            var cur = Session.CurrentUser?.Msnv;
            if (!string.IsNullOrWhiteSpace(cur) && items.Any(i => i.Value == cur))
                comboBox1.SelectedValue = cur;
        }

        private void tab1_Click(object sender, EventArgs e)
        {
            dataGridView2.Visible = false;
            tab1.BackColor = Color.FromArgb(74, 33, 109);
            dataGridView1.ReadOnly = false;
            label1.Text = "Bảng chấm công ngày:";
            comboBox1.Visible = false;
            grd = 1;
            grid(grd);
        }

        private void tab2_Click(object sender, EventArgs e)
        {
            dataGridView2.Visible = false;
            dataGridView1.ReadOnly = false;
            label1.Text = "Bảng chấm công tháng:";
            comboBox1.Visible = true;
            grd = 2;
            grid(grd);
        }

        private void tab3_Click(object sender, EventArgs e)
        {
            dataGridView2.Visible = false;
            dataGridView1.ReadOnly = true;
            label1.Text = "Xem công ngày:";
            comboBox1.Visible = false;
            grd = 1;
            grid(grd);
        }

        private void tab4_Click(object sender, EventArgs e)
        {
            dataGridView2.Visible = false;
            dataGridView1.ReadOnly = true;
            label1.Text = "Xem công tháng:";
            comboBox1.Visible = true;
            grd = 2;
            grid(grd);
        }

        private void tab5_Click(object sender, EventArgs e)
        {
            dataGridView2.Visible = true;
            label1.Text = "Bảng chấm công tháng:";
            comboBox1.Visible = false;

            var summaries = Session.CalculateMonthlySummary(dateTimePicker1.Value)
                                   .OrderBy(s => ParseMSNV(s.Msnv)) // numeric MSNV sort
                                   .ToList();

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            grd = 3;

            int stt = 1;
            foreach (var summary in summaries)
            {
                float WHmonthly = summary.TotalWorkHour / 8f;
                float EHmonthly = summary.TotalExtraHour / 8f;
                float nettotal = WHmonthly + EHmonthly;

                dataGridView2.Rows.Add(
                    stt++,
                    summary.Msnv,
                    summary.Hoten,
                    summary.TotalWorkHour,
                    summary.TotalExtraHour,
                    WHmonthly,
                    EHmonthly,
                    nettotal,
                    summary.AbsentDays
                );
            }

            // Hide columns and enforce initial numeric sort (MSNV expected at index 1)
            HideCols(dataGridView2);
            if (dataGridView2.Columns.Count > 1)
            {
                dataGridView2.Columns[1].SortMode = DataGridViewColumnSortMode.Programmatic;
                dataGridView2.Sort(dataGridView2.Columns[1], ListSortDirection.Ascending);
            }
        }

        private void grid(int mode)
        {
            EnsureUsersLoaded(); // defensive

            dataGridView1.Rows.Clear();

            // Ensure columns (idempotent)
            if (!dataGridView1.Columns.Contains("CheckIn"))
                dataGridView1.Columns.Add("CheckIn", "Giờ vào");
            if (!dataGridView1.Columns.Contains("CheckOut"))
                dataGridView1.Columns.Add("CheckOut", "Giờ ra");

            // keep them hidden proactively
            if (dataGridView1.Columns.Contains("CheckIn"))
                dataGridView1.Columns["CheckIn"].Visible = false;
            if (dataGridView1.Columns.Contains("CheckOut"))
                dataGridView1.Columns["CheckOut"].Visible = false;

            int stt = 1;

            if (mode == 1)
            {
                string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

                int msnvMe = int.TryParse(Session.CurrentUser?.Msnv, out var meTmp) ? meTmp : 0;

                var usersQuery = Session.User_s
                    .Where(u => u != null);

                if (msnvMe == 35)
                {
                    usersQuery = usersQuery
                        .OrderBy(u => u.Tonhom)                         // 1) Tổ Nhóm
                        .ThenBy(u => ExtractFirstName(u.Hoten));         // 2) First name
                }
                else
                {
                    usersQuery = usersQuery
                        .OrderBy(u => ParseMSNV(u.Msnv));               // default behavior
                }

                foreach (var user in usersQuery)
                {
                    var w = WorkDateToPicker(user.Msnv);

                    dataGridView1.Rows.Add(
                        stt++,
                        user.Msnv,
                        user.Hoten,
                        w?.Date ?? selectedDate,
                        w?.WorkHour ?? 0f,
                        w?.ExtraWork ?? 0f,
                        w?.Absent ?? "",
                        user.Tonhom,
                        w?.Note ?? ""
                    );
                }


                // Hide columns and enforce initial numeric sort (MSNV expected at index 1)
                // Hide columns
                HideCols(dataGridView1);

                int msnvMe2 = int.TryParse(Session.CurrentUser?.Msnv, out var meTmp2) ? meTmp2 : 0;

                // For most users: keep numeric MSNV sort
                if (msnvMe2 != 35 && dataGridView1.Columns.Count > 1)
                {
                    dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.Programmatic;
                    dataGridView1.Sort(dataGridView1.Columns[1], ListSortDirection.Ascending);
                }
                // For MSNV 35: keep the existing row order (Tổ Nhóm + first name)

            }
            else if (mode == 2)
            {
                var selectedMsnv = comboBox1.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selectedMsnv))
                {
                    MessageBox.Show("Vui lòng chọn người dùng.");
                    return;
                }

                var user = Session.User_s.FirstOrDefault(u => u.Msnv == selectedMsnv);
                if (user == null)
                {
                    MessageBox.Show("Không tìm thấy người dùng.");
                    return;
                }

                var allWorks = SQL.GetWorkData();

                int year = dateTimePicker1.Value.Year;
                int month = dateTimePicker1.Value.Month;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(year, month, day);
                    string dateStr = currentDate.ToString("yyyy-MM-dd");
                    var work = allWorks.FirstOrDefault(w => w.Msnv == selectedMsnv && w.Date == dateStr);

                    dataGridView1.Rows.Add(
                        stt++,
                        user.Msnv,
                        user.Hoten,
                        dateStr,
                        work?.WorkHour ?? 0f,
                        work?.ExtraWork ?? 0f,
                        work?.Absent ?? "",
                        user.Tonhom,
                        work?.Note ?? ""
                    );
                }

                // Hide columns and enforce numeric sort as a safety (though single user view)
                HideCols(dataGridView1);
                if (dataGridView1.Columns.Count > 1)
                {
                    dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.Programmatic;
                    dataGridView1.Sort(dataGridView1.Columns[1], ListSortDirection.Ascending);
                }

            }
            else if (mode == 3)
            {
                tab5_Click(null, null);
            }

            dataGridView1.AutoGenerateColumns = false;
        }

        private SQL.Work? WorkDateToPicker(string msnvStr)
        {
            if (!int.TryParse(msnvStr, out _)) return null;

            var workList = SQL.GetWorkData();
            string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            return workList.FirstOrDefault(w => w.Date == selectedDate && w.Msnv == msnvStr);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) => grid(grd);
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) => grid(grd);
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using var conn = new SqlConnection(SQL.GetConnectionString());
            conn.Open();

            using var transaction = conn.BeginTransaction();
            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string msnv = row.Cells[1].Value?.ToString();
                    string date = row.Cells[3].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(msnv) || string.IsNullOrWhiteSpace(date)) continue;

                    float workHour = float.TryParse(row.Cells[4].Value?.ToString(), out var wh) ? wh : 0f;
                    float extraHour = float.TryParse(row.Cells[5].Value?.ToString(), out var eh) ? eh : 0f;
                    string absent = row.Cells[6].Value?.ToString() ?? "";
                    string note = row.Cells[8].Value?.ToString() ?? "";

                    if (workHour > 8f) { extraHour = workHour - 8f; workHour = 8f; }

                    const string sql =
                        "IF EXISTS (SELECT 1 FROM Work WHERE MSNV=@msnv AND WorkDate=@date) " +
                        " UPDATE Work SET WorkHour=@wh, ExtraHour=@eh, Absent=@absent, Note=@note " +
                        " WHERE MSNV=@msnv AND WorkDate=@date " +
                        "ELSE " +
                        " INSERT INTO Work (MSNV, WorkDate, WorkHour, ExtraHour, Absent, Note) " +
                        " VALUES (@msnv, @date, @wh, @eh, @absent, @note)";

                    using var cmd = new SqlCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@msnv", msnv);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@wh", workHour);
                    cmd.Parameters.AddWithValue("@eh", extraHour);
                    cmd.Parameters.AddWithValue("@absent", absent);
                    cmd.Parameters.AddWithValue("@note", note);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                MessageBox.Show("Dữ liệu đã được lưu thành công.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message);
            }

            tab1_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int qh = int.TryParse(Session.CurrentUser?.Quyenhan, out var qtmp) ? qtmp : 0;
            if (qh >= 4)
            {
                int year = dateTimePicker1.Value.Year;
                int month = dateTimePicker1.Value.Month;

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Nhập số ngày công chuẩn:", "Chuẩn ngày công", "26");

                if (int.TryParse(input, out int standardDays))
                    CC_Helpers.ExportHelper.ExportToExcel(year, month, standardDays);
                else
                    MessageBox.Show("Vui lòng nhập số nguyên.");
            }
        }

        // AFTER (typed alias)
        private void ArrangeButtonsRight(params WFControl[] buttons)
        {
            if (buttons == null || buttons.Length == 0) return;

            int rightMargin = 10;
            int spacing = 5;
            int x = this.Width - rightMargin;

            // pick a reference control that’s visible to grab Top; fallback to first
            var refCtrl = buttons.FirstOrDefault(b => b != null && b.Visible) ?? buttons[0];
            int y = refCtrl.Top;

            for (int i = buttons.Length - 1; i >= 0; i--)
            {
                var btn = buttons[i];
                if (btn == null || btn.IsDisposed) continue;

                x -= btn.Width;
                btn.Location = new Point(x, y);
                x -= spacing;
            }
        }

        private void CC_panel_SizeChanged(object sender, EventArgs e)
        {
            int rightMargin = 5;
            int bottomMargin = 40;

            dataGridView1.Width = this.Width - dataGridView1.Location.X - rightMargin;
            dataGridView1.Height = this.Height - dataGridView1.Location.Y - bottomMargin;
            dataGridView2.Width = this.Width - dataGridView2.Location.X - rightMargin;
            dataGridView2.Height = this.Height - dataGridView2.Location.Y - bottomMargin;

            label2.Location = new Point(5, this.Height - label2.Height - 5);
            ArrangeButtonsRight(btnExportNS, btnExport, btnSave);
        }

        private void btnExportNS_Click(object sender, EventArgs e)
        {
            CC_Helpers.ExportHelper.ExportNSToExcel(dateTimePicker1);
        }
    }
}
