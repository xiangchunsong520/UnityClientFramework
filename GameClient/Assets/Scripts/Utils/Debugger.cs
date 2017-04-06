using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public static class Debugger
{
    private static LogWriter normalLogWriter = null;
    private static LogWriter errorLogWriter = null;

    public static void SetWriter(LogWriter normal, LogWriter error)
    {
        normalLogWriter = normal;
        errorLogWriter = error;
    }

    public static void Release()
    {
        if (normalLogWriter != null)
            normalLogWriter.Release();
        if (errorLogWriter != null)
            errorLogWriter.Release();
    }

    public static void Log(string message)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Debug.Log(message);
#endif
        message = GetLogFormat(LogType.Log, message);
        if (normalLogWriter != null)
            normalLogWriter.Log(message);
    }

    public static void Log(object obj)
    {
        Log(obj.ToString());
    }

    public static void Log(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    public static void LogFormat(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    public static void LogAssertion(string message)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Debug.LogAssertion(message);
#endif
        message = GetLogFormat(LogType.Assert, message);
        if (normalLogWriter != null)
            normalLogWriter.LogAssertion(message);
    }

    public static void LogAssertion(object obj)
    {
        LogAssertion(obj.ToString());
    }

    public static void LogAssertion(string format, params object[] args)
    {
        LogAssertion(string.Format(format, args));
    }

    public static void LogAssertionFormat(string format, params object[] args)
    {
        LogAssertion(string.Format(format, args));
    }

    public static void LogError(string message)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Debug.LogError(message);
#endif
        message = GetLogFormat(LogType.Error, message);
        if (normalLogWriter != null)
            normalLogWriter.LogError(message);
        if (errorLogWriter != null)
            errorLogWriter.LogError(message);
    }

    public static void LogError(object obj)
    {
        LogError(obj.ToString());
    }

    public static void LogError(string format, params object[] args)
    {
        LogError(string.Format(format, args));
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        LogError(string.Format(format, args));
    }

    public static void LogException(Exception exception)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Debug.LogException(exception);
#endif
        string message = GetLogFormat(LogType.Exception, exception.ToString());
        if (normalLogWriter != null)
            normalLogWriter.LogException(message);
        if (errorLogWriter != null)
            errorLogWriter.LogException(message);
    }

    public static void LogException(string message)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Debug.LogException(new Exception(message));
#endif
        message = GetLogFormat(LogType.Exception, message);
        if (normalLogWriter != null)
            normalLogWriter.LogException(message);
        if (errorLogWriter != null)
            errorLogWriter.LogException(message);
    }

    public static void LogWarning(string message)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Debug.LogWarning(message);
#endif
        message = GetLogFormat(LogType.Warning, message);
        if (normalLogWriter != null)
            normalLogWriter.LogWarning(message);
    }

    public static void LogWarning(object obj)
    {
        LogWarning(obj.ToString());
    }

    public static void LogWarning(string format, params object[] args)
    {
        LogWarning(string.Format(format, args));
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        LogWarning(string.Format(format, args));
    }

    public static void LogColor(string color, string message)
    {
        StringBuilder sb = StringBuilderCache.Acquire(256);
#if UNITY_EDITOR
        sb.Append("<color=");
        sb.Append(color);
        sb.Append(">");
        sb.Append(message);
        sb.Append("</color>");
        Log(StringBuilderCache.GetStringAndRelease(sb));
#else
        Log(message);
#endif
    }

    private static string GetLogFormat(LogType logType, string str)
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
        /*sb.Append("-");
        sb.Append(Time.frameCount);*/
        sb.Append(" ");
        sb.Append(GetTypeStr(logType));
        sb.Append(" ");
        sb.Append(str);
        sb.Append("\n");
        return StringBuilderCache.GetStringAndRelease(sb);
    }

    private static string GetTypeStr(LogType logType)
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
    private List<string> logList = new List<string>();
    private string writeFile;
    private StreamWriter writer = null;

    private ManualResetEvent haveDataEvent;
    private Thread writeThread = null;

    public LogWriter(string fileName)
    {
        fileName = fileName.Replace("\\", "/");
        string path = fileName.Substring(0, fileName.LastIndexOf("/"));
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        if (File.Exists(fileName))
            File.Delete(fileName);
        writeFile = fileName;

        haveDataEvent = new ManualResetEvent(false);
        writeThread = new Thread(writeLog);
        writeThread.Start();
    }

    public void Release()
    {
        if (writer != null)
            writer.Close();

        if (writeThread != null)
            writeThread.Abort();
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
                        writer = new StreamWriter(writeFile, true, Encoding.Default);
                    
                    writer.WriteLine(logList[0]);
                    writer.Flush();

                    logList.RemoveAt(0);
                }
            }
        }
    }
}