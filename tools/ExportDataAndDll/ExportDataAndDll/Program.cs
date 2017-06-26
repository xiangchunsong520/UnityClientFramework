using Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExportDataAndDll
{
    class Program
    {
        static string exportPath;
        static string projectPath;
        static string resourcePath;
        static string dllbinPath;
        static string dataPath;
        static void Main(string[] args)
        {
            string[] plantforms = new string[] { "Android", "IOS", "Windows" };
            string rootPath = args[0];
            string dllPath = rootPath + "output/GameLogic.dll";

            exportPath = rootPath + "Builds";
            projectPath = rootPath + "GameClient/";
            resourcePath = rootPath + "GameClient/Assets/Resources/";
            dllbinPath = rootPath + "GameClient/Assets/Resources/Install/Unpackage/GameLogic.bytes";
            dataPath = rootPath + "GameClient/Assets/Resources/Install/Unpackage/Data/";

            /**/////////////////////////////////////////////////////////////////////////
            //copy lua
            byte[] bytes = File.ReadAllBytes(dllPath);
            Rc4.rc4_go(ref bytes, bytes, (long)bytes.Length, Rc4.key, Rc4.key.Length, 0);
            File.WriteAllBytes(dllbinPath, bytes);
            /////////////////////////////////////////////////////////////////////////*/

            ResourceDatas resourceList = new ResourceDatas();

            AddDllBinData(ref resourceList);

            AddConfigDatas(ref resourceList);

            for (int i = 0; i < plantforms.Length; ++i)
            {
                CopyFiles(plantforms[i], resourceList);
            }

            //Console.ReadKey();
        }

        static void AddDllBinData(ref ResourceDatas resourceList)
        {
            string subpath = dllbinPath.Substring(resourcePath.Length);
            string md5 = FileHelper.GetStringMd5(subpath.ToLower());
            uint crc = FileHelper.GetFileCrc(dllbinPath);
            int size = FileHelper.GetFileSize(dllbinPath);
            string path = "Assets/Resources/" + subpath;
            ResourceType type = ResourceType.Install | ResourceType.Unpackage;
            ResourceData rd = new ResourceData();
            rd.Crc = crc;
            rd.Size = size;
            rd.Type = type;
            rd.Path = path;
            resourceList.Resources.Add(md5, rd);
        }

        static void AddConfigDatas(ref ResourceDatas resourceList)
        {
            DirectoryInfo dir = new DirectoryInfo(dataPath);
            FileInfo[] files = dir.GetFiles("*.bytes");
            for (int i = 0; i < files.Length; ++i)
            {
                FileInfo f = files[i];
                string filepath = f.FullName.Replace("\\", "/");
                string subpath = filepath.Substring(resourcePath.Length);
                string md5 = FileHelper.GetStringMd5(subpath.ToLower());
                uint crc = FileHelper.GetFileCrc(filepath);
                int size = FileHelper.GetFileSize(filepath);
                string path = "Assets/Resources/" + subpath;
                ResourceType type = ResourceType.Install | ResourceType.Unpackage;
                ResourceData rd = new ResourceData();
                rd.Crc = crc;
                rd.Size = size;
                rd.Type = type;
                rd.Path = path;
                resourceList.Resources.Add(md5, rd);
            }
        }

        static void CopyFiles(string plantform, ResourceDatas resourceList)
        {
            string targetPath = exportPath + "/ExportResources/" + plantform + "/";
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            if (File.Exists(targetPath + "export_names.txt"))
                File.Copy(targetPath + "export_names.txt", targetPath + "_export_names.txt", true);

            string r1 = targetPath + "_ResourceList_1.ab";
            string r = targetPath + "_ResourceList.ab";
            ResourceDatas l1 = BuildHelper.LoadResourceDatas(r1);
            if (l1 == null)
                l1 = new ResourceDatas();
            ResourceDatas l = BuildHelper.LoadResourceDatas(r);
            if (l == null)
                l = new ResourceDatas();
            var e = resourceList.Resources.GetEnumerator();
            while (e.MoveNext())
            {
                var key = e.Current.Key;
                var rd = e.Current.Value;
                File.Copy(projectPath + rd.Path, targetPath + key + ".ab", true);
                l1.Resources[key] = rd;
                l.Resources[key] = rd;
            }

            BuildHelper.SaveResourceDatas(r1, l1);
            BuildHelper.SaveResourceDatas(r, l);

            SevenZipHelper.CompressFile(r, targetPath + "ResourceList.ab");

            StreamWriter writer = new StreamWriter(targetPath + "export_names.txt", false, Encoding.Default);
            writer.WriteLine("string,uint,int,int,string,string");
            writer.WriteLine("key,crc,size,type,path,depends");
            foreach (var c in l1.Resources)
            {
                string[] dependsArray = new string[c.Value.Depends.Count];
                for (int i = 0; i < dependsArray.Length; ++i)
                {
                    dependsArray[i] = c.Value.Depends[i];
                }
                string depends = c.Value.Depends.Count == 0 ? "" : string.Join("|", dependsArray);
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", c.Key, c.Value.Crc, c.Value.Size, (int)c.Value.Type, c.Value.Path, depends));
            }

            ResourceDatas resources_2 = BuildHelper.LoadResourceDatas(targetPath + "_ResourceList_2.ab");
            if (resources_2 != null)
            {
                foreach (var c in resources_2.Resources)
                {
                    string[] dependsArray = new string[c.Value.Depends.Count];
                    for (int i = 0; i < dependsArray.Length; ++i)
                    {
                        dependsArray[i] = c.Value.Depends[i];
                    }
                    string depends = c.Value.Depends.Count == 0 ? "" : string.Join("|", dependsArray);
                    writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", c.Key, c.Value.Crc, c.Value.Size, (int)c.Value.Type, c.Value.Path, depends));
                }
            }
            writer.Close();

            if (File.Exists(targetPath + "version.txt"))
            {
                string[] strs = File.ReadAllLines(targetPath + "version.txt");
                string[] vers = strs[0].Split(' ');
                string versions = vers[0];
                versions += " ";
                versions += FileHelper.GetFileCrc(targetPath + "_ResourceList.ab");
                byte[] buf = System.Text.Encoding.Default.GetBytes(versions);
                File.WriteAllBytes(targetPath + "version.txt", buf);
            }
        }
    }
}
