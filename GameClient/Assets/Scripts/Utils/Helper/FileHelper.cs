/*
auth: Xiang ChunSong
purpose:
*/

using SevenZip;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class FileHelper
{
    public static uint GetFileCrc(string path)
    {
        if (!File.Exists(path))
        {
            Debugger.LogWarning("The file : " + path + " don't exist!");
            return 0;
        }

        byte[] buffer = File.ReadAllBytes(path);
        return GetCrc(buffer);
    }

    public static uint GetCrc(byte[] buffer)
    {
        try
        {
            return CRC.CalculateDigest(buffer, 0, (uint)buffer.Length);
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
            return 0;
        }
    }

    public static string GetFileMd5(string path)
    {
        if (!File.Exists(path))
        {
            Debugger.LogWarning("The file : " + path + " don't exist!");
            return "";
        }

        byte[] buffer = File.ReadAllBytes(path);
        return GetMd5(buffer);
    }

    public static string GetStringMd5(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            Debugger.LogWarning("The string is empty!");
            return "";
        }

        byte[] buffer = Encoding.Default.GetBytes(str);
        return GetMd5(buffer);
    }

    public static string GetMd5(byte[] buffer)
    {
        try
        {
            MD5 md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "").ToLower();
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
            return "";
        }
    }
}
