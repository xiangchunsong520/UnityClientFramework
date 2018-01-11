using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public static class Debugger
{
    public static bool hasInit = false;
    public static long frameCount = 0;
    static LogWriter normalLogWriter = null;
    static LogWriter errorLogWriter = null;
    static bool notWriteLog = true;
    static bool isEditor = true;

    public static void Init(string logPath)
    {
        if (hasInit)
            return;

        hasInit = true;
        notWriteLog = false;
        normalLogWriter = new LogWriter(Path.Combine(logPath, "log.txt"));
        errorLogWriter = new LogWriter(Path.Combine(logPath, "error.txt"));
        Application.logMessageReceived += LogCallback;
        Application.logMessageReceivedThreaded += LogCallback;
        isEditor = Application.isEditor;
    }

    static void LogCallback(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                Log(condition, true);
                break;
            case LogType.Assert:
                LogAssertion(condition);
                break;
            case LogType.Warning:
                LogWarning(condition);
                break;
            case LogType.Error:
                LogError(condition);
                break;
            case LogType.Exception:
                LogException(condition);
                break;
        }
    }

    static string AddColor(string message, string color)
    {
        if (isEditor)
        {
            StringBuilder sb = StringBuilderCache.Acquire(256);
            sb.Append("<color=#");
            sb.Append(color);
            sb.Append(">");
            sb.Append(message);
            sb.Append("</color>");
            return StringBuilderCache.GetStringAndRelease(sb);
        }
        else
        {
            return message;
        }
    }

    static void Log(string message, bool write)
    {
        if (notWriteLog)
        {
            if (write)
            {
                Debug.Log(AddColor(message, "00FF00FF"));
            }
            else
            {
                Debug.Log(AddColor(message, "FFFFFFFF"));
            }
        }
        else
        {
            if (write)
            {
                message = GetLogFormat(LogType.Log, message);
                if (normalLogWriter != null)
                {
                    normalLogWriter.Log(message);
                }
            }
        }
    }

    public static void Log(object obj, bool write = false)
    {
        Log(obj != null ? obj.ToString() : "null", write);
    }

    static void LogAssertion(string message)
    {
        if (notWriteLog)
        {
            Debug.LogAssertion(message);
        }
        else
        {
            message = GetLogFormat(LogType.Assert, message);
            if (normalLogWriter != null)
            {
                normalLogWriter.LogAssertion(message);
            }
        }
    }

    public static void LogAssertion(object obj)
    {
        LogAssertion(obj != null ? obj.ToString() : "null");
    }

    static void LogError(string message)
    {
        if (notWriteLog)
        {
            Debug.LogError(message);
        }
        else
        {
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
    }

    public static void LogError(object obj)
    {
        LogError(obj != null ? obj.ToString() : "null");
    }
    
    static void LogException(string message)
    {
        if (notWriteLog)
        {
            Debug.LogException(new Exception(message));
        }
        else
        {
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
    }

    public static void LogException(object obj)
    {
        LogException(obj != null ? obj.ToString() : "null");
    }

    static void LogWarning(string message)
    {
        if (notWriteLog)
        {
            Debug.LogWarning(message);
        }
        else
        {
            message = GetLogFormat(LogType.Warning, message);
            if (normalLogWriter != null)
            {
                normalLogWriter.LogWarning(message);
            }
            if (errorLogWriter != null)
            {
                errorLogWriter.LogWarning(message);
            }
        }
    }

    public static void LogWarning(object obj)
    {
        LogWarning(obj != null ? obj.ToString() : "null");
    }

    static void LogColor(string color, string message, bool write)
    {
        if (notWriteLog)
        {
            Debug.Log(AddColor(message, color));
        }
        else
        {
            Log(message, write);
        }
    }

    public static void LogColor(string color, object obj, bool write = false)
    {
        LogColor(color, obj != null ? obj.ToString() : "null", write);
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