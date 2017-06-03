/*
auth: Xiang ChunSong
purpose:
*/

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
        }

        public static void LoadSceneAdditive(string name)
        {
            string key = ResourceManager.Instance.GetResourceKey(name + ".unity");
            if (!string.IsNullOrEmpty(key))
            {
                ResourceManager.Instance.LoadAssetBundle(key);
            }
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
        }

        public static SceneAsyncLoader LoadSceneAsync(string name)
        {
            GameObject go = new GameObject("SceneAsyncLoader");
            SceneAsyncLoader sal = go.AddComponent<SceneAsyncLoader>();
            sal.LoadSceneAsync(name, false);
            return sal;
        }

        public static SceneAsyncLoader LoadSceneAsyncAdditive(string name)
        {
            GameObject go = new GameObject("SceneAsyncLoader");
            SceneAsyncLoader sal = go.AddComponent<SceneAsyncLoader>();
            sal.LoadSceneAsync(name, true);
            return sal;
        }
    }

    public class SceneAsyncLoader : MonoBehaviour
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
                        progress = _assetLoader.progress * 0.7f;
                    }
                    else
                    {
                        progress = _assetLoader.progress * 0.7f + _async.progress * 0.3f;
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

        void Awake()
        {
            _assetLoader = null;
            _async = null;
            _lastShowProgress = 0f;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadSceneAsync(string name, bool additive)
        {
            _sceneName = name;
            _additive = additive;
            StartCoroutine(LoadScene());
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
    }
}