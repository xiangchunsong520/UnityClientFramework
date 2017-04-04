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
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
        }
    }
}