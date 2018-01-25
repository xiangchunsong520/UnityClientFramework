using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    class Game : Singleton<Game>
    {
        public void Init()
        {

        }

        public string GetNextActorType()
        {
            return "";
        }
        
        bool IsLargerRecipeType(string type1, string type2)
        {
            /*
            string tmp = type2;
            while (tmp.length() > 0)
            {
                const Recipe&larger = m_RecipeList.FindRecipeBySourceType(tmp);
                if (larger.SourceType.length() > 0)
                {
                    tmp = larger.TargetType;
                }
                else
                {
                    tmp = "";
                }
                if (tmp == type1)
                {
                    return true;
                }
            }
            */
            return false;
        }

        public Actor CurrentActor()
        {
            return DataManager.Instance.actorDatas.GetActorData(m_CurrentActorType);
        }

        public void SetCurrentActorType(string type)
        {
            string currentActorType = m_CurrentActorType;
            m_CurrentActorType = type;
            if (WorldMap.sCurrentMap != null && WorldMap.sCurrentMap.m_PreviewNode != null)
            {
                WorldMap.sCurrentMap.m_PreviewNode.m_SelectedNode = null;
                WorldMap.sCurrentMap.m_PreviewNode.SetActor(Game.Instance.m_CurrentActorType);
                WorldMap.sCurrentMap.m_PreviewNode.gameObject.SetActive(false);
                GameWindow.s_GameForm.UpdateNextActor();
            }
            if (WorldMap.sCurrentMap != null)
            {
                WorldMap.sCurrentMap.Save();
            }
        }

        public bool gift()
        {
            return false;
        }

        string m_CurrentActorType;

        public string getRandomTips()
        {
            return "";
        }
    }
}
