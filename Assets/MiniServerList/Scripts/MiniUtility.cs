using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MiniUtility {

    private static string k = "94B534EA831C3672CAFBDAEC";

    [Serializable]
    public class IpInfo {

        public string ip;
        public string hostname;
        public string city;
        public string region;
        public string country;
        public string loc;
        public string org;
        public string postal;
        public string timezone;
        public string readme;
    }

    public static IpInfo GetIpInfo()
    {
        IpInfo ipInfo = new IpInfo();

        try
        {
            // 5 secondes timeout
            string s = new MiniWebClient(5).DownloadString("http://ipinfo.iod/json");
            ipInfo = JsonUtility.FromJson<IpInfo>(s);
            return ipInfo;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("MiniIpInfo::GetIpInfo: failed to get ip information from ipinfo.io: " + ex.ToString());
        }

        try
        {
            Debug.Log("MiniIpInfo::GetIpInfo: trying to get external ip address from icanhazip.com");
            // 5 secondes timeout
            string s = new MiniWebClient(5).DownloadString("http://icanhazip.com/");
            ipInfo.ip = s;
            return ipInfo;
        }
        catch (Exception ex)
        {
            Debug.LogError("MiniIpInfo::GetIpInfo: failed to get ip information from icanhazip.com: " + ex.ToString());
        }

        return ipInfo;
    }

    public static string Read(TcpClient socket)
    {
        Debug.Log("MiniCommon::Read: address: " + socket.Client.RemoteEndPoint.ToString());

        string message = string.Empty;

        try
        {
            NetworkStream stream = socket.GetStream();
            if (stream.CanRead)
            {
                int len = 0;

                // receive message length
                Byte[] bytes = new Byte[4];
                len = stream.Read(bytes, 0, bytes.Length);
                if (len != 4)
                {
                    Debug.LogError("MiniCommon::Read: couldn't read message size");
                    return message;
                }
                uint size = BitConverter.ToUInt32(bytes, 0);

                // receive message
                bytes = new Byte[size];
                len = stream.Read(bytes, 0, bytes.Length);
                if (len != size)
                {
                    Debug.LogError("MiniCommon::Read: couldn't read message");
                    return message;
                }
                message = Encoding.ASCII.GetString(bytes);
            }
            else
            {
                Debug.LogError("MiniCommon::Read: can't read from stream");
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniCommon::Read: Socket exception: " + socketException);
        }

        return message;
    }

    public static bool Write(TcpClient socket, string data)
    {
        Debug.Log("MiniCommon::Write: address: " + socket.Client.RemoteEndPoint.ToString());

        try
        {
            NetworkStream stream = socket.GetStream();
            if (stream.CanWrite)
            {
                // first send message length
                Byte[] bytes = Encoding.ASCII.GetBytes(data);
                Byte[] size = BitConverter.GetBytes(bytes.Length);
                stream.Write(size, 0, size.Length);
                // now send data
                stream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                Debug.LogError("MiniCommon::Write: can't write to stream");
                return false;
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniCommon::Write: Socket exception: " + socketException);
            return false;
        }

        return true;
    }

    public static string Serialize(object o)
    {
        if (o == null || !o.GetType().IsSerializable)
        {
            return null;
        }

        MemoryStream memoryStream = new MemoryStream();
        new BinaryFormatter().Serialize(memoryStream, o);

        return Encrypt(memoryStream.ToArray());
    }

    public static object Deserialize(string s)
    {
        byte[] decrypted = null;

        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        try
        {
            decrypted = Decrypt(s);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return null;
        }

        MemoryStream memoryStream = new MemoryStream(decrypted);
        return new BinaryFormatter().Deserialize(memoryStream);
    }

    private static string Encrypt(byte[] toEncrypt)
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(k);

        // 256-AES key
        byte[] toEncryptArray = toEncrypt;

        RijndaelManaged rDel = new RijndaelManaged
        {
            Key = keyArray,
            Mode = CipherMode.ECB,
            // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
            Padding = PaddingMode.PKCS7
        };

        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray);
    }

    private static byte[] Decrypt(string toDecrypt)
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(k);

        // AES-256 key
        byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

        RijndaelManaged rDel = new RijndaelManaged
        {
            Key = keyArray,
            Mode = CipherMode.ECB,
            // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
            Padding = PaddingMode.PKCS7
        };

        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor();

        return cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }
}
