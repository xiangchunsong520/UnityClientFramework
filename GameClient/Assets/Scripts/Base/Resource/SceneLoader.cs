/*
auth: Xiang ChunSong
purpose:
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base
{
    public class SceneLoader
    {
        public static void LoadScene(string name)
        {
            string key = ResourceManager.Instance.GetResourceKey(name + ".unity");
            if (!string.IsNullOrEmpty(key))
            {
                ResourceManager.Instance.LoadAssetBundle(key);
            }

            SceneManager.LoadScene(name, LoadSceneMode.Single);

            if (!string.IsNullOrEmpty(key))
            {
                TimerManager.Instance.AddFarmeTimer(1, () =>
                {
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                });
            }
        }

        public static void LoadSceneAdditive(string name)
        {
            string key = ResourceManager.Instance.GetResourceKey(name + ".unity");
            if (!string.IsNullOrEmpty(key))
            {
                ResourceManager.Instance.LoadAssetBundle(key);
            }

            SceneManager.LoadScene(name, LoadSceneMode.Additive);

            if (!string.IsNullOrEmpty(key))
            {
                TimerManager.Instance.AddFarmeTimer(1, () =>
                {
                    ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
                });
            }
        }

        public static SceneAsyncLoader LoadSceneAsync(string name)
        {
            return new SceneAsyncLoader(name, false);
        }

        public static SceneAsyncLoader LoadSceneAsyncAdditive(string name)
        {
            return new SceneAsyncLoader(name, true);
        }
    }

    public class SceneAsyncLoader : IDisposable
    {
        AssertBundleAsyncLoader _assetLoader = null;
        AsyncOperation _async = null;
        string _sceneName;
        bool _additive;

        float _lastShowProgress = 0;

        public float Progress
        {
            get
            {
                float progress = 0f;
                if (_assetLoader == null)
                {
                    if (_async == null)
                    {
                        progress = 0f;
                    }
                    else
                    {
                        progress = _async.progress;
                    }
                }
                else
                {
                    if (_async == null)
                    {
                        progress = _assetLoader.progress * 0.3f;
                    }
                    else
                    {
                        progress = _assetLoader.progress * 0.3f + _async.progress * 0.7f;
                    }
                }

                if (_lastShowProgress > progress)
                    progress = _lastShowProgress;
                _lastShowProgress = progress;

                return progress;
            }
        }

        public bool IsDone
        {
            get
            {
                if (_async == null)
                    return false;
                else
                    return _async.isDone;
            }
        }

        public SceneAsyncLoader(string name, bool additive)
        {
            _assetLoader = null;
            _async = null;
            _lastShowProgress = 0f;
            _sceneName = name;
            _additive = additive;
            GameClient.Instance.StartCoroutine(LoadScene());
        }

        IEnumerator LoadScene()
        {
            string key = ResourceManager.Instance.GetResourceKey(_sceneName + ".unity");
            if (!string.IsNullOrEmpty(key))
            {
                _assetLoader = new AssertBundleAsyncLoader();
                yield return ResourceManager.Instance.LoadAssetBundleAsync(key, _assetLoader);
                _assetLoader.progress = 1f;
            }

            yield return 0;

            _async = SceneManager.LoadSceneAsync(_sceneName, _additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }

        public void Dispose()
        {
            string key = ResourceManager.Instance.GetResourceKey(_sceneName + ".unity");
            if (!string.IsNullOrEmpty(key))
            {
                ResourceManager.Instance.RemoveUnreferenceAssetBundle(key);
            }
            _assetLoader = null;
            _async = null;

            GC.SuppressFinalize(this);
        }
    }
}