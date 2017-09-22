using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.AI;
using System.Collections.Generic;

namespace TriggerEditor
{
    [Serializable]
    public class Variant
    {
        [SerializeField] private string m_typeName;

        [SerializeField] private VariantData m_data;
        [SerializeField] private VariantData[] m_dataArray;
        [SerializeField] bool m_isArray;

        #region Constructors
        public Variant(object data, Type type)
        {
            Setup(data, type);
        }

        public Variant(Type type)
        {
            Setup(GetTypeDefaultValue(type), type);
        }

        public void Setup(object data, Type type)
        {
            m_typeName = type.AssemblyQualifiedName;
            m_isArray = type.IsArray;

            if (type.IsArray)
            {
                m_data = new VariantData(data, type.GetElementType());
                m_dataArray = new VariantData[1];
                m_dataArray[0] = new VariantData(m_data);
            }
            else
            {
                m_data = new VariantData(data, type);
            }
        }

        public Variant(Variant variant)
        {
            m_data = new VariantData(variant.m_data);
            if(variant.m_dataArray != null)
            {
                m_dataArray = new VariantData[variant.m_dataArray.Length];
                for(int i = 0; i < m_dataArray.Length; i++)
                {
                    m_dataArray[i] = new VariantData(variant.m_dataArray[i]);
                }
            }
            m_isArray = variant.m_isArray;
            m_typeName = variant.m_typeName;
        }

        public Variant(VariantData variantData, string typeName)
        {
            m_isArray = false;
            m_data = new VariantData(variantData);
            m_dataArray = null;
            m_typeName = typeName;
        }

        public Variant(VariantData[] variantDataArray, VariantData defaultData, string typeName)
        {
            m_isArray = true;
            m_data = new VariantData(defaultData);
            if(variantDataArray == null)
            {
                m_dataArray = null;
            }
            else
            {
                m_dataArray = new VariantData[variantDataArray.Length];
                for (int i = 0; i < m_dataArray.Length; i++)
                {
                    m_dataArray[i] = new VariantData(variantDataArray[i]);
                }
            }
            m_typeName = typeName;
        }
        #endregion

        #region Data
        public Type GetVariantType()
        {
            if (m_isArray) return Type.GetType(m_typeName).GetElementType();
            return Type.GetType(m_typeName);
        }

        public T GetData<T>()
        {
            return (T)rawData;
        }

        public object rawData
        {
            get
            {
                if(m_isArray)
                {
                    object[] result = new object[m_dataArray.Length];
                    for (int i = 0; i < m_dataArray.Length; i++)
                    {
                        result[i] = m_dataArray[i].rawData;
                    }
                    Array array = Array.CreateInstance(GetVariantType(), m_dataArray.Length);
                    Array.Copy(result, array, m_dataArray.Length);
                    return array;
                }
                else
                {
                    return m_data.rawData;
                }
            }
        }

        static public object[] GetVariantsRawData(Variant[] variants)
        {
            object[] result = new object[variants.Length];
            for (int i = 0; i < variants.Length; i++)
            {
                result[i] = variants[i].rawData;
            }
            return result;
        }

        static public Variant[] GetVariantsParameters(MethodInfo methodInfo)
        {
            ParameterInfo[] parametersInfos = methodInfo.GetParameters();
            Variant[] parameters = new Variant[parametersInfos.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parametersInfos[i].GetType();

                parameters[i] = new Variant(GetTypeDefaultValue(parameterType), parameterType);
            }
            return parameters;
        }

        static public object GetTypeDefaultValue(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Accessors
        public bool isArray
        {
            get { return m_isArray; }
        }

        public string typeName
        {
            get { return m_typeName; }
        }

        public VariantData variantData
        {
            get { return m_data; }
        }

        public VariantData[] variantDataArray
        {
            get { return m_dataArray; }
        }

        public Color typeColor
        {
            get
            {
                Type variantType = variantData.GetVariantType();
                if (variantType == typeof(Trigger))
                {
                    return new Color(0.8f, 0.5f, 0.1f);
                }
                else if(variantType == typeof(Node))
                {
                    return Color.white;
                }
                else if (variantType == typeof(bool))
                {
                    return new Color(0.85f, 0.7f, 0.6f);
                }
                else if (variantType == typeof(int))
                {
                    return new Color(0.9f, 0.9f, 0.1f);
                }
                else if (variantType == typeof(float))
                {
                    return new Color(0.4f, 0.4f, 0.6f);
                }
                else if (variantType == typeof(string))
                {
                    return new Color(0f, 0f, 0.5f);
                }
                else if ((variantType == typeof(Vector2)) || (variantType == typeof(Vector3)) || (variantType == typeof(Vector4)))
                {
                    return new Color(0.4f, 0f, 1f);
                }
                else
                {
                    return new Color(0.1f, 0.9f, 0.5f);
                }
            }
        }
        #endregion

        #region Operators
        public override bool Equals(object obj)
        {
            Variant b = (Variant)obj;

            if (m_isArray != b.m_isArray) return false;

            Type aType = GetVariantType();
            Type bType = b.GetVariantType();

            if (aType != bType) return false;

            if (m_isArray)
            {
                if ((m_dataArray == null) && (b.m_dataArray != null))
                {
                    return false;
                }
                else
                {
                    if (b.m_dataArray == null) return false;
                    else if (m_dataArray.Length != b.m_dataArray.Length) return false;
                    else
                    {
                        for (int i = 0; i < m_dataArray.Length; i++)
                        {
                            if (m_dataArray[i] != b.m_dataArray[i]) return false;
                        }
                        return true;
                    }
                }
            }
            else
            {
                return m_data == b.m_data;
            }
        }

        public override int GetHashCode()
        {
            return m_data.GetHashCode() + m_typeName.GetHashCode();
        }

        public static bool operator ==(Variant a, Variant b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Variant a, Variant b)
        {
            return !(a == b);
        }
        #endregion
    }

