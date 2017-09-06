using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace TriggerEditor
{
    public class Routine : ScriptableObject
    {
        public enum RoutineType
        {
            Event,
            Condition,
            Action
        }

        [SerializeField] private Node[] m_nodes;
        [SerializeField] private Node m_finalNode;
        [SerializeField] private RoutineType m_type;

        [SerializeField] private Vector2 m_viewCenter;

        static public Routine CreateRoutine(string name, RoutineType type)
        {
            Routine routine = CreateInstance<Routine>();
            routine.m_type = type;
            routine.name = name;
            routine.CreateFinalNode();

            return routine;
        }

        public Routine Clone()
        {
            Routine clone = CreateInstance<Routine>();
            clone.m_type = m_type;
            clone.name = name + "(Clone)";

            clone.m_viewCenter = m_viewCenter;

            List<NodeValue> previousNodeValueMap = new List<NodeValue>();
            List<NodeValue> newNodeValueMap = new List<NodeValue>();

            clone.m_finalNode = m_finalNode.CloneToRoutine(clone);
            previousNodeValueMap.AddRange(m_finalNode.GetAllNodeValues());
            newNodeValueMap.AddRange(clone.m_finalNode.GetAllNodeValues());

            Node[] clonedNodes = null;
            if (m_nodes != null)
            {
                clonedNodes = new Node[m_nodes.Length];
                for(int i = 0; i < m_nodes.Length; i++)
                {
                    clonedNodes[i] = m_nodes[i].CloneToRoutine(clone);

                    previousNodeValueMap.AddRange(m_nodes[i].GetAllNodeValues());
                    newNodeValueMap.AddRange(clonedNodes[i].GetAllNodeValues());
                }

                clone.m_finalNode.ReMapConnections(previousNodeValueMap, newNodeValueMap);

                for (int i = 0; i < clonedNodes.Length; i++)
                {
                    clonedNodes[i].ReMapConnections(previousNodeValueMap, newNodeValueMap);
                }
            }

            clone.m_nodes = clonedNodes;

            return clone;
        }

        public void CreateFinalNode()
        {
            MethodInfo methodInfo = null;
            switch (m_type)
            {
                case RoutineType.Event:
                    methodInfo = typeof(Routine).GetMethod("EventOutput");
                    break;
                case RoutineType.Condition:
                    methodInfo = typeof(Routine).GetMethod("ConditionOutput");
                    break;
                case RoutineType.Action:
                    methodInfo = typeof(Routine).GetMethod("ActionOutput");
                    break;
            }

            m_finalNode = Node.CreateNode(methodInfo, this);
        }

        public object Evaluate()
        {
            if (m_finalNode == null) return null;

            return m_finalNode.Evaluate();
        }

        public IEnumerator Run()
        {
            return (IEnumerator) Evaluate();
        }

        public void AddNode(MethodInfo nodeMethodInfo)
        {
            Node newNode = Node.CreateNode(nodeMethodInfo, this);
            if (m_nodes == null)
            {
                m_nodes = new Node[1];
                m_nodes[0] = newNode;
            }
            else
            {
                List<Node> nodes = new List<Node>(m_nodes);
                nodes.Add(newNode);
                m_nodes = nodes.ToArray();
            }
        }

        public void RemoveNode(Node node)
        {
            if (m_nodes == null) return;
            List<Node> nodes = new List<Node>(m_nodes);
            nodes.Remove(node);
            m_nodes = nodes.ToArray();

            if(node.inputs != null)
            {
                for (int i = 0; i < node.inputs.Length; i++)
                {
                    node.inputs[i].Disconnect();
                }
            }

            for(int i = 0; i < m_nodes.Length; i++)
            {
                Node otherNode = m_nodes[i];
                if (otherNode.inputs == null) continue;

                Disconnect(node, otherNode);
            }

            Disconnect(node, m_finalNode);
        }

        public void Disconnect(Node outputNode, Node inputNode)
        {
            for (int j = 0; j < inputNode.inputs.Length; j++)
            {
                NodeValue otherNodeInput = inputNode.inputs[j];

                for (int k = 0; k < outputNode.outputs.Length; k++)
                {
                    if (otherNodeInput.connection == outputNode.outputs[k])
                    {
                        otherNodeInput.Disconnect();
                    }
                }
            }
        }

        #region Accessors
        public Vector2 viewCenter
        {
            get { return m_viewCenter; }
            set { m_viewCenter = value; }
        }

        public Node[] nodes
        {
            get { return m_nodes; }
        }

        public Node finalNode
        {
            get { return m_finalNode; }
        }

        public RoutineType type
        {
            get { return m_type; }
        }
        #endregion

        #region Final nodes method
        [NodeMethod("Hidden_FinalNode", "Event output",  "")]
        static public bool EventOutput(bool result)
        {
            return result;
        }

        [NodeMethod("Hidden_FinalNode", "Condition output",  "")]
        static public bool ConditionOutput(bool result)
        {
            return result;
        }

        [NodeMethod("Hidden_FinalNode", "Action output",  "")]
        static public IEnumerator ActionOutput(Node nodeValue)
        {
            return (IEnumerator) nodeValue.Evaluate();
        }
        #endregion
    }
}