using APPPC.Control;
using Microsoft.Data.SqlClient;
using System.Data;

namespace APPPC.Functions
{
    public partial class TaiKhoan : UserControl
    {
        public TaiKhoan()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            load();

        }

        private void load()
        {
            if (int.Parse(Session.CurrentUser.Quyenhan) <= 4)
            {
                System.Windows.Forms.Control[] controlsToHide = new System.Windows.Forms.Control[]
                {
                    group, leaders, members, infochange, chosengroup, chosenleader, chosenMSNV, comboBox1, comboBox2, comboBox3, comboBox4, comboBox5, comboBox6, comboBox9, adddescript, addgroup, addleader, addmember, addmsnv, addname, deletedmember, deletemember, btnAddMember, btnDeleteMember, btnChangeInfo, checkBox1, newgroup
                };

                foreach (var control in controlsToHide)
                    control.Visible = false;
            }
            else
            {
                // Prepare items
                var items = Session.User_s.Select(user =>
                {
                    var toTruong = Session.User_s.FirstOrDefault(u =>
                        u.Nhom == user.Nhom &&
                        int.Parse(u.Quyenhan) >= 4);

                    string toTruongName = toTruong?.Hoten ?? "Không rõ";

                    return new
                    {
                        Display = $"{user.Msnv} - {user.Hoten} - {user.Chucvu} - {toTruongName}",
                        Value = user.Msnv
                    };
                }).ToList();

                comboBox1.DisplayMember = "Display";
                comboBox1.ValueMember = "Value";
                comboBox1.DataSource = items;


                comboBox9.DisplayMember = "Display";
                comboBox9.ValueMember = "Value";
                comboBox9.DataSource = items;

                comboBox6.Items.Clear();
                comboBox6.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });

                comboBox4.Items.Clear();
                foreach (var user in Session.User_s.Where(u => int.Parse(u.Quyenhan) > 3))
                {
                    comboBox4.Items.Add($"{user.Msnv} - {user.Hoten}");
                }

                comboBox5.Items.Clear();
                var groupList = Session.User_s.Select(u => u.Nhom).Distinct().OrderBy(n => n);
                foreach (var nhom in groupList)
                {
                    comboBox5.Items.Add(nhom);
                }

                // Load full leader data
                List<(string MSNV, string HoTen, string NhomFinal)> leaderData = new List<(string, string, string)>();

                using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                    SELECT 
                        MSNV, 
                        HoTen, 
                        CASE 
                            WHEN Nhom_OVW IS NOT NULL THEN CAST(Nhom_OVW AS VARCHAR)
                            ELSE CAST(Nhom AS VARCHAR)
                        END AS NhomFinal
                    FROM Users
                    WHERE QuyenHan > 3", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string msnv = reader["MSNV"].ToString();
                            string hoTen = reader["HoTen"].ToString();
                            string nhomFinal = reader["NhomFinal"].ToString();