    [Serializable]
    public class VariantData
    {
        public enum VariantGeneralType
        {
            SerializableByCSharp,
            MonoBehaviour,
            UnityObject,
            Vector,
            Quaternion,
            IEnumerator,
            Null,
            Other
        }

        [SerializeField] private string m_typeName;

        [SerializeField] private byte[] m_data;
        [SerializeField] private Vector4 m_vector;
        [SerializeField] private Quaternion m_quaternion;
        [SerializeField] private UnityEngine.Object m_unityObject;
        [SerializeField] private MonoBehaviour m_monoBehaviour;
        [SerializeField] private IEnumerator m_iEnumator;

        #region Constructors
        public VariantData(object data, Type type)
        {
            m_typeName = type.AssemblyQualifiedName;
            VariantGeneralType variantGeneralType = VariantGeneralTypeOf(type);
            switch (variantGeneralType)
            {
                case VariantGeneralType.SerializableByCSharp:
                    m_data = Serialize(data);
                    break;
                case VariantGeneralType.MonoBehaviour:
                    m_monoBehaviour = (MonoBehaviour)data;
                    break;
                case VariantGeneralType.UnityObject:
                    m_unityObject = (UnityEngine.Object)data;
                    break;
                case VariantGeneralType.Vector:
                    if (data == null) m_vector = new Vector4();
                    else m_vector = VectorFromObject(data, type);
                    break;
                case VariantGeneralType.Quaternion:
                    if (data == null) m_quaternion = new Quaternion();
                    else m_quaternion = (Quaternion)data;
                    break;
                case VariantGeneralType.IEnumerator:
                    m_iEnumator = (IEnumerator)data;
                    break;
                default:
                    throw new Exception("Non general type " + m_typeName + " is not supported yet");
            }

        }

        public VariantData(VariantData other)
        {
            m_typeName = other.m_typeName;

            m_data = other.m_data;
            m_unityObject = other.m_unityObject;
            m_monoBehaviour = other.m_monoBehaviour;
            m_vector = other.m_vector;
            m_quaternion = other.m_quaternion;
            m_iEnumator = other.m_iEnumator;
        }
        #endregion

        #region Data
        public T GetData<T>()
        {
            return (T)rawData;
        }

        public object rawData
        {
            get
            {
                switch (VariantGeneralTypeOf(GetVariantType()))
                {
                    case VariantGeneralType.SerializableByCSharp:
                        if ((m_data == null) || (m_data.Length == 0)) return null;
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            memoryStream.Write(m_data, 0, m_data.Length);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            return binaryFormatter.Deserialize(memoryStream);
                        }
                    case VariantGeneralType.MonoBehaviour:
                        return m_monoBehaviour;
                    case VariantGeneralType.UnityObject:
                        return m_unityObject;
                    case VariantGeneralType.Vector:
                        Type vectorType = GetVariantType();
                        if (vectorType == typeof(Vector2)) return (Vector2)m_vector;
                        else if (vectorType == typeof(Vector3)) return (Vector3)m_vector;
                        else return m_vector;
                    case VariantGeneralType.Quaternion:
                        return m_quaternion;
                    case VariantGeneralType.Null:
                        return null;
                    case VariantGeneralType.IEnumerator:
                        return m_iEnumator;
                    default:
                        throw new Exception("Non general type " + m_typeName + " is not supported yet");
                }
            }
        }

        static private byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        static public Vector4 VectorFromObject(object obj, Type type)
        {
            if (type == typeof(Vector2))
            {
                return (Vector2)obj;
            }
            else if (type == typeof(Vector3))
            {
                return (Vector3)obj;
            }
            else if (type == typeof(Vector4))
            {
                return (Vector4)obj;
            }
            else
            {
                throw new Exception(type + " : Not a vector");
            }
        }

        public Type GetVariantType()
        {
            return Type.GetType(m_typeName);
        }

        static public VariantGeneralType VariantGeneralTypeOf(Type type)
        {
            if (type == null)
            {
                return VariantGeneralType.Null;
            }

            if (type.IsSerializable)
            {
                return VariantGeneralType.SerializableByCSharp;
            }

            if (type.IsSubclassOf(typeof(MonoBehaviour)) || (type == typeof(MonoBehaviour)))
            {
                return VariantGeneralType.MonoBehaviour;
            }

            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return VariantGeneralType.UnityObject;
            }

            if ((type == typeof(Vector2)) || (type == typeof(Vector3)) || (type == typeof(Vector4)))
            {
                return VariantGeneralType.Vector;
            }

            if (type == typeof(Quaternion))
            {
                return VariantGeneralType.Quaternion;
            }

            if((type is IEnumerator) || (type == typeof(IEnumerator)))
            {
                return VariantGeneralType.IEnumerator;
            }

            return VariantGeneralType.Other;
        }
        #endregion

        #region Operators
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            VariantData b = (VariantData)obj;

            Type aType = GetVariantType();
            Type bType = b.GetVariantType();

            if (aType != bType) return false;

            if (rawData == null)
            {
                return b.rawData == null;
            }
            else if (b.rawData == null) return false;

            return rawData.Equals(b.rawData);
        }

        public override int GetHashCode()
        {
            return rawData.GetHashCode();
        }

        public static bool operator ==(VariantData a, VariantData b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(VariantData a, VariantData b)
        {
            return !(a == b);
        }
        #endregion
    }
}
