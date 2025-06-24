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
            button1.BackColor = Color.FromArgb(74, 33, 109);
            chamcong panelView = new chamcong();
            panel.Controls.Add(panelView);
            this.Size = new Size(1600, 900);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            // Disable maximize and resize
            this.MaximizeBox = false;

        }


        private void button1_Click(object sender, EventArgs e)
        {

            button1.BackColor = Color.FromArgb(74, 33, 109);
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

            button2.BackColor = Color.FromArgb(74, 33, 109);
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

            button3.BackColor = Color.FromArgb(74, 33, 109);
            button2.BackColor = Color.Transparent;
            button1.BackColor = Color.Transparent;
            button4.BackColor = Color.Transparent;
            button5.BackColor = Color.Transparent;
            button6.BackColor = Color.Transparent;
            panel.Controls.Clear();
            KHSX panelView = new KHSX();
            panel.Controls.Add(panelView);
        }

        private void button4_Click(object sender, EventArgs e)
        {

            button4.BackColor = Color.FromArgb(74, 33, 109);
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
            button5.BackColor = Color.FromArgb(74, 33, 109);
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

            button6.BackColor = Color.FromArgb(74, 33, 109);
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
            this.Close();
        }

        private void HoTen_Click(object sender, EventArgs e)
        {

        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {

        }
    }
}
