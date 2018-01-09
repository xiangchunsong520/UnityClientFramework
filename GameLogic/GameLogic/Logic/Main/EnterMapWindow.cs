using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class EnterMapWindow : UIWindow
    {
        Map _data;
        Text _textName;
        Text _textHighScore;
        Text _textDiscrib;
        Text _textMode;
        Image _imgIcon;

        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/EnterMapWindow";
            Settings.CameraName = "Top Camera";
            Settings.IsHover = true;
        }

        protected override void OnInit()
        {
            _textName = GetChildComponent<Text>("TextName");
            _textHighScore = GetChildComponent<Text>("TextHighScore");
            _textDiscrib = GetChildComponent<Text>("TextDiscrib");
            _textMode = GetChildComponent<Text>("TextMode");
            _imgIcon = GetChildComponent<Image>("ImageIcon");

            EventTriggerListener.Get(GetChildGameObject("ButtonReturn")).onClick = OnClickRetrun;
            EventTriggerListener.Get(GetChildGameObject("ButtonEnter")).onClick = OnClickEnterMap;
        }

        protected override void OnOpen(object[] args)
        {
            _data = args[0] as Map;

            UpdateInfo();
        }

        void UpdateInfo()
        {
            _textName.text = Helper.GetLanguage(_data.LanguageId);
            _textHighScore.text = "0";
            _textDiscrib.text = Helper.GetLanguage(_data.DiscribeLanguage);
            _textMode.text = Helper.GetLanguage(_data.ModeLanguage);
            _imgIcon.sprite = UIAtlas.GetSprite("Main/EnterMap", _data.EnterIcon);
        }

        void OnClickRetrun(GameObject go)
        {
            ReturnOrCloseSelf();
        }

        void OnClickEnterMap(GameObject go)
        {
            ReturnOrCloseSelf();
            UIManager.OpenWindow<GameWindow>(_data);
        }
    }
}
