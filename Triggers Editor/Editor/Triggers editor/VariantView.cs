using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

namespace TriggerEditor
{
    static public class VariantView
    {
        static public VariantData VariantDataField(VariantData variantData)
        {
            Type variantType = variantData.GetVariantType();
            if (variantType == typeof(Trigger))
            {
                return VariantDataTrigger(variantData);
            }
            else if (variantType == typeof(bool))
            {
                return new VariantData(EditorGUILayout.Toggle(variantData.GetData<bool>()), typeof(bool));
            }
            else if (variantType == typeof(int))
            {
                return new VariantData(EditorGUILayout.IntField(variantData.GetData<int>()), typeof(int));
            }
            else if (variantType == typeof(float))
            {
                return new VariantData(EditorGUILayout.FloatField(variantData.GetData<float>()), typeof(float));
            }
            else if (variantType == typeof(string))
            {
                return new VariantData(EditorGUILayout.TextArea(variantData.GetData<string>()), typeof(string));
            }
            else if (variantType == typeof(Vector2))
            {
                return new VariantData(EditorGUILayout.Vector2Field("", variantData.GetData<Vector2>()), typeof(Vector2));
            }
            else if (variantType == typeof(Vector3))
            {
                return new VariantData(EditorGUILayout.Vector3Field("", variantData.GetData<Vector3>()), typeof(Vector3));
            }
            else if (variantType == typeof(Vector4))
            {
                return new VariantData(EditorGUILayout.Vector4Field("", variantData.GetData<Vector4>()), typeof(Vector4));
            }
            else if (variantType.IsEnum)
            {
                return new VariantData(EditorGUILayout.EnumPopup(variantData.GetData<Enum>()), variantType);
            }
            else
            {
                return VariantDataUnity(variantData);
            }
        }

        static public VariantData VariantDataTrigger(VariantData variantData)
        {
            if ((TriggersManager.triggers == null) || (TriggersManager.triggers.Length == 0)) return new VariantData(null, typeof(Trigger));

            string[] triggerNames = TriggersManager.GetTriggersNames();
            int triggerIndex = 0;
            for (int i = 0; i < triggerNames.Length; i++)
            {
                Trigger tmp = variantData.GetData<Trigger>();
                if ((tmp != null) && (triggerNames[i] == tmp.name))
                {
                    triggerIndex = i;
                }
            }
            triggerIndex = EditorGUILayout.Popup(triggerIndex, triggerNames);
            return new VariantData(TriggersManager.triggers[triggerIndex], typeof(Trigger));
        }

        static public VariantData VariantDataUnity<T>(VariantData variantData) where T : UnityEngine.Object
        {
            return new VariantData((T)EditorGUILayout.ObjectField(variantData.GetData<T>(), typeof(T), true), typeof(T));
        }

        static public VariantData VariantDataUnity(VariantData variantData)
        {
            Type variantType = variantData.GetVariantType();
            UnityEngine.Object tmp = EditorGUILayout.ObjectField((UnityEngine.Object)variantData.rawData, variantType, true);
            return new VariantData(tmp, variantType);
        }
    }
}