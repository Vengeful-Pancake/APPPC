using APPPC.Control;
using Microsoft.Data.SqlClient;
using System.Data;


namespace APPPC.Functions
{
    public partial class chamcong : UserControl
    {
        public static int grd { get; set; }
        public chamcong()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd/MM/yyyy";
            if(int.Parse(Session.CurrentUser.Quyenhan) > 4 || int.Parse(Session.CurrentUser.Msnv) == 35)
            {
                btnExportNS.Visible = true;
            }

            dataGridView2.Location = new Point(12, 178);
            label1.Text = "Bảng chấm công ngày:";
            comboBox1.DisplayMember = "Display";
            comboBox1.ValueMember = "Msnv";
            comboBox1.DataSource = Session.User_s
                .Select(u => new { Display = $"{u.Msnv} - {u.Hoten}", u.Msnv })
                .ToList();
            comboBox1.Visible = false;
            dataGridView2.Visible = false;

            if (int.Parse(Session.CurrentUser.Quyenhan) < 4)
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
            var summaries = Session.CalculateMonthlySummary(dateTimePicker1.Value);
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            grd = 3;

            int stt = 1;
            foreach (var summary in summaries)
            {
                float WHmonthly = summary.TotalWorkHour / 8;
                float EHmonthly = summary.TotalExtraHour / 8;
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
        }

        private void grid(int mode)
        {

            dataGridView1.Rows.Clear();

            // Ensure columns for check-in/out exist only once
            if (dataGridView1.Columns.Count < 10)
            {
                dataGridView1.Columns.Add("CheckIn", "Giờ vào");
                dataGridView1.Columns.Add("CheckOut", "Giờ ra");
            }


            if (Session.User_s == null || Session.User_s.Count == 0)
            {
                MessageBox.Show("Danh sách người dùng chưa được khởi tạo.");
                return;
            }

            int stt = 1;
            if (mode == 1)
            {
                string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

                foreach (SQL.User user in Session.User_s)
                {
                    SQL.Work w = WorkDateToPicker(int.Parse(user.Msnv));

                    //var logs = GetCheckInOutFromChamcongLine(user.Msnv, selectedDate);


                    dataGridView1.Rows.Add(
                        stt++,
                        user.Msnv,
                        user.Hoten,
                        w?.Date ?? selectedDate,
                        w?.WorkHour ?? 0f,
                        w?.ExtraWork ?? 0f,
                        w?.Absent ?? "",
                        w?.Note ?? ""
                    //logs?.Item1 ?? "",  // Giờ vào
                    //logs?.Item2 ?? ""   // Giờ ra
                    );
                }
            }

            else if (mode == 2)
            {
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn người dùng.");
                    return;
                }

                var selected = (dynamic)comboBox1.SelectedItem;
                string selectedMsnv = selected.Msnv.ToString();
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
                    DateTime currentDate = new DateTime(year, month, day);
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
                        work?.Note ?? ""
                    );
                }
            }
            else if (mode == 3)
            {
                tab5_Click(null, null);
            }

            dataGridView1.AutoGenerateColumns = false;
        }

        private SQL.Work WorkDateToPicker(int msnv)
        {
            var workList = SQL.GetWorkData();
            string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            return workList.FirstOrDefault(w => w.Date == selectedDate && int.Parse(w.Msnv) == msnv);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) => grid(grd);
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) => grid(grd);
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow) continue;

                            string msnv = row.Cells[1].Value?.ToString();
                            string date = row.Cells[3].Value?.ToString();
                            float workHour = float.TryParse(row.Cells[4].Value?.ToString(), out float wh) ? wh : 0f;
                            float extraHour = float.TryParse(row.Cells[5].Value?.ToString(), out float eh) ? eh : 0f;
                            string absent = row.Cells[6].Value?.ToString() ?? "";
                            string note = row.Cells[7].Value?.ToString() ?? "";

                            if (workHour > 8)
                            {
                                extraHour = workHour - 8;
                                workHour = 8;
                            }

                            string sql = "IF EXISTS (SELECT 1 FROM Work WHERE MSNV=@msnv AND WorkDate=@date) " +
                                         "UPDATE Work SET WorkHour=@wh, ExtraHour=@eh, Absent=@absent, Note=@note " +
                                         "WHERE MSNV=@msnv AND WorkDate=@date " +
                                         "ELSE INSERT INTO Work (MSNV, WorkDate, WorkHour, ExtraHour, Absent, Note) " +
                                         "VALUES (@msnv, @date, @wh, @eh, @absent, @note)";

                            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@msnv", msnv);
                                cmd.Parameters.AddWithValue("@date", date);
                                cmd.Parameters.AddWithValue("@wh", workHour);
                                cmd.Parameters.AddWithValue("@eh", extraHour);
                                cmd.Parameters.AddWithValue("@absent", absent);
                                cmd.Parameters.AddWithValue("@note", note);
                                cmd.ExecuteNonQuery();
                            }
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
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.Parse(Session.CurrentUser.Quyenhan) >= 4)
            {
                int year = dateTimePicker1.Value.Year;
                int month = dateTimePicker1.Value.Month;

                string input = Microsoft.VisualBasic.Interaction.InputBox("Nhập số ngày công chuẩn:", "Chuẩn ngày công", "26");

                if (int.TryParse(input, out int standardDays))
                {
                    CC_Helpers.ExportHelper.ExportToExcel(year, month, standardDays);
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập số nguyên.");
                }
            }

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
