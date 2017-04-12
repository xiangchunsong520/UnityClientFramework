/*
auth: Xiang ChunSong
purpose:
*/

using System;
using System.IO;

public class FileHelper
{
    public static uint GetCrc(string path)
    {
        if (!File.Exists(path))
        {
            return 0;
        }
        try
        {
            byte[] buffer = File.ReadAllBytes(path);
            return SevenZip.CRC.CalculateDigest(buffer, 0, (uint)buffer.Length);
        }
        catch (System.Exception ex)
        {
            Debugger.LogError(ex.Message);
            return 0;
        }
    }
}
