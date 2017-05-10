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
            path = path.Substring(0, path.LastIndexOf("."));
            return Resources.Load<T>(path);
        }

        public static string GetUnpackageResPath(string path)
        {
            if (!path.Contains("Unpackage"))
            {
                Debugger.LogError(path + " is not Unpackage resource!!");
                return "";
            }
#if UNITY_EDITOR
            return Application.dataPath + "/Resources/" + path;
#else
            throw new NotImplementedException();
#endif
        }

        public static Stream LoadUnpackageResStream(string path)
        {
            string resPath = GetUnpackageResPath(path);
            if (string.IsNullOrEmpty(resPath))
            {
                return null;
            }

            return new MemoryStream(File.ReadAllBytes(resPath));
        }

        public static byte[] LoadUnpackageResBuffer(string path)
        {
            string resPath = GetUnpackageResPath(path);
            if (string.IsNullOrEmpty(resPath))
            {
                return null;
            }

            return File.ReadAllBytes(resPath);
        }
    }
}