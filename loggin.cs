using APPPC.Control;
using APPPC.Functions;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

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
            this.KeyDown += labelInput_KeyDown;
            this.MinimumSize = this.Size;
            
            // Disable maximize and resize

            CheckForUpdate();



        }


        private async void CheckForUpdate()
        {
            string currentVersion = Application.ProductVersion.Split('+')[0];

            label2.Text = "Phiên bản " + currentVersion;

            string apiUrl = "https://api.github.com/repos/Vengeful-Pancake/APPPC/releases/latest";


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return;

                string json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                string latestVersion = release.tag_name.Replace("v", "");


                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    var result = MessageBox.Show($"Đã có phiên bản {latestVersion} mới hơn. Tải về cập nhập và sử dụng?", "Cập nhập!", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        var installer = release.assets.FirstOrDefault(a => a.name.EndsWith(".msi"));
                        var installer2 = release.assets.FirstOrDefault(a => a.name.EndsWith(".exe"));
                        if (installer != null && installer2 != null)
                        {
                            string tempFile = Path.Combine(Path.GetTempPath(), installer.name);
                            string tempFile2 = Path.Combine(Path.GetTempPath(), installer2.name);
                            using (var wc = new WebClient())
                            {
                                wc.DownloadFile(installer.browser_download_url, tempFile);
                                wc.DownloadFile(installer2.browser_download_url, tempFile2);
                            }

                            Process.Start(tempFile2);
                            Application.Exit();
                        }
                    }
                }
            }
        }

        private void labelInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (current == "username")
                {
                    btn_password.BackgroundImage = Properties.Resources.l11;
                    btn_username.BackgroundImage = Properties.Resources.l00;
                    current = "password";
                }
                else
                {
                    btn_username.BackgroundImage = Properties.Resources.l01;
                    btn_password.BackgroundImage = Properties.Resources.l10;
                    current = "username";
                }

                e.Handled = true;
            }

        }


        private void labelInput_KeyPress(object sender, KeyPressEventArgs e)
        {


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

                if (int.Parse(matchedUser.Quyenhan) >= 4 && int.Parse(matchedUser.Quyenhan) < 7)
                {
                    // Add users from the same NH
                    Session.User_s = users.Where(u => u.Nhom == mainmatchedUser.Nhom).ToList();
                }
                else if (int.Parse(matchedUser.Quyenhan) > 6)
                {
                    // Add all users
                    Session.User_s = Control.SQL.GetUsers();
                }
                else if (int.Parse(matchedUser.Quyenhan) < 4)
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

        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public GitHubAsset[] assets { get; set; }
    }

    public class GitHubAsset
    {
        public string name { get; set; }
        public string browser_download_url { get; set; }
    }


}
