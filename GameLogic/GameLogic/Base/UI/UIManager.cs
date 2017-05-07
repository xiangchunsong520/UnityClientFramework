/*
auth: Xiang ChunSong
purpose:
*/

using System.Collections.Generic;
using UnityEngine;
using Data;
using System;
using Base;
using UnityEngine.UI;

namespace GameLogic
{
    public class UIManager : Singleton<UIManager>
    {
        Dictionary<string, Camera> _cameras = new Dictionary<string, Camera>();
        Dictionary<string, UIWindow> _windowCache = new Dictionary<string, UIWindow>();     //缓存的窗口对象
        List<WinNameParam> _openWindowStack = new List<WinNameParam>();                     //打开过的非悬浮窗口堆栈,用于返回按钮
        List<string> _openingHoverWindow = new List<string>();                              //正在打开的悬浮窗口
        string _curOpenWindow;                                                              //当前打开的非悬浮窗口
        Vector2 _uiResolution = new Vector2(960, 640);

        public Camera HideCamera
        {
            get
            {
                return _cameras["Hide Camera"];
            }
        }

        class WinNameParam
        {
            public string winname;
            public object[] param = null;
        }

        public void Init()
        {
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("UICamera");
            for (int i = 0; i < cameras.Length; ++i)
            {
                UnityEngine.Object.DontDestroyOnLoad(cameras[i]);
                _cameras.Add(cameras[i].name, cameras[i].GetComponent<Camera>());
            }
        }

        public static bool OpenWindow(string winName, params object[] pars)
        {
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Start();
            if (Instance._curOpenWindow == winName)
                return false;

            if (Instance._openingHoverWindow.Contains(winName))
                return false;

            UIWindow win = null;
            if (!Instance._windowCache.ContainsKey(winName))
            {
                Type type = Type.GetType("GameLogic." + winName);
                if (type != null)
                {
                    win = Activator.CreateInstance(type) as UIWindow;
                    if (win != null)
                    {
                        WindowConfig winCfg = DataManager.Instance.windowConfigDatas.GetUnit(winName);
                        if (winCfg != null)
                        {
                            win.ConfigData = winCfg;
                            Instance._windowCache.Add(winName, win);
                        }
                        else
                        {
                            Debugger.LogError("Open : " + winName + " window fail, the WindowConfig " + winName + " don't exist!");
                            return false;
                        }
                    }
                    else
                    {
                        Debugger.LogError("Open : " + winName + " window fail, create class " + winName + " fail!");
                        return false;
                    }
                }
                else
                {
                    Debugger.LogError("Open : " + winName + " window fail, the class " + winName + " don't exist!");
                    return false;
                }
            }

            win = Instance._windowCache[winName];
            if (!win.ConfigData.IsHover)
            {
                bool find = UpdateOpenList(winName);
                CloseCurOpenWindow(find);
                Instance._curOpenWindow = winName;
            }
            else
            {
                Instance._openingHoverWindow.Add(winName);
            }

            if (win.Root == null)
            {
                GameObject root = LoadUIWindow(win);
                if (root)
                {
                    win.Init();
                }
                else
                {
                    Debugger.LogError("Open : " + winName + " window fail, load prefab " + win.ConfigData.PrefabName + " fail!");
                    return false;
                }
            }

            bool rsl = win.Open(pars);

            //             LogOpenWindowStack();

            w.Stop();
            if (UnityDefine.UnityEditor)
            {
                Debugger.Log("Open " + winName + " finish. Use time : " + w.ElapsedMilliseconds + " ms");
            }
            return rsl;
        }

        public static void PopToOpenWindowStack(string winName)
        {
            if (Instance._curOpenWindow == winName)
                return;

            var win = Instance._openWindowStack.Find((a) =>
            {
                return a.winname == winName;
            });
            if (win == null)
                return;

            WindowConfig winCfg = null;
            if (Instance._windowCache.ContainsKey(winName))
                winCfg = Instance._windowCache[winName].ConfigData;
            else
                winCfg = DataManager.Instance.windowConfigDatas.GetUnit(winName);

            if (winCfg != null && !winCfg.IsHover)
            {
                OpenWindow("EmptyWindow");
                bool find = UpdateOpenList(winName);
                Instance._openWindowStack.Add(win);
            }
        }

