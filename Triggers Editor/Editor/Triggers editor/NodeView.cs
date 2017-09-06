using UnityEngine;
using UnityEditor;

namespace TriggerEditor
{
    static public class NodeView
    {
        static private readonly float s_cornerSize = 16;
        static private readonly float s_iconSize = 24;
        static private readonly float s_iconXOffset = 8;
        static private readonly float s_connectorDiameter = 12;

        static public void Draw(Node node)
        {
            DrawFrame(node);
            DrawHeader(node);
            DrawConnectors(node);
        }

        #region Draw Node frame
        static private void DrawFrame(Node node)
        {
            DrawNodeFrame(node.rect, s_cornerSize, Node.nodeTopSize, Color.black, new Color(0.5f, 0.5f, 0.5f, 1), new Color(0.45f, 0.45f, 0.45f, 1));
            EditorGUIUtility.AddCursorRect(GetDragRect(node), MouseCursor.MoveArrow);
        }

        static public void DrawNodeFrame(Rect rect, float cornerSize, float topSize, Color penColor, Color bottomColor, Color topColor)
        {
            Color previousColor = Handles.color;

            Vector2 cornerNW = new Vector2(rect.xMin, rect.yMin);
            Vector2 cornerNE = new Vector2(rect.xMax, rect.yMin);
            Vector2 cornerSE = new Vector2(rect.xMax, rect.yMax);
            Vector2 cornerSW = new Vector2(rect.xMin, rect.yMax);

            // Draw Inside
            // Top
            Handles.color = topColor;
            Handles.DrawSolidArc(cornerNW + new Vector2(cornerSize, cornerSize), Vector3.forward, Vector3.left, 90, cornerSize);
            Handles.DrawSolidArc(cornerNE + new Vector2(-cornerSize, cornerSize), Vector3.forward, Vector3.down, 90, cornerSize);

            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(new Rect(rect.xMin + cornerSize, rect.yMin, rect.width - cornerSize * 2, cornerSize), topColor, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Rect(rect.xMin, rect.yMin + cornerSize, rect.width, topSize - cornerSize), topColor, Color.clear);

            // Bottom
            Handles.color = bottomColor;
            Handles.DrawSolidArc(cornerSE + new Vector2(-cornerSize, -cornerSize), Vector3.forward, Vector3.right, 90, cornerSize);
            Handles.DrawSolidArc(cornerSW + new Vector2(cornerSize, -cornerSize), Vector3.forward, Vector3.up, 90, cornerSize);

            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(new Rect(rect.xMin, rect.yMin + topSize, rect.width, rect.height - cornerSize - topSize), bottomColor, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Rect(rect.xMin + cornerSize, rect.yMax - cornerSize, rect.width - cornerSize * 2, cornerSize), bottomColor, Color.clear);

            // Draw Outside
            Handles.color = penColor;
            Handles.DrawLine(cornerNW + Vector2.right * cornerSize, cornerNE + Vector2.left * cornerSize);
            Handles.DrawLine(cornerNE + Vector2.up * cornerSize, cornerSE + Vector2.down * cornerSize);
            Handles.DrawLine(cornerSE + Vector2.left * cornerSize, cornerSW + Vector2.right * cornerSize);
            Handles.DrawLine(cornerSW + Vector2.down * cornerSize, cornerNW + Vector2.up * cornerSize);

            Handles.DrawWireArc(cornerNW + new Vector2(cornerSize, cornerSize), Vector3.forward, Vector3.left, 90, cornerSize);
            Handles.DrawWireArc(cornerNE + new Vector2(-cornerSize, cornerSize), Vector3.forward, Vector3.down, 90, cornerSize);
            Handles.DrawWireArc(cornerSE + new Vector2(-cornerSize, -cornerSize), Vector3.forward, Vector3.right, 90, cornerSize);
            Handles.DrawWireArc(cornerSW + new Vector2(cornerSize, -cornerSize), Vector3.forward, Vector3.up, 90, cornerSize);

            Handles.color = previousColor;
        }
        #endregion
        
        #region Draw Node header
        static private void DrawHeader(Node node)
        {
            GUIStyle nameGUIStyle = new GUIStyle(EditorStyles.boldLabel);
            nameGUIStyle.alignment = TextAnchor.MiddleLeft;

            EditorGUI.LabelField(GetHeaderNameRect(node.rect), new GUIContent(node.name), nameGUIStyle);

            NodeCategory category = NodeCategory.GetNodeCategory(node.nodeMethodAttribute.category);
            EditorGUI.LabelField(GetHeaderIconRect(node.rect), new GUIContent((category == null) ? null : category.icon));
        }
        #endregion

        #region Draw Connectors
        static private void DrawConnectors(Node node)
        {
            if (!node.isFinalNode)
            {
                DrawConnectors(node, node.outputs);
            }

            DrawConnectors(node, node.inputs);
        }

