﻿/*
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
        public AssetBundle assetBundle = null;
        public float progress = 0f;
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        public static readonly int CodeVersion = 0;    //客户端代码版本号,用于判断版本是否需要升级

        string _dataPath;
        string _optionalPath;
        string _streamingPath;
        string _resourceUrl;

        ResourceDatas _resourceList = null;
        Dictionary<string, List<WeakReference>> _resourceReferences = new Dictionary<string, List<WeakReference>>();
        Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();

        public ResourceManager()
        {
#if UNITY_EDITOR
            _dataPath = _optionalPath = _streamingPath = Application.dataPath + "/../../Builds/ExportResources/Windows/";
#elif UNITY_STANDALONE_WIN
            _dataPath = Application.streamingAssetsPath + "/files/";
            _optionalPath = Application.streamingAssetsPath + "/optional/";
            _streamingPath = Application.streamingAssetsPath + "/GameResources/";
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
#if UNITY_ANDROID
            Debugger.Log("obbPath : " + GoogleObbPath.GetMainObbPath(), true);
#endif
        }

        public void Init()
        {
#if !UNITY_EDITOR
            bool newVersion = PlayerPrefs.GetInt("CLIENT_CODE_VERSION", -1) != CodeVersion;
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

                PlayerPrefs.SetInt("CLIENT_CODE_VERSION", CodeVersion);
            }
#endif

            LoadResourceList();
        }

        public void AfterInit()
        {
#if UNITY_EDITOR
            _resourceUrl = ILRuntimeHelper.GetResourceUrl() + "Windows/";
#elif UNITY_STANDALONE_WIN
            _resourceUrl = ILRuntimeHelper.GetResourceUrl() + "Windows/";
#elif UNITY_ANDROID
            _resourceUrl = ILRuntimeHelper.GetResourceUrl() + "Android/";
#elif UNITY_IPHONE
            _resourceUrl = ILRuntimeHelper.GetResourceUrl() + "IOS/";
#else
            throw new Exception("Not supported platform!");
#endif
            Debugger.Log(_resourceUrl, true);
        }

        public void LoadResourceList()
        {
#if UNITY_EDITOR
            _resourceList = LoadResourceDatas(_dataPath + "_ResourceList_2.ab");
#else
            _resourceList = LoadResourceDatas(_dataPath + "ResourceList.ab");

            if (_resourceList == null)
            {
                CopyStreamingFile("ResourceList.ab", _dataPath + "ResourceList.ab");
            }
#endif
            //Debugger.Log(_resourceList.Resourcenames.Count);
            if (_resourceList != null)
                Debugger.Log(_resourceList.Resources.Count);
        }

        void CopyStreamingFile(string file, string dirFile)
        {
#if !UNITY_EDITOR
            string dirPath = Path.GetDirectoryName(dirFile);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
#if UNITY_ANDROID
            Stream stream = GetStreamingFile(_streamingPath + file);
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
            string key = FileHelper.GetStringMd5(name.ToLower());
            if (_resourceList.Resources.ContainsKey(key))
                return key;

            return null;
        }

        public AssetBundle LoadAssetBundle(string key, int depth = 0)
        {
            ResourceData rd = _resourceList.Resources[key];
            for (int i = 0; i < rd.Depends.Count; ++i)
            {
                LoadAssetBundle(rd.Depends[i], depth + 1);
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
                    assetFile = _streamingPath + filename;
                    isStreaming = true;
                }
            }
            AssetBundle asset = null;
            if (_loadedAssetBundles.ContainsKey(key))
            {
                asset = _loadedAssetBundles[key];
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
                AddLoadedAssetBundle(key, asset);
                if (depth > 0)
                {
                    UnityEngine.Object[] objs = asset.LoadAllAssets();
                    if (objs != null && objs.Length != 0)
                    {
                        AddResourcesReference(key, objs);
                    }
                }
            }

            return asset;
        }

        public IEnumerator LoadAssetBundleAsync(string key, AssertBundleAsyncLoader progress, int depth = 0)
        {
            yield return 0;
        }

        AssetBundle LoadAssetBundleFile(string assetFile, bool isStreaming)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (isStreaming)
            {
                Stream stream = GetStreamingFile(assetFile);
                if (stream != null)
                {
                    AssetBundle ab = null;
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    try
                    {
                        ab = AssetBundle.LoadFromMemory(bytes);
                    }
                    catch (Exception ex)
                    {
                        Debugger.LogException(ex);
                    }
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

        void AddResourceReference(string key, object obj)
        {
            List<WeakReference> list = new List<WeakReference>();
            list.Add(new WeakReference(obj));
            if (!_resourceReferences.ContainsKey(key))
                _resourceReferences.Add(key, list);
            else
                _resourceReferences[key] = list;
        }

        void AddResourcesReference(string key, UnityEngine.Object[] objs)
        {
            List<WeakReference> list = new List<WeakReference>();
            for (int i = 0; i < objs.Length; ++i)
            {
                list.Add(new WeakReference(objs[i]));
            }
            if (!_resourceReferences.ContainsKey(key))
                _resourceReferences.Add(key, list);
            else
                _resourceReferences[key] = list;
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
            return false;
        }

        void AddLoadedAssetBundle(string key, AssetBundle asset)
        {
            if (!_loadedAssetBundles.ContainsKey(key))
                _loadedAssetBundles.Add(key, asset);
            else
                _loadedAssetBundles[key] = asset;
        }

        public static string GetResourceFileName(string key)
        {
            return key.Substring(0, 2) + "/" + key + ".ab";
        }
        
        public static Stream GetStreamingFile(string file)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
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
                FileStream fs = new FileStream(path, FileMode.Open);
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

        static ResourceDatas LoadResourceDatas(Stream stream)
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
