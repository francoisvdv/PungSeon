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

namespace QQServer
{
    public partial class MainForm : Form
    {
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
        }
    }
}
