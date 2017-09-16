/*
auth: Xiang ChunSong
purpose:
*/

using Google.Protobuf;
using System;
using System.IO;
using Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Base
{
    public class AssertBundleAsyncLoader
    {
        public AssetBundle assetBundle;
        public float progress;
        public DateTime lastUpdateTime;

        public AssertBundleAsyncLoader()
        {
            assetBundle = null;
            progress = 0f;
            lastUpdateTime = DateTime.Now;
        }
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        public static readonly float CodeVersion = 0;    //客户端代码版本号,用于判断版本是否需要升级,只有小数部分不同不需要升级版本,只有整数部分不同才需要升级版本

        static string _dataPath;
        static string _optionalPath;
        static string _streamingPath;
        static string _resourceUrl;

        ResourceDatas _resourceList = null;
        Dictionary<string, List<WeakReference>> _resourceReferences = new Dictionary<string, List<WeakReference>>();
        Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();
        Dictionary<string, List<AssetBundle>> _unreferenceAssetBundles = new Dictionary<string, List<AssetBundle>>();

        public static string DataPath { get { return _dataPath; } }
        public static string OptionalPath { get { return _optionalPath; } }
        public static string StreamingPath { get { return _streamingPath; } }
        public static string ResourceUrl { set { _resourceUrl = value; } get { return _resourceUrl; } }
        public ResourceDatas ResourceList { get { return _resourceList; } }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN && ILRUNTIME_DEBUG
        public static bool IsILRuntimeDebug = false;
#endif

        public ResourceManager()
        {
#if UNITY_EDITOR
            _dataPath = _optionalPath = _streamingPath = Application.dataPath + "/../../Builds/ExportResources/Windows/";
#elif UNITY_STANDALONE_WIN
#if ILRUNTIME_DEBUG
            if (File.Exists(Application.dataPath + "/DebugPath.txt"))
            {
                string[] lines = File.ReadAllLines(Application.dataPath + "/DebugPath.txt");
                _dataPath = _optionalPath = _streamingPath = Application.dataPath + lines[0];
                IsILRuntimeDebug = true;
            }
            else
            {
#endif
            _dataPath = Application.streamingAssetsPath + "/files/";
            _optionalPath = Application.streamingAssetsPath + "/optional/";
            _streamingPath = Application.streamingAssetsPath + "/GameResources/";
#if ILRUNTIME_DEBUG
            }
#endif
#elif UNITY_ANDROID
            _dataPath = Application.persistentDataPath + "/files/";
            _optionalPath = Application.persistentDataPath + "/optional/";
            _streamingPath = "GameResources/";
#elif UNITY_IPHONE
            _dataPath = Application.persistentDataPath + "/files/";
            _optionalPath = Application.persistentDataPath + "/optional/";
            _streamingPath = Application.streamingAssetsPath + "/GameResources/";
#else
            throw new Exception("Not supported platform!");
#endif
            Debugger.Log("dataPath : " + _dataPath, true);
            Debugger.Log("optionalPath : " + _optionalPath, true);
            Debugger.Log("streamingPath : " + _streamingPath, true);
#if UNITY_ANDROID && !UNITY_EDITOR
            Debugger.Log("obbPath : " + GoogleObbPath.GetMainObbPath(), true);
#endif
        }

        public void Init()
        {
#if !UNITY_EDITOR
            bool newVersion = PlayerPrefs.GetFloat("CLIENT_CODE_VERSION", -1) != CodeVersion;
#if UNITY_ANDROID
            int nativeVersionCode = GoogleObbPath.GetApkVerCode();
#elif UNITY_IPHONE
            int nativeVersionCode = IOSVersionCode.GetIOSVersionCode();
#else
            int nativeVersionCode = 0;
#endif

            if (PlayerPrefs.GetInt("NATIVE_VERSION_CODE", -1) != nativeVersionCode)
            {
                newVersion = true;
            }

            Debugger.Log("newVersion : " + newVersion, true);
            if (newVersion)
            {
                if (!GameClient.Instance.BuildSettings.MiniBuild)
                {
                    if (Directory.Exists(_dataPath))
                        Directory.Delete(_dataPath, true);
                    Directory.CreateDirectory(_dataPath);
            
                    CopyStreamingFile("ResourceList.ab", _dataPath + "ResourceList.ab");
                }
                else
                {
                    if (!File.Exists(_dataPath + "ResourceList.ab"))
                    {
                        if (Directory.Exists(_dataPath))
                            Directory.Delete(_dataPath, true);
                        Directory.CreateDirectory(_dataPath);

                        CopyStreamingFile("ResourceList.ab", _dataPath + "ResourceList.ab");
                    }
                    else
                    {
#if UNITY_ANDROID
                        ResourceDatas newDatas = LoadResourceDatas(GetStreamingFile("ResourceList.ab"));
#else
                        ResourceDatas newDatas = LoadResourceDatas(_streamingPath + "ResourceList.ab");
#endif
                        if (newDatas != null)
                        {
                            ResourceDatas datas = LoadResourceDatas(_dataPath + "ResourceList.ab");
                            var e = newDatas.Resources.GetEnumerator();
                            while (e.MoveNext())
                            {
                                string filename = _dataPath + GetResourceFileName(e.Current.Key);
                                if (File.Exists(filename))
                                    File.Delete(filename);

                                if (datas != null)
                                {
                                    if (!datas.Resources.ContainsKey(e.Current.Key))
                                        datas.Resources.Add(e.Current.Key, e.Current.Value);
                                    else
                                        datas.Resources[e.Current.Key] = e.Current.Value;
                                }
                            }
            
                            if (datas != null)
                                SaveResourceDatas(_dataPath + "ResourceList.ab", datas);
                        }
                    }
                }
            
                PlayerPrefs.SetFloat("CLIENT_CODE_VERSION", CodeVersion);
                PlayerPrefs.SetInt("NATIVE_VERSION_CODE", nativeVersionCode);
                PlayerPrefs.Save();
            }
#endif

            LoadResourceList();
        }

        public void LoadResourceList()
        {
#if UNITY_EDITOR
#if RECOURCE_CLIENT
            _resourceList = LoadResourceDatas(_dataPath + "_ResourceList_1.ab");
#else
            _resourceList = LoadResourceDatas(_dataPath + "_ResourceList_2.ab");
#endif
#else
#if ILRUNTIME_DEBUG && UNITY_STANDALONE_WIN
            if (ResourceManager.IsILRuntimeDebug)
            {
                _resourceList = LoadResourceDatas(_dataPath + "_ResourceList.ab");
            }
            else
            {
#endif
            _resourceList = LoadResourceDatas(_dataPath + "ResourceList.ab");
#if ILRUNTIME_DEBUG && UNITY_STANDALONE_WIN
            }
#endif
            if (_resourceList == null || _resourceList.Resources.Count == 0)
            {
                CopyStreamingFile("ResourceList.ab", _dataPath + "ResourceList.ab");
                _resourceList = LoadResourceDatas(_dataPath + "ResourceList.ab");
            }
#endif

            if (_resourceList != null)
                Debugger.Log("Resource count : " + _resourceList.Resources.Count, true);
#if !UNITY_EDITOR
            else
                Debugger.LogError("ResourceList load fail!!!");
#endif
        }

        void CopyStreamingFile(string file, string dirFile)
        {
#if !UNITY_EDITOR
            string dirPath = Path.GetDirectoryName(dirFile);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
#if UNITY_ANDROID
            Stream stream = GetStreamingFile(file);
            if (stream != null)
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                File.WriteAllBytes(dirFile, bytes);
            }
#else
            File.Copy(_streamingPath + file, dirFile, true);
#endif
#endif
        }

        public string GetResourceKey(string name)
        {
            if (_resourceList != null)
            {
                string key = FileHelper.GetStringMd5(name.ToLower());
                if (_resourceList.Resources.ContainsKey(key))
                    return key;
            }

            return null;
        }

        public ResourceData GetResourceData(string key)
        {
            if (_resourceList != null)
            {
                if (_resourceList.Resources.ContainsKey(key))
                    return _resourceList.Resources[key];
            }

            return null;
        }

        public AssetBundle LoadAssetBundle(string key, string root = null)
        {
            if (_resourceList == null)
                return null;

            if (!_resourceList.Resources.ContainsKey(key))
                return null;

            if (string.IsNullOrEmpty(root))
            {
                root = key;
            }
            
            AssetBundle asset = null;
            if (_loadedAssetBundles.ContainsKey(key))
            {
                asset = _loadedAssetBundles[key];

                if (asset != null)
                {
                    return asset;
                }
            }

            ResourceData rd = _resourceList.Resources[key];
            for (int i = 0; i < rd.Depends.Count; ++i)
            {
                LoadAssetBundle(rd.Depends[i], root);
            }

            string filename = GetResourceFileName(key);
            string assetFile = "";
            bool isStreaming = false;
            if (rd.IsOptional())
            {
                assetFile = _optionalPath + filename;
            }
            else
            {
                assetFile = _dataPath + filename;
                if (!File.Exists(assetFile))
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    assetFile = filename;
#else
                    assetFile = _streamingPath + filename;
#endif
                    isStreaming = true;
                }
            }

            if (asset == null)
            {
                asset = LoadAssetBundleFile(assetFile, isStreaming);
            }

            if (asset == null)
            {
                Debugger.LogError("Load AssetBundle : " + key + " fail!");
                Debugger.LogError("Resource path : " + rd.Path);
            }
            else
            {
                if (rd.Reference > 0)
                {
                    AddLoadedAssetBundle(key, asset);
                }
                else
                {
                    AddUnreferenceAssetBundle(root, asset);
                }
            }

            return asset;
        }

        public IEnumerator LoadAssetBundleAsync(string key, AssertBundleAsyncLoader asyncLoader)
        {
            asyncLoader.progress = 0;

            if (_resourceList == null)
                yield break;

            if (!_resourceList.Resources.ContainsKey(key))
                yield break;

            AssetBundle asset = null;
            if (_loadedAssetBundles.ContainsKey(key))
            {
                asset = _loadedAssetBundles[key];

                if (asset != null)
                {
                    asyncLoader.assetBundle = asset;

                    yield break;
                }
            }

            ResourceData rd = _resourceList.Resources[key];
            float childRate = 1 / (rd.Depends.Count + 1);
            int loadCount = 0;
            for (int i = 0; i < rd.Depends.Count; ++i)
            {
                LoadAssetBundle(rd.Depends[i], key);
                asyncLoader.progress = (++loadCount) * childRate;
                TimeSpan ts = DateTime.Now - asyncLoader.lastUpdateTime;
                if (ts.TotalSeconds > 0.3d)
                {
                    asyncLoader.lastUpdateTime = DateTime.Now;
                    yield return 0;
                }
            }

            string filename = GetResourceFileName(key);
            string assetFile = "";
            bool isStreaming = false;
            if (rd.IsOptional())
            {
                assetFile = _optionalPath + filename;
            }
            else
            {
                assetFile = _dataPath + filename;
                if (!File.Exists(assetFile))
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    assetFile = filename;
#else
                    assetFile = _streamingPath + filename;
#endif
                    isStreaming = true;
                }
            }

            if (asset == null)
            {
                asset = LoadAssetBundleFile(assetFile, isStreaming);
            }

            if (asset == null)
            {
                Debugger.LogError("Load AssetBundle : " + key + " fail! 2");
                Debugger.LogError("Resource path : " + rd.Path);
            }
            else
            {
                AddUnreferenceAssetBundle(key, asset);
                asyncLoader.assetBundle = asset;
            }

            asyncLoader.progress = 1;
        }

        AssetBundle LoadAssetBundleFile(string assetFile, bool isStreaming)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (isStreaming)
            {
                string path = Application.dataPath + "!assets/" + _streamingPath + assetFile;
                if (ObbAssetLoad.ExistsFile(_streamingPath + assetFile))
                {
                    path = GoogleObbPath.GetMainObbPath() + "/" + _streamingPath + assetFile;
                }

                AssetBundle ab = null;
                try
                {
                    ab = AssetBundle.LoadFromFile(path);
                }
                catch (System.Exception ex)
                {
                    Debugger.LogException(ex);
                }
                if (ab != null)
                {
                    return ab;
                }
                Debugger.LogError("Read StreamingAssets : " + assetFile + " fail!");
            }
