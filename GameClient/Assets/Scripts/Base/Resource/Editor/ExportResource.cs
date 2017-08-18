/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using BuildBase;
using System.IO;
using Base;
using System;
using System.Text;

public delegate void FindFileFunction(FileInfo file);

public class ExportResource : Editor
{
    static string resourceDir = Application.dataPath + "/Resources/";
    static string exportDir;
    
    static Dictionary<string, string> sResourceFiles = new Dictionary<string, string>();
    static Dictionary<string, string> sExportFiles = new Dictionary<string, string>();
    static Dictionary<string, List<string>> sAtlasFiles = new Dictionary<string, List<string>>();
    static Dictionary<string, List<string>> sFileDepends = new Dictionary<string, List<string>>();
    static List<string> sLeastInstallFils = new List<string>();
    static List<string> sOptionalFiles = new List<string>();
    static Dictionary<string, List<string>> sFileReferences = new Dictionary<string, List<string>>();

    public static void ExportResources(List<BuildGroup> groups)
    {
#if !RECOURCE_CLIENT
        UIEditor.CreateUIAtlas();
        UIEditor.CreateUIIcons();
#endif
        SetAssetBundleNames(false);
        foreach (BuildGroup group in groups)
        {
            Export(BuildHelper.GetBuildTarget(group.Platform));
        }
#if !RECOURCE_CLIENT
        AutoBuildGameLogic.AutoBuild();
#endif
    }

