using System.Drawing.Printing;
using APPPC.Control;
using APPPC.Functions;
using Microsoft.VisualBasic.ApplicationServices;
namespace APPPC
{
    public partial class loggin : Form
    {

        private string PasswordInput = "";
        private string UsernameInput = "";
        private string current = "none";
        public loggin()
        {
            InitializeComponent();
            btn_password.Text = "";
            btn_username.Text = "";
            this.KeyPreview = true;
            this.KeyPress += labelInput_KeyPress;
            this.Size = new Size(1600,900);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            // Disable maximize and resize
            this.MaximizeBox = false;



        }
        private void labelInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Tab)
            {
                // Toggle between username and password fields
                if (current == "username")
                {
                    btn_password_Click(null, null); // switch to password input
                }
                else
                {
                    btn_username_Click(null, null); // switch to username input
                }
                e.Handled = true; // prevent default tab behavior
            }

            if (e.KeyChar == (char)Keys.Enter)
            {
                btn_login_Click(null, null); // simulate login button click
                e.Handled = true;
            }

            if (current == "password")
            {
                if (e.KeyChar == (char)Keys.Back && PasswordInput.Length > 0)
                {
                    PasswordInput = PasswordInput.Substring(0, PasswordInput.Length - 1);
                }
                else if (!char.IsControl(e.KeyChar))
                {
                    PasswordInput += e.KeyChar;
                }

                btn_password.Text = new string('*', PasswordInput.Length); // hide input
            }
            else if (current == "username")
            {
                if (e.KeyChar == (char)Keys.Back && UsernameInput.Length > 0)
                {
                    UsernameInput = UsernameInput.Substring(0, UsernameInput.Length - 1);
                }
                else if (!char.IsControl(e.KeyChar))
                {
                    UsernameInput += e.KeyChar;
                }

                btn_username.Text = UsernameInput;
            }
        }



        private void btn_username_Click(object sender, EventArgs e)
        {
            btn_username.BackgroundImage = Properties.Resources.l01;
            btn_password.BackgroundImage = Properties.Resources.l10;
            current = "username";
        }

        private void btn_password_Click(object sender, EventArgs e)
        {
            btn_password.BackgroundImage = Properties.Resources.l11;
            btn_username.BackgroundImage = Properties.Resources.l00;
            current = "password";
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            var mainusers = Control.SQL.GetMainUsers();
            var users = Control.SQL.GetUsers();


            var mainmatchedUser = mainusers.FirstOrDefault(u =>
                (u.Taikhoan.Equals(UsernameInput, StringComparison.OrdinalIgnoreCase) || u.Msnv.Equals(UsernameInput)) &&
                u.Matkhau == PasswordInput);
            var matchedUser = users.FirstOrDefault(u =>
                (u.Taikhoan.Equals(UsernameInput, StringComparison.OrdinalIgnoreCase) || u.Msnv.Equals(UsernameInput)) &&
                u.Matkhau == PasswordInput);

            if (btn_username.Text == "" || btn_password.Text == "")
            {
                MessageBox.Show("Thiếu tài khoản hoặc mật khẩu!");
            }
            else if (matchedUser != null)
            {
                // Store the logged-in user globally
                Session.CurrentUser = mainmatchedUser;

                // Create a list to hold filtered users

                if (int.Parse(matchedUser.Quyenhan) == 4)
                {
                    // Add users from the same NH
                    Session.User_s = users.Where(u => u.Nhom == mainmatchedUser.Nhom).ToList();
                }
                else if (int.Parse(matchedUser.Quyenhan) > 5)
                {
                    // Add all users
                    Session.User_s = Control.SQL.GetUsers();
                }
                else if (int.Parse(matchedUser.Quyenhan) == 1)
                {
                    // Add only the current user
                    Session.User_s = users.Where(u => u.Msnv == matchedUser.Msnv).ToList();
                }


                // Proceed to main form
                var mainmenu = new MainMenuForm();
                mainmenu.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu.");
                PasswordInput = "";
                btn_password.Text = "";
                current = "password";
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loggin_Load(object sender, EventArgs e)
        {

        }
    }
}
