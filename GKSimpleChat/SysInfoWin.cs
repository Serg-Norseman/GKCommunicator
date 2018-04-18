using System;
using System.Windows.Forms;
using GKNet.Core;

namespace GKSimpleChat
{
    public partial class SysInfoWin : Form
    {
        public SysInfoWin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text += "UserName: " + SysHelper.GetUserName() + "\r\n";
            textBox1.Text += "UserCountry: " + SysHelper.GetUserCountry() + "\r\n";
            textBox1.Text += "TimeZone: " + SysHelper.GetTimeZone() + "\r\n";
            textBox1.Text += "Languages: " + SysHelper.GetLanguages() + "\r\n";
        }
    }
}
