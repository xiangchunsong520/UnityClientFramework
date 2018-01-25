using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class PreviewNode : UIObject
    {
        public PreviewNode(GameObject go) : base(go)
        {
            m_ActorImage = new SmallActor(GetChildGameObject("SmallActor"));
        }

        public void SetActor(string type)
        {
            
        }

        //public Image m_BackImage;
        public SmallActor m_ActorImage;
        public MapNode m_SelectedNode;
    }
}