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

        public MainForm()
        {
            InitializeComponent();

            Server.Instance.Client.OnLog = x => Console.WriteLine(x);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tbServerIp.Text = Client.GetLocalIPAddress().ToString();
            RegisterOnWeb();

            StartServer();
        }

        void RegisterOnWeb()
        {
            Console.WriteLine("Registering IP with web server...");
            HttpWebRequest w = HttpWebRequest.CreateHttp("http://iamde.co.de/pungseon.php?set=" + Client.GetLocalIPAddress().ToString());
            HttpWebResponse r = null;
            try
            {
                r = w.GetResponse() as HttpWebResponse;
                Console.WriteLine("Done: " + r.StatusCode + " - " + r.StatusDescription);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }

        void StartServer()
        {
            Server.Instance.Start();

            Action a = () =>
                {
                    while (!stop)
                    {
                        Thread.Sleep(1000 / 10);
                        Server.Instance.Client.Update();
                    }
                };
            serverThread = new Thread(new ThreadStart(a));
            serverThread.Start();
        }
        void StopServer()
        {
            stop = true;

            Server.Instance.Stop();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopServer();
        }
    }
}
