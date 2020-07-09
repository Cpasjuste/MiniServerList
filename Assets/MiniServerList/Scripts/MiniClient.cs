using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class MiniClient : MonoBehaviour {

    public string miniServerAddress = "127.0.0.1";
    public int miniServerPort = 8089;
    public bool testing = false;

    private MiniHostData testHostData;

    private void Start()
    {
        if (testing)
        {
            // get real ip address
            MiniUtility.IpInfo ipInfo = MiniUtility.GetIpInfo();

            testHostData = new MiniHostData
            {
                name = "Test Server",
                comment = "Some usefull data",
                password = "abcdefgh",
                map = "superMap",
                country = ipInfo.country,
                ip = ipInfo.ip,
                port = 8090,
                mapTime = 1000,
                roundTime = 100,
                playerNow = Random.Range(0, 10),
                playerMax = 10,
                version = 1
            };
        }
    }

    private void OnGUI()
    {
        if (testing)
        {
            if (GUI.Button(new Rect(10, 10, 200, 30), "Client: Register/Update"))
            {
                // update playerNow to simulate a host update on server
                testHostData.playerNow = Random.Range(0, 10);

                if (Register(miniServerAddress, miniServerPort, testHostData))
                {
                    Debug.Log("MiniClient: registration success");
                }
                else
                {
                    Debug.LogError("MiniClient: registration failed");
                }
            }

            if (GUI.Button(new Rect(10, 50, 200, 30), "Client: UnRegister"))
            {
                if (UnRegister(miniServerAddress, miniServerPort, testHostData))
                {
                    Debug.Log("MiniClient: unregistration success");
                }
                else
                {
                    Debug.LogError("MiniClient: unregistration failed");
                }
            }

            if (GUI.Button(new Rect(10, 90, 200, 30), "Client: GetServerList"))
            {
                List<MiniHostData> serverList = GetServerList(miniServerAddress, miniServerPort);
                Debug.Log("MiniClient: servers found: " + serverList.Count);
                if (serverList.Count > 0)
                {
                    Debug.Log("MiniClient: server #1: address: " + serverList[0].ip + ", name: "
                        + serverList[0].name + ", players: " + serverList[0].playerNow);
                }
            }
        }
    }

    public static List<MiniHostData> GetServerList(string serverAddress, int serverPort)
    {
        List<MiniHostData> serverList = new List<MiniHostData>();
        TcpClient socket = null;

        try
        {
            socket = new TcpClient(serverAddress, serverPort);

            if (MiniUtility.Write(socket, "list"))
            {
                string s = MiniUtility.Read(socket);
                if (!string.IsNullOrEmpty(s))
                {
                    serverList = (List<MiniHostData>)MiniUtility.Deserialize(s);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::GetServerList: Socket exception: " + socketException);
        }

        if (socket != null && socket.Connected)
        {
            socket.Close();
        }

        return serverList;
    }

    public static bool Register(string serverAddress, int serverPort, MiniHostData hostData)
    {
        TcpClient socket = null;
        bool success = false;

        try
        {
            socket = new TcpClient(serverAddress, serverPort);

            if (MiniUtility.Write(socket, "register"))
            {
                string s = MiniUtility.Serialize(hostData);
                if (!string.IsNullOrEmpty(s))
                {
                    success = MiniUtility.Write(socket, s);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::Register: Socket exception: " + socketException);
        }

        if (socket != null && socket.Connected)
        {
            socket.Close();
        }

        return success;
    }

    public static bool UnRegister(string serverAddress, int serverPort, MiniHostData hostData)
    {
        TcpClient socket = null;
        bool success = false;

        try
        {
            socket = new TcpClient(serverAddress, serverPort);

            if (MiniUtility.Write(socket, "unregister"))
            {
                string s = MiniUtility.Serialize(hostData);
                if (!string.IsNullOrEmpty(s))
                {
                    success = MiniUtility.Write(socket, s);
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniClient::UnRegister: Socket exception: " + socketException);
        }

        if (socket != null && socket.Connected)
        {
            socket.Close();
        }

        return success;
    }
}
