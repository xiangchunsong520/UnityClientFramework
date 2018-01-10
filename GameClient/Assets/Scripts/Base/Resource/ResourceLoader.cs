/*
auth: Xiang ChunSong
purpose:
*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Base
{
    public class ResourceLoader
    {
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (rd.IsUnpackage() || rd.IsOptional())
                {
                    Debugger.LogError("The source : " + path + " is not normal resource!");
                    return null;
                }

                T obj = ResourceManager.Instance.GetReferenceResource<T>(key);
                if (obj != null)
                {
                    return obj;
                }

                AssetBundle asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    string assetPaht = "Assets/Resources/" + path;
                    obj = asset.LoadAsset<T>(assetPaht);
                    if (obj != null)
                    {
                        ResourceManager.Instance.AddResourceReference(key, obj);
                    }
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    return obj;
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
            }

            if (path.Contains("Unpackage") || path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not normal resource!");
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.Load<T>(path);
        }

        public static T[] LoadAll<T>(string path) where T : UnityEngine.Object
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (rd.IsUnpackage() || rd.IsOptional())
                {
                    Debugger.LogError("The source : " + path + " is not normal resource!");
                    return null;
                }

                T[] objs = ResourceManager.Instance.GetReferenceResources<T>(key);
                if (objs != null && objs.Length > 0)
                {
                    return objs;
                }

                AssetBundle asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    objs = asset.LoadAllAssets<T>();
                    if (objs != null && objs.Length > 0)
                    {
                        ResourceManager.Instance.AddResourcesReference(key, objs);
                    }
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    return objs;
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
            }

            if (path.Contains("Unpackage") || path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not normal resource!");
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.LoadAll<T>(path);
        }

        public static UnityEngine.Object Load(string path, Type type)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (rd.IsUnpackage() || rd.IsOptional())
                {
                    Debugger.LogError("The source : " + path + " is not normal resource!");
                    return null;
                }

                UnityEngine.Object obj = ResourceManager.Instance.GetReferenceResource(key, type);
                if (obj != null)
                {
                    return obj;
                }

                AssetBundle asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    string assetPaht = "Assets/Resources/" + path;
                    obj = asset.LoadAsset(assetPaht, type);
                    if (obj != null)
                    {
                        ResourceManager.Instance.AddResourceReference(key, obj);
                    }
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    return obj;
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
            }

            if (path.Contains("Unpackage") || path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not normal resource!");
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.Load(path, type);
        }

        public static UnityEngine.Object[] LoadAll(string path, Type type)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (rd.IsUnpackage() || rd.IsOptional())
                {
                    Debugger.LogError("The source : " + path + " is not normal resource!");
                    return null;
                }

                UnityEngine.Object[] objs = ResourceManager.Instance.GetReferenceResources(key, type);
                if (objs != null && objs.Length > 0)
                {
                    return objs;
                }

                AssetBundle asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    objs = asset.LoadAllAssets(type);
                    if (objs != null && objs.Length > 0)
                    {
                        ResourceManager.Instance.AddResourcesReference(key, objs);
                    }
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    return objs;
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
            }

            if (path.Contains("Unpackage") || path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not normal resource!");
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.LoadAll(path, type);
        }

        public static UnityEngine.Object[] LoadAll(string path)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (rd.IsUnpackage() || rd.IsOptional())
                {
                    Debugger.LogError("The source : " + path + " is not normal resource!");
                    return null;
                }

                UnityEngine.Object[] objs = ResourceManager.Instance.GetReferenceResources(key);
                if (objs != null && objs.Length > 0)
                {
                    return objs;
                }

                AssetBundle asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    objs = asset.LoadAllAssets();
                    if (objs != null && objs.Length > 0)
                    {
                        ResourceManager.Instance.AddResourcesReference(key, objs);
                    }
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    return objs;
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
            }

            if (path.Contains("Unpackage") || path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not normal resource!");
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.LoadAll(path);
        }

        public static Stream LoadUnpackageResStream(string path)
        {
            bool isStreaming = false;
            string resPath = ResourceManager.Instance.GetUnpackageResPath(path, ref isStreaming);
            if (string.IsNullOrEmpty(resPath))
            {
                return null;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (isStreaming)
            {
                return ResourceManager.GetStreamingFile(resPath);
            }
#endif
            if (!File.Exists(resPath))
            {
                return null;
            }

            return new FileStream(resPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static byte[] LoadUnpackageResBuffer(string path)
        {
            bool isStreaming = false;
            string resPath = ResourceManager.Instance.GetUnpackageResPath(path, ref isStreaming);
            if (string.IsNullOrEmpty(resPath))
            {
                return null;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (isStreaming)
            {
                Stream stream = ResourceManager.GetStreamingFile(resPath);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
#endif

            if (!File.Exists(resPath))
            {
                return null;
            }

            return File.ReadAllBytes(resPath);
        }

        public static Downloader LoadOptionalRes<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional resource!");
                    callback(null);
                    return null;
                }

                T obj = ResourceManager.Instance.GetReferenceResource<T>(key);
                if (obj != null)
                {
                    callback(obj);
                    return null;
                }

                AssetBundle asset = null;

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) => 
                    {
                        asset = ResourceManager.Instance.LoadAssetBundle(key);
                        if (asset != null)
                        {
                            string assetPaht = "Assets/Resources/" + path;
                            obj = asset.LoadAsset<T>(assetPaht);
                            if (obj != null)
                            {
                                ResourceManager.Instance.AddResourceReference(key, obj);
                            }
                            callback(obj);
                        }
                        else
                        {
                            callback(null);
                        }
                        ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    });

                    return downloader;
                }

                asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    string assetPaht = "Assets/Resources/" + path;
                    obj = asset.LoadAsset<T>(assetPaht);
                    if (obj != null)
                    {
                        ResourceManager.Instance.AddResourceReference(key, obj);
                    }
                    callback(obj);
                }
                else
                {
                    callback(null);
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);

                return null;
            }

            if (!path.Contains("Optional") || path.Contains("Unpackage"))
            {
                Debugger.LogError("The source : " + path + " is not Optional resource!");
                callback(null);
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            callback(Resources.Load<T>(path));
            return null;
        }

        public static Downloader LoadOptionalResAll<T>(string path, Action<T[]> callback) where T : UnityEngine.Object
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional resource!");
                    callback(null);
                    return null;
                }

                T[] objs = ResourceManager.Instance.GetReferenceResources<T>(key);
                if (objs != null && objs.Length > 0)
                {
                    callback(objs);
                    return null;
                }

                AssetBundle asset = null;

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) =>
                    {
                        asset = ResourceManager.Instance.LoadAssetBundle(key);
                        if (asset != null)
                        {
                            objs = asset.LoadAllAssets<T>();
                            if (objs != null && objs.Length > 0)
                            {
                                ResourceManager.Instance.AddResourcesReference(key, objs);
                            }
                            callback(objs);
                        }
                        else
                        {
                            callback(null);
                        }
                        ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    });

                    return downloader;
                }

                asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    objs = asset.LoadAllAssets<T>();
                    if (objs != null && objs.Length > 0)
                    {
                        ResourceManager.Instance.AddResourcesReference(key, objs);
                    }
                    callback(objs);
                }
                else
                {
                    callback(null);
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);

                return null;
            }

            if (!path.Contains("Optional") || path.Contains("Unpackage"))
            {
                Debugger.LogError("The source : " + path + " is not Optional resource!");
                callback(null);
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            callback(Resources.LoadAll<T>(path));
            return null;
        }

        public static Downloader LoadOptionalRes(string path, Type type, Action<UnityEngine.Object> callback)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional resource!");
                    callback(null);
                    return null;
                }

                UnityEngine.Object obj = ResourceManager.Instance.GetReferenceResource(key, type);
                if (obj != null)
                {
                    callback(obj);
                    return null;
                }

                AssetBundle asset = null;

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) =>
                    {
                        asset = ResourceManager.Instance.LoadAssetBundle(key);
                        if (asset != null)
                        {
                            string assetPaht = "Assets/Resources/" + path;
                            obj = asset.LoadAsset(assetPaht, type);
                            if (obj != null)
                            {
                                ResourceManager.Instance.AddResourceReference(key, obj);
                            }
                            callback(obj);
                        }
                        else
                        {
                            callback(null);
                        }
                        ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    });

                    return downloader;
                }

                asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    string assetPaht = "Assets/Resources/" + path;
                    obj = asset.LoadAsset(assetPaht, type);
                    if (obj != null)
                    {
                        ResourceManager.Instance.AddResourceReference(key, obj);
                    }
                    callback(obj);
                }
                else
                {
                    callback(null);
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);

                return null;
            }

            if (!path.Contains("Optional") || path.Contains("Unpackage"))
            {
                Debugger.LogError("The source : " + path + " is not Optional resource!");
                callback(null);
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            callback(Resources.Load(path, type));
            return null;
        }

        public static Downloader LoadOptionalResAll(string path, Type type, Action<UnityEngine.Object[]> callback)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional resource!");
                    callback(null);
                    return null;
                }

                UnityEngine.Object[] objs = ResourceManager.Instance.GetReferenceResources(key, type);
                if (objs != null)
                {
                    callback(objs);
                    return null;
                }

                AssetBundle asset = null;

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) =>
                    {
                        asset = ResourceManager.Instance.LoadAssetBundle(key);
                        if (asset != null)
                        {
                            objs = asset.LoadAllAssets(type);
                            if (objs != null && objs.Length > 0)
                            {
                                ResourceManager.Instance.AddResourcesReference(key, objs);
                            }
                            callback(objs);
                        }
                        else
                        {
                            callback(null);
                        }
                        ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    });

                    return downloader;
                }

                asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    objs = asset.LoadAllAssets(type);
                    if (objs != null && objs.Length > 0)
                    {
                        ResourceManager.Instance.AddResourcesReference(key, objs);
                    }
                    callback(objs);
                }
                else
                {
                    callback(null);
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);

                return null;
            }

            if (!path.Contains("Optional") || path.Contains("Unpackage"))
            {
                Debugger.LogError("The source : " + path + " is not Optional resource!");
                callback(null);
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            callback(Resources.LoadAll(path, type));
            return null;
        }

        public static Downloader LoadOptionalResAll(string path, Action<UnityEngine.Object[]> callback)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional resource!");
                    callback(null);
                    return null;
                }

                UnityEngine.Object[] objs = ResourceManager.Instance.GetReferenceResources(key);
                if (objs != null)
                {
                    callback(objs);
                    return null;
                }

                AssetBundle asset = null;

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) =>
                    {
                        asset = ResourceManager.Instance.LoadAssetBundle(key);
                        if (asset != null)
                        {
                            objs = asset.LoadAllAssets();
                            if (objs != null && objs.Length > 0)
                            {
                                ResourceManager.Instance.AddResourcesReference(key, objs);
                            }
                            callback(objs);
                        }
                        else
                        {
                            callback(null);
                        }
                        ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                    });

                    return downloader;
                }

                asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    objs = asset.LoadAllAssets();
                    if (objs != null && objs.Length > 0)
                    {
                        ResourceManager.Instance.AddResourcesReference(key, objs);
                    }
                    callback(objs);
                }
                else
                {
                    callback(null);
                }
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);

                return null;
            }

            if (!path.Contains("Optional") || path.Contains("Unpackage"))
            {
                Debugger.LogError("The source : " + path + " is not Optional resource!");
                callback(null);
                return null;
            }

            path = path.Substring(0, path.LastIndexOf("."));
            callback(Resources.LoadAll(path));
            return null;
        }

        public static Downloader LoadOptionalUnpackageResStream(string path, Action<Stream> callback)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || !rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional and UnpackageRes resource!");
                    callback(null);
                    return null;
                }

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) =>
                    {
                        callback(LoadUnpackageResStream(path));
                    });

                    return downloader;
                }

                callback(LoadUnpackageResStream(path));
                return null;
            }

            if (!path.Contains("Unpackage") || !path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not Optional and UnpackageRes resource!");
                callback(null);
                return null;
            }

            callback(LoadUnpackageResStream(path));
            return null;
        }

        public static Downloader LoadOptionalUnpackageResBuffer(string path, Action<byte[]> callback)
        {
            string key = ResourceManager.Instance.GetResourceKey(path);
            if (!string.IsNullOrEmpty(key))
            {
                ResourceData rd = ResourceManager.Instance.GetResourceData(key);
                if (!rd.IsOptional() || !rd.IsUnpackage())
                {
                    Debugger.LogError("The source : " + path + " is not Optional and UnpackageRes resource!");
                    callback(null);
                    return null;
                }

                List<DownloadFile> list = ResourceManager.Instance.GetOptionalNeedDownladList(key);
                if (list.Count > 0)
                {
                    Downloader downloader = Downloader.DownloadFiles(list,
                    (o) =>
                    {
                        callback(LoadUnpackageResBuffer(path));
                    });

                    return downloader;
                }

                callback(LoadUnpackageResBuffer(path));
                return null;
            }

            if (!path.Contains("Unpackage") || !path.Contains("Optional"))
            {
                Debugger.LogError("The source : " + path + " is not Optional and UnpackageRes resource!");
                callback(null);
                return null;
            }

            callback(LoadUnpackageResBuffer(path));
            return null;
        }
    }
}