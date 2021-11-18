/*
 * Konstantinos Gkinis <aka raidenfreeman>
 * https://github.com/raidenfreeman/ICE-Experiment
 * 
 * A C# ICE attempt with UDP Hole punching, using a simple UDP P2P Chat as an example (2019).
 */

using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using GKNet;
using GKNet.STUN;
using LumiSoft.Net.STUN.Client;

namespace GKCommunicatorApp
{
    public partial class ICEExperimentForm : Form
    {
        private delegate void PrintOutputCallback(string text);
        private delegate void ChatAppendCallback(string text, bool isIncoming);

        private const int portNumber = 43788;
        private byte[] byteBuffer = new byte[1024];
        private Socket sk;
        private IPEndPoint targetEndpoint;

        public ICEExperimentForm()
        {
            InitializeComponent();
        }

        private void btnSTUNDetect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            startServer.Visible = false;
            chatInput.Visible = false;
            chatOutput.Visible = false;
            try {
                STUN_Result result = STUNUtility.Detect(portNumber);

                PrintOutput("NAT Type: " + result.NetType.ToString());
                if (result.NetType != STUN_NetType.UdpBlocked) {
                    PrintOutput("Public Endpoint: " + result.PublicEndPoint.ToString());
                    idText.Text = NetHelper.Base64Encode(result.PublicEndPoint.ToString());
                }
            } catch (Exception x) {
                MessageBox.Show(this, "Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                Cursor = Cursors.Default;
            }
        }

        private void PrintOutput(string s)
        {
            if (outputText.InvokeRequired) {
                PrintOutputCallback d = new PrintOutputCallback(PrintOutput);
                Invoke(d, new object[] { s });
            } else {
                outputText.AppendText(s + Environment.NewLine);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(idText.Text);
        }

        private void ChatAppend(string text, bool isIncoming)
        {
            if (chatOutput.InvokeRequired) {
                ChatAppendCallback d = new ChatAppendCallback(ChatAppend);
                Invoke(d, new object[] { text, isIncoming });
            } else {
                text += Environment.NewLine;
                if (isIncoming) {
                    text += Environment.NewLine;
                }

                chatOutput.SelectionStart = chatOutput.TextLength;
                chatOutput.SelectionLength = 0;

                chatOutput.SelectionColor = isIncoming ? Color.Salmon : Color.SpringGreen;
                chatOutput.SelectionAlignment = HorizontalAlignment.Right;
                chatOutput.AppendText(text);
                chatOutput.SelectionColor = chatOutput.ForeColor;
            }
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            string strEndpoint = NetHelper.Base64Decode(idInput.Text);
            var targetIP = IPAddress.Parse(strEndpoint.Split(':')[0]);
            var targetPort = Convert.ToInt32(strEndpoint.Split(':')[1]);
            targetEndpoint = new IPEndPoint(targetIP, targetPort);

            if (sk != null) {
                sk.Close();
            }

            sk = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try {
                // Bind the socket to the local endpoint and listen for incoming connections
                sk.Bind(new IPEndPoint(IPAddress.Any, portNumber));

                PrintOutput($"Started server at port: {portNumber}");

                // target: 85.72.40.234:43788  base64=> ODUuNzIuNDAuMjM0OjQzNzg4
                PrintOutput($"Hole punching...");
                // Punchthrough
                SendHolePunch(sk, targetEndpoint);
                PrintOutput($"Done");

                // TODO: CHANGE THIS TO TARGET ENDPOINT
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                // The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;
                // Start receiving data
                sk.BeginReceiveFrom(byteBuffer, 0, byteBuffer.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);

                PrintOutput($"Listenning for data...");
                chatInput.Visible = true;
                chatOutput.Visible = true;
            } catch (Exception ex) {
                PrintOutput("Something bad happened");
                PrintOutput(ex.Message);
                Console.WriteLine(ex.ToString());
                sk.Close();
                PrintOutput("Connection closed");
                startServer.Visible = false;
                chatInput.Visible = false;
                chatOutput.Visible = false;
            }
        }

        /// <summary>
        /// Send a few packets to the target, to punch a hole in our NAT.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="endPoint"></param>
        public static void SendHolePunch(Socket socket, IPEndPoint endPoint)
        {
            if (socket == null)
                return;

            byte[] bytes = Encoding.ASCII.GetBytes("holepunch");
            for (int i = 0; i < 10; i++) {
                socket.SendTo(bytes, endPoint);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            EndPoint epSender = (EndPoint)targetEndpoint;
            try {
                sk.EndReceiveFrom(ar, ref epSender);

                var msgReceived = Encoding.ASCII.GetString(byteBuffer);

                ChatAppend(msgReceived, true);
                PrintOutput("Got: " + msgReceived);
                byteBuffer = new byte[1024];

                // Start listening to receive more data from the user
                sk.BeginReceiveFrom(byteBuffer, 0, byteBuffer.Length, SocketFlags.None, ref epSender,
                                           new AsyncCallback(OnReceive), null);
            } catch (Exception ex) {
                PrintOutput(ex.Message);
                sk.Close();
                PrintOutput("Connection closed");
            }
        }

        private void idInput_TextChanged(object sender, EventArgs e)
        {
            var target = NetHelper.Base64Decode(idInput.Text);
            var isMatch = NetHelper.IsValidIpAddress(target);

            startServer.Visible = isMatch;
            chatInput.Visible = false;
            chatOutput.Visible = false;

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sk != null) {
                sk.Close();
            }
        }

        private void chatInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter)) {
                var textToSend = chatInput.Text;
                if (textToSend != string.Empty) {
                    if (sk != null) {
                        try {
                            byte[] byteData = Encoding.ASCII.GetBytes(chatInput.Text);

                            // Send it to the target
                            sk.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, targetEndpoint, new AsyncCallback(OnSend), null);
                        } catch (Exception) {
                            PrintOutput("Unable to send message to the target.");
                        }
                    }
                }
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try {
                sk.EndSend(ar);
                PrintOutput($"Sent: {chatInput.Text}");
                ChatAppend(chatInput.Text, false);
            } catch (ObjectDisposedException) { } catch (Exception ex) {
                PrintOutput(ex.Message);
            }
            if (InvokeRequired) {
                Invoke(new Action(() => chatInput.Clear()));
                return;
            }
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            idInput.Text = Clipboard.GetText();
        }
    }
}