    static void Export(BuildTarget target)
    {
#if !RECOURCE_CLIENT
        CopyDll(target);
#endif
        UnityEngine.Debug.Log("Start Export Resources " + target + " " + DateTime.Now);
        //UpdateProgressBar("正在导出 " + GetBuildTargetName(target) + " 平台资源");
        string manifest;
        if (target == BuildTarget.Android)
        {
            exportDir = Application.dataPath + "/../../Builds/ExportResources/Android/";
            manifest = exportDir + "Android";
        }
        else if (target == BuildTarget.iOS)
        {
            exportDir = Application.dataPath + "/../../Builds/ExportResources/IOS/";
            manifest = exportDir + "IOS";
        }
        else
        {
            exportDir = Application.dataPath + "/../../Builds/ExportResources/Windows/";
            manifest = exportDir + "Windows";
        }

        if (!Directory.Exists(exportDir))
            Directory.CreateDirectory(exportDir);

        if (File.Exists(exportDir + "export_names.txt"))
        {
            File.Copy(exportDir + "export_names.txt", exportDir + "_export_names.txt", true);
        }

        string saveManifest = manifest + "_2";
        string resourceList1Name = "_ResourceList_1.ab";
        string resourceList2Name = "_ResourceList_2.ab";
        string resourceListName = resourceList2Name;
#if !RECOURCE_CLIENT
        saveManifest = manifest + "_1";
        resourceListName = resourceList1Name;
#endif

        if (File.Exists(saveManifest))
            File.Copy(saveManifest, manifest, true);
        if (File.Exists(saveManifest + ".manifest"))
            File.Copy(saveManifest + ".manifest", manifest + ".manifest", true);

        ResourceDatas lastCompressFiles = BuildHelper.LoadResourceDatas(exportDir + resourceListName);
        List<string> lastExports = new List<string>();

        if (lastCompressFiles != null && lastCompressFiles.Resources.Count > 0)
        {
            var e = lastCompressFiles.Resources.GetEnumerator();
            while (e.MoveNext())
            {
                lastExports.Add(e.Current.Key);
            }
        }

//         if (target == BuildTarget.Android)
//         {
//             for (int idx = 0; idx < sSoundPoolMusic.Count; ++idx)
//             {
//                 int num = sSoundPoolMusic[idx];
//                 string oldName = "Assets/Resources/Sound/" + num + ".mp3";
//                 string newName = "Assets/Resources/UnPackage/Sound/" + num + ".mp3";
//                 string oldId = GetPathID(oldName);
//                 sExportFiles.Remove(oldId);
//                 AssetDatabase.MoveAsset(oldName, newName);
//                 AssetDatabase.Refresh();
//                 string newId = GetPathID(newName);
//                 sExportFiles.Add(newId, newName);
//             }
//         }

        foreach (KeyValuePair<string, string> pair in sExportFiles)
        {
            string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + pair.Value;
            if (lastExports.Contains(pair.Key))
                lastExports.Remove(pair.Key);

            if (pair.Value.StartsWith("Assets/Resources/") && pair.Value.Contains("Unpackage"))
            {
                string targetDir = exportDir + pair.Key + ".ab";
                FileHelper.CopyFile(pair.Value, targetDir);
                SetAssetBundleName(fullPath, "", "");
            }
        }
        foreach (string str in lastExports)
        {
            if (File.Exists(exportDir + str + ".ab"))
                File.Delete(exportDir + str + ".ab");

            if (File.Exists(exportDir + str + ".ab.manifest"))
                File.Delete(exportDir + str + ".ab.manifest");
        }

        AssetDatabase.Refresh();

        BuildPipeline.BuildAssetBundles(exportDir, BuildAssetBundleOptions.ChunkBasedCompression, target);
        AssetDatabase.Refresh();
        //UpdateProgressBar();
        
        ResourceDatas resources = new ResourceDatas();
        
        foreach (KeyValuePair<string, string> pair in sExportFiles)
        {
            if (!File.Exists(exportDir + pair.Key + ".ab"))
            {
                FileHelper.CopyFile(pair.Value, exportDir + pair.Key + ".ab");
                UnityEngine.Debug.LogError("Export AssetBundle " + pair.Value + " fail, use copy instead!");
            }

            ResourceData rd = GetResourceData(pair.Key, pair.Value);
            resources.Resources.Add(pair.Key ,rd);
            
            //UpdateProgressBar();
        }
        BuildHelper.SaveResourceDatas(exportDir + resourceListName, resources);
        
        ResourceDatas resources_1 = BuildHelper.LoadResourceDatas(exportDir + resourceList1Name);
        ResourceDatas resources_2 = BuildHelper.LoadResourceDatas(exportDir + resourceList2Name);
#if !RECOURCE_CLIENT
        if (resources_2 != null)
        {
            foreach (var c in resources_2.Resources)
            {
#else
        if (resources_1 != null)
        {
            foreach (var c in resources_1.Resources)
            {
#endif
                if (!resources.Resources.ContainsKey(c.Key))
                {
                    resources.Resources.Add(c.Key, c.Value);
                }
                else
                {
                    UnityEngine.Debug.LogError("ResourceClient与GameClient中有相同路径的资源 : " + c.Value.Path);
                }
            }
        }

        BuildHelper.SaveResourceDatas(exportDir + "_ResourceList.ab", resources);
        SevenZipHelper.CompressFile(exportDir + "_ResourceList.ab", exportDir + "ResourceList.ab");
        //#endif


        StreamWriter writer = new StreamWriter(exportDir + "export_names.txt", false, Encoding.Default);
        writer.WriteLine("string,uint,int,int,string,string");
        writer.WriteLine("key,crc,size,type,reference,path,depends");
        if (resources_1 != null)
        {
            foreach (var c in resources_1.Resources)
            {
                string[] dependsArray = new string[c.Value.Depends.Count];
                for (int i = 0; i < dependsArray.Length; ++i)
                {
                    dependsArray[i] = c.Value.Depends[i];
                }
                string depends = c.Value.Depends.Count == 0 ? "" : string.Join("|", dependsArray);
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", c.Key, c.Value.Crc, c.Value.Size, (int)c.Value.Type, c.Value.Reference, c.Value.Path, depends));
            }
        }
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
                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", c.Key, c.Value.Crc, c.Value.Size, (int)c.Value.Type, c.Value.Reference, c.Value.Path, depends));
            }
        }
        writer.Close();

#if !RECOURCE_CLIENT
        string versions = ResourceManager.CodeVersion.ToString();
        if (File.Exists(exportDir + "version.txt"))
        {
            string[] strs = File.ReadAllLines(exportDir + "version.txt");
            string[] vers = strs[0].Split(' ');
            versions = vers[0];
        }
        versions += " ";
        versions += FileHelper.GetFileCrc(exportDir + "_ResourceList.ab");
        byte[] buf = System.Text.Encoding.Default.GetBytes(versions);
        File.WriteAllBytes(exportDir + "version.txt", buf);
#else
        if (File.Exists(exportDir + "version.txt"))
        {
            string[] strs = File.ReadAllLines(exportDir + "version.txt");
            string[] vers = strs[0].Split(' ');
            string versions = vers[0]; versions += " ";
            versions += FileHelper.GetFileCrc(exportDir + "_ResourceList.ab");
            byte[] buf = System.Text.Encoding.Default.GetBytes(versions);
            File.WriteAllBytes(exportDir + "version.txt", buf);
        }
#endif

        //         if (target == BuildTarget.Android)
        //         {
        //             for (int idx = 0; idx < sSoundPoolMusic.Count; ++idx)
        //             {
        //                 int num = sSoundPoolMusic[idx];
        //                 string oldName = "Assets/Resources/UnPackage/Sound/" + num + ".mp3";
        //                 string newName = "Assets/Resources/Sound/" + num + ".mp3";
        //                 string oldId = GetPathID(oldName);
        //                 sExportFiles.Remove(oldId);
        //                 AssetDatabase.MoveAsset(oldName, newName);
        //                 AssetDatabase.Refresh();
        //                 string newId = GetPathID(newName);
        //                 sExportFiles.Add(newId, newName);
        // 
        //                 string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + newName;
        //                 string[] abs = newId.Split('.');
        //                 SetAssetBundleName(fullPath, abs[0], abs[1]);
        //                 AssetDatabase.Refresh();
        //             }
        //         }
        //         UpdateProgressBar();

        File.Copy(manifest, saveManifest, true);
        File.Copy(manifest + ".manifest", saveManifest + ".manifest", true);

        UnityEngine.Debug.Log("Finish Export Resources " + target + " " + DateTime.Now);
    }

#if !RECOURCE_CLIENT
    static void CopyDll(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                AutoBuildGameLogic.ChangeGameLogicDefines(new string[] { "UNITY_ANDROID" });
                AutoBuildGameLogic.BuildGameLogic();
                break;
            case BuildTarget.iOS:
                AutoBuildGameLogic.ChangeGameLogicDefines(new string[] { "UNITY_IPHONE" });
                AutoBuildGameLogic.BuildGameLogic();
                break;
            case BuildTarget.StandaloneWindows:
                AutoBuildGameLogic.ChangeGameLogicDefines(new string[] { "UNITY_STANDALONE_WIN" });
                AutoBuildGameLogic.BuildGameLogic();
                break;
        }

