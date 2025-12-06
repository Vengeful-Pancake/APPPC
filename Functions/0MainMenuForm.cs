using System;
using System.Drawing;
using System.Windows.Forms;
using APPPC.Control;

namespace APPPC.Functions
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();

            HoTen.Text = Session.CurrentUser?.Hoten ?? "";

            button1.BackColor = Color.FromArgb(12, 115, 46);
            var panelView = new chamcong();
            panel.Controls.Add(panelView);
            panel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.BackColor = Color.FromArgb(12, 115, 46);
            button2.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;

            panel.Controls.Clear();
            panel.Controls.Add(new chamcong());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.BackColor = Color.FromArgb(12, 115, 46);
            button1.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;

            panel.Controls.Clear();
            panel.Controls.Add(new NSLD());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.BackColor = Color.FromArgb(12, 115, 46);
            button2.BackColor = Color.Transparent;
            button1.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;

            panel.Controls.Clear();
            panel.Controls.Add(new KHSX_panel());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.BackColor = Color.FromArgb(12, 115, 46);
            button2.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button1.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;

            panel.Controls.Clear();
            panel.Controls.Add(new YCSX());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.BackColor = Color.FromArgb(12, 115, 46);
            button2.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;
            button1.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;

            panel.Controls.Clear();
            panel.Controls.Add(new TimKiem());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.BackColor = Color.FromArgb(12, 115, 46);
            button2.BackColor = Color.Transparent;
            button3.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button1.BackColor = Color.Transparent;

            panel.Controls.Clear();
            panel.Controls.Add(new TaiKhoan());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var login = new loggin();
            Session.CurrentUser = null;
            login.Show();
            this.FormClosing -= MainMenuForm_FormClosing;
            Close();
        }

        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e) => Application.Exit();

        private void ForSizeChanged(object sender, EventArgs e)
        {
            int marginLeft = panel.Left;
            int marginTop = panel.Top;

            panel.Width = this.ClientSize.Width - marginLeft - 20;
            panel.Height = this.ClientSize.Height - marginTop - 40;

            button7.Location = new Point(13, this.Height - button7.Height - 50);
        }
    }
}
