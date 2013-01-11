using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace QQServer
{
    public partial class MainForm : Form
    {
        volatile bool stop = false;
        Thread serverThread;
        Server server = new Server();

        public MainForm()
        {
            InitializeComponent();

            Client.Instance.OnLog = x => Console.WriteLine(x);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            tbServerIp.Text = localIP.ToString();

            StartServer();
        }

        void StartServer()
        {
            Client.Instance.SetMode(Client.Mode.ClientServer);
            Client.Instance.StartConnectionListener(4551);

            Client.Instance.AddListener(server);

            Action a = () =>
                {
                    while (!stop)
                    {
                        Thread.Sleep(1000 / 10);
                        Client.Instance.Update();
                    }
                };
            serverThread = new Thread(new ThreadStart(a));
            serverThread.Start();
        }
        void StopServer()
        {
            stop = true;
        }
    }
}
