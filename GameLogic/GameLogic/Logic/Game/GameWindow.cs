using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class GameWindow : UIWindow
    {
        Map _data;

        Image _imgBackground;
        Image _imgBackground2;

        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/GameWindow";
            Settings.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            Settings.matchWidthOrHeight = 0;
        }

        protected override void OnInit()
        {
            _imgBackground = GetChildComponent<Image>("Top/Background");
            _imgBackground2 = GetChildComponent<Image>("Top/Background2");

            EventTriggerListener.Get(GetChildGameObject("Top/ButtonMain")).onClick = OnClickMain;
        }

        protected override void OnOpen(object[] args)
        {
            _data = args[0] as Map;

            InitMap();
        }

        void InitMap()
        {
            _imgBackground.sprite = UIAtlas.GetSprite(_data.MapAtlas, "bg_main-hd");
            _imgBackground2.color = Helper.PraseColor(_data.MapColor);
        }

        void OnClickMain(GameObject go)
        {
            ReturnOrCloseSelf();
        }
    }
}
