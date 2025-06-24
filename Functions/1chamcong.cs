using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using APPPC.Control;
using Microsoft.Data.SqlClient;

namespace APPPC.Functions
{
    public partial class chamcong : UserControl
    {
        public static int grd { get; set; }
        public chamcong()
        {
            InitializeComponent();
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

            foreach (var summary in summaries)
            {
                float WHmonthly = summary.TotalWorkHour / 8;
                float EHmonthly = summary.TotalExtraHour / 8;
                float nettotal = WHmonthly + EHmonthly;
                dataGridView2.Rows.Add(
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
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            grid(grd);
        }


        private void grid(int mode)
        {
            dataGridView1.Rows.Clear();

            if (Session.User_s == null || Session.User_s.Count == 0)
            {
                MessageBox.Show("Danh sách người dùng chưa được khởi tạo.");
                return;
            }

            if (mode == 1)
            {
                string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

                foreach (SQL.User user in Session.User_s)
                {
                    SQL.Work w = WorkDateToPicker(int.Parse(user.Msnv));
                    if (w != null)
                    {
                        dataGridView1.Rows.Add(
                            user.Msnv,
                            user.Hoten,
                            w.Date,           // actual work date
                            w.WorkHour,
                            w.ExtraWork,
                            w.Absent,
                            w.Note
                        );
                    }
                    else
                    {
                        dataGridView1.Rows.Add(
                            user.Msnv,
                            user.Hoten,
                            selectedDate,     // fallback to selected date
                            0f,
                            0f,
                            "",
                            ""
                        );
                    }
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

                    var work = allWorks.FirstOrDefault(w =>
                        w.Msnv == selectedMsnv &&
                        w.Date == dateStr
                    );

                    if (work != null)
                    {
                        dataGridView1.Rows.Add(
                            user.Msnv,
                            user.Hoten,
                            currentDate.ToString("yyyy-MM-dd"),
                            work.WorkHour,
                            work.ExtraWork,
                            work.Absent,
                            work.Note
                        );
                    }
                    else
                    {
                        dataGridView1.Rows.Add(
                            user.Msnv,
                            user.Hoten,
                            currentDate.ToString("yyyy-MM-dd"),
                            0f,
                            0f,
                            "",
                            ""
                        );
                    }
                }
            }else if (mode == 3)
            {
                MessageBox.Show("3");
                tab5_Click(null, null);
            }

            dataGridView1.AutoGenerateColumns = false;
        }

        private SQL.Work WorkDateToPicker(int msnv)
        {
            var workList = SQL.GetWorkData();

            string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

            foreach (SQL.Work w in workList)
            {
                if (w.Date == selectedDate && int.Parse(w.Msnv) == msnv)
                {
                    return w;
                }
            }
            return null;

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            grid(grd);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int mode = comboBox1.Visible ? 2 : 1;

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {

                            if (row.IsNewRow) continue;
                            
                            string msnv = row.Cells[0].Value?.ToString();
                            string hoten = row.Cells[1].Value?.ToString(); // not used in DB
                            string date = row.Cells[2].Value?.ToString();
                            float workHour = float.TryParse(row.Cells[3].Value?.ToString(), out float wh) ? wh : 0f;
                            float extraHour = float.TryParse(row.Cells[4].Value?.ToString(), out float eh) ? eh : 0f;
                            string absent = row.Cells[5].Value?.ToString() ?? "";
                            string note = row.Cells[6].Value?.ToString() ?? "";

                            if (!float.TryParse(row.Cells[3].Value?.ToString(), out workHour))
                            {
                                workHour = 0f;
                            }

                            if (!float.TryParse(row.Cells[4].Value?.ToString(), out extraHour))
                            {
                                extraHour = 0f;
                            }

                            if (workHour > 8)
                            {
                                extraHour = 0;
                                extraHour += workHour - 8;
                                workHour = 8;
                            }

                            string checkQuery = "SELECT COUNT(*) FROM Work WHERE MSNV = @msnv AND WorkDate = @date";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@msnv", msnv);
                                checkCmd.Parameters.AddWithValue("@date", date);
                                int count = (int)checkCmd.ExecuteScalar();

                                if (count > 0)
                                {
                                    // UPDATE
                                    string updateQuery = @"UPDATE Work SET 
                                WorkHour = @workHour, 
                                ExtraHour = @extraHour, 
                                Absent = @absent, 
                                Note = @note 
                                WHERE MSNV = @msnv AND WorkDate = @date";

                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction))
                                    {
                                        updateCmd.Parameters.AddWithValue("@workHour", workHour);
                                        updateCmd.Parameters.AddWithValue("@extraHour", extraHour);
                                        updateCmd.Parameters.AddWithValue("@absent", absent);
                                        updateCmd.Parameters.AddWithValue("@note", note);
                                        updateCmd.Parameters.AddWithValue("@msnv", msnv);
                                        updateCmd.Parameters.AddWithValue("@date", date);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // INSERT
                                    string insertQuery = @"INSERT INTO Work (MSNV, WorkDate, WorkHour, ExtraHour, Absent, Note) 
                                VALUES (@msnv, @date, @workHour, @extraHour, @absent, @note)";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@msnv", msnv);
                                        insertCmd.Parameters.AddWithValue("@date", date);
                                        insertCmd.Parameters.AddWithValue("@workHour", workHour);
                                        insertCmd.Parameters.AddWithValue("@extraHour", extraHour);
                                        insertCmd.Parameters.AddWithValue("@absent", absent);
                                        insertCmd.Parameters.AddWithValue("@note", note);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
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

    }
}