                            leaderData.Add((msnv, hoTen, nhomFinal));
                        }
                    }
                }

                // Fill DataTable
                DataTable dt = new DataTable();
                dt.Columns.Add("MSNV");
                dt.Columns.Add("HoTen");
                dt.Columns.Add("Nhom");

                foreach (var item in leaderData)
                {
                    dt.Rows.Add(item.MSNV, item.HoTen, item.NhomFinal);
                }

                leaders.DataSource = dt;
                leaders.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                leaders.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                leaders.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;



            }

        }

        private void TaiKhoan_Load(object sender, EventArgs e)
        {

        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            if (old_pass.Text == Session.CurrentUser.Matkhau && newpass.Text == newpass2.Text && old_pass.Text != "")
            {
                SQL.SaveValue("MatKhau", newpass.Text);
                old_pass.Text = "";
                newpass.Text = "";
                newpass2.Text = "";
                MessageBox.Show("Thay đổi mật khẩu thành công.");
            }
            else
            {
                old_pass.Text = "";
                newpass.Text = "";
                newpass2.Text = "";
                MessageBox.Show("Mật khẩu không thay đổi");
            }
        }

        private void btnChangeInfo_Click(object sender, EventArgs e)
        {
        }
        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if (c == 'đ') stringBuilder.Append('d');
                    else if (c == 'Đ') stringBuilder.Append('D');
                    else stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            string msnv = textBox2.Text.Trim();
            string name = textBox1.Text.Trim();
            string chucvu = textBox3.Text.Trim();
            string quyen = comboBox6.SelectedItem?.ToString();
            string nhom = comboBox5.SelectedItem?.ToString();
            string leaderMSNV = comboBox4.SelectedItem?.ToString()?.Split('-')[0]?.Trim();
            bool isNewGroup = newgroup.Checked;

            if (string.IsNullOrWhiteSpace(msnv))
            {
                MessageBox.Show("Vui lòng nhập MSNV.");
                return;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng chọn Tên.");
                return;
            }
            if (string.IsNullOrWhiteSpace(chucvu))
            {
                MessageBox.Show("Vui lòng chọn Chức Vụ.");
                return;
            }
            if (string.IsNullOrWhiteSpace(quyen))
            {
                MessageBox.Show("Vui lòng chọn Quyền hạn.");
                return;
            }
            if (string.IsNullOrWhiteSpace(nhom) && !isNewGroup)
            {
                MessageBox.Show("Vui lòng chọn Nhóm hoặc đánh dấu 'Tạo nhóm mới'.");
                return;
            }

            if (Session.User_s.Any(u => u.Msnv == msnv))
            {
                MessageBox.Show("MSNV đã tồn tại.");
                return;
            }

            int groupNumber = 0;
            int.TryParse(nhom, out groupNumber);

            using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
            {
                conn.Open();

                if (isNewGroup)
                {
                    SqlCommand getMaxGroup = new SqlCommand("SELECT ISNULL(MAX(Nhom), 0) + 1 FROM Users", conn);
                    groupNumber = Convert.ToInt32(getMaxGroup.ExecuteScalar());
                    quyen = "4"; // auto assign leader
                }
                else if (!string.IsNullOrEmpty(leaderMSNV))
                {
                    SqlCommand getLeaderGroup = new SqlCommand("SELECT ISNULL(Nhom_OVW, Nhom) FROM Users WHERE MSNV = @msnv", conn);
                    getLeaderGroup.Parameters.AddWithValue("@msnv", leaderMSNV);
                    object val = getLeaderGroup.ExecuteScalar();
                    groupNumber = Convert.ToInt32(val);
                }

                // ✅ Get ToNhom for the group
                SqlCommand getToNhomCmd = new SqlCommand("SELECT TOP 1 ToNhom FROM Users WHERE Nhom = @nhom", conn);
                getToNhomCmd.Parameters.AddWithValue("@nhom", groupNumber);
                string toNhom = (getToNhomCmd.ExecuteScalar()?.ToString()) ?? "";
                string taikhoan = RemoveDiacritics(name.Trim().ToLower()).Replace(" ","");

                SqlCommand insert = new SqlCommand(@"
            INSERT INTO Users (MSNV, HoTen, QuyenHan, Nhom, ChucVu, ToNhom, TaiKhoan, MatKhau)
            VALUES (@msnv, @hoten, @quyenhan, @nhom, @chucvu, @tonhom, @taikhoan, 123456)", conn);

                if (!int.TryParse(msnv, out int parsedMSNV))
                {
                    MessageBox.Show("MSNV không hợp lệ.");
                    return;
                }

                insert.Parameters.AddWithValue("@msnv", parsedMSNV);
                insert.Parameters.AddWithValue("@hoten", name);
                insert.Parameters.AddWithValue("@quyenhan", int.Parse(quyen));
                insert.Parameters.AddWithValue("@nhom", groupNumber);
                insert.Parameters.AddWithValue("@chucvu", chucvu);
                insert.Parameters.AddWithValue("@tonhom", toNhom);
                insert.Parameters.AddWithValue("@taikhoan", taikhoan);

                try
                {
                    insert.ExecuteNonQuery();
                    MessageBox.Show("Thêm nhân viên thành công.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm nhân viên: " + ex.Message);
                }
            }

            load();
        }


        private void btnDeleteMember_Click(object sender, EventArgs e)
        {
            if (comboBox9.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để xoá.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedText = comboBox9.Text;
            string msnvStr = selectedText.Split('-')[0].Trim();

            if (!int.TryParse(msnvStr, out int msnv))
            {
                MessageBox.Show("Không thể xác định mã số nhân viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Bạn có chắc muốn xoá nhân viên có MSNV {msnv} không?",
                "Xác nhận xoá",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    SQL.SaveUserValue("NghiViec", "1", msnv.ToString());           // Set nghỉ việc = true
                    SQL.SaveUserValue("MSNV", (-msnv).ToString(), msnv.ToString()); // Optional: change MSNV to negative if needed

                    MessageBox.Show("Đã xoá nhân viên thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xoá nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            load();
        }



        private void leaders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            {
                if (e.RowIndex < 0 || leaders.Rows[e.RowIndex].Cells["Nhom"].Value == null)
                    return;

                string selectedNhom = leaders.Rows[e.RowIndex].Cells["Nhom"].Value.ToString();

                List<(string MSNV, string HoTen)> membersList = new List<(string, string)>();

                using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                        SELECT MSNV, HoTen
                        FROM Users
                        WHERE 
                            (Nhom = @nhom)
                            AND (NghiViec IS NULL OR NghiViec = 0)", conn);
                    cmd.Parameters.AddWithValue("@nhom", selectedNhom);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            membersList.Add((reader["MSNV"].ToString(), reader["HoTen"].ToString()));
                        }
                    }
                }

                // Bind members to right-hand grid
                DataTable dtMembers = new DataTable();
                dtMembers.Columns.Add("MSNV");
                dtMembers.Columns.Add("HoTen");

                foreach (var m in membersList)
                {
                    dtMembers.Rows.Add(m.MSNV, m.HoTen);
                }

                members.DataSource = dtMembers;
                members.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // MSNV
                members.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;     // HoTen

            }
        }


        private void leaders_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e) //add-quyen
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e) //add-MSNV
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) //add-name
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e) //add-nhom
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e) //add-leader
        {
            if (newgroup.Checked || comboBox4.SelectedItem == null) return;

            string selected = comboBox4.SelectedItem.ToString();
            string leaderMSNV = selected.Split('-')[0].Trim();

            using (SqlConnection conn = new SqlConnection(SQL.GetConnectionString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ISNULL(Nhom_OVW, Nhom) FROM Users WHERE MSNV = @msnv", conn);
                cmd.Parameters.AddWithValue("@msnv", leaderMSNV);
                object val = cmd.ExecuteScalar();
                comboBox5.Text = val?.ToString();
            }
        }

        private void newgroup_CheckedChanged(object sender, EventArgs e) //add-newgroup
        {
            if (newgroup.Checked)
            {
                comboBox5.Enabled = false; // nhom
                comboBox4.Enabled = false; // leader
                textBox3.Enabled = false;
            }
            else
            {
                comboBox5.Enabled = true;
                comboBox4.Enabled = true;
                textBox3.Enabled = true;
            }

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
