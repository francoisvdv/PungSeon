using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
            StartServer();
        }

        void StartServer()
        {
            Client.Instance.SetMode(Client.Mode.ClientServer);
            Client.Instance.StartTcpListener();

            Client.Instance.AddListener(server);
        }
    }
}