        static private void DrawConnectors(Node node, NodeValue[] nodeValues)
        {
            if ((nodeValues == null) || (nodeValues.Length == 0)) return;

            int offset = 0;
            if (!nodeValues[0].isOutput && (node.outputs != null) && (!node.isFinalNode)) offset = node.outputs.Length - 1;

            for (int i = 0; i < nodeValues.Length; i++)
            {
                DrawConnector(node.rect, nodeValues[i], i + offset);
            }
        }

        static private void DrawConnector(Rect nodeRect, NodeValue nodeValue, int index)
        {
            Vector2 connectorPosition = GetConnectorPosition(nodeValue);

            DrawCircle(connectorPosition, nodeValue.variantValue.typeColor);
            EditorGUIUtility.AddCursorRect(GetConnectorRect(nodeValue), MouseCursor.ArrowPlus);

            float width = nodeRect.width - s_connectorDiameter * 2;
            if (nodeValue.isOutput)
            {
                GUIStyle outputNameStyle = new GUIStyle(EditorStyles.label);
                outputNameStyle.alignment = TextAnchor.MiddleRight;
                Rect labelRect = new Rect(connectorPosition - new Vector2(width + 4 + s_connectorDiameter / 2, Node.connectorSpacing / 2), new Vector2(width, Node.connectorSpacing));

                if (index >= 1)
                {
                    EditorGUI.LabelField(labelRect, nodeValue.name, outputNameStyle);
                }
            }
            else if (nodeValue.connection == null)
            {
                Rect variantArea = new Rect(connectorPosition + new Vector2(Node.connectorSpacing / 2 + 2, -Node.connectorSpacing / 2), new Vector2(width, Node.connectorSpacing));
                GUILayout.BeginArea(variantArea);

                EditorGUI.BeginChangeCheck();

                Variant tmp = new Variant(VariantView.VariantDataField(nodeValue.variantValue.variantData), nodeValue.variantValue.typeName);
                if (tmp != nodeValue.variantValue)
                {
                    Undo.RecordObject(nodeValue, "Changed node value");
                    nodeValue.variantValue = tmp;
                    TriggerEditorWindow.SetSceneDirty();
                }

                GUILayout.EndArea();
            }
        }

        static public Vector2 GetConnectorPosition(NodeValue nodeValue)
        {
            Rect nodeRect = nodeValue.node.rect;
            float x = nodeValue.isOutput ? nodeRect.xMax : nodeRect.xMin;
            float y = nodeRect.yMin + Node.nodeTopSize;

            if (nodeValue == nodeValue.node.outputs[0])
            {
                y -= Node.nodeTopSize / 2;
            }
            else
            {
                NodeValue[] container = nodeValue.isOutput ? nodeValue.node.outputs : nodeValue.node.inputs;

                for(int i = 0; i < container.Length; i++)
                {
                    if(container[i] == nodeValue)
                    {
                        y += i * Node.connectorSpacing;
                        break;
                    }
                }

                y += nodeValue.isOutput ? 0 : GetNodeInputsOffset(nodeValue.node);
            }

            return new Vector2(x, y);
        }

        static public Rect GetConnectorRect(NodeValue nodeValue)
        {
            return new Rect(GetConnectorPosition(nodeValue) - Vector2.one * s_connectorDiameter / 2, Vector2.one * s_connectorDiameter);
        }

        static public float GetNodeInputsOffset(Node node)
        {
            return node.outputs.Length * Node.connectorSpacing;
        }

        #endregion

        #region Helpers
        static public Rect GetHeaderRect(Rect nodeRect)
        {
            return new Rect(nodeRect.xMin + s_cornerSize, nodeRect.yMin, nodeRect.width - s_cornerSize * 2, Node.nodeTopSize);
        }

        static public Rect GetHeaderNameRect(Rect nodeRect)
        {
            Rect result = GetHeaderRect(nodeRect);
            result.xMin += s_iconSize - s_iconXOffset;
            return result;
        }

        static public Rect GetHeaderIconRect(Rect nodeRect)
        {
            Rect result = GetHeaderRect(nodeRect);
            result.width = s_iconSize;
            result.height = s_iconSize;
            result.y += (Node.nodeTopSize - s_iconSize) / 2;
            result.x -= s_iconXOffset;
            return result;
        }

        static public Rect GetDragRect(Rect nodeRect)
        {
            return new Rect(nodeRect.xMin + s_connectorDiameter / 2, nodeRect.yMin, nodeRect.width - s_connectorDiameter, nodeRect.height);
        }

        static public Rect GetDragRect(Node node)
        {
            return GetDragRect(node.rect);
        }

        static private void DrawCircle(Vector2 position, Color color)
        {
            Handles.color = color;
            Handles.DrawSolidArc(position, Vector3.forward, Vector3.right, 360, s_connectorDiameter / 2);
            Handles.color = Color.black;
            Handles.DrawWireArc(position, Vector3.forward, Vector3.right, 360, s_connectorDiameter / 2);
        }
        #endregion
    }
}