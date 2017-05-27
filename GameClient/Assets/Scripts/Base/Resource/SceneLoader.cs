/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine.SceneManagement;

namespace Base
{
    public class SceneLoader
    {
        public static void LoadSceneAdditive(string name)
        {
            string key = ResourceManager.Instance.GetResourceKey(name + ".unity");
            if (!string.IsNullOrEmpty(key))
            {
                ResourceManager.Instance.LoadAssetBundle(key);
            }
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
        }
    }
}