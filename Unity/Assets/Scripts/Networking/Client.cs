using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client : IDisposable, INetworkListener
{
    class RemoteClient
    {
        public string username;
        public byte[] readBuffer;
        public StringBuilder receiveBuffer = new StringBuilder();
    }
    public enum Mode { ClientServer, ClientClient }

    public Action<string> OnLog;
    public bool VerboseDebugMessages = false;
    public bool ConnectBack = false;
    public List<DataPackageFactory> Factories = new List<DataPackageFactory>();

    TcpListener tcpListener;
    Dictionary<TcpClient, RemoteClient> clients = new Dictionary<TcpClient, RemoteClient>(); //string contains the client's 'username'
    List<TcpClient> outgoing = new List<TcpClient>();

    Queue<DataPackage> queue = new Queue<DataPackage>();
    List<WeakReference> networkListeners = new List<WeakReference>();

    List<DataPackage> receiveBuffer = new List<DataPackage>();

    string username;
    bool hasToken = false;
    TcpClient nextClient = null;

    Encoding encoding = Encoding.UTF8;



    public Client()
    {
        username = GenerateUniqueUsername();
        SetMode(Mode.ClientServer);

        AddListener(this);
    }
    public void Dispose()
    {
        StopConnectionListener();
        foreach (var v in clients)
        {
            v.Key.Close();
        }
    }

    public static string GenerateUniqueUsername()
    {
        return "John Doe " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + "." + DateTime.Now.Millisecond;
    }
    public static IPAddress GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress localIP = null;
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localIP = ip;
        }
        return localIP;
    }

    public void SetMode(Mode m)
    {
        Factories.Clear();

        if (m == Mode.ClientClient)
        {
            ConnectBack = false;
            Factories.Add(TokenChangePackage.factory);
            Factories.Add(ChatMessagePackage.factory);
            Factories.Add(PlayerMovePackage.factory);
            Factories.Add(BaseCapturePackage.factory);
        }
        else if (m == Mode.ClientServer)
        {
            ConnectBack = true;
            Factories.Add(CreateLobbyPackage.factory);
            Factories.Add(JoinLobbyPackage.factory);
            Factories.Add(LobbyUpdatePackage.factory);
            Factories.Add(PlayerReadyPackage.factory);
            Factories.Add(RequestHighscorePackage.factory);
            Factories.Add(RequestLobbyListPackage.factory);
            Factories.Add(ResponsePackage.factory);
            Factories.Add(SetHighscorePackage.factory);
        }
    }
    public void SetHasToken(bool hasToken)
    {
        this.hasToken = hasToken;
    }
    public void SetNextTokenClient(TcpClient nextClient)
    {
        this.nextClient = nextClient;
    }

    public DataPackageFactory GetFactory(int id)
    {
        foreach (DataPackageFactory dpf in Factories)
        {
            if (dpf.Id == id)
                return dpf;
        }
        return null;
    }

    public void AddListener(INetworkListener l)
    {
        networkListeners.Add(new WeakReference(l));
    }
    public void RemoveListener(INetworkListener l)
    {
        for (int i = 0; i < networkListeners.Count; i++)
        {
            INetworkListener nl = networkListeners[i].Target as INetworkListener;
            if (nl != l)
                continue;

            networkListeners.RemoveAt(i);
            i--;
        }
    }

    public void StartConnectionListener(int port = 4550)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(ConnectCallback, tcpListener);

        OnLog("Started TCP listening for incoming connections");
    }
    public void StopConnectionListener()
    {
        if (tcpListener != null)
            tcpListener.Stop();

        OnLog("Stopped TCP listening for incoming connections");
    }

    void ConnectCallback(IAsyncResult iar)
    {
        TcpListener l = (TcpListener)iar.AsyncState;
        TcpClient c;
        try
        {
            c = l.EndAcceptTcpClient(iar);
            l.BeginAcceptTcpClient(new AsyncCallback(ConnectCallback), l);
        }
        catch (SocketException ex)
        {
            if (OnLog != null)
                OnLog("Error accepting TCP connection: " + ex.Message);

            return;
        }
        catch (ObjectDisposedException)
        {
            // The listener was Stop()'d, disposing the underlying socket and
            // triggering the completion of the callback. We're already exiting,
            // so just return.
            if (OnLog != null)
                OnLog("Listen canceled.");

            return;
        }

        if (c != null && c.Connected)
        {
            AddClient(c, true);

            if (OnLog != null)
            {
                OnLog("Connected (incoming): " + c.GetRemoteIPEndPoint().ToString() +
                    " | Total connections: " + clients.Count);
            }
        }
    }
    void ReceiveCallback(IAsyncResult iar)
    {
        TcpClient c = (TcpClient)iar.AsyncState;
        RemoteClient rc = clients[c];

        int read;
        NetworkStream networkStream;
        try
        {
            networkStream = c.GetStream();
            read = networkStream.EndRead(iar);
        }
        catch (Exception exc)
        {
            if (OnLog != null)
                OnLog(exc.ToString());

            //An error has occured when reading
            return;
        }

        if (read == 0)
            OnLog("Read 0 bytes!");

        //TCP is a protocol that might split messages. We store our incoming message chunks and split on newlines.
        string chunk = encoding.GetString(rc.readBuffer, 0, read);
        if (VerboseDebugMessages && OnLog != null)
            OnLog("Received from " + c.GetRemoteIPEndPoint().ToString() + ": " + chunk);

        rc.receiveBuffer.Append(chunk);
        for (int i = 0; i < rc.receiveBuffer.Length; i++)
        {
            char character = rc.receiveBuffer[i];
            if (character == Options.NewLineChar)
            {
                string data = rc.receiveBuffer.ToString(0, i);
                rc.receiveBuffer.Remove(0, i + 1);
                i = -1;

                if (!string.IsNullOrEmpty(data))
                    ReceiveData(c, data);
            }
        }

        if (VerboseDebugMessages && rc.receiveBuffer.Length != 0 && OnLog != null)
            OnLog("ReceiveBuffer not empty");

        networkStream.BeginRead(rc.readBuffer, 0, rc.readBuffer.Length, ReceiveCallback, c);
    }
    void WriteCallback(IAsyncResult iar)
    {
        TcpClient client = (TcpClient)iar.AsyncState;

        NetworkStream networkStream = client.GetStream();
        networkStream.EndWrite(iar);
    }

    void AddClient(TcpClient c, bool receive)
    {
        RemoteClient rc = new RemoteClient();
        rc.username = GenerateUniqueUsername();
        rc.readBuffer = new byte[c.ReceiveBufferSize];
        lock (clients)
        {
            clients.Add(c, rc);
        }

        if(receive || ConnectBack)
            c.GetStream().BeginRead(rc.readBuffer, 0, rc.readBuffer.Length, ReceiveCallback, c);
    }
    public int GetConnectionCount()
    {
        return clients.Count;
    }
    public List<IPAddress> GetOutgoingAddresses()
    {
        List<IPAddress> r = new List<IPAddress>();
        foreach (var v in outgoing)
        {
            r.Add(v.GetRemoteIPEndPoint().Address);
        }
        return r;
    }

    public void Write(TcpClient client, DataPackage data)
    {
        Write(client, data.ToString() + Options.NewLineChar);
    }
    public void Write(TcpClient client, string data)
    {
        byte[] bytes = encoding.GetBytes(data);
        Write(client, bytes);

        if (VerboseDebugMessages && OnLog != null)
            OnLog("Written to " + client.GetRemoteIPEndPoint().ToString() + ":" + data);
    }
    public void Write(TcpClient client, byte[] data)
    {
        NetworkStream networkStream = client.GetStream();
        networkStream.BeginWrite(data, 0, data.Length, WriteCallback, client);
    }
    public void WriteAll(DataPackage data)
    {
        foreach (var kvp in clients)
        {
            Write(kvp.Key, data);
        }
    }
    public void WriteAll(string data)
    {
        foreach (var kvp in clients)
        {
            Write(kvp.Key, data);
        }
    }
    public void WriteAll(byte[] data)
    {
        foreach (var kvp in clients)
        {
            Write(kvp.Key, data);
        }
    }

    public TcpClient Connect(string ip, int port = 4550)
    {
        return Connect(IPAddress.Parse(ip), port);
    }
    public TcpClient Connect(IPAddress ip, int port = 4550)
    {
        TcpClient tcpClient = new TcpClient();
        tcpClient.Connect(ip, port);
        AddClient(tcpClient, false);

        outgoing.Add(tcpClient);

        if (OnLog != null)
        {
            OnLog("Connected (outgoing): " + tcpClient.GetRemoteIPEndPoint().ToString() +
                " | Total connections: " + clients.Count);
        }

        return tcpClient;
    }
    public void SendData(DataPackage dp)
    {
        queue.Enqueue(dp);
    }
    void ReceiveData(TcpClient sender, string data)
    {
        DataPackage dp = DataPackage.FromString(this, data);
        dp.SenderTcpClient = sender;
        lock (receiveBuffer)
        {
            receiveBuffer.Add(dp);
        }
    }

    public void Update()
    {
        //process received messages
        {
            List<DataPackage> received = null;
            lock (receiveBuffer)
            {
                received = new List<DataPackage>(receiveBuffer);
                receiveBuffer.Clear();
            }

            for (int i = 0; i < networkListeners.Count; i++)
            {
                INetworkListener nl = networkListeners[i].Target as INetworkListener;
                if (nl == null)
                {
                    networkListeners.RemoveAt(i);
                    i--;
                }
                else
                {
                    foreach (DataPackage dp in received)
                    {
                        nl.OnDataReceived(dp);
                    }
                }
            }
        }

        if (!hasToken)
            return;

        while (queue.Count != 0)
        {
            DataPackage dp = queue.Dequeue();
            WriteAll(dp);
        }

        hasToken = false;
        Write(nextClient, new TokenChangePackage());
    }

    public void OnDataReceived(DataPackage dp)
    {
        TokenChangePackage tcp = dp as TokenChangePackage;
        if (tcp == null)
            return;

        hasToken = true;

        if (VerboseDebugMessages && OnLog != null)
            OnLog("I (" + dp.SenderLocalIPEndpoint.ToString() + ") received the token from " + dp.SenderRemoteIPEndpoint.ToString());
    }
}