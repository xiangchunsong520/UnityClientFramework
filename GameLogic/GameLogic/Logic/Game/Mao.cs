using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;
using UnityEngine;

namespace GameLogic
{
    class Mao : GameActor
    {
        public bool alive;

        public Mao(GameObject go) : base(go)
        {
            alive = false;
        }

        public override void LoadData(Actor data, bool special = false)
        {
            base.LoadData(data, special);
            alive = true;
        }

        public virtual void Step() { }
        public virtual void onExit() { }
        public  virtual void PerformAction() { }
        public virtual bool CalcIsAlive() { return true; }
    }

    class JuMaoActor : Mao
    {
        public JuMaoActor(GameObject go) : base(go)
        {

        }
    }

    class FeiMaoActor : Mao
    {
        public FeiMaoActor(GameObject go) : base(go)
        {

        }
    }
}
