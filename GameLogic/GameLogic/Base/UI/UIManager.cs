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
        Dictionary<string, List<UIWindow>> _windowCache = new Dictionary<string, List<UIWindow>>();     //缓存的窗口对象
        List<WinNameParam> _openWindowStack = new List<WinNameParam>();                     //打开过的非悬浮窗口堆栈,用于返回按钮
        List<UIWindow> _openingHoverWindow = new List<UIWindow>();                              //正在打开的悬浮窗口
        string _curOpenWindow;                                                              //当前打开的非悬浮窗口
        Vector2 _uiResolution = new Vector2(640, 1136);

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
                _cameras.Add(cameras[i].name, cameras[i].GetComponent<Camera>());
            }
        }

        public static UIWindow OpenWindow<T>(params object[] pars) where T : UIWindow
        {
            return OpenWindow(typeof(T), pars);
        }

        static UIWindow OpenWindow(string winName, params object[] pars)
        {
            Type type = Type.GetType(winName);
            if (type == null)
            {
                Debugger.LogError("Open : " + winName + " window fail, the class " + winName + " don't exist!");
                return null;
            }

            return OpenWindow(type, pars);
        }

        static UIWindow OpenWindow(Type type, params object[] pars)
        {
            string winName = type.ToString();
#if UNITY_EDITOR
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Start();
#endif
            if (Instance._curOpenWindow == winName)
                return null;

            UIWindow win = GetCacheWindow(winName);
            if (win == null)
            {
                win = Activator.CreateInstance(type) as UIWindow;
                if (win != null)
                {
                    AddCacheWindow(win);
                }
                else
                {
                    Debugger.LogError("Open : " + winName + " window fail, create class " + winName + " fail!");
                    return null;
                }
            }
            else if (win.IsOpening)
            {
                Debugger.LogError("Then Window : " + winName + " is Already Opened");
                return null;
            }

            if (!win.Settings.IsHover)
            {
                bool find = UpdateOpenList(winName);
                CloseCurOpenWindow(find);
                Instance._curOpenWindow = winName;
            }
            else
            {
                Instance._openingHoverWindow.Add(win);
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
                    Debugger.LogError("Open : " + winName + " window fail, load prefab " + win.Settings.PrefabName + " fail!");
                    return null;
                }
            }

            bool rsl = win.Open(pars);
            if (!rsl)
            {
                return null;
            }

#if UNITY_EDITOR
            w.Stop();
            Debugger.Log("Open " + winName + " finish. Use time : " + w.ElapsedMilliseconds + " ms");
#endif
            return win;
        }

        public static bool ClosetWindow<T>() where T : UIWindow
        {
            string winName = typeof(T).ToString();
            return CloseWindow(winName);
        }

        static bool CloseWindow(string winName, bool user = true)
        {
            if (!Instance._windowCache.ContainsKey(winName))
                return false;

            UIWindow win = GetCacheWindow(winName);
            return CloseWindow(win, user);
        }

        public static bool CloseWindow(UIWindow win, bool user = true)
        {
            if (!user)
            {
                if (win.Settings.IsRecord)
                    AppendOpenWindow(win.Settings.WinName, win.Param);
            }

            if (win.Settings.IsHover)
                Instance._openingHoverWindow.Remove(win);

            bool b = win.Close();
            if (win.Settings.CloseDelete)
            {
                RemoveCacheWindow(win);
                win.Release();
                win = null;
            }
            else if (win.Settings.IsMultiple)
            {
                RemoveCacheWindow(win);
                AddCacheWindow(win);    //将关掉的窗口移到队列的末尾
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

        static void CloseCurOpenWindow(bool user = true)
        {
            if (string.IsNullOrEmpty(Instance._curOpenWindow))
                return;

            CloseWindow(Instance._curOpenWindow, user);
        }

        public static void CloseAllHoverWindow()
        {
            for (int i = Instance._openingHoverWindow.Count - 1; i >= 0; --i)
            {
                CloseWindow(Instance._openingHoverWindow[i]);
            }
        }

        static void AddCacheWindow(UIWindow window)
        {
            string winName = window.Settings.WinName;
            if (!Instance._windowCache.ContainsKey(winName))
            {
                Instance._windowCache.Add(winName, new List<UIWindow>());
            }
            Instance._windowCache[winName].Add(window);
        }

        static void RemoveCacheWindow(UIWindow window)
        {
            string winName = window.Settings.WinName;
            if (Instance._windowCache.ContainsKey(winName))
            {
                for (int i = 0; i < Instance._windowCache[winName].Count; ++i)
                {
                    var win = Instance._windowCache[winName][i];
                    if (win == window)
                    {
                        Instance._windowCache[winName].RemoveAt(i);
                        break;
                    }
                }

                if (Instance._windowCache[winName].Count == 0)
                {
                    Instance._windowCache.Remove(winName);
                }
            }
        }

        static UIWindow GetCacheWindow(string winName)
        {
            if (!Instance._windowCache.ContainsKey(winName))
            {
                return null;
            }
            
            for (int i = 0; i < Instance._windowCache[winName].Count; ++i)
            {
                var win = Instance._windowCache[winName][i];
                if (!win.Settings.IsHover)
                    return win;
                
                if (!win.Settings.IsMultiple)
                    return win;
                
                if (!win.IsOpening)
                    return win;
            }

            return null;
        }

        public static UIWindow GetOpeingWindow(string winName)
        {
            List<UIWindow> list = GetOpeingWindows(winName);
            if (list.Count > 0)
                return list[list.Count - 1];

            return null;
        }

        public static List<UIWindow> GetOpeingWindows(string winName)
        {
            List<UIWindow> list = new List<UIWindow>();
            if (Instance._windowCache.ContainsKey(winName))
            {
                for (int i = 0; i < Instance._windowCache[winName].Count; ++i)
                {
                    var win = Instance._windowCache[winName][i];
                    if (win.IsOpening)
                    {
                        list.Add(win);

                        if (!win.Settings.IsHover)
                        {
                            break;
                        }

                        if (!win.Settings.IsMultiple)
                        {
                            break;
                        }
                    }
                }
            }

            return list;
        }

        static GameObject LoadUIWindow(UIWindow win)
        {
            string cameraName = win.Settings.CameraName;
            if (string.IsNullOrEmpty(cameraName))
                cameraName = "Normal Camera";

            Camera camera;
            if (Instance._cameras.TryGetValue(cameraName, out camera))
            {
                try
                {
                    GameObject go = UnityEngine.Object.Instantiate(ResourceLoader.Load<GameObject>(win.Settings.PrefabName + ".prefab"));
                    Canvas canvas = go.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = Instance.HideCamera;
                    CanvasScaler scaler = go.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.screenMatchMode = win.Settings.screenMatchMode;
                    scaler.matchWidthOrHeight = win.Settings.matchWidthOrHeight;
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
    }
}
