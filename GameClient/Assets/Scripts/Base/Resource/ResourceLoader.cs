/*
auth: Xiang ChunSong
purpose:
*/

using System;
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
                AssetBundle asset = ResourceManager.Instance.LoadAssetBundle(key);
                if (asset != null)
                {
                    string assetPaht = "Assets/Resources/" + path;
                    T obj = asset.LoadAsset<T>(assetPaht);
                    if (obj != null)
                    {
                        ResourceManager.Instance.AddResourceReference(key, obj);
                    }
                    return obj;
                }
            }

            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.Load<T>(path);
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

            return new MemoryStream(File.ReadAllBytes(resPath));
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
    }
}