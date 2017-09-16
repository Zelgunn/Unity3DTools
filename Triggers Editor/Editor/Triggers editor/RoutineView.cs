using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace TriggerEditor
{
    static public class RoutineView
    {
        public struct SetNodeFinalCallbackObject
        {
            public Routine routine;
            public Node newFinalNode;
        }

        public struct DeleteNodeCallbackObject
        {
            public Routine routine;
            public Node nodeToDelete;
        }

        static private NodeValue m_selectedConnector;
        static private Vector2 m_selectedConnectorPosition;
        static private Node m_selectedNode;

        static public void Draw(Routine routine)
        {
            if (routine == null) return;

            DrawNodes(routine);
            DrawLinks(routine);

            ProcessEvent(routine, Event.current);
        }

        static private void DrawNodes(Routine routine)
        {
            if (routine.nodes != null)
            {
                for (int i = 0; i < routine.nodes.Length; i++)
                {
                    NodeView.Draw(routine.nodes[i]);
                }
            }
        }

        static private void DrawLinks(Routine routine)
        {
            if (routine.nodes == null) return;

            for (int i = 0; i < routine.nodes.Length; i++)
            {
                Node node = routine.nodes[i];
                if (node.inputs == null) continue;

                for (int j = 0; j < node.inputs.Length; j++)
                {
                    DrawLink(node.inputs[j]);
                }
            }
        }

        static private void DrawLink(NodeValue input)
        {
            if (input.connection == null) return;

            Vector2 inputPosition = NodeView.GetConnectorPosition(input);
            Vector2 connectionPosition = NodeView.GetConnectorPosition(input.connection);

            DrawBezier(inputPosition, connectionPosition, Color.red);
        }

        static private void ProcessEvent(Routine routine, Event eventToProcess)
        {
            if(eventToProcess.isMouse)
            {
                OnMouseEvent(routine, eventToProcess);
            }

            UpdateLinkCreation(routine, eventToProcess);
        }

        static public void OnMouseEvent(Routine routine, Event mouseEvent)
        {
            switch (mouseEvent.button)
            {
                case 0:
                    OnLeftMouseButton(routine, mouseEvent);
                    break;
                case 1:
                    OnRightMouseButton(routine, mouseEvent);
                    break;
            }
        }

        static public void OnLeftMouseButton(Routine routine, Event mouseEvent)
        {
            switch (mouseEvent.type)
            {
                case EventType.MouseDown:
                    m_selectedConnector = GetConnectorAtPosition(routine, mouseEvent.mousePosition);
                    if (m_selectedConnector != null)
                    {
                        m_selectedConnectorPosition = NodeView.GetConnectorPosition(m_selectedConnector);
                        m_selectedNode = null;
                    }
                    else
                    {
                        m_selectedNode = GetNodeAtPosition(routine, mouseEvent.mousePosition);
                        if(m_selectedNode != null)
                        {
                            Undo.RegisterCompleteObjectUndo(m_selectedNode, "Drag Node");
                            Undo.FlushUndoRecordObjects();
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if (m_selectedNode != null)
                    {
                        m_selectedNode.MoveBy(mouseEvent.delta);
                        TriggerEditorWindow.SetSceneDirty();
                        mouseEvent.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (m_selectedConnector != null)
                    {
                        NodeValue otherConnector = GetConnectorAtPosition(routine, mouseEvent.mousePosition);
                        if(m_selectedConnector.FakeConnect(otherConnector))
                        {
                            if(m_selectedConnector.isOutput)
                            {
                                Undo.RegisterCompleteObjectUndo(otherConnector, "Changed node connection");
                            }
                            else
                            {
                                Undo.RegisterCompleteObjectUndo(m_selectedConnector, "Changed node connection");
                            }
                            m_selectedConnector.TryConnectWith(otherConnector);
                            TriggerEditorWindow.SetSceneDirty();
                        }

                        mouseEvent.Use();
                    }
                    m_selectedNode = null;
                    m_selectedConnector = null;
                    break;
            }
        }

        static public Node GetNodeAtPosition(Routine routine, Vector2 position)
        {
            if (routine.nodes == null) return null;
            for(int i = routine.nodes.Length - 1; i >= 0 ; i--)
            {
                if (NodeView.GetDragRect(routine.nodes[i]).Contains(position)) return routine.nodes[i];
            }

            return null;
        }

        static public NodeValue GetConnectorAtPosition(Routine routine, Vector2 position)
        {
            if (routine.nodes == null) return null;

            for (int i = routine.nodes.Length - 1; i >= 0; i--)
            {
                Node node = routine.nodes[i];
                NodeValue connector = GetConnectorAtPosition(node.inputs, position);
                if (connector != null) return connector;
                connector = GetConnectorAtPosition(node.outputs, position);
                if (connector != null) return connector;
            }

            return null;
        }

        static public NodeValue GetConnectorAtPosition(NodeValue[] connectors, Vector2 position)
        {
            if (connectors == null) return null;

            for (int i = 0; i < connectors.Length; i++)
            {
                if (NodeView.GetConnectorRect(connectors[i]).Contains(position))
                {
                    return connectors[i];
                }
            }

            return null;
        }

        static public void DrawBezier(Vector2 from, Vector2 to, Color color)
        {
            float distance = to.x - from.x;
            Vector3 startTangent = Vector2.right * distance / 2 + from;
            Vector3 endTangent = Vector2.left * distance / 2 + to;
            Handles.DrawBezier(from, to, startTangent, endTangent, Color.red, null, 2);
        }

        static public void OnRightMouseButton(Routine routine, Event mouseEvent)
        {
            Node clickedNode = GetNodeAtPosition(routine, mouseEvent.mousePosition);
            if(clickedNode != null)
            {
                OnRightClickNode(routine, mouseEvent, clickedNode);
            }
        }

        static public void OnRightClickNode(Routine routine, Event mouseEvent, Node clickedNode)
        {
            if (mouseEvent.type == EventType.MouseUp)
            {
                GenericMenu menu = new GenericMenu();
                if(clickedNode != routine.finalNode)
                {
                    if(routine.NodeEligibleAsFinal(clickedNode))
                    {
                        SetNodeFinalCallbackObject setNodeFinalCallbackObject = new SetNodeFinalCallbackObject
                        {
                            routine = routine,
                            newFinalNode = clickedNode
                        };
                        menu.AddItem(new GUIContent("Set as Final node"), false, SetNodeAsFinalNode, setNodeFinalCallbackObject);
                    }

                    DeleteNodeCallbackObject deleteNodeCallbackObject = new DeleteNodeCallbackObject
                    {
                        routine = routine,
                        nodeToDelete = clickedNode
                    };
                    menu.AddItem(new GUIContent("Delete node/Validate"), false, DeleteNodeCallback, deleteNodeCallbackObject);
                }
                menu.ShowAsContext();
            }
        }

        static private void SetNodeAsFinalNode(object obj)
        {
            SetNodeFinalCallbackObject setNodeFinalCallbackObject = (SetNodeFinalCallbackObject)obj;
            setNodeFinalCallbackObject.routine.finalNode = setNodeFinalCallbackObject.newFinalNode;
        }

        static private void DeleteNodeCallback(object obj)
        {
            DeleteNodeCallbackObject deleteNodeCallbackObject = (DeleteNodeCallbackObject)obj;
            Undo.RegisterCompleteObjectUndo(deleteNodeCallbackObject.routine, "Deleted node");
            deleteNodeCallbackObject.routine.RemoveNode(deleteNodeCallbackObject.nodeToDelete);
            TriggerEditorWindow.SetSceneDirty();
        }

        static private void UpdateLinkCreation(Routine routine, Event eventToProcess)
        {
            if (m_selectedConnector != null)
            {
                DrawBezier(m_selectedConnectorPosition, eventToProcess.mousePosition, Color.red);
                if (eventToProcess.type == EventType.MouseDrag) eventToProcess.Use();
            }
        }
    }
}