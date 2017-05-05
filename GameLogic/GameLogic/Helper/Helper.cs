/*
auth: Xiang ChunSong
purpose:
*/

namespace GameLogic
{
    public class Helper
    {
        public static string GetLanguage(int id)
        {
            if (DataManager.Instance.languageDatas.ContainsKey(id))
            {
                string str = DataManager.Instance.languageDatas.GetUnit(id).Text;
                str = str.Replace("\\n", "\n");
                return str;
            }
            return string.Format("[ff0000]id:{0}[-]", id);
        }
    }
}