        byte[] bytes = File.ReadAllBytes(Application.dataPath + "/../../output/GameLogic.dll");
        Rc4.rc4_go(ref bytes, bytes, (long)bytes.Length, Rc4.key, Rc4.key.Length, 0);
        File.WriteAllBytes(Application.dataPath + "/Resources/Install/Unpackage/GameLogic.bytes", bytes);
        AssetDatabase.Refresh();
    }
#endif

    static void TraverseDirectory(FileSystemInfo info, FindFileFunction func)
    {
        if (!info.Exists) return;
        DirectoryInfo dir = info as DirectoryInfo;
        if (dir == null) return;
        FileSystemInfo[] files = dir.GetFileSystemInfos();
        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i] as FileInfo;
            if (file != null)
            {
                func(file);
            }
            else
                TraverseDirectory(files[i], func);
        }
    }


    static string PathFormat(string path)
    {
        return path.Replace("\\", "/");
    }

    static string GetPathID(string path)
    {
        path = PathFormat(path);
        return FileHelper.GetStringMd5(path.ToLower());
    }
    
    static void FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
#if !RECOURCE_CLIENT
        int i = 0;
#endif
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
#if !RECOURCE_CLIENT
            ++i;
            if (i <= 1) continue;
#endif
            EditorScenes.Add(scene.path);
            //UpdateProgressBar();
        }
        foreach (string scene in EditorScenes)
        {
            string path = PathFormat(scene);
            string name = path.Substring(path.LastIndexOf('/') + 1);
            name = PathFormat(name);
            name = name.ToLower();
            string id = GetPathID(name);
            sResourceFiles.Add(id, path);
            if (path.Contains("Install"))
                sLeastInstallFils.Add(id);
            else if (path.Contains("Optional"))
                sOptionalFiles.Add(id);
            //UpdateProgressBar();
        }
    }

    static void FindResourceFile(FileInfo file)
    {
        if (file.Name.EndsWith(".meta") || file.Name.EndsWith(".DS_Store"))
            return;

        string path = file.FullName.Substring(Application.dataPath.Length - "Assets".Length);
        path = PathFormat(path);
        string name = file.FullName.Substring(resourceDir.Length);
        name = PathFormat(name);
        name = name.ToLower();
        string id = GetPathID(name);
        sResourceFiles.Add(id, path);
        if (path.Contains("Install"))
            sLeastInstallFils.Add(id);
        else if (path.Contains("Optional"))
            sOptionalFiles.Add(id);
        //UpdateProgressBar();
    }
    
    static void AnalyzeDependencies(string path, ref List<string> dependencies, ResourceType type)
    {
        string[] childPaths = AssetDatabase.GetDependencies(new string[] { path });
        
        for (int i = 0; i < childPaths.Length; ++i)
        {
            string childPath = childPaths[i];
            if (childPath.Equals(path))
                continue;

            if (childPath.EndsWith(".cs") /*|| childPath.EndsWith(".shader")*/ || childPath.EndsWith(".dll") ||
                childPath.EndsWith(".js") || childPath.EndsWith(".boo") || childPath.EndsWith(".asset"))
                continue;

            childPath = PathFormat(childPath);
            string id = GetPathID(childPath);
            if (childPath.StartsWith("Assets/Resources/"))
            {
                string tempstr = childPath.Substring("Assets/Resources/".Length);
                id = GetPathID(tempstr);
            }
            bool isAtlas = false;
            if (childPath.StartsWith("Assets/UI_Atlas/"))
            {
                string name = childPath.Substring("Assets/UI_Atlas/".Length);
                if (name.Contains("/"))
                {
                    name = name.Substring(0, name.LastIndexOf("/"));
                    string tempstr = "UI/Atlas/" + name + ".prefab";
                    id = GetPathID(tempstr);
                    if (path.StartsWith("Assets/Resources/UI/Atlas/"))
                        isAtlas = true;
                }
            }
            if (childPath.StartsWith("Assets/UI_Icons/"))
            {
                if (!path.StartsWith("Assets/Resources/UI/Icons/"))
                {
                    Debugger.LogError("the icon sprite " + childPath + " is refreced by : " + path);
                }
            }
            if (sOptionalFiles.Contains(id) && type != ResourceType.Optional)
                sOptionalFiles.Remove(id);
            if (type == ResourceType.Install && !sLeastInstallFils.Contains(id))
                sLeastInstallFils.Add(id);
            List<string> depenList = new List<string>();
            if (sFileDepends.ContainsKey(id))
                depenList = sFileDepends[id];
            else if (!sExportFiles.ContainsKey(id) && !isAtlas)
            {
                if (type == ResourceType.Optional)
                    sOptionalFiles.Add(id);
                sExportFiles.Add(id, childPath);
                AnalyzeDependencies(childPath, ref depenList, type);
                if (depenList.Count > 0)
                    sFileDepends.Add(id, depenList);
                //UpdateProgressBar();
            }
            if (isAtlas)
            {
                if (!sAtlasFiles.ContainsKey(id))
                    sAtlasFiles.Add(id, new List<string>());
                sAtlasFiles[id].Add(childPath);
            }

            if (!isAtlas && !dependencies.Contains(id))
                dependencies.Add(id);
        }
    }
    
    static void AnalyzeChildDepended(string fileID)
    {
        if (sFileDepends.ContainsKey(fileID))
        {
            List<string> depends = new List<string>();
            while (sFileDepends.ContainsKey(fileID) && depends.Count != sFileDepends[fileID].Count)
            {
                depends = new List<string>(sFileDepends[fileID]);
                foreach (string id in depends)
                {
                    AnalyzeChildDepended(id);
                }
                //UpdateProgressBar();
            }
        }

        if (sFileReferences.ContainsKey(fileID))
        {
            List<string> reference = new List<string>(sFileReferences[fileID]);
            if (reference.Count == 1)
                RemoveSingleReferenceFile(fileID);
        }
        //UpdateProgressBar();
    }

    static void RemoveSingleReferenceFile(string fileID)
    {
        if (sResourceFiles.ContainsKey(fileID)) return;

        if (sFileReferences.ContainsKey(fileID))
        {
            List<string> reference = new List<string>(sFileReferences[fileID]);
            if (reference.Count == 1)
            {
                string parent = reference[0];
                if (sFileDepends.ContainsKey(parent))
                {
                    if (sFileDepends[parent].Contains(fileID))
                        sFileDepends[parent].Remove(fileID);

                    if (!sFileDepends.ContainsKey(fileID))
                    {
                        if (sFileDepends[parent].Count == 0)
                            sFileDepends.Remove(parent);
                    }
                    else
                    {
                        List<string> depends = sFileDepends[fileID];
                        foreach (string id in depends)
                        {
                            if (sFileReferences.ContainsKey(id))
                            {
                                if (sFileReferences[id].Contains(fileID))
                                    sFileReferences[id].Remove(fileID);

                                if (!sFileReferences[id].Contains(parent))
                                    sFileReferences[id].Add(parent);
                            }
                            if (!sFileDepends[parent].Contains(id))
                                sFileDepends[parent].Add(id);
                        }

                        sFileDepends.Remove(fileID);
                    }
                }

                sFileReferences.Remove(fileID);

                if (sExportFiles.ContainsKey(fileID))
                    sExportFiles.Remove(fileID);
            }
        }
    }
    
    static ResourceData GetResourceData(string key, string path)
    {
        ResourceData rd = new ResourceData();
        //rd.Key = key;
        rd.Path = path;

        //rd.size = GetFileSize(exportDir + key);
        rd.Type = ResourceType.Normal;
        if (sLeastInstallFils.Contains(key))
            rd.Type = ResourceType.Install;
        else if (sOptionalFiles.Contains(key))
            rd.Type = ResourceType.Optional;
        if (path.Contains("Unpackage"))
            rd.Type |= ResourceType.Unpackage;

        string fileName = exportDir + key + ".ab";
        rd.Crc = FileHelper.GetFileCrc(fileName);

        rd.Size = FileHelper.GetFileSize(fileName);

        if (sFileDepends.ContainsKey(key))
        {
            rd.Depends.AddRange(sFileDepends[key]);
        }

        rd.Reference = sFileReferences.ContainsKey(key) ? sFileReferences[key].Count : 0;
        if (path.StartsWith("Assets/Resources/UI/Atlas/") && rd.Reference < 2)
        {
            rd.Reference = 2;
        }

        return rd;
    }
    
    [MenuItem("BuildProject/Set AssetBundleName")]
    static void SetAssetBundleName()
    {
        SetAssetBundleNames(true);
    }

    static void SetAssetBundleNames(bool b)
    {
        UnityEngine.Debug.Log("----------Start Set AssetBundleName " + DateTime.Now);
        //UpdateProgressBar("正在遍历资源");
        sResourceFiles.Clear();
        sExportFiles.Clear();
        sAtlasFiles.Clear();
        sFileDepends.Clear();
        sOptionalFiles.Clear();
        sLeastInstallFils.Clear();
        sFileReferences.Clear();
        //CopyLua(true);
        //UnityEngine.Debug.LogError("Step 1: get export resource files");
        if (b)
            ClearAssetBundleName();

        FindEnabledEditorScenes();
        FileSystemInfo info = new DirectoryInfo(resourceDir);
        TraverseDirectory(info, FindResourceFile);
        //UpdateProgressBar("正在分析资源依赖项");
        //UnityEngine.Debug.LogError("Step 2: analyze depend files");
        foreach (KeyValuePair<string, string> pair in sResourceFiles)
        {
            if (sExportFiles.ContainsKey(pair.Key)) continue;

            if (!pair.Value.Contains("UnPackage"))
            {
                List<string> dependencies = new List<string>();
                ResourceType type = ResourceType.Normal;
                if (sLeastInstallFils.Contains(pair.Key))
                    type = ResourceType.Install;
                else if (sOptionalFiles.Contains(pair.Key))
                    type = ResourceType.Optional;
                AnalyzeDependencies(pair.Value, ref dependencies, type);
                if (dependencies.Count > 0)
                    sFileDepends.Add(pair.Key, dependencies);
            }

            sExportFiles.Add(pair.Key, pair.Value);
            //UpdateProgressBar();
        }

        foreach (KeyValuePair<string, List<string>> pair in sFileDepends)
        {
            foreach (string id in pair.Value)
            {
                if (!sFileReferences.ContainsKey(id))
                {
                    List<string> list = new List<string>();
                    sFileReferences.Add(id, list);
                }
                if (!sFileReferences[id].Contains(pair.Key))
                    sFileReferences[id].Add(pair.Key);
            }
            //UpdateProgressBar();
        }

        //UnityEngine.Debug.LogError("Step 3: analyze only one reference files");
        foreach (KeyValuePair<string, string> pair in sResourceFiles)
        {
            AnalyzeChildDepended(pair.Key);
        }

        //UnityEngine.Debug.LogError("Step 4: set assetbundle names");
        //UpdateProgressBar("正在设置AssetBundle");
        ClearAssetBundleName(b);
        foreach (KeyValuePair<string, string> pair in sExportFiles)
        {
            string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + pair.Value;
            SetAssetBundleName(fullPath, pair.Key, "ab");
            if (sAtlasFiles.ContainsKey(pair.Key))
            {
                foreach (string str in sAtlasFiles[pair.Key])
                {
                    fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + str;
                    SetAssetBundleName(fullPath, pair.Key, "ab");
                }
            }
            //UpdateProgressBar();
        }
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("----------Finish Set AssetBundleName " + DateTime.Now);
    }

    [MenuItem("BuildProject/Clear AssetBundleName")]
    static void ClearAssetBundleName()
    {
        ClearAssetBundleName(true);
    }
    static void ClearAssetBundleName(bool refresh)
    {
        DateTime time = DateTime.Now;
        FileSystemInfo info = new DirectoryInfo(Application.dataPath);
        TraverseDirectory(info, RemoveAssetBundleName);
        if (refresh)
            AssetDatabase.Refresh();
        AssetDatabase.RemoveUnusedAssetBundleNames();
        if (refresh)
            AssetDatabase.Refresh();
        TimeSpan ts = DateTime.Now - time;
        UnityEngine.Debug.Log("Clear Finish!  Use time : " + ts.TotalSeconds);
    }

    static void RemoveAssetBundleName(FileInfo file)
    {
        if (file.Name.EndsWith(".meta")/* || file.Name.EndsWith(".cs") ||
            file.Name.EndsWith(".shader") || file.Name.EndsWith(".dll") ||
            file.Name.EndsWith("js") || file.Name.EndsWith(".boo")*/)
            return;

        SetAssetBundleName(file.FullName, "", "");
    }

    static void SetAssetBundleName(string file, string assetBundleName, string assetBundleVariant)
    {
        string metaFile = file + ".meta";
        if (!File.Exists(metaFile))
            return;

        bool needChange = false;
        bool hasName = false;
        bool hasVariant = false;

        List<string> lines = new List<string>(File.ReadAllLines(metaFile));
        if (lines[lines.Count - 2].StartsWith("  assetBundleName: "))
        {
            if ((!string.IsNullOrEmpty(assetBundleName) && !lines[lines.Count - 2].Contains(assetBundleName)) ||
                (string.IsNullOrEmpty(assetBundleName) && !lines[lines.Count - 2].Equals("  assetBundleName: ")))
                needChange = true;
            hasName = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(assetBundleName))
                needChange = true;
        }
        if (lines[lines.Count - 1].StartsWith("  assetBundleVariant: "))
        {
            if ((!string.IsNullOrEmpty(assetBundleVariant) && !lines[lines.Count - 1].Contains(assetBundleVariant)) ||
                (string.IsNullOrEmpty(assetBundleVariant) && !lines[lines.Count - 1].Equals("  assetBundleVariant: ")))
                needChange = true;
            hasVariant = true;
        }
        else
        {
            if (!string.IsNullOrEmpty(assetBundleVariant))
                needChange = true;
        }

        if (!needChange)
            return;

        if (!hasName)
            lines.Add("");
        if (!hasVariant)
            lines.Add("");
        lines[lines.Count - 2] = string.Format("  assetBundleName: {0}", assetBundleName);
        lines[lines.Count - 1] = string.Format("  assetBundleVariant: {0}", assetBundleVariant);
        string text = string.Join("\n", lines.ToArray()) + "\n";

        FileStream fs = new FileStream(metaFile, FileMode.OpenOrCreate);
        fs.Position = 0;
        fs.SetLength(0);
        byte[] bytes = System.Text.Encoding.Default.GetBytes(text);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
    }

    /*[MenuItem("BuildProject/Create Android Obb")]
    public static void CreateObb()
    {
        UnityEngine.Debug.Log("Create Obb start! " + DateTime.Now);

        exportDir = Application.dataPath + "/../../Builds/ExportResources/Android/";
        if (!Directory.Exists(exportDir))
        {
            UnityEngine.Debug.LogError("You must export resources first!");
            return;
        }

        if (!File.Exists(exportDir + "_ResourceList.ab"))
        {
            UnityEngine.Debug.LogError("the _ResourceList.ab file don't exist!");
            return;
        };

        string obbFolader = Application.dataPath + "/../../Builds/GameResources/";
        string obbFile = Application.dataPath + "/../../Builds/android.obb";
        if (Directory.Exists(obbFolader))
            Directory.Delete(obbFolader, true);

        CreateChildDirectorys(obbFolader);

        UnityEngine.Debug.Log("Create Obb 1!");

        DataHash<ResourceData> resourceList = new DataHash<ResourceData>("key");
        resourceList.LoadInCS(exportDir + "_ResourceList.ab");
        if (resourceList.Count == 0)
        {
            UnityEngine.Debug.LogError("You must export resources first!");
            return;
        }
        CopyFile(exportDir + "_ResourceList.ab", obbFolader + "ResourceList.ab");
        List<ResourceData> resourceDatas = new List<ResourceData>(resourceList.GetUnits().Values);
        foreach (ResourceData rd in resourceDatas)
        {
            if (rd.type == UpdateType.UT_REQUIRED)
            {
                CopyFile(exportDir + rd.key, obbFolader + rd.key);
            }
        }
        File.WriteAllText(obbFolader + "tag", DateTime.Now.Ticks.ToString());

        UnityEngine.Debug.Log("Create Obb 2!");

        CreateZipFile(obbFolader, obbFile);

        UnityEngine.Debug.Log("Create Obb 3!");

        Directory.Delete(obbFolader, true);

        UnityEngine.Debug.Log("Create Obb finish! " + DateTime.Now);
    }*/
}
