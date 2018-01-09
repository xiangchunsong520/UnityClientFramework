using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameLogic
{
    public class MonoBehaviourTest : MonoBehaviour
    {
        public int a;
        public float b;

        void Awake()
        {
            Debugger.LogError("MonoBehaviourTest:Awake()");
            Debugger.LogError(a);
            Debugger.LogError(b);
        }

        void OnEnable()
        {
            Debugger.LogError("MonoBehaviourTest:OnEnable()");
            Debugger.LogError(a);
            Debugger.LogError(b);
        }
    }
}
