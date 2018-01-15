using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class GameActor : UIObject
    {
        public uint actorId;
        public Actor data;
        public bool beingPull;
        public bool isSpecial;
        public GameActor attachActor; //盘子专用
        public MapNode m_OwnerNode;
        public virtual string DefaultImage
        {
            get
            {
                return "";
            }
        }

        public Actor ActorData
        {
            get
            {
                if (data != null)
                    return data;
                return GameDataExtenson.sDefaultActor;
            }
        }

        int YOffSet
        {
            get
            {
                return isSpecial ? ActorData.Specialyoffset : ActorData.Yoffset;
            }
        }

        protected Image image;

        public static GameActor CreateActorByType(string type, bool special = false)
        {
            //System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            //w.Start();

            GameActor pActor = null;
            GameObject go = UnityEngine.Object.Instantiate(WorldMap.sCurrentMap.actorpfb);
            go.transform.parent = WorldMap.sCurrentMap.actorpfb.transform.parent;
            go.transform.localScale = Vector3.one;
            //go.SetActive(true);

            Actor data = DataManager.Instance.actorDatas.GetActorData(type);
            if (data.Isjumao)
            {
                pActor = new JuMaoActor(go);
            }
            else if (data.Isfeimao)
            {
                pActor = new FeiMaoActor(go);
            }
            else
            {
                pActor = new GameActor(go);
            }
            pActor.LoadData(data, special);
            //w.Stop();
            //Debugger.LogError(w.ElapsedMilliseconds);
            return pActor;
        }

        public GameActor(GameObject go) : base(go)
        {
            image = GetChildComponent<Image>("Image");
        }

        public virtual bool Init()
        {
            return true;
        }

        public virtual void OnCreated()
        {

        }

        public virtual void OnDestroy()
        {

        }

        public void SetZOrder(int zorder)
        {
            transform.parent = WorldMap.sCurrentMap.actorRowParent[zorder];
        }

        /*
        public virtual void LoadData(string type, bool special = false)
        {
            LoadData(DataManager.Instance.actorDatas[type], special);
        }
        */

        public virtual void LoadData(Actor data, bool special = false)
        {
            this.data = data;
            if (special && !string.IsNullOrEmpty(data.SpecialImage))
            {
                image.sprite = UIAtlas.GetSprite("texturePack", data.SpecialImage + "-hd");
            }
            else if (!string.IsNullOrEmpty(data.Image))
            {
                image.sprite = UIAtlas.GetSprite("texturePack", data.Image + "-hd");
            }
                image.SetNativeSize();
            image.rectTransform.anchoredPosition = new Vector2(0, YOffSet);
        }

        public void Pull()
        {

        }

        public void StopEffect()
        {

        }

        public void Merge()
        {

        }

        public void Attach(string type, bool step = true)
        {

        }

        public void OnSpawn(float f)
        {

        }

        public void Shake()
        {

        }

        public void Heave()
        {

        }
    }
}
