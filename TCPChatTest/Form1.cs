using System;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace TCPChatTest
{
    public partial class Form1 : Form
    {
        private TCPDuplexClient fClient;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            fClient = new TCPDuplexClient();
            fClient.DataReceive += RaiseDataReceive;
            fClient.Start(int.Parse(txtLocalPort.Text));

            txtLocalPort.Enabled = false;
            btnConnect.Enabled = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(txtRemoteAddress.Text), int.Parse(txtRemotePort.Text));
            fClient.Send(endPoint, txtMsg.Text);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //fServer.Disconnect();
        }

        public void RaiseDataReceive(object sender, DataReceiveEventArgs e)
        {
            Invoke((MethodInvoker)delegate {
                txtLog.Text += Encoding.UTF8.GetString(e.Data) + "\r\n";
            });
        }
    }
}
