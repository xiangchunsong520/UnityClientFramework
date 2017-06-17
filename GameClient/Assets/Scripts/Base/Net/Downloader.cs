using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;

namespace Base
{
    public class DownloadFile
    {
        public DownloadFile(string url, string savePath, int size = 100, uint crc = 0, bool uncompress = false)
        {
            this.url = url;
            this.savePath = savePath;
            this.size = size;
            this.crc = crc;
            this.uncompress = uncompress;
            string folader = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(folader))
                Directory.CreateDirectory(folader);
        }

        public string url;
        public int size;
        public string savePath;
        public bool uncompress;
        public uint crc;
    }

    public class Downloader : MonoBehaviour
    {
        static int maxDownloadCnt = 10;

        static object threadLock = new object();
        private List<DownloadFile> _files;
        private Action<object> _onFinish;
        private Action<object> _onProgress;
        private Action<object> _onSingleFinish;
        private object[] _tempArgs;
        private object[] _progressParam = new object[3];
        private int _totalSize = 0;
        private int _downloadSize = 0;
        private float _lastProgress;
        private bool _downloading = true;
        
        List<WebDownloader> downloaders = new List<WebDownloader>();
        List<int> downloadindexs = new List<int>();
        List<int> tryTimes = new List<int>();
        bool downloadfinish = false;
        float childProgress = 0;

        Thread thread = null;

        public int TotoalSize
        {
            get
            { return _totalSize; }
        }

        public static Downloader DowloadFiles(List<DownloadFile> files, Action<object> onFinish, Action<object> onProgress = null, Action<object> onSingleFinish = null, params object[] tempParams)
        {
            if (files == null || files.Count == 0)
            {
                onFinish(tempParams);
                return null;
            }

            GameObject go = new GameObject("ResourceDownloader");
            Downloader wd = go.AddComponent<Downloader>();
            wd.DownloadFiles(files, onFinish, onProgress, onSingleFinish, tempParams);
            return wd;
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (!downloadfinish && thread != null)
                thread.Abort();

            for (int i = 0; i < downloadindexs.Count; ++i)
            {
                downloaders[i].Close();
            }
            downloaders.Clear();
        }

        public void DownloadFiles(List<DownloadFile> files, Action<object> onFinish, Action<object> onProgress, Action<object> onSingleFinish, object[] args)
        {
            if (files.Count == 0)
            {
                onFinish(args);
                return;
            }

            _files = files;
            _onFinish = onFinish;
            _onProgress = onProgress;
            _onSingleFinish = onSingleFinish;
            _tempArgs = args;
            _totalSize = 0;
            for (int i = 0; i < files.Count; ++i)
            {
                _totalSize += files[i].size;
            }
            _downloadSize = 0;
            _lastProgress = 0f;

            if (_onProgress != null)
            {
                _progressParam[0] = 0f;
                _progressParam[1] = _totalSize;
                _progressParam[2] = _tempArgs;
                _onProgress(_progressParam);
            }

            thread = new Thread(DownloadFiles);
            thread.Start();
            StartCoroutine(UpdateProgress());
        }

        public void StopDownload()
        {
            Debugger.Log("StopDownload");
            _onFinish(_tempArgs);
            DestroyImmediate(gameObject);
        }

        public void PauseDownload(bool pause)
        {
            Debugger.Log("PauseDownload : " + pause);
            _downloading = !pause;
        }

        void DownloadFiles()
        {
            int index = 0;
            int downloadcnt = _files.Count > maxDownloadCnt ? maxDownloadCnt : _files.Count;
            for (int i = 0; i < downloadcnt; ++i)
            {
                WebDownloader wd = new WebDownloader();
                if (!_files[index].uncompress)
                    wd.DownloadFile(_files[index].url, _files[index].savePath);
                else
                    wd.DownloadData(_files[index].url, _files[index].savePath, DownloadDataCallBack);
                downloaders.Add(wd);
                downloadindexs.Add(index++);
                tryTimes.Add(0);
            }

            float downloadingProgress = 0;
            while (index < _files.Count || downloaders.Count > 0)
            {
                if (_downloading)
                {
                    downloadingProgress = 0;

                    for (int i = 0; i < downloaders.Count;)
                    {
                        try
                        {
                            WebDownloader wd = downloaders[i];
                            int idx = downloadindexs[i];

                            if (wd.finish)
                            {
                                bool finish = false;
                                bool downloaded = false;
                                if (string.IsNullOrEmpty(wd.error))
                                {
                                    uint crc = 0;
                                    if (_files[idx].crc != 0)
                                    {
                                        crc = FileHelper.GetFileCrc(_files[idx].savePath);
                                    }

                                    if (crc == _files[idx].crc)
                                    {
                                        if (_onSingleFinish != null)
                                        {
                                            _onSingleFinish(_files[idx]);
                                        }
                                        _downloadSize += _files[idx].size;
                                        finish = true;
                                        downloaded = true;
                                    }
                                    else
                                    {
                                        Debugger.LogError("the downloaded file's crc not match!");
                                        File.Delete(_files[idx].savePath);
                                    }
                                }
                                else
                                {
                                    Debugger.LogError("Error:" + wd.error);
                                }

                                if (!downloaded)
                                {
                                    if (tryTimes[i] >= 2)
                                    {
                                        if (_onSingleFinish != null)
                                        {
                                            _onSingleFinish(_files[idx]);
                                        }
                                        Debugger.LogError("download file : " + _files[idx].url + " fail!");
                                        _downloadSize += _files[idx].size;
                                        finish = true;
                                    }
                                    else
                                    {
                                        Debugger.LogError("retry download file : " + _files[idx].url);
                                        if (!_files[idx].uncompress)
                                            wd.DownloadFile(_files[idx].url, _files[idx].savePath);
                                        else
                                            wd.DownloadData(_files[idx].url, _files[idx].savePath, DownloadDataCallBack);
                                        ++tryTimes[i];
                                    }
                                }

                                if (finish)
                                {
                                    if (index < _files.Count)
                                    {
                                        if (!_files[index].uncompress)
                                            wd.DownloadFile(_files[index].url, _files[index].savePath);
                                        else
                                            wd.DownloadData(_files[index].url, _files[index].savePath, DownloadDataCallBack);
                                        downloadindexs[i] = index++;
                                        tryTimes[i] = 0;
                                    }
                                    else
                                    {
                                        wd.Close();
                                        downloaders.RemoveAt(i);
                                        downloadindexs.RemoveAt(i);
                                        tryTimes.RemoveAt(i);
                                        Debugger.Log("downloaders.Count : " + downloaders.Count);
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                float scale = _files[idx].size / _totalSize;
                                downloadingProgress += scale * wd.process;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debugger.LogError("DownloadFiles");
                            Debugger.LogError(ex);
                        }
                        ++i;
                    }

                    childProgress = downloadingProgress;
                }
                Thread.Sleep(1);
            }

            downloadfinish = true;
        }

        void DownloadDataCallBack(object obj)
        {
            if (obj == null || !(obj is object[]))
                return;

            object[] pars = obj as object[];
            if (pars != null && pars.Length == 2 && pars[0] is byte[] && pars[1] is string)
            {
                byte[] buffer = pars[0] as byte[];
                string savepath = pars[1] as string;
                FileStream output = null;
                lock (threadLock)
                {
                    output = new FileStream(savepath, FileMode.Create);
                }

                if (!SevenZipHelper.DecompressFile(buffer, output))
                    Debugger.LogError("DecompressFile file : " + savepath + " fail!");
            }
        }

        IEnumerator UpdateProgress()
        {
            while (!downloadfinish)
            {
                if (_onProgress != null)
                {
                    float progress = (float)_downloadSize / (float)_totalSize;
                    progress += childProgress;
                    if (progress < _lastProgress)
                        progress = _lastProgress;
                    _lastProgress = progress;
                    _progressParam[0] = progress;
                    _onProgress(_progressParam);
                }
                yield return 0;
            }

            yield return 0;

            if (downloadfinish)
            {
                if (_onProgress != null)
                {
                    _progressParam[0] = 1f;
                    _onProgress(_progressParam);
                    yield return 0;
                }
                _onFinish(_tempArgs);
                DestroyImmediate(gameObject);
            }
        }
    }
}