using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public static class Debugger
{
    public static long frameCount = 0;

    static LogWriter normalLogWriter = null;
    static LogWriter errorLogWriter = null;
    static bool isEditor = true;

    public static void SetWriter(LogWriter normal, LogWriter error)
    {
        normalLogWriter = normal;
        errorLogWriter = error;
    }

    public static void Release()
    {
        if (normalLogWriter != null)
        {
            normalLogWriter.Release();
        }
        if (errorLogWriter != null)
        {
            errorLogWriter.Release();
        }
    }

    public static void NotEditor()
    {
        isEditor = false;
    }

    static void Log(string message, bool write)
    {
        if (isEditor)
        {
            if (write)
            {
                StringBuilder sb = StringBuilderCache.Acquire(256);
                sb.Append("<color=#00FF00FF>");
                sb.Append(message);
                sb.Append("</color>");
                Debug.Log(StringBuilderCache.GetStringAndRelease(sb));
            }
            else
            {
                Debug.Log(message);
            }
        }

        if (write)
        {
            message = GetLogFormat(LogType.Log, message);
            if (normalLogWriter != null)
            {
                normalLogWriter.Log(message);
            }
        }
    }

    public static void Log(object obj, bool write = false)
    {
        Log(obj.ToString(), write);
    }

    static void LogAssertion(string message)
    {
        if (isEditor)
        {
            Debug.LogAssertion(message);
        }
        
        message = GetLogFormat(LogType.Assert, message);
        if (normalLogWriter != null)
        {
            normalLogWriter.LogAssertion(message);
        }
    }

    public static void LogAssertion(object obj)
    {
        LogAssertion(obj.ToString());
    }

    static void LogError(string message)
    {
        if (isEditor)
        {
            Debug.LogError(message);
        }
        
        message = GetLogFormat(LogType.Error, message);
        if (normalLogWriter != null)
        {
            normalLogWriter.LogError(message);
        }
        if (errorLogWriter != null)
        {
            errorLogWriter.LogError(message);
        }
    }

    public static void LogError(object obj)
    {
        LogError(obj.ToString());
    }
    
    static void LogException(string message)
    {
        if (isEditor)
        {
            Debug.LogException(new Exception(message));
        }
        
        message = GetLogFormat(LogType.Exception, message);
        if (normalLogWriter != null)
        {
            normalLogWriter.LogException(message);
        }
        if (errorLogWriter != null)
        {
            errorLogWriter.LogException(message);
        }
    }

    public static void LogException(object obj)
    {
        LogException(obj.ToString());
    }
    
    static void LogWarning(string message, bool write)
    {
        if (isEditor)
        {
            if (write)
            {
                StringBuilder sb = StringBuilderCache.Acquire(256);
                sb.Append("<color=#FFFF00FF>");
                sb.Append(message);
                sb.Append("</color>");
                Debug.Log(StringBuilderCache.GetStringAndRelease(sb));
            }
            else
            {
                StringBuilder sb = StringBuilderCache.Acquire(256);
                sb.Append("<color=#909000FF>");
                sb.Append(message);
                sb.Append("</color>");
                Debug.Log(StringBuilderCache.GetStringAndRelease(sb));
            }
        }

        if (write)
        {
            message = GetLogFormat(LogType.Warning, message);
            if (normalLogWriter != null)
            {
                normalLogWriter.LogWarning(message);
            }
        }
    }

    public static void LogWarning(object obj, bool write = false)
    {
        LogWarning(obj.ToString(), write);
    }

    static void LogColor(string color, string message)
    {
        if (isEditor)
        {
            StringBuilder sb = StringBuilderCache.Acquire(256);
            sb.Append("<color=#");
            sb.Append(color);
            sb.Append(">");
            sb.Append(message);
            sb.Append("</color>");
            Debug.Log(StringBuilderCache.GetStringAndRelease(sb));

            message = GetLogFormat(LogType.Log, message);
            if (normalLogWriter != null)
            {
                normalLogWriter.LogWarning(message);
            }
        }
        else
        {
            Log(message, true);
        }
    }

    public static void LogColor(string color, object obj)
    {
        LogColor(color, obj.ToString());
    }

    static string GetLogFormat(LogType logType, string str)
    {
        StringBuilder sb = StringBuilderCache.Acquire(0x100);
        DateTime now = DateTime.Now;
        sb.Append(now.Hour.ToString("00"));
        sb.Append(":");
        sb.Append(now.Minute.ToString("00"));
        sb.Append(":");
        sb.Append(now.Second.ToString("00"));
        sb.Append(".");
        sb.Append(now.Millisecond.ToString("000"));
        sb.Append("-");
        sb.Append(frameCount.ToString("00000000"));
        sb.Append(" ");
        sb.Append(GetTypeStr(logType));
        sb.Append(" ");
        sb.Append(str);
        sb.Append("\n");
        return StringBuilderCache.GetStringAndRelease(sb);
    }

    static string GetTypeStr(LogType logType)
    {
        switch (logType)
        {
            case LogType.Log:
                return "[LOG]      ";
            case LogType.Assert:
                return "[ASSERT]   ";
            case LogType.Error:
                return "[ERROR]    ";
            case LogType.Exception:
                return "[EXCEPTION]";
            case LogType.Warning:
                return "[WARNING]  ";
        }
        return "";
    }
}

public class LogWriter
{
    List<string> logList = new List<string>();
    string writeFile;
    StreamWriter writer = null;

    ManualResetEvent haveDataEvent;
    Thread writeThread = null;

    public LogWriter(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        string path = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        writeFile = fileName;

        haveDataEvent = new ManualResetEvent(false);
        writeThread = new Thread(writeLog);
        writeThread.Start();
    }

    public void Release()
    {
        if (writer != null)
        {
            writer.Close();
        }

        if (haveDataEvent != null)
        {
            haveDataEvent.Close();
        }

        if (writeThread != null)
        {
            writeThread.Abort();
        }
    }
    
    public void Log(string message)
    {
        writeLogToFile(message);
    }
    public void LogAssertion(string message)
    {
        writeLogToFile(message);
    }
    public void LogError(string message)
    {
        writeLogToFile(message);
    }
    public void LogException(string message)
    {
        writeLogToFile(message);
    }
    public void LogWarning(string message)
    {
        writeLogToFile(message);
    }

    void writeLogToFile(string str)
    {
        logList.Add(str);
        haveDataEvent.Set();
    }

    void writeLog()
    {
        while (true)
        {
            if (haveDataEvent.WaitOne())
            {
                haveDataEvent.Reset();

                int num = logList.Count;
                for (int i = 0; i < num; ++i)
                {
                    if (writer == null)
                    {
                        writer = new StreamWriter(writeFile, true, Encoding.Default);
                    }

                    writer.WriteLine(logList[0]);
                    writer.Flush();

                    logList.RemoveAt(0);
                }
            }
        }
    }
}