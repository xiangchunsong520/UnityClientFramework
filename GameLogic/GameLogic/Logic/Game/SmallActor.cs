﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data;
using UnityEngine;

namespace GameLogic
{
    class SmallActor : GameActor
    {
        public SmallActor(GameObject go) : base(go)
        {
        }

        public override bool Init()
        {
            base.Init();
            image.transform.localScale = new Vector3(0.8f, 0.8f);
            return true;
        }

        public override void LoadData(Actor data, bool special = false)
        {
            this.data = data;
            if (!string.IsNullOrEmpty(data.SmallImage))
            {
                image.sprite = UIAtlas.GetSprite("texturePack", data.SmallImage + "-hd");
            }
            image.SetNativeSize();
            image.rectTransform.anchoredPosition = new Vector2(0, data.Smallyoffset * 2);
        }
    }
}
