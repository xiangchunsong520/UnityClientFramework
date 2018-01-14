using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace GameLogic
{
    class GameActor
    {
        public uint actorId;
        public Actor data;
        public bool beingPull;
        public bool isSpecial;
        public GameActor attachActor; //盘子专用
        public virtual string DefaultImage
        {
            get
            {
                return "";
            }
        }

        public string ActorType
        {
            get
            {
                return data.Name;
            }
        }

        protected Image image;

        public static GameActor CreateActorByType(string type, bool special = false)
        {
            return null;
        }

        public GameActor()
        {

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

        public virtual void LoadData(string type, bool special = false)
        {
            LoadData(DataManager.Instance.actorDatas[type], special);
        }

        public virtual void LoadData(Actor data, bool special = false)
        {

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
