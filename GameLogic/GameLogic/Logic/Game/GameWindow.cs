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
        WorldMap _worldMap;
        Image _imgBackground;
        Image _imgBackground2;
        Slider _sliderStep;
        Text _textStep;
        Text _textCoin;
        Text _textScore;
        Image _imgNext;
        GameObject _tipRoot;
        Text _textNeedNum;
        Image _imgSource;
        Image _imgTarget;

        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/GameWindow";
            Settings.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            Settings.matchWidthOrHeight = 0;
        }

        protected override void OnInit()
        {
            _worldMap = new WorldMap(GetChildGameObject("Top/Container"));
            _imgBackground = GetChildComponent<Image>("Top/Background");
            _imgBackground2 = GetChildComponent<Image>("Top/Background2");
            _sliderStep = GetChildComponent<Slider>("Top/SliderStep");
            _textStep = GetChildComponent<Text>("Top/TextStep");
            _textCoin = GetChildComponent<Text>("Top/TextCoin");
            _imgNext = GetChildComponent<Image>("Top/ImageNext");
            _tipRoot = GetChildGameObject("Top/TipRoot");
            _textNeedNum = GetChildComponent<Text>("Top/TipRoot/TextNeedNum");
            _imgSource = GetChildComponent<Image>("Top/TipRoot/ImageSource");
            _imgTarget = GetChildComponent<Image>("Top/TipRoot/ImageTarget");

            EventTriggerListener.Get(GetChildGameObject("Top/ButtonMain")).onClick = OnClickMain;
        }

        protected override void OnOpen(object[] args)
        {
            Map data = args[0] as Map;

            LoadMap(data);
        }

        void LoadMap(Map data)
        {
            Restart(data, true);

            _imgBackground.sprite = UIAtlas.GetSprite(data.MapAtlas, "bg_main-hd");
            _imgBackground2.color = Helper.PraseColor(data.MapColor);
            
        }

        bool Restart(Map data, bool loadSavedMap = false)
        {
            if (data.IsTutorial)
            {
                loadSavedMap = false;
            }

            bool reset = true;

            _worldMap.Clear();
            //_worldMap.mapData = data;
            _worldMap.m_EnablePreviewCounter = 0;
            _worldMap.Create(6, 6, 600, 600, data);
            if (loadSavedMap)
            {
                reset = false;
            }
            else
            {
                Debugger.LogError("TODO:Set Max score");
                _worldMap.m_MaxScore = 0;
            }

            if (data.IsTutorial)
            {
                _worldMap.GenerateTutorialData();
            }
            else if (!loadSavedMap || !reset)
            {
                _worldMap.ResetShopItemLeft();
                _worldMap.m_ResetItemLeft = 5;
                _worldMap.GenerateRandomData();
                _worldMap.Save();
            }

            _worldMap.Step(true);

            CheckGameOver();

            return reset;
        }

        public void OnStep()
        {

        }

        public void UpdateNextActor()
        {

        }

        public void JieSuan()
        {

        }

        public void CheckGameOver()
        {

        }

        void OnClickMain(GameObject go)
        {
            Debugger.LogError("TODO:Save map");
            ReturnOrCloseSelf();
        }

        void OnClickRestart(GameObject go)
        {
            Restart(_worldMap.mapData);
        }
    }
}