        public static void PopOpenWindowStack()
        {
            if (Instance._curOpenWindow == "")
                return;

            var win = Instance._windowCache[Instance._curOpenWindow];
            if (win != null && !win.ConfigData.IsHover)
            {
                OpenWindow("EmptyWindow");
                if (win.ConfigData.IsRecord)
                    Instance._openWindowStack.RemoveAt(Instance._openWindowStack.Count - 1);
            }

        }

        static void LogOpenWindowStack()
        {
            string str = "";
            foreach (var v in Instance._openWindowStack)
            {
                str += v.winname;
                str += "->";
            }
            str += Instance._curOpenWindow;
            Debugger.LogWarning(str);
        }

        public static bool CloseWindow(string winName, bool user = true)
        {
            if (!Instance._windowCache.ContainsKey(winName))
                return false;

            UIWindow win = Instance._windowCache[winName];
            if (!user)
            {
                if (win.ConfigData.IsRecord)
                    AppendOpenWindow(winName, win.Param);
            }
            if (win.ConfigData.IsHover)
                Instance._openingHoverWindow.Remove(winName);

            bool b = win.Close();
            if (win.ConfigData.CloseDelete)
            {
                win.Release();
                win = null;
                GC.Collect();
                Instance._windowCache.Remove(winName);
            }

            return b;
        }

        public static void ReturnOpenWindow()
        {
            if (Instance._openWindowStack.Count > 0)
            {
                WinNameParam last = Instance._openWindowStack[Instance._openWindowStack.Count - 1];
                CloseWindow(Instance._curOpenWindow);
                Instance._curOpenWindow = "";
                OpenWindow(last.winname, last.param);
            }
        }

        public static void CloseAllWindow()
        {
            CloseCurOpenWindow();
            CloseAllHoverWindow();
        }

        public static void CloseAllHoverWindow()
        {
            List<string> _tmp = new List<string>();
            _tmp.AddRange(Instance._openingHoverWindow);
            foreach (string winName in _tmp)
            {
                if (!"EffectWindow".Equals(winName))
                    CloseWindow(winName);
            }
        }

        public static UIWindow GetWindow(string winName)
        {
            if (Instance._windowCache.ContainsKey(winName))
                return Instance._windowCache[winName];

            return null;
        }

        static GameObject LoadUIWindow(UIWindow win)
        {
            string cameraName = win.ConfigData.CameraName;
            if (string.IsNullOrEmpty(cameraName))
                cameraName = "Normal Camera";

            Camera camera;
            if (Instance._cameras.TryGetValue(cameraName, out camera))
            {
                try
                {
                    GameObject go = GameObject.Instantiate(ResourceLoader.Load<GameObject>(win.ConfigData.PrefabName));
                    GameObject.DontDestroyOnLoad(go);
                    Canvas canvas = go.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = Instance.HideCamera;
                    CanvasScaler scaler = go.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                    scaler.referenceResolution = Instance._uiResolution;
                    win.Root = go;
                    win.SetCamera(camera);
                    win.SetCanvas(canvas);
                    return go;
                }
                catch (Exception ex)
                {
                    Debugger.LogError(ex);
                    return null;
                }
            }
            else
            {
                Debugger.LogError("Can't find the UICamera : \"" + cameraName + "\"");
            }
            return null;
        }

        static void AppendOpenWindow(string winName, object[] param = null)
        {
            WinNameParam p = null;
            while ((p = Instance._openWindowStack.Find((a) =>
            {
                return a.winname == winName;
            })) != null)
            {
                Instance._openWindowStack.Remove(p);
            }
            p = new WinNameParam();
            p.winname = winName;
            p.param = param;
            Instance._openWindowStack.Add(p);
        }

        static bool UpdateOpenList(string winName)
        {
            bool find = false;
            for (int i = 0; i < Instance._openWindowStack.Count;)
            {
                if (Instance._openWindowStack[i].winname.Equals(winName) || find)
                {
                    find = true;
                    Instance._openWindowStack.RemoveAt(i);
                    continue;
                }

                ++i;
            }

            return find;
        }

        static public void RemoveOpenHistroy(string winName)
        {
            UpdateOpenList(winName);
        }

        static void CloseCurOpenWindow(bool user = true)
        {
            if (string.IsNullOrEmpty(Instance._curOpenWindow))
                return;

            CloseWindow(Instance._curOpenWindow, user);
        }
    }
}
