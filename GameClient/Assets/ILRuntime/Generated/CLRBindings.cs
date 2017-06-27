using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Int32_Binding.Register(app);
            System_UInt32_Binding.Register(app);
            System_Int16_Binding.Register(app);
            System_UInt16_Binding.Register(app);
            System_SByte_Binding.Register(app);
            System_Byte_Binding.Register(app);
            System_Single_Binding.Register(app);
            System_Double_Binding.Register(app);
            System_Int64_Binding.Register(app);
            System_UInt64_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_String_Binding.Register(app);
            System_IO_MemoryStream_Binding.Register(app);
            System_DateTime_Binding.Register(app);
            System_Diagnostics_Stopwatch_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Collections_Hashtable_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            UnityEngine_Vector4_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_MonoBehaviour_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_RectTransform_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            UnityEngine_GUILayout_Binding.Register(app);
            UnityEngine_GUIStyle_Binding.Register(app);
            UnityEngine_GUI_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            //Debugger_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_ILTypeInstance_ILTypeInstance_Binding.Register(app);
        }
    }
}
