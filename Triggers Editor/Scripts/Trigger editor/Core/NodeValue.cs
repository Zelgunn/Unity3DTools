using UnityEngine;
using System.Collections;
using System;

namespace TriggerEditor
{
    public class NodeValue : ScriptableObject
    {
        [SerializeField] private Variant m_value;
        [SerializeField] private Node m_node;
        [SerializeField] private NodeValue m_connection;
        [SerializeField] private bool m_isOutput;

        static public NodeValue CreateNodeValue(Type valueType, Node node, bool isOutput)
        {
            return CreateNodeValue(new Variant(valueType), node, isOutput);
        }

        static public NodeValue CreateNodeValue(object defaultValue, Type valueType, Node node, bool isOutput)
        {
            return CreateNodeValue(new Variant(defaultValue, valueType), node, isOutput);
        }

        static public NodeValue CreateNodeValue(Variant defaultValue, Node node, bool isOutput)
        {
            NodeValue nodeValue = CreateInstance<NodeValue>();
            nodeValue.m_value = defaultValue;
            nodeValue.m_node = node;
            nodeValue.m_isOutput = isOutput;
            return nodeValue;
        }

        public NodeValue CloneToNode(Node newNode, bool cloneInputConnection)
        {
            NodeValue clone = CreateInstance<NodeValue>();

            clone.name = name;
            clone.m_value = new Variant(m_value);
            clone.m_node = newNode;
            clone.m_isOutput = m_isOutput;

            if(!m_isOutput && cloneInputConnection)
            {
                clone.m_connection = m_connection;
            }

            return clone;
        }

        public bool CanConnectWith(NodeValue other)
        {
            //if (other == null) return false;
            if (other.m_isOutput == m_isOutput) return false;
            if (other.m_node == m_node) return false;

            Type type = m_value.GetVariantType();
            Type otherType = other.m_value.GetVariantType();
            if ((type == typeof(object)) || (otherType == typeof(object))) return true;

            if(m_isOutput)
            {
                if (!type.IsAssignableFrom(otherType))
                {
                    return false;
                }
            }
            else
            {
                if (!otherType.IsAssignableFrom(type)) return false;
            }

            return true;
        }

        public bool TryConnectWith(NodeValue other)
        {
            if(other == null)
            {
                m_connection = null;
                return false;
            }

            if (!CanConnectWith(other)) return false;

            if(m_isOutput)
            {
                other.m_connection = this;
            }
            else
            {
                m_connection = other;
            }

            return true;
        }

        public bool FakeConnect(NodeValue other)
        {
            if (other == null)
            {
                return m_connection != null;
            }

            if (!CanConnectWith(other)) return false;

            if(m_isOutput)
            {
                return other.m_connection != this;
            }
            else
            {
                return m_connection != other;
            }
        }

        public void Disconnect()
        {
            if (m_isOutput) throw new Exception("Output nodes can not be disconnected");
            m_connection = null;
        }

        public object Evaluate()
        {
            if (m_isOutput)
            {
                return m_node.EvaluateOutput(this);
            }
            else if (m_connection == null)
            {
                return m_value.rawData;
            }
            else
            {
                return m_connection.Evaluate();
            }
        }

        public bool isOutput
        {
            get { return m_isOutput; }
        }

        public NodeValue connection
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        public Variant variantValue
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public Node node
        {
            get { return m_node; }
        }
    }
}