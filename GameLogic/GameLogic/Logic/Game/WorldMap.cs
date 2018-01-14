using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameLogic
{
    class WorldMap : UIObject
    {
        GameObject prefab;
        public static WorldMap currentMap;
        public Map mapData;
        bool hasCat;
        List<PopNode> m_PopScoreActorList;
        List<RewardPopNode> m_RewardPopScoreActorList;
        List<List<MapNode>> m_RowList;
        //PreviewNode* m_PreviewNode;
        //MarkNode* m_MarkNode;
        //UIImage* m_ForbidImage;
        int m_Rows;
        int m_Cols;
        float m_XSize;
        float m_YSize;
        public int m_EnablePreviewCounter;
        string m_MapName;
        int m_CurrentScore;
        public int m_MaxScore;
        int m_TotalStep;
        string m_TileType;
        List<string> m_NextActorStack;
        //AnimEffectNode* m_BoompEffect;

        //UIImage* m_TutorialIndicatorBG;
        //CCSprite* m_TutorialIndicatorEffect;
        //int m_TutorialStep;
        bool m_GameOver;
        //vector<MapSnapShot> m_History;
        //set<MapNode*> m_MaoNodeList;//used for jumao move
        //set<MapNode*> m_AllCouldCombineNodeSet;
        //typedef map<string, int> RecordMap;
        //RecordMap m_ActorRecord;
        //RecordMap m_ShopItemLeft;
        public int m_ResetItemLeft;
        //vector<vector<CCPoint>> m_PeopleRoad;
        bool m_PanziDrawnBG;
        bool m_KongdiDrawnBG;

        public MapNode this[int row, int col]
        {
            get
            {
                if (row >= 0 && col >= 0 && row < m_Rows && col < m_Cols)
                {
                    return m_RowList[row][col];
                }
                return null;
            }
        }

        public WorldMap(GameObject go) : base(go)
        {
            prefab = GetChildGameObject("node");
            prefab.SetActive(false);
        }

        public void Clear()
        {

        }

        public bool Init()
        {
            return true;
        }

        public void Create(int Rows, int Cols, float XSize, float YSize, Map data)
        {
            m_Rows = Rows;
            m_Cols = Cols;
            m_XSize = XSize;
            m_YSize = YSize;
            mapData = data;
            m_RowList = new List<List<MapNode>>();
            for (int i = Rows - 1; i >= 0; --i)
            {
                List<MapNode> row = new List<MapNode>();
                for (int j = 0; j < Cols; ++j)
                {
                    GameObject go = UnityEngine.Object.Instantiate(prefab);
                    go.transform.parent = prefab.transform.parent;
                    go.transform.localScale = Vector3.one;
                    go.name = string.Format("node_{0}_{1}", i, j);
                    MapNode node = new MapNode(go, this, i, j);
                    row.Add(node);
                }
                m_RowList.Insert(0, row);
            }
            BuildNeighbours();
            OnCreated();
        }

        void OnCreated()
        {
            for (int i = 0; i < m_Rows; ++i)
            {
                for (int j = 0; j < m_Cols; ++j)
                {
                    this[i, j].OnCreated();
                }
            }
        }

        void BuildNeighbours()
        {
            for (int i = 0; i < m_Rows; i++)
            {
                for (int j = 0; j < m_Cols; j++)
                {
                    MapNode pNode = this[i, j];

                    pNode.Neighbours[(int)enumDirection.EDirection_Left]    = this[i, j - 1];
                    pNode.Neighbours[(int)enumDirection.EDirection_Up]      = this[i + 1, j];
                    pNode.Neighbours[(int)enumDirection.EDirection_Right]   = this[i, j + 1];
                    pNode.Neighbours[(int)enumDirection.EDirection_Down]    = this[i - 1, j];
                }
            }
        }

        void BuildNodes()
        {
            int XBlock = (int)m_XSize / m_Cols;
            int YBlock = (int)m_YSize / m_Rows;
            int X = 0, Y = 0;
            for (int i = 0; i < m_Rows; i++)
            {
                for (int j = 0; j < m_Cols; j++)
                {
                    X = XBlock * (j % m_Cols);
                    Y = YBlock * (i % m_Rows);
                    MapNode pNode = this[i, j];
                    pNode.transform.localPosition = new Vector3(X, Y, 0);
                    pNode.BuildNode();
                }
            }
        }

        public void Build()
        {
            BuildNeighbours();
            BuildNodes();
        }

        public void StepJuMao()
        {

        }

        public void StepFeiMao()
        {

        }

        public void GenerateRandomData()
        {

        }

        public void GenerateTutorialData()
        {
            this[5, 0].GenerateFilledData();
            this[5, 2].GenerateFilledData();
            this[5, 5].GenerateFilledData();
            this[4, 0].GenerateFilledData();
            this[4, 2].GenerateFilledData();
            this[4, 3].GenerateFilledData();
            this[4, 4].GenerateFilledData();
            this[3, 1].GenerateFilledData();
            this[3, 3].GenerateFilledData();
            this[3, 4].GenerateFilledData();
            this[2, 1].GenerateFilledData();
            this[2, 3].GenerateFilledData();
            this[2, 5].GenerateFilledData();
            this[1, 5].GenerateFilledData();
            BuildNodes();
            this[5, 0].CreateActor("panzi");
            this[5, 2].CreateActor("guanmu");
            this[5, 5].CreateActor("guanmu");
            this[4, 0].CreateActor("dashu");
            this[4, 2].CreateActor("xiaochao");
            this[4, 3].CreateActor("mubei");
            this[4, 4].CreateActor("mubei");
            this[3, 1].CreateActor("shitou");
            this[3, 3].CreateActor("xiaochao");
            this[3, 4].CreateActor("xiaochao");
            this[2, 1].CreateActor("xiaochao");
            this[2, 3].CreateActor("dashu");
            this[2, 5].CreateActor("guanmu");
            this[1, 5].CreateActor("maowu");
            BuildNodes();
        }

        public void Step(bool bFirstStep = false)
        {

        }

        public void ClearAllActorPullEffect()
        {

        }

        public void CombineAllNodes()
        {

        }

        public void ResetShopItemLeft()
        {

        }

        public void Save()
        {

        }
    }
}
