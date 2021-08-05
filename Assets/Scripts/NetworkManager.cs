using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections;
using System.IO;

public class NetworkManager : MonoBehaviour
{
    TcpClient TCPclient;
    UdpClient UDPclient;
    TcpListener listener;
    NetworkStream stream;
    IPAddress localIP;
    IPAddress remoteDeviceIP;
    [SerializeField] int remoteDevicePort;
    [SerializeField] int localDevicePort;
    Queue receiveBuffer = new Queue();

    Thread udpSenderThread;

    public TMP_Text awaitText;
    public GameObject menu;
    public GameObject factionPanel;
    public TMP_Dropdown factionDropdown;
    public Animation StartAnimation;
    public AnimationClip animationClip;

    Encoding encoding = Encoding.UTF8;
    
    public delegate void MessageHandler(string message);
    public event MessageHandler ReceivedMessage;

    bool connected;
    public bool isServer;
    public Player.Faction faction;

    [Header("Name Picker")]
    public GameObject namePanel;
    public TMP_InputField nameField;
    public string playerName;

    void Start()
    {
        
        localIP = Dns.GetHostAddresses(Dns.GetHostName()).Where(u => u.AddressFamily == AddressFamily.InterNetwork).ElementAt(0);
        //remoteDevicePort = 5001;
        connected = false;
        DontDestroyOnLoad(this.gameObject);
        StreamReader reader;
        try
        {
            reader = new StreamReader(@$"{Application.persistentDataPath}\playername.txt");
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            return;
        }
        nameField.text = reader.ReadLine();
        reader.Close();
    }
    void Update()
    {
        if(connected && SceneManager.GetActiveScene().name == "MainMenu")
        {
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            // Send("name:" + playerName);
            new Thread(new ThreadStart(SendJunk)).Start();
        }

        while (receiveBuffer.Count > 0)
        {
            try
            {
                ReceivedMessage(receiveBuffer.Dequeue().ToString());
            }
            catch (System.Exception)
            {
                continue;
            }
        }
    }

    private void SendJunk() 
    {
        while (true)
        {
            Send("shfghaskkaghdj:shfajdsghkasjfga");
            Thread.Sleep(500);
        }
    }

    public void StartAsServer()
    {
        Debug.Log("Starting as Server");

        isServer = true;
        udpSenderThread = new Thread(new ThreadStart(UDP_SendConnectionRequests));
        udpSenderThread.Start();

        menu.transform.gameObject.SetActive(false);
        awaitText.text = "Ожидание второго игрока...";
        awaitText.transform.parent.gameObject.SetActive(true);
        StartAnimation.AddClip(animationClip, "StartGame");
        StartAnimation.clip = animationClip;
        StartAnimation.Play();

        listener = new TcpListener(localIP, localDevicePort);
        listener.Start();
        Debug.Log($"Awaiting connection at {localIP}:{localDevicePort}");
        Thread awaitTcp = new Thread(new ThreadStart(AwaitTcpConnection));
        awaitTcp.Start();
    }
    public void ShowFactionMenu(bool asServer)
    {
        factionPanel.SetActive(true);
        if (asServer) 
            isServer = true;
    }
    public void FactionChanged(int fac) => faction = (Player.Faction)factionDropdown.value;

    public void StartGame()
    {
        factionPanel.SetActive(false);
        if (isServer)
            StartAsServer();
        else 
            StartAsClient();
    }
    void AwaitTcpConnection()
    {
        TCPclient = listener.AcceptTcpClient();
        Debug.Log(TCPclient.Client.RemoteEndPoint + " connected");
        udpSenderThread.Abort(); UDPclient.Close();
        listener.Stop();
        StartThreads();

        connected = true;
    }
    public void Disconnect()
    {
        stream?.Close();
        TCPclient?.Close();
        UDPclient?.Close();
        listener?.Stop();

        Application.Quit(0);
    }
    public void StartAsClient()
    {
        Debug.Log("Starting as Client");

        isServer = false;
        menu.transform.gameObject.SetActive(false);
        awaitText.text = "Поиск игры...";
        awaitText.transform.parent.gameObject.SetActive(true);
        StartAnimation.AddClip(animationClip, "StartGame");
        StartAnimation.clip = animationClip;
        StartAnimation.Play();

        new Thread(new ThreadStart(UdpToTcpClient)).Start();
    }

    void StartThreads()
    {
        stream = TCPclient.GetStream();
        Thread receiver = new Thread(new ThreadStart(Receiver));
        receiver.Start();
    }

    private void UdpToTcpClient()
    {
        remoteDeviceIP = UDP_GetRemoteAdress();
        IPEndPoint remoteDevice = new IPEndPoint(remoteDeviceIP, remoteDevicePort);
        TCPclient = new TcpClient();
        TCPclient.Connect(remoteDevice);

        StartThreads();
        connected = true;
    }
    private IPAddress UDP_GetRemoteAdress()
    {
        IPEndPoint receivedIP = null;
        UDPclient = new UdpClient(localDevicePort);
        UDPclient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 3);
        UDPclient.JoinMulticastGroup(IPAddress.Parse("224.5.5.1"));
        UDPclient.Receive(ref receivedIP);
        Debug.Log("GOT IT");
        byte[] data = encoding.GetBytes("Received");
        UDPclient.Send(data, data.Length, receivedIP.Address.ToString(), remoteDevicePort);
        return receivedIP.Address;
    }
    private void UDP_SendConnectionRequests()
    {
        UDPclient = new UdpClient(localDevicePort);
        UDPclient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 3);
        UDPclient.JoinMulticastGroup(IPAddress.Parse("224.5.5.1"));
        byte[] message = encoding.GetBytes(Environment.MachineName);
        while (true)
        {
            UDPclient.Send(message, message.Length, new IPEndPoint(IPAddress.Broadcast, remoteDevicePort));
            Thread.Sleep(3000);
        }
    }

    public void Send(string message)
    {
        byte[] data = encoding.GetBytes(message + ';');
        stream.Write(data, 0, data.Length);
    }
    void Receiver()
    {
        while (true)
        {
            string message = "";
            byte[] data = new byte[256];
            int bytes;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                message += encoding.GetString(data, 0, bytes).Trim();
            }
            while (stream.DataAvailable);
            lock (receiveBuffer)
            {
                if(message.Length == 0)
                    continue;
                if (message.Contains(';'))
                    foreach (string item in message.Split(';'))
                    {
                        if (item.Length != 0)
                            receiveBuffer.Enqueue(item);
                        else continue;
                    }
                else
                    receiveBuffer.Enqueue(message);
            }
        }
    }

    public void LoadDeckbuilder() => SceneManager.LoadScene("DeckBuilder", LoadSceneMode.Single);

    #region NamePicker
    
    public void ShowNamePicker() => namePanel.SetActive(true);
    public void SetName()
    {
        StreamWriter writer = new StreamWriter(@$"{Application.persistentDataPath}\playername.txt", false);
        writer.WriteLine(nameField.text);
        writer.Close();

        namePanel.SetActive(false);
    }

    #endregion
}