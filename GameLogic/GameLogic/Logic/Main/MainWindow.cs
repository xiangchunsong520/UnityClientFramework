using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class MainWindow : UIWindow
    {
        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/MainWindow";
            Settings.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            Settings.matchWidthOrHeight = 0;
        }

        protected override void OnInit()
        {
            Transform ts = GetChildTransform("Top/MapIcons");
            //Transform[] childs = ts.child
            for (int i = 0; i < ts.childCount; ++i)
            {
                new MapItem(ts.GetChild(i).gameObject);
            }
        }

        protected override void OnOpen(object[] args)
        {
            StartUpdate();
        }

        protected override bool OnUpdate()
        {
            return false;
        }
    }

    class MapItem : UIObject
    {
        Map _data;
        Image _img;
        Text _text;
        GameObject _lock;
        bool _locked;

        public MapItem(GameObject go) : base(go)
        {
            if (DataManager.Instance.mapDatas.ContainsKey(go.name))
            {
                _data = DataManager.Instance.mapDatas.GetUnit(go.name);

                _img = GetComponen<Image>();
                _text = GetChildComponent<Text>("Text");
                _lock = GetChildGameObject("Image");
                EventTriggerListener.Get(go).onClick = OnClickItem;
                _locked = _data.UnlockCoin != 0;
                UpdateItem();
            }
        }

        void UpdateItem()
        {
            _lock.SetActive(_locked);
            _img.sprite = UIAtlas.GetSprite("Main", _data.Icon);
            _text.text = Helper.GetLanguage(_data.LanguageId);
        }

        void OnClickItem(GameObject go)
        {
            if (_data.IsTutorial)
            {
                UIManager.OpenWindow<GameWindow>(_data);
                return;
            }

            if (_locked)
            {
                UIManager.OpenWindow<LockMapWindow>(_data);
            }
            else
            {
                UIManager.OpenWindow<EnterMapWindow>(_data);
            }
        }
    }
}
