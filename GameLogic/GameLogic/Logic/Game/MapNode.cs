using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    /*
    以下为布局图
    7 8 9
    4 5 6
    1 2 3
    */
    enum enumNodeType
    {
        ENodeType_None,
        ENodeType_Fill,             //实心死路
        ENodeType_NoWay,            //空心无通路

        ENodeType_1WayRight,        //右侧单通路
        ENodeType_1WayLeft,         //左侧单通路
        ENodeType_1WayUp,           //上侧单通路
        ENodeType_1WayDown,         //下侧单通路

        ENodeType_2WayUpDown,       //上下侧双通路
        ENodeType_2WayLeftRight,    //左右侧双通路

        ENodeType_2WayDownRight,    //下右侧双通路
        ENodeType_2WayDownLeft,     //下左侧双通路
        ENodeType_2WayTopRight,     //上右侧双通路
        ENodeType_2WayTopLeft,      //下左侧双通路

        ENodeType_3WayRight,        //右侧3通路
        ENodeType_3WayLeft,         //左3通路
        ENodeType_3WayDown,         //下侧3通路
        ENodeType_3WayUp,           //上侧3通路
        ENodeType_MAX
    }

    enum enumDirection
    {
        EDirection_Left,
        EDirection_Up,
        EDirection_Right,
        EDirection_Down,
        EDirection_Max,
    }

    enum enumTileType
    {
        ETileType_None,
        ETileType_A,
        ETileType_B,
        ETileType_C,
        ETileType_D,
        ETileType_MAX,
    }

    class MapNode : UIObject
    {
        public WorldMap ownerMap;
        int m_Row;
        int m_Col;
        bool m_Filled;
        Image m_BackImage;
        Image[] m_CornerImage = new Image[4];
        GameActor m_Actor;
        GameActor m_TmpActor;
        Actor m_OverrideActorType;
        public MapNode[] Neighbours = new MapNode[4];
        enumNodeType m_OldNodeType;
        enumNodeType m_NodeType;
        Actor ActorType
        {
            get
            {
                if (m_OverrideActorType != null)
                    return m_OverrideActorType;

                if (m_Actor != null)
                    return m_Actor.data;

                return GameDataExtenson.sDefaultActor;
            }
        }

        public int ActorZOrder
        {
            get
            {
                return m_Row;// 100 - m_Row * 2;
            }
        }

        public MapNode(GameObject go, WorldMap map, int row, int col) : base(go)
        {
            ownerMap = map;
            m_Row = row;
            m_Col = col;
            m_BackImage = GetChildComponent<Image>("ImageBG");
            for (int i = 0; i < m_CornerImage.Length; ++i)
            {
                m_CornerImage[i] = GetChildComponent<Image>("ImageCorner_" + i);
                System.Random rd = new System.Random();
                string img = rd.Next(2) == 1 ? "tile19-hd" : "tile20-hd";
                m_CornerImage[i].sprite = UIAtlas.GetSprite(ownerMap.mapData.MapAtlas, img);
                m_CornerImage[i].SetNativeSize();
                m_CornerImage[i].gameObject.SetActive(false);
            }
        }

        public virtual void OnCreated()
        {
            gameObject.SetActive(true);
        }


        bool IsFill(enumDirection dir)
        {
            return Neighbours[(int)dir] == null || Neighbours[(int)dir].m_Filled;
        }

        bool NotFill(enumDirection dir)
        {
            return Neighbours[(int)dir] != null && !Neighbours[(int)dir].m_Filled;
        }
        bool HasLeftBottomCorner()
        {
            if (NotFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Down))
            {
                MapNode pNode = ownerMap[m_Row - 1, m_Col - 1];
                if (pNode == null || pNode.m_Filled)
                {
                    return true;
                }
            }
            return false;
        }

        bool HasLeftTopCorner()
        {
            if (NotFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up))
            {
                MapNode pNode = ownerMap[m_Row + 1, m_Col - 1];
                if (pNode == null || pNode.m_Filled)
                {
                    return true;
                }
            }
            return false;
        }

        bool HasRightTopCorner()
        {
            if (NotFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Up))
            {
                MapNode pNode = ownerMap[m_Row + 1, m_Col + 1];
                if (pNode == null || pNode.m_Filled)
                {
                    return true;
                }
            }
            return false;
        }

        bool HasRightBottomCorner()
        {
            if (NotFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                MapNode pNode = ownerMap[m_Row - 1, m_Col + 1];
                if (pNode == null || pNode.m_Filled)
                {
                    return true;
                }
            }
            return false;
        }

        bool IsFreeNodeForPlace()
        {
            //return !m_Filled && !(ActorType.IsMao()) && m_Actor->getIsVisible());
            return true;
        }

        public void BuildNode()
        {
            //gameObject.SetActive(true);
            m_OldNodeType = m_NodeType;
            m_NodeType = enumNodeType.ENodeType_None;
            if (m_Filled)//实心死路
            {
                if (ActorType.Iskongdi && !WorldMap.sCurrentMap.mapData.KongdiDrawBG || ActorType.Ispanzi && !WorldMap.sCurrentMap.mapData.PanziDrawBG)
                {
                    m_NodeType = enumNodeType.ENodeType_None;
                }
                else
                    m_NodeType = enumNodeType.ENodeType_Fill;
            }
            else if (IsFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_NoWay;//空心无通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_1WayRight;//右侧单通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_1WayLeft;//左侧单通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_1WayLeft;//左侧单通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_1WayUp;//上侧单通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_1WayDown;//下侧单通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_2WayUpDown;//上下侧双通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_2WayLeftRight;//左右侧双通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_2WayDownRight;//下右侧双通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_2WayDownLeft;//下左侧双通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_2WayTopRight;//上右侧双通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_2WayTopLeft;//下左侧双通路
            }
            else if (IsFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_3WayRight;//右侧3通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && IsFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_3WayLeft;//左3通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && IsFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && NotFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_3WayDown;//下侧3通路
            }
            else if (NotFill(enumDirection.EDirection_Left) && NotFill(enumDirection.EDirection_Up) && NotFill(enumDirection.EDirection_Right) && IsFill(enumDirection.EDirection_Down))
            {
                m_NodeType = enumNodeType.ENodeType_3WayUp;//上侧3通路
            }

            UpdateResources();
            ApplyFormatting();
        }

        void UpdateResources()
        {
            string texturefile = null;
            switch (m_NodeType)
            {
                case enumNodeType.ENodeType_Fill://实心死路
                    texturefile = "tile2-hd";
                    break;
                case enumNodeType.ENodeType_NoWay://空心无通路
                    if (m_OldNodeType != m_NodeType)
                    {
                        System.Random rd = new System.Random();
                        texturefile =  (rd.Next(2) == 1) ? "tile3-hd" : "tile4-hd";
                    }
                    break;

                case enumNodeType.ENodeType_1WayRight://右侧单通路
                    texturefile = "tile5-hd";
                    break;
                case enumNodeType.ENodeType_1WayLeft://左侧单通路
                    texturefile = "tile6-hd";
                    break;
                case enumNodeType.ENodeType_1WayUp://上侧单通路
                    texturefile = "tile7-hd";
                    break;
                case enumNodeType.ENodeType_1WayDown://下侧单通路
                    texturefile = "tile8-hd";
                    break;

                case enumNodeType.ENodeType_2WayUpDown://上下侧双通路
                    texturefile = "tile9-hd";
                    break;
                case enumNodeType.ENodeType_2WayLeftRight://左右侧双通路
                    texturefile = "tile10-hd";
                    break;

                case enumNodeType.ENodeType_2WayDownRight://下右侧双通路
                    texturefile = "tile11-hd";
                    break;
                case enumNodeType.ENodeType_2WayDownLeft://下左侧双通路
                    texturefile = "tile12-hd";
                    break;
                case enumNodeType.ENodeType_2WayTopRight://上右侧双通路
                    texturefile = "tile13-hd";
                    break;
                case enumNodeType.ENodeType_2WayTopLeft://下左侧双通路
                    texturefile = "tile14-hd";
                    break;

                case enumNodeType.ENodeType_3WayRight://右侧3通路
                    texturefile = "tile15-hd";
                    break;
                case enumNodeType.ENodeType_3WayLeft://左3通路
                    texturefile = "tile16-hd";
                    break;
                case enumNodeType.ENodeType_3WayDown://下侧3通路
                    texturefile = "tile17-hd";
                    break;
                case enumNodeType.ENodeType_3WayUp://上侧3通路
                    texturefile = "tile18-hd";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(texturefile))
            {
                m_BackImage.sprite = UIAtlas.GetSprite(ownerMap.mapData.MapAtlas, texturefile);
                m_BackImage.SetNativeSize();
                m_BackImage.gameObject.SetActive(true);
            }
            else if (enumNodeType.ENodeType_None == m_NodeType)
            {
                //m_BackImage->setIsVisible(false);
                m_BackImage.gameObject.SetActive(false);
            }
        }

        void ApplyFormatting()
        {
            switch (m_NodeType)
            {
                case enumNodeType.ENodeType_3WayLeft://左3通路
                    m_BackImage.rectTransform.anchorMax = new Vector2(1, 0);
                    m_BackImage.rectTransform.anchorMin = new Vector2(1, 0);
                    m_BackImage.rectTransform.pivot = new Vector2(1, 0);
                    m_BackImage.rectTransform.anchoredPosition = new Vector2(0, 0);
                    break;
                case enumNodeType.ENodeType_3WayDown://下侧3通路
                    m_BackImage.rectTransform.anchorMax = new Vector2(0, 1);
                    m_BackImage.rectTransform.anchorMin = new Vector2(0, 1);
                    m_BackImage.rectTransform.pivot = new Vector2(0, 1);
                    m_BackImage.rectTransform.anchoredPosition = new Vector2(0, 0);
                    break;
                default:
                    m_BackImage.rectTransform.anchorMax = new Vector2(0, 0);
                    m_BackImage.rectTransform.anchorMin = new Vector2(0, 0);
                    m_BackImage.rectTransform.pivot = new Vector2(0, 0);
                    m_BackImage.rectTransform.anchoredPosition = new Vector2(0, 0);
                    break;
            }
            
            for (int i = 0; i < 4; i++)
            {
                m_CornerImage[i].gameObject.SetActive(false);
            }
            if (!m_Filled)
            {
                if (HasLeftBottomCorner())
                {
                    m_CornerImage[0].gameObject.SetActive(true);
                }
                if (HasLeftTopCorner())
                {
                    m_CornerImage[1].gameObject.SetActive(true);
                }
                if (HasRightTopCorner())
                {
                    m_CornerImage[2].gameObject.SetActive(true);
                }
                if (HasRightBottomCorner())
                {
                    m_CornerImage[3].gameObject.SetActive(true);
                }
            }
        }

        public void GenerateFilledData()
        {
            m_Filled = true;
        }

        public GameActor CreateActor(string type, float FadeInTime = -1.0f, bool bTmp = false, bool Special = false)
        {
            Actor data = DataManager.Instance.actorDatas.GetActorData(type);
            if (!m_Filled && !data.IsMao())
            {
                return null;
            }
            if (bTmp && data.IsMao())
            {
                return null;
            }
            int zorder = ActorZOrder;
            GameActor pActor = bTmp ? m_TmpActor : m_Actor;
            if (pActor != null)
            {
                if ((data.IsMao() || pActor.ActorData.IsMao())
                && !string.Equals(type, pActor.ActorData.Name))
                {
                    pActor.OnDestroy();
                    //pActor.removeFromParentAndCleanup(true);
                    UnityEngine.Object.DestroyImmediate(pActor.gameObject);
                    pActor = null;
                    if (m_TmpActor != null)
                    {
                        UnityEngine.Object.DestroyImmediate(m_TmpActor.gameObject);
                        //m_TmpActor->removeFromParentAndCleanup(true);
                        m_TmpActor = null;
                    }
                }
                else
                {
                    pActor.OnCreated();
                    pActor.LoadData(data, Special);
                    pActor.isSpecial = Special;
                    //pActor.reorder(zorder); 
                    pActor.SetZOrder(zorder);
                }
            }
            if (pActor == null)
            {
                pActor = GameActor.CreateActorByType(type, Special);
                //ownerMap->addChild(pActor, zorder);
                pActor.SetZOrder(zorder);
                pActor.isSpecial = Special;
                pActor.OnCreated();
            }

            pActor.m_OwnerNode = this;
            if (pActor != null)
            {
                pActor.transform.localPosition = transform.localPosition;
                pActor.gameObject.SetActive(true);
                //pActor->setPosition(ccp(getPosition().x + GetCenterPos().x, getPosition().y + GetCenterPos().y + pActor->GetYOffset()));
                //pActor->setOpacity(255);
                //pActor->setIsVisible(true);
                if (FadeInTime > 0)
                {
                    Debugger.Log("TODO:set Opacity fade in");
                    //pActor->setOpacity(0);
                    //pActor->runAction(CCFadeIn::actionWithDuration(FadeInTime));
                }
            }
            return pActor;
        }
    }
}
