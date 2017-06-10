using System.Net;
using System.ComponentModel;
using System;
using System.Threading;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Base
{
    public class WebClientEx : WebClient
    {
        int _timeout;
        int _readWriteTimeout;
        public WebClientEx(int timeout, int readWriteTimeout)
        {
            _timeout = timeout;
            _readWriteTimeout = readWriteTimeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = 1000 * _timeout;
            request.ReadWriteTimeout = 1000 * _readWriteTimeout;
            return request;
        }
    }

    public class WebDownloader
    {
        WebClientEx _webclient;

        public bool finish = false;
        public string error = null;
        public float process = 0f;

        static int _flag = 0;
        int _id;
        object[] _parms = new object[2];

        public WebDownloader()
        {
            try
            {
                ServicePointManager.DefaultConnectionLimit = 512;
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
                finish = false;
                error = null;
                process = 0f;
                _id = _flag++;
                Debugger.Log("WebDownloader : " + _id + " start");
            }
            catch (System.Exception ex)
            {
                Debugger.LogError(ex);
            }
        }

        static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void DownloadFile(string url, string savePath)
        {
            try
            {
                _webclient = new WebClientEx(30 * 60, 60);
                _webclient.DownloadProgressChanged += DownloadProgressChanged;
                _webclient.DownloadFileCompleted += DownloadFileCompleted;
                _webclient.DownloadDataCompleted += DownloadDataCompleted;
                if (_webclient.IsBusy)
                    _webclient.CancelAsync();
                finish = false;
                error = null;
                process = 0f;
                _webclient.DownloadFileAsync(new Uri(url), savePath + ".temp", savePath);
            }
            catch (System.Exception ex)
            {
                Debugger.LogError(ex);
            }
        }


        public void DownloadData(string url, string savePath, Action<object> callback)
        {
            try
            {
                _webclient = new WebClientEx(30 * 60, 60);
                _webclient.DownloadProgressChanged += DownloadProgressChanged;
                _webclient.DownloadFileCompleted += DownloadFileCompleted;
                _webclient.DownloadDataCompleted += DownloadDataCompleted;
                if (_webclient.IsBusy)
                    _webclient.CancelAsync();
                finish = false;
                error = null;
                process = 0f;
                _parms[0] = savePath;
                _parms[1] = callback;
                _webclient.DownloadDataAsync(new Uri(url), _parms);
            }
            catch (System.Exception ex)
            {
                Debugger.LogError(ex);
            }
        }

        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            float p = e.ProgressPercentage / 100f;
            if (p > process)
            {
                process = p;
            }
        }

        void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                Thread.Sleep(1);
                Close(true);
                string savePath = e.UserState as string;
                string tempPath = savePath + ".temp";
                if (e.Error != null)
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                    error = e.Error.ToString();
                }
                else
                {
                    if (File.Exists(savePath))
                        File.Delete(savePath);
                    File.Move(tempPath, savePath);
                }
            }
            catch (System.Exception ex)
            {
                error = ex.ToString();
            }
            finish = true;
        }

        void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                Thread.Sleep(1);
                Close(true);
                if (e.Error != null)
                {
                    error = e.Error.ToString();
                }
                else
                {
                    object[] userdata = e.UserState as object[];
                    if (userdata != null && userdata.Length == 2 && userdata[0] is string & userdata[1] is Action<object>)
                    {
                        string savepath = userdata[0] as string;
                        Action<object> callback = userdata[1] as Action<object>;
                        _parms[0] = e.Result;
                        _parms[1] = savepath;
                        callback(_parms);
                    }
                    else
                    {
                        error = "download data fail! the userdata is error!";
                    }
                }
            }
            catch (System.Exception ex)
            {
                error = ex.ToString();
            }
            finish = true;
        }

        public void Close(bool byself = false)
        {
            if (!byself)
                Debugger.Log("WebDownloader : " + _id + " finish");
            if (_webclient != null)
            {
                _webclient.CancelAsync();
                _webclient.Dispose();
                _webclient = null;
            }
        }
    }
}