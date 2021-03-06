﻿/*
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
    static MD5 md5 = MD5.Create();

    public static uint GetFileCrc(string path)
    {
        if (!File.Exists(path))
        {
            //Debugger.LogWarning("The file : " + path + " don't exist!");
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
            //Debugger.LogException(ex);
            return 0;
        }
    }

    public static string GetFileMd5(string path)
    {
        if (!File.Exists(path))
        {
            //Debugger.LogWarning("The file : " + path + " don't exist!");
            return "";
        }

        byte[] buffer = File.ReadAllBytes(path);
        return GetMd5(buffer);
    }

    public static string GetStringMd5(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            //Debugger.LogWarning("The string is empty!");
            return "";
        }

        byte[] buffer = Encoding.Default.GetBytes(str);
        return GetMd5(buffer);
    }

    public static string GetMd5(byte[] buffer)
    {
        try
        {
            return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "").ToLower();
        }
        catch (Exception ex)
        {
            //Debugger.LogException(ex);
            return "";
        }
    }

    public static void CopyFile(string srcFile, string targetFile)
    {
        if (!File.Exists(targetFile))
        {
            File.Copy(srcFile, targetFile);
        }
        else
        {
            if (!GetFileMd5(targetFile).Equals(GetFileMd5(srcFile)))
            {
                File.Copy(srcFile, targetFile, true);
            }
        }
    }

    public static int GetFileSize(string path)
    {
        long size = 0;
        try
        {
            FileInfo fi = new FileInfo(path);
            size = (fi.Length - 1) / 1024 + 1;
        }
        catch (Exception ex)
        {
            //Debugger.LogException(ex);
        }
        return (int)size;
    }
    
    public static void CopyFolder(string sourceFolderName, string destFolderName)
    {
        CopyFolder(sourceFolderName, destFolderName, false);
    }
    
    public static void CopyFolder(string sourceFolderName, string destFolderName, bool overwrite)
    {
        var sourceFilesPath = Directory.GetFileSystemEntries(sourceFolderName);

        for (int i = 0; i < sourceFilesPath.Length; i++)
        {
            var sourceFilePath = sourceFilesPath[i];
            var sourceFileName = Path.GetFileName(sourceFilePath);

            if (File.Exists(sourceFilePath))
            {
                if (!Directory.Exists(destFolderName))
                {
                    Directory.CreateDirectory(destFolderName);
                }
                File.Copy(sourceFilePath, Path.Combine(destFolderName, sourceFileName), overwrite);
            }
            else
            {
                CopyFolder(sourceFilePath, Path.Combine(destFolderName, sourceFileName), overwrite);
            }
        }
    }
}
