using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Forms;

//namespace MapPickerApp
//{
//    public partial class MainForm : Form
//    {




//    }
//}

namespace APPPC.Functions
{
    public partial class YCSX : UserControl
    {
        public YCSX()
        {
            InitializeComponent();
        }

        private void YCSX_Load(object sender, EventArgs e)
        {

        }
        private void WebView21_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
        }
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var coordsJson = e.WebMessageAsJson;
            MessageBox.Show("Picked coordinates:\n" + coordsJson, "Location Picked");
            // Optional: Parse JSON and use lat/lng in your app logic
        }

        private void tab1_Click(object sender, EventArgs e)
        {

        }
    }
}