#endif
            if (File.Exists(assetFile))
            {
                AssetBundle ab = null;
                try
                {
                    ab = AssetBundle.LoadFromFile(assetFile);
                }
                catch (System.Exception ex)
                {
                    Debugger.LogException(ex);
                }
                if (ab == null)
                {
                    Debugger.LogError("The file : " + assetFile + " error!!");
                    try
                    {
                        File.Delete(assetFile);
                        PlayerPrefs.SetInt("CHECK_RESOURCE", 0);
                    }
                    catch (Exception ex)
                    {
                        Debugger.LogException(ex);
                    }
                }
                return ab;
            }
            PlayerPrefs.SetInt("CHECK_RESOURCE", 0);
            Debugger.LogError("The file : " + assetFile + " not exist!!");
            return null;
        }

        public string GetUnpackageResPath(string name, ref bool isStreaming)
        {
            isStreaming = false;
            string key = GetResourceKey(name);
            if (!string.IsNullOrEmpty(key))
            {
                string fileName = GetResourceFileName(key);
                ResourceData rd = _resourceList.Resources[key];
                if (!rd.IsUnpackage())
                {
                    Debugger.LogError(name + " is not Unpackage resource!! 1");
                    return null;
                }

                if (rd.IsOptional())
                    return _optionalPath + fileName;

                if (File.Exists(_dataPath + fileName))
                    return _dataPath + fileName;

                isStreaming = true;
#if UNITY_ANDROID && !UNITY_EDITOR
                return fileName;
#else
                return _streamingPath + fileName;
#endif
            }
#if UNITY_EDITOR
            else
            {
                if (!name.Contains("Unpackage"))
                {
                    Debugger.LogError(name + " is not Unpackage resource!!");
                    return null;
                }
                return Application.dataPath + "/Resources/" + name;
            }
#else
            return null;
#endif
        }

        public List<DownloadFile> GetOptionalNeedDownladList(string key)
        {
            List<DownloadFile> list = new List<DownloadFile>();
            ResourceData rd = GetResourceData(key);
            if (rd != null && rd.IsOptional())
            {
                string saveName = _optionalPath + GetResourceFileName(key);
                if (FileHelper.GetFileCrc(saveName) != rd.Crc)
                {
                    list.Add(new DownloadFile(_resourceUrl + key + ".ab", saveName, rd.Size, rd.Crc));
                }

                for (int i = 0; i < rd.Depends.Count; ++i)
                {
                    list.AddRange(GetOptionalNeedDownladList(rd.Depends[i]));
                }
            }
            return list;
        }

        public void AddResourceReference(string key, UnityEngine.Object obj)
        {
            if (!_resourceReferences.ContainsKey(key))
                _resourceReferences.Add(key, new List<WeakReference>());

            _resourceReferences[key].Add(new WeakReference(obj));
        }

        public void AddResourcesReference(string key, UnityEngine.Object[] objs)
        {
            for (int i = 0; i < objs.Length; ++i)
            {
                AddResourceReference(key, objs[i]);
            }
        }

        public T GetReferenceResource<T>(string key) where T : UnityEngine.Object
        {
            T[] objs = GetReferenceResources<T>(key);
            if (objs != null && objs.Length > 0)
                return objs[0];

            return null;
        }

        public T[] GetReferenceResources<T>(string key) where T : UnityEngine.Object
        {
            if (!_resourceReferences.ContainsKey(key))
                return null;

            List<T> list = new List<T>();
            for (int i = 0; i < _resourceReferences[key].Count; ++i)
            {
                object obj = _resourceReferences[key][i].Target;
                if (obj != null)
                {
                    if (obj.GetType() == typeof(T))
                        list.Add(obj as T);
                }
            }
            return list.ToArray();
        }

        public UnityEngine.Object GetReferenceResource(string key, Type type)
        {
            UnityEngine.Object[] objs = GetReferenceResources(key, type);
            if (objs != null && objs.Length > 0)
                return objs[0];

            return null;
        }

        public UnityEngine.Object[] GetReferenceResources(string key, Type type)
        {
            if (!_resourceReferences.ContainsKey(key))
                return null;

            List<UnityEngine.Object> list = new List<UnityEngine.Object>();
            for (int i = 0; i < _resourceReferences[key].Count; ++i)
            {
                UnityEngine.Object obj = _resourceReferences[key][i].Target as UnityEngine.Object;
                if (obj != null)
                {
                    if (obj.GetType() == type)
                        list.Add(obj);
                }
            }
            return list.ToArray();
        }

        public UnityEngine.Object[] GetReferenceResources(string key)
        {
            if (!_resourceReferences.ContainsKey(key))
                return null;

            List<UnityEngine.Object> list = new List<UnityEngine.Object>();
            for (int i = 0; i < _resourceReferences[key].Count; ++i)
            {
                UnityEngine.Object obj = _resourceReferences[key][i].Target as UnityEngine.Object;
                if (obj != null)
                {
                    list.Add(obj);
                }
            }
            return list.ToArray();
        }

        bool IsResourceReference(string key)
        {
            if (!_resourceReferences.ContainsKey(key))
                return false;

            for (int i = 0; i < _resourceReferences[key].Count; ++i)
            {
                if (_resourceReferences[key][i].Target != null)
                    return true;
            }

            _resourceReferences.Remove(key);
            return false;
        }

        public void AddLoadedAssetBundle(string key, AssetBundle asset)
        {
            if (!_loadedAssetBundles.ContainsKey(key))
                _loadedAssetBundles.Add(key, asset);
            else
                _loadedAssetBundles[key] = asset;
        }

        public void ClearLoadedAssetBundles()
        {
            var e = _loadedAssetBundles.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value)
                    e.Current.Value.Unload(false);
            }

            _loadedAssetBundles.Clear();
        }

        public void AddUnreferenceAssetBundle(string key, AssetBundle asset)
        {
            if (!_unreferenceAssetBundles.ContainsKey(key))
                _unreferenceAssetBundles.Add(key, new List<AssetBundle>());

            _unreferenceAssetBundles[key].Add(asset);
        }

        public void RemoveUnreferenceAssetBundle(string key)
        {
            if (_unreferenceAssetBundles.ContainsKey(key))
            {
                for (int i = 0; i < _unreferenceAssetBundles[key].Count; ++i)
                {
                    if (_unreferenceAssetBundles[key][i] != null)
                    {
                        _unreferenceAssetBundles[key][i].Unload(false);
                    }
                }

                _unreferenceAssetBundles[key].Clear();
                _unreferenceAssetBundles.Remove(key);
            }
        }

        public static void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        public static string GetResourceFileName(string key)
        {
#if UNITY_EDITOR
            return key + ".ab";
#else
#if ILRUNTIME_DEBUG && UNITY_STANDALONE_WIN
            if (ResourceManager.IsILRuntimeDebug)
            {
                return key + ".ab";
            }
#endif
            return key.Substring(0, 2) + "/" + key + ".ab";
#endif
        }

        public static Stream GetStreamingFile(string file)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            file = _streamingPath + file;
            if (ObbAssetLoad.ExistsFile(file))
            {
                return ObbAssetLoad.GetFile(file);
            }

            return StreamingAssetLoad.GetFile(file);
#endif
            throw new NotImplementedException();
        }

        public static ResourceDatas LoadResourceDatas(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                ResourceDatas rds = LoadResourceDatas(fs);
                fs.Close();
                return rds;
            }
            catch (Exception ex)
            {
                Debugger.LogException(ex);
                return null;
            }
        }

        public static ResourceDatas LoadResourceDatas(Stream stream)
        {
            try
            {
                return ResourceDatas.Parser.ParseFrom(stream);
            }
            catch (Exception ex)
            {
                Debugger.LogException(ex);
                return null;
            }
        }

        static void SaveResourceDatas(string path, ResourceDatas datas)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            datas.WriteTo(fs);
            fs.Flush();
            fs.Close();
        }
    }
}
