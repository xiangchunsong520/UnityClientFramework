using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections;

namespace ApkPatchGenerator
{
    class Program
    {
        static SortedList<float, DirectoryInfo> versionList = new SortedList<float, DirectoryInfo>();
        static void Main(string[] args)
        {
            string path = Environment.CurrentDirectory;
            string toolPath = path + "/tool/bsdiff.exe";
            string configPath = path + "/ChannelConfig.csv";
            string outputPath = path + "/patchs/";
            string outputJson = outputPath + "patchs.txt";
            DirectoryInfo folder = new DirectoryInfo(path);
            DirectoryInfo[] childFolders = folder.GetDirectories();
            versionList.Clear();
            foreach (DirectoryInfo di in childFolders)
            {
                float val = GetVersionValue(di.Name);
                if (val >= 0)
                {
                    versionList.Add(val, di);
                }
            }
            if (versionList.Count == 0)
            {
                Console.WriteLine("no version!");
                Console.ReadKey();
                return;
            }
            PatchConfig patchConfig = new PatchConfig();
            patchConfig.channels = new List<PatchConfig.Channel>();
            patchConfig.newVersion = versionList.Keys[versionList.Count - 1].ToString();
            Console.WriteLine("new version : " + patchConfig.newVersion);

            ChannelConfig channels = new ChannelConfig(configPath);
            foreach (KeyValuePair<string, string> pair in channels.channelConfigs)
            {
                string channelName = pair.Key;
                string downloadName = pair.Value;
                string apk_all = downloadName + ".apk";
                string apk_mini = "_" + downloadName + ".apk";

                string new_all = path + "/" + patchConfig.newVersion + "/" + apk_all;
                string new_mini = path + "/" + patchConfig.newVersion + "/" + apk_mini;
                if (!File.Exists(new_all) && !File.Exists(new_mini))
                {
                    continue;
                }

                bool hasOldVersion = false;

                for (int i = 0; i < versionList.Count - 1; ++i)
                {
                    if ((int)versionList.Keys[i] == (int)GetVersionValue(patchConfig.newVersion))
                        continue;

                    string oldVersion = versionList[versionList.Keys[i]].Name;
                    if (File.Exists(path + "/" + oldVersion + "/" + apk_all) || File.Exists(path + "/" + oldVersion + "/" + apk_mini))
                    {
                        hasOldVersion = true;
                        break;
                    }
                }

                if (hasOldVersion)
                {
                    Console.WriteLine("begin patch channel : " + channelName);
                    PatchConfig.Channel patchChannel = new PatchConfig.Channel();
                    patchChannel.patchs = new List<PatchConfig.Channel.Patch>();
                    patchChannel.name = channelName;
                    patchChannel.md5_all = GetFileMd5(new_all);
                    patchChannel.md5_mini = GetFileMd5(new_mini);
                    patchChannel.size_all = GetFileSize(new_all);
                    patchChannel.size_mini = GetFileSize(new_mini);
                    for (int i = 0; i < versionList.Count - 1; ++i)
                    {
                        float oldVersion = versionList.Keys[i];
                        if ((int)oldVersion == (int)GetVersionValue(patchConfig.newVersion))
                            continue;

                        string old_all = path + "/" + oldVersion + "/" + apk_all;
                        string old_mini = path + "/" + oldVersion + "/" + apk_mini;
                        if (!File.Exists(old_all) && !File.Exists(old_mini))
                            continue;

                        Console.WriteLine("begin patch version:" + oldVersion + "->" + patchConfig.newVersion);
                        PatchConfig.Channel.Patch patch = new PatchConfig.Channel.Patch();
                        patch.oldVersion = oldVersion.ToString();

                        string patchFolderName = oldVersion + "-" + patchConfig.newVersion;
                        string patchFolderPath = outputPath + "/" + patchFolderName;
                        if (!Directory.Exists(patchFolderPath))
                            Directory.CreateDirectory(patchFolderPath);

                        if (File.Exists(old_all))
                        {
                            if (File.Exists(new_all))
                            {
                                string patchName = patchFolderName + "/" + downloadName + ".patch";
                                Console.WriteLine("begin patch : " + patchName);
                                string patchPath = outputPath + patchName;
                                patch.patch_all = patchName;
                                patch.oldmd5_all = GetFileMd5(old_all);

                                Process p = new Process();
                                string param = old_all + " " + new_all + " " + patchPath;
                                param = param.Replace("/", "\\");
                                //Console.WriteLine("param : " + param);
                                ProcessStartInfo pi = new ProcessStartInfo(toolPath, param);
                                pi.UseShellExecute = false;
                                pi.CreateNoWindow = true;
                                p.StartInfo = pi;
                                p.Start();
                                p.WaitForExit();

                                patch.size_all = GetFileSize(patchPath);
                            }
                            else
                            {
                                Console.WriteLine("lost new version : " + new_all);
                            }
                        }

                        if (File.Exists(old_mini))
                        {
                            if (File.Exists(new_mini))
                            {
                                string patchName = patchFolderName + "/_" + downloadName + ".patch";
                                Console.WriteLine("begin patch : " + patchName);
                                string patchPath = outputPath + patchName;
                                patch.patch_mini = patchName;
                                patch.oldmd5_mini = GetFileMd5(old_mini);

                                Process p = new Process();
                                string param = old_mini + " " + new_mini + " " + patchPath;
                                param = param.Replace("/", "\\");
                                //Console.WriteLine("param : " + param);
                                ProcessStartInfo pi = new ProcessStartInfo(toolPath, param);
                                pi.UseShellExecute = false;
                                pi.CreateNoWindow = true;
                                p.StartInfo = pi;
                                p.Start();
                                p.WaitForExit();

                                patch.size_mini = GetFileSize(patchPath);
                            }
                            else
                            {
                                Console.WriteLine("lost new version : " + new_mini);
                            }
                        }

                        patchChannel.patchs.Add(patch);
                    }

                    patchConfig.channels.Add(patchChannel);
                }
            }

            string json = LitJson.JsonMapper.ToJson(patchConfig);
            File.WriteAllText(outputJson, json);

            Console.WriteLine(json);

            try
            {
                PatchConfig a = LitJson.JsonMapper.ToObject<PatchConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("finish!");
            Console.ReadKey();
        }

        static float GetVersionValue(string version)
        {
            float value;
            if (float.TryParse(version, out value))
            {
                return value;
            }

            return -1f;
        }

        static string GetFileMd5(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }

            try
            {
                byte[] buffer = File.ReadAllBytes(path);

                MD5 md5 = MD5.Create();
                return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "").ToLower();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        static int GetFileSize(string path)
        {
            long size = 0;
            if (!File.Exists(path))
            {
                return 0;
            }
            try
            {
                FileInfo fi = new FileInfo(path);
                size = (fi.Length - 1) / 1024 + 1;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (int)size;
        }
    }

    class ChannelConfig
    {
        public Dictionary<string, string> channelConfigs = new Dictionary<string, string>();
        public ChannelConfig(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            int nameIndex = -1, platformIndex = -1, downloadNameIndex = -1;
            string[] names = lines[2].Split(',');
            for (int i = 0; i < names.Length; ++i)
            {
                if (names[i] == "channelName")
                    nameIndex = i;
                if (names[i] == "platform")
                    platformIndex = i;
                if (names[i] == "downloadName")
                    downloadNameIndex = i;
            }
            for (int i = 3; i < lines.Length; ++i)
            {
                string[] strs = lines[i].Split(',');
                if (strs[platformIndex] == "android")
                {
                    channelConfigs.Add(strs[nameIndex], strs[downloadNameIndex]);
                }
            }
        }
    }

    public class PatchConfig
    {
        public class Channel
        {
            public class Patch
            {
                public string oldVersion;
                public string patch_all;
                public string oldmd5_all;
                public int size_all;
                public string patch_mini;
                public string oldmd5_mini;
                public int size_mini;
            }

            public string name;
            public string md5_all;
            public int size_all;
            public string md5_mini;
            public int size_mini;
            public List<Patch> patchs;
        }

        public string newVersion;
        public List<Channel> channels;
    }
}
