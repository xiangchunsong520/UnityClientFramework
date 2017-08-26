using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class MessageBox : UIWindow
    {
        Text _title;
        Text _content;
        Action<bool> _towkeyCallback;
        Action _onekeyCallback;

        GameObject _ok_0;
        GameObject _ok;
        GameObject _cancel;

        bool _onekey;

        protected override void OnSetWindow()
        {
            Settings.PrefabName = "UI/Install/MessageBox";
            Settings.CameraName = "Top Camera";
            Settings.IsHover = true;
            Settings.IsMultiple = true;
        }

        protected override void OnInit()
        {
            _title = GetChildComponent<Text>("Title");
            _content = GetChildComponent<Text>("Content");
            _ok_0 = GetChildGameObject("ButtonOK_0");
            _ok = GetChildGameObject("ButtonOK");
            _cancel = GetChildGameObject("ButtonCancel");

            EventTriggerListener.Get(_ok_0).onClick = OnClickOK;
            EventTriggerListener.Get(_ok).onClick = OnClickOK;
            EventTriggerListener.Get(_cancel).onClick = OnClickCancel;
        }

        protected override void OnOpen(object[] args)
        {
            if (args.Length != 3)
            {
                Debugger.LogError("MessageBox Error args!");
                _title.text = "Error!";
                _content.text = "MessageBox Error args!";
                return;
            }

            _onekey = true;
 
            _title.text = args[0] as string;
            _content.text = args[1] as string;
            if (args[2] == null)
            {
                _onekeyCallback = null;
                _towkeyCallback = null;
                _onekey = true;
            }
            else
            {
                if (args[2] is Action)
                {
                    _onekeyCallback = args[2] as Action;
                    _towkeyCallback = null;
                    _onekey = true;
                }
                else if (args[2] is Action<bool>)
                {
                    _onekeyCallback = null;
                    _towkeyCallback = args[2] as Action<bool>;
                    _onekey = false;
                }
            }

            _ok_0.SetActive(_onekey);
            _ok.SetActive(!_onekey);
            _cancel.SetActive(!_onekey);
        }

        void OnClickOK(GameObject go)
        {
            if (_onekeyCallback != null)
            {
                _onekeyCallback();
                _onekeyCallback = null;
            }

            if (_towkeyCallback != null)
            {
                _towkeyCallback(true);
                _towkeyCallback = null;
            }

            ReturnOrCloseSelf();
        }

        void OnClickCancel(GameObject go)
        {
            if (_onekeyCallback != null)
            {
                _onekeyCallback();
                _onekeyCallback = null;
            }

            if (_towkeyCallback != null)
            {
                _towkeyCallback(false);
                _towkeyCallback = null;
            }

            ReturnOrCloseSelf();
        }
    }
}
