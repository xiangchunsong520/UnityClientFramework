using System;
using System.Collections.Generic;
using CW;
using System.IO;
using UnityEngine;

namespace GameLogic
{
    public delegate void HandleMsgCallback(MemoryStream ms);

    public class MsgDispatcher
    {
        private Dictionary<int, HandleMsgCallback> m_HandleMap = new Dictionary<int, HandleMsgCallback>();
        List<HandleMsgCallback> m_dispatchDelHandle = new List<HandleMsgCallback>();
        public void Register(PacketID id1, PacketID2 id2, HandleMsgCallback onHandleMsg)
        {
            int cmdID = (int)id1 << 16 | (int)id2;
            //Debugger.LogError("Register : " + cmdID);
            if (!m_HandleMap.ContainsKey(cmdID))
                m_HandleMap[cmdID] = onHandleMsg;
            else
                m_HandleMap[cmdID] += onHandleMsg;
        }

        public void Unregister(PacketID id1, PacketID2 id2, HandleMsgCallback onHandleMsg)
        {
            int cmdID = (int)id1 << 16 | (int)id2;
            if (m_HandleMap.ContainsKey(cmdID))
            {
                m_HandleMap[cmdID] -= onHandleMsg;
                if (m_HandleMap[cmdID] == null)
                    m_HandleMap.Remove(cmdID);
                m_dispatchDelHandle.Add(onHandleMsg);
            }
        }

        public void Dispatch(PacketHeader ph, MemoryStream ms)
        {
            /*if (GuideManager.Instance.IsGuiding)
            {
                string str = ph.id1 + "|" + ph.id2;
                ClientMsgDispatcher.Instance.Dispatch(MsgHandle.MH_Guide, MsgAction.MA_GuideTrigger, GuideTriggerType.GTT_GetServerMsg, str);
            }*/
            m_dispatchDelHandle.Clear();
            int cmdID = (int)ph.Id1 << 16 | (int)ph.Id2;
            HandleMsgCallback handler = null;
            if (m_HandleMap.TryGetValue(cmdID, out handler))
            {
                /*
                Delegate[] list = handler.GetInvocationList();
                Debugger.LogError(list.Length);
                for (int i = 0; i < list.Length; ++i)
                {
                    Delegate de = list[i];
                    try {
                        ms.Position = 0;
                        HandleMsgCallback callback = de as HandleMsgCallback;
                        Debugger.LogError(callback);
                        if (callback != null && !m_dispatchDelHandle.Contains(callback))
                        {
                            callback(ms);
                        }
                    }
                    catch (Exception e)
                    {
                        Debugger.LogError(e);
                    }
                }
                */
                try
                {
                    handler(ms);
                }
                catch (System.Exception ex)
                {
                    Debugger.LogException(ex);
                }
            }
#if UNITY_EDITOR
            else
            {
                Debugger.LogColor("FFFF00FF", "The msg call back not exist id1 : " + ph.Id1 + " id2 : " + ph.Id2);
            }
#endif
            m_dispatchDelHandle.Clear();
        }
    }
}
