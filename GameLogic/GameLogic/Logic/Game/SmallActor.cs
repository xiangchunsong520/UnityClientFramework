using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;
using UnityEngine;

namespace GameLogic
{
    class SmallActor : GameActor
    {
        public override bool Init()
        {
            base.Init();
            image.transform.localScale = new Vector3(0.8f, 0.8f);
            return true;
        }

        public override void LoadData(Actor data, bool special = false)
        {
            
        }
    }
}
