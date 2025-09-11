using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using APPPC.Control;

namespace APPPC.Functions
{
    public partial class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            InitializeComponent();
            HoTen.Text = Session.CurrentUser.Hoten.ToString();
            button1.BackColor = Color.FromArgb(12, 115, 46);
            chamcong panelView = new chamcong();
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
            chamcong panelView = new chamcong();
            panel.Controls.Add(panelView);
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
            NSLD panelView = new NSLD();
            panel.Controls.Add(panelView);
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
            KHSX_panel panelView = new KHSX_panel();
            panel.Controls.Add(panelView);
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
            YCSX panelView = new YCSX();
            panel.Controls.Add(panelView);
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
            TimKiem panelView = new TimKiem();
            panel.Controls.Add(panelView);
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
            TaiKhoan panelView = new TaiKhoan();
            panel.Controls.Add(panelView);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var login = new loggin();
            Session.CurrentUser = null;
            login.Show();
            this.FormClosing -= MainMenuForm_FormClosing;
            this.Close();
        }

        private void MainMenuForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

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
