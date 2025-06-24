using APPPC.Control;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APPPC.Functions
{
    public partial class TaiKhoan : UserControl
    {
        public TaiKhoan()
        {
            InitializeComponent();
            load();

        }

        private void load()
        {
            if (int.Parse(Session.CurrentUser.Quyenhan) <= 4)
            {
                group.Visible = false;
                leaders.Visible = false;
                members.Visible = false;
                infochange.Visible = false;
                chosengroup.Visible = false;
                chosenleader.Visible = false;
                chosenMSNV.Visible = false;
                comboBox1.Visible = false;
                comboBox2.Visible = false;
                comboBox3.Visible = false;
                comboBox4.Visible = false;
                comboBox5.Visible = false;
                comboBox6.Visible = false;
                comboBox7.Visible = false;
                comboBox9.Visible = false;
                comboBox10.Visible = false;
                adddescript.Visible = false;
                addgroup.Visible = false;
                addleader.Visible = false;
                addmember.Visible = false;
                addmsnv.Visible = false;
                addname.Visible = false;
                deletedmember.Visible = false;
                deletemember.Visible = false;
                btnAddMember.Visible = false;
                btnDeleteMember.Visible = false;
                btnChangeInfo.Visible = false;
                checkBox1.Visible = false;
                newgroup.Visible = false;

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



            }

        }

        private void TaiKhoan_Load(object sender, EventArgs e)
        {

        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            if (old_pass.Text == Session.CurrentUser.Matkhau && newpass.Text == newpass2.Text && old_pass.Text != "" )
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

        private void btnAddMember_Click(object sender, EventArgs e)
        {

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
                            (Nhom = @nhom OR Nhom_OVW = @nhom)
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
            }
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void leaders_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
