using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MiniCommon
{
    private static string k = "84B534DA932C4781CDFBDAEB";

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

    public static void Write(TcpClient socket, string data)
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
            }
        }
        catch (SocketException socketException)
        {
            Debug.LogError("MiniCommon::Write: Socket exception: " + socketException);
        }
    }

    public static string Encrypt(byte[] toEncrypt)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(k);

        // 256-AES key
        byte[] toEncryptArray = toEncrypt;

        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return System.Convert.ToBase64String(resultArray);
    }

    public static byte[] Decrypt(string toDecrypt)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(k);

        // AES-256 key
        byte[] toEncryptArray = System.Convert.FromBase64String(toDecrypt);

        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor();

        return cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    }

    public static string Serialize(object serializableObject)
    {
        if (serializableObject == null)
        {
            Debug.Log("serialize: serializableObject == null");
            return null;
        }

        if (!serializableObject.GetType().IsSerializable)
        {
            Debug.Log("serialize: not serializable");
            return null;
        }

        MemoryStream memoryStream = new MemoryStream();
        new BinaryFormatter().Serialize(memoryStream, serializableObject);

        return Encrypt(memoryStream.ToArray());
    }

    public static object Deserialize(string serializedString)
    {
        if (serializedString == null
            || serializedString == string.Empty
            || serializedString.Equals("null"))
            return null;

        byte[] decrypted = null;
        try
        {
            decrypted = Decrypt(serializedString);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        MemoryStream memoryStream = new MemoryStream(decrypted);
        return new BinaryFormatter().Deserialize(memoryStream);
    }
}
