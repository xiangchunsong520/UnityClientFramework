using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class LockMapWindow : UIWindow
    {
        Map _data;
        Text _textName;
        Text _textUnlockDisc;
        Text _textUnlockCoin;

        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/LockMapWindow";
            Settings.IsHover = true;
        }

        protected override void OnInit()
        {
            _textName = GetChildComponent<Text>("TextName");
            _textUnlockDisc = GetChildComponent<Text>("TextDiscrib");
            _textUnlockCoin = GetChildComponent<Text>("ButtonUnlock/TextUnlockCoin");

            EventTriggerListener.Get(GetChildGameObject("ButtonReturn")).onClick = OnClickRetrun;
            EventTriggerListener.Get(GetChildGameObject("ButtonUnlock")).onClick = OnClickUnlock;
        }

        protected override void OnOpen(object[] args)
        {
            _data = args[0] as Map;

            UpdateInfo();
        }

        void UpdateInfo()
        {
            _textName.text = Helper.GetLanguage(_data.LanguageId);
            _textUnlockDisc.text = Helper.GetLanguage(_data.UnlockLanguage);
            _textUnlockCoin.text = _data.UnlockCoin.ToString();
        }

        void OnClickRetrun(GameObject go)
        {
            ReturnOrCloseSelf();
        }

        void OnClickUnlock(GameObject go)
        {

        }
    }
}
