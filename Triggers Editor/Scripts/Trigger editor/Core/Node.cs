using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace TriggerEditor
{
    public class Node : ScriptableObject
    {
        static private readonly float s_nodeTopSize = 32;
        static private readonly float s_connectorSpacing = 18;
        static private readonly float s_topConnectorSpacing = 16;
        static private readonly float s_defaultWidth = 200;

        #region Method
        [SerializeField] protected string m_methodName;
        [SerializeField] protected string m_methodDeclaringTypeName;
        [SerializeField] protected string[] m_methodParametersTypesNames;

        protected MethodInfo m_methodInfo = null;
        protected NodeMethodAttribute m_nodeMethodAttribute = null;
        #endregion

        #region Graph
        [SerializeField] protected Routine m_routine;
        [SerializeField] protected NodeValue[] m_inputs;
        [SerializeField] protected NodeValue[] m_outputs;
        #endregion

        [SerializeField] protected Rect m_rect;

        static public Node CreateNode(MethodInfo methodInfo, Routine routine)
        {
            Node node = CreateInstance<Node>();
            node.m_routine = routine;
            node.m_rect = new Rect(routine.viewCenter, new Vector2(s_defaultWidth, 0));
            node.LoadMethodInfo(methodInfo);

            return node;
        }

        #region Copy
        public Node Clone(bool cloneInputConnection = false)
        {
            Node clone = CreateInstance<Node>();

            clone.name = name;

            clone.m_methodName = m_methodName;
            clone.m_methodDeclaringTypeName = m_methodDeclaringTypeName;
            clone.m_methodParametersTypesNames = (string[]) m_methodParametersTypesNames.Clone();

            clone.m_routine = m_routine;
            clone.m_inputs = CloneNodeValues(clone, m_inputs, cloneInputConnection);
            clone.m_outputs = CloneNodeValues(clone, m_outputs, cloneInputConnection);

            clone.m_rect = m_rect;
            clone.m_rect.position += Vector2.one * 50;

            return clone;
        }

        protected NodeValue[] CloneNodeValues(Node toNode, NodeValue[] originals, bool cloneInputConnection)
        {
            if (originals == null) return null;

            NodeValue[] clones = new NodeValue[originals.Length];
            for(int i = 0; i < originals.Length; i++)
            {
                clones[i] = originals[i].CloneToNode(toNode, cloneInputConnection);
            }

            return clones;
        }

        public Node CloneToRoutine(Routine newRoutine)
        {
            Node clone = Clone(true);
            clone.m_routine = newRoutine;
            clone.m_rect = m_rect;
            return clone;
        }

        public void ReMapConnections(List<NodeValue> from, List<NodeValue> to)
        {
            ReMapConnections(m_inputs, from, to);
            ReMapConnections(m_outputs, from, to);
        }

        protected void ReMapConnections(NodeValue[] nodeValues, List<NodeValue> from, List<NodeValue> to)
        {
            if (nodeValues == null) return;

            for(int i = 0; i < nodeValues.Length; i++)
            {
                int connectionIndex = from.IndexOf(nodeValues[i].connection);
                if (connectionIndex == -1)
                {
                    continue;
                }

                nodeValues[i].TryConnectWith(to[connectionIndex]);
            }
        }
        #endregion

        #region Evaluation
        public object EvaluateOutput(NodeValue outputNodeValue)
        {
            int nodeValueIndex = GetOutputNodeValueIndex(outputNodeValue);
            if (nodeValueIndex < 0) throw new Exception("Given output not found in Node outputs");
            if (nodeValueIndex == 0) return this;

            object methodObject = GetMethodObjectValue();
            object[] parameters = GetMethodParametersValues();
            object output = methodInfo.Invoke(methodObject, parameters);

            if (nodeValueIndex == 1) return output;
            else return parameters[MapNodeIndexToParameterIndex(nodeValueIndex)];
        }

        public object Evaluate()
        {
            object methodObject = GetMethodObjectValue();
            object[] parameters = GetMethodParametersValues();

            return methodInfo.Invoke(methodObject, parameters);
        }

        public IEnumerator Run()
        {
            object methodObject = GetMethodObjectValue();
            object[] parameters = GetMethodParametersValues();

            if(methodInfo.ReturnType == typeof(IEnumerator)) yield return methodInfo.Invoke(methodObject, parameters);
            else methodInfo.Invoke(methodObject, parameters);
        }

        public int GetOutputNodeValueIndex(NodeValue nodeValue)
        {
            for(int i = 0; i < m_outputs.Length; i++)
            {
                if(m_outputs[i] == nodeValue)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetInputNodeValueIndex(NodeValue nodeValue)
        {
            for (int i = 0; i < m_inputs.Length; i++)
            {
                if (m_inputs[i] == nodeValue)
                {
                    return i;
                }
            }

            return -1;
        }

        public object GetMethodObjectValue()
        {
            if (methodInfo.IsStatic) return null;
            else return m_inputs[0].Evaluate();
        }

        public object[] GetMethodParametersValues()
        {
            ParameterInfo[] methodParametersInfos = methodInfo.GetParameters();
            object[] parameterValues = new object[methodParametersInfos.Length];

            int inputIndex = methodInfo.IsStatic ? 0 : 1;
            for (int i = 0; i < methodParametersInfos.Length; i++)
            {
                if (methodParametersInfos[i].IsOut)
                {
                    parameterValues[i] = null;
                }
                else
                {
                    parameterValues[i] = m_inputs[inputIndex++].Evaluate();
                }
            }

            return parameterValues;
        }

        public int MapNodeIndexToParameterIndex(int nodeValueIndex)
        {
            int outputCount = 2; // 0 is the node itself, 1 is the regular return, 2+ are "out"
            ParameterInfo[] methodParametersInfos = methodInfo.GetParameters();
            for (int i = 0; i < methodParametersInfos.Length; i++)
            {
                if(methodParametersInfos[i].IsOut)
                {
                    if (nodeValueIndex == outputCount) return i;
                    outputCount++;
                }
            }

            throw new Exception("Failed to map NodeValue index to parameter index");
        }
        #endregion

        #region Method Info & Attributes
        protected void LoadMethodInfo(MethodInfo methodInfo)
        {
            m_methodInfo = methodInfo;

            m_methodName = methodInfo.Name;
            m_methodDeclaringTypeName = methodInfo.DeclaringType.AssemblyQualifiedName;
            ParameterInfo[] methodParametersInfos = methodInfo.GetParameters();
            m_methodParametersTypesNames = new string[methodParametersInfos.Length];
            for (int i = 0; i < methodParametersInfos.Length; i++)
            {
                m_methodParametersTypesNames[i] = methodParametersInfos[i].ParameterType.AssemblyQualifiedName;
            }

            name = nodeMethodAttribute.name;

            List<NodeValue> inputs = new List<NodeValue>();
            List<NodeValue> outputs = new List<NodeValue>();

            if(!m_methodInfo.IsStatic)
            {
                NodeValue tmp = NodeValue.CreateNodeValue(m_methodInfo.DeclaringType, this, false);
                tmp.name = "Object";
                inputs.Add(tmp);
            }

            outputs.Add(NodeValue.CreateNodeValue(typeof(Node), this, true));
            if(m_methodInfo.ReturnType != typeof(void))
            {
                NodeValue tmp = NodeValue.CreateNodeValue(m_methodInfo.ReturnType, this, true);
                tmp.name = nodeMethodAttribute.outputName;
                outputs.Add(tmp);
            }

            for (int i = 0; i < methodParametersInfos.Length; i++)
            {
                Type realNodeValueType = methodParametersInfos[i].IsOut ? methodParametersInfos[i].ParameterType.GetElementType() : methodParametersInfos[i].ParameterType;
                NodeValue nodeValue = NodeValue.CreateNodeValue(realNodeValueType, this, methodParametersInfos[i].IsOut);
                nodeValue.name = methodParametersInfos[i].Name;
                if (methodParametersInfos[i].IsOut)
                {
                    outputs.Add(nodeValue);
                }
                else
                {
                    inputs.Add(nodeValue);
                }
            }

            m_inputs = inputs.ToArray();
            m_outputs = outputs.ToArray();

            
            m_rect.height = (m_inputs.Length + m_outputs.Length - 1) * s_connectorSpacing + s_topConnectorSpacing + 10 + s_nodeTopSize;
        }

        protected void RetrieveMethodInfo()
        {
            Type[] NodeMethodTypes = new Type[m_methodParametersTypesNames.Length];
            for (int i = 0; i < m_methodParametersTypesNames.Length; i++)
            {
                NodeMethodTypes[i] = Type.GetType(m_methodParametersTypesNames[i]);
            }
            m_methodInfo = Type.GetType(m_methodDeclaringTypeName).GetMethod(m_methodName, NodeMethodTypes);
        }

        protected void RetrieveNodeMethodAttribute()
        {
            m_nodeMethodAttribute = methodInfo.GetCustomAttributes(typeof(NodeMethodAttribute), true)[0] as NodeMethodAttribute;
        }
        #endregion

        #region Accessors
        public MethodInfo methodInfo
        {
            get
            {
                if (m_methodInfo == null) RetrieveMethodInfo();
                return m_methodInfo;
            }
        }

        public NodeMethodAttribute nodeMethodAttribute
        {
            get
            {
                if (m_nodeMethodAttribute == null) RetrieveNodeMethodAttribute();
                return m_nodeMethodAttribute;
            }
        }

        public string category
        {
            get { return nodeMethodAttribute.category; }
        }

        public Rect rect
        {
            get { return m_rect; }
        }

        public bool isFinalNode
        {
            get { return m_routine.finalNode == this; }
        }

        public NodeValue[] inputs
        {
            get { return m_inputs; }
        }

        public NodeValue[] outputs
        {
            get { return m_outputs; }
        }

        public NodeValue[] GetAllNodeValues()
        {
            int resultLength = (m_inputs == null) ? 0 : m_inputs.Length;
            resultLength += (m_outputs == null) ? 0 : m_outputs.Length;
            NodeValue[] result = new NodeValue[resultLength];

            int index = 0;
            if(m_inputs != null)
            {
                for(int i = 0; i < m_inputs.Length; i++)
                {
                    result[i] = m_inputs[i];
                }
                index = m_inputs.Length;
            }

            if(m_outputs != null)
            {
                for (int i = 0; i < m_outputs.Length; i++)
                {
                    result[i + index] = m_outputs[i];
                }
            }

            return result;
        }

        static public float nodeTopSize
        {
            get { return s_nodeTopSize; }
        }

        static public float connectorSpacing
        {
            get { return s_connectorSpacing; }
        }

        static public float topConnectorSpacing
        {
            get { return s_topConnectorSpacing; }
        }

        static public float defaultWidth
        {
            get { return s_defaultWidth; }
        }
        #endregion

        public void MoveBy(Vector2 delta)
        {
            m_rect.position += delta;
        }
    }

    public enum NodeMethodType
    {
        Event = 1,
        Condition = 2,
        Action = 4,
        Other = 8
    }

    public class NodeMethodAttribute : Attribute
    {
        [SerializeField] private string m_category;
        [SerializeField] private string m_name;
        [SerializeField] private NodeMethodType m_type;
        [SerializeField] private string m_outputName;

        public NodeMethodAttribute(string category, string name, NodeMethodType type, string outputName = "Output")
        {
            m_category = category;
            m_name = name;
            m_type = type;
            m_outputName = outputName;
        }

        static public MethodInfo[] GetNodeMethods(out NodeMethodAttribute[] nodeMethodsAttribute, NodeMethodType filter)
        {
            List<MethodInfo> resultMethodsInfo = new List<MethodInfo>();
            List<NodeMethodAttribute> resultAttributes = new List<NodeMethodAttribute>();

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types)
            {
                MethodInfo[] allMethodsOfType = type.GetMethods();
                foreach (MethodInfo methodInfo in allMethodsOfType)
                {
                    if (methodInfo.DeclaringType != methodInfo.ReflectedType)
                    {
                        continue;
                    }

                    object[] methodAttributes = methodInfo.GetCustomAttributes(typeof(NodeMethodAttribute), true);
                    if (methodAttributes.Length > 0)
                    {
                        NodeMethodAttribute nodeMethodAttribute = (NodeMethodAttribute)methodAttributes[0];
                        if((nodeMethodAttribute.m_type & filter) != nodeMethodAttribute.m_type)
                        {
                            continue;
                        }

                        resultMethodsInfo.Add(methodInfo);
                        resultAttributes.Add(nodeMethodAttribute);
                    }
                }
            }

            nodeMethodsAttribute = resultAttributes.ToArray();
            return resultMethodsInfo.ToArray();
        }

        static public bool IsActionType(Type type)
        {
            return (type == typeof(void)) || (type == typeof(IEnumerator));
        }

        static public bool IsTestType(Type type)
        {
            return type == typeof(bool);
        }

        #region Accessors
        public string category
        {
            get { return m_category; }
        }

        public string name
        {
            get { return m_name; }
        }

        public NodeMethodType type
        {
            get { return m_type; }
        }

        static public NodeMethodType anyMethodType
        {
            get { return NodeMethodType.Action | NodeMethodType.Condition | NodeMethodType.Event | NodeMethodType.Other; }
        }

        public string outputName
        {
            get { return m_outputName; }
        }
        #endregion
    }
}