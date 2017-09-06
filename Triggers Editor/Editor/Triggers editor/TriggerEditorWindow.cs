using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace TriggerEditor
{
    public class TriggerEditorWindow : EditorWindow
    {
        #region Defines
        static private readonly float s_resizerWidth = 5;
        static private readonly float s_minRatio = 0.01f;

        static private readonly Color s_sidePanelsColor = new Color(0.75f, 0.75f, 0.75f);
        static private readonly Color s_topPanelRoutineColor = new Color(0.65f, 0.65f, 0.7f);
        static private readonly Color s_topPanelSelectedRoutineColor = new Color(0.5f, 0.7f, 0.5f);
        static private readonly Color s_nodeEditorColor = new Color(0.4f, 0.4f, 0.4f);
        #endregion

        static private TriggerEditorWindow s_window;

        [NonSerialized] private bool m_initialized = false;

        #region Buttons
        public enum ButtonAnswer
        {
            NoButton,
            Add,
            Modify,
            Delete,
            MoveUp,
            MoveDown,
            Duplicate
        }

        private Dictionary<ButtonAnswer, Texture2D> m_buttonIcons;
        private Dictionary<ButtonAnswer, string> m_buttonTooltips;
        #endregion

        #region Left panel
        private bool m_resizingLeftPanel = false;
        private Vector2 m_leftPanelScroolViewPosition;
        #region Triggers list
        [SerializeField] private string m_newTriggerName;
        #endregion
        #region Routine list
        
        #endregion
        #endregion

        #region Right panel
        private bool m_resizingRightPanel = false;
        private Vector2 m_rightPanelScroolViewPosition;
        private int m_currentNodeCategoryIndex = 0;
        private bool m_onlyShowMatchingMethod = false;
        #endregion

        #region Top panel
        [SerializeField] private string m_newRoutineName;
        private bool m_resizingTopPanel = false;
        private Vector2 m_topPanelScroolViewPosition;
        private bool m_routineFoldoutEvents = true;
        private bool m_routineFoldoutConditions = true;
        private bool m_routineFoldoutActions = true;
        #endregion

        [SerializeField] private Trigger m_selectedTrigger;
        [SerializeField] private Routine m_selectedRoutine;

        public struct NodeSetData
        {
            public string[] nodeCategories;
            public MethodInfo[][] nodeMethods;
            public NodeMethodAttribute[][] nodeMethodsAttributes;
        }

        private NodeSetData m_defaultNodeSet;
        private NodeSetData m_actionNodeSet;
        private NodeSetData m_testNodeSet;

        private Dictionary<string, NodeCategory> m_categoriesData;

        #region Initialization
        static public void CreateWindow()
        {
            s_window = GetWindow<TriggerEditorWindow>("Triggers editor");
        }

        [MenuItem("Window/Triggers editor")]
        static public void ShowWindow()
        {
            window.Show();
        }

        protected void InitializeIfNeeded()
        {
            if (m_initialized) return;

            if(!EditorPrefs.HasKey("TriggerEditor_LeftPanelRatio")) EditorPrefs.SetFloat("TriggerEditor_LeftPanelRatio", 0.15f);
            if(!EditorPrefs.HasKey("TriggerEditor_RightPanelRatio")) EditorPrefs.SetFloat("TriggerEditor_RightPanelRatio", 0.15f);
            if(!EditorPrefs.HasKey("TriggerEditor_TopPanelHeightRatio")) EditorPrefs.SetFloat("TriggerEditor_TopPanelHeightRatio", 0.5f);
            InitButtonsDictionaries();
            InitializeNodeMethods();

            Undo.undoRedoPerformed += Repaint;

            m_initialized = true;
        }

        private void InitButtonsDictionaries()
        {
            m_buttonIcons = new Dictionary<ButtonAnswer, Texture2D>();
            m_buttonTooltips = new Dictionary<ButtonAnswer, string>();

            AddButtonInfo(ButtonAnswer.Add, "Buttons/AddIcon.png", "Add");
            AddButtonInfo(ButtonAnswer.Modify, "Buttons/ModifyIcon.png", "Edit");
            AddButtonInfo(ButtonAnswer.Delete, "Buttons/DeleteIcon.png", "Delete");
            AddButtonInfo(ButtonAnswer.MoveUp, "Buttons/MoveUpIcon.png", "Move up");
            AddButtonInfo(ButtonAnswer.MoveDown, "Buttons/MoveDownIcon.png", "Move down");
            AddButtonInfo(ButtonAnswer.Duplicate, "Buttons/DuplicateIcon.png", "Duplicate");
            AddButtonInfo(ButtonAnswer.NoButton, "", "");
        }

        private void AddButtonInfo(ButtonAnswer buttonAnswer, string iconName, string tooltip)
        {
            m_buttonIcons.Add(buttonAnswer, LoadIcon(iconName));
            m_buttonTooltips.Add(buttonAnswer, tooltip);
        }

        private void InitializeNodeMethods()
        {
            m_defaultNodeSet = RetrieveNodeMethods(NodeMethodAttribute.MethodType.Any);
            m_actionNodeSet = RetrieveNodeMethods(NodeMethodAttribute.MethodType.Action);
            m_testNodeSet = RetrieveNodeMethods(NodeMethodAttribute.MethodType.Test);
        }

        static private NodeSetData RetrieveNodeMethods(NodeMethodAttribute.MethodType filter)
        {
            NodeMethodAttribute[] nodeMethodsAttribute;
            MethodInfo[] nodeMethodsInfo = NodeMethodAttribute.GetNodeMethods(out nodeMethodsAttribute, filter);

            List<string> categories = GetNodeCategories(nodeMethodsAttribute);
            categories.Sort();

            List<NodeMethodAttribute>[] attributesByCategory = new List<NodeMethodAttribute>[categories.Count];
            List<MethodInfo>[] methodsByCategory = new List<MethodInfo>[categories.Count];
            for (int i = 0; i < categories.Count; i++)
            {
                attributesByCategory[i] = new List<NodeMethodAttribute>();
                methodsByCategory[i] = new List<MethodInfo>();
            }

            for (int i = 0; i < nodeMethodsAttribute.Length; i++)
            {
                int categoryIndex = categories.IndexOf(nodeMethodsAttribute[i].category);
                attributesByCategory[categoryIndex].Add(nodeMethodsAttribute[i]);
                methodsByCategory[categoryIndex].Add(nodeMethodsInfo[i]);
            }

            List<string> filteredCategories = new List<string>(categories.Count);
            List<NodeMethodAttribute[]> filteredNodeMethodsAttributes = new List<NodeMethodAttribute[]>(categories.Count);
            List<MethodInfo[]> filteredNodeMethods = new List<MethodInfo[]>(categories.Count);

            for (int i = 0; i < categories.Count; i++)
            {
                if (!categories[i].StartsWith("Hidden"))
                {
                    filteredCategories.Add(categories[i]);
                    filteredNodeMethodsAttributes.Add(attributesByCategory[i].ToArray());
                    filteredNodeMethods.Add(methodsByCategory[i].ToArray());
                }
            }

            return new NodeSetData
            {
                nodeMethodsAttributes = filteredNodeMethodsAttributes.ToArray(),
                nodeMethods = filteredNodeMethods.ToArray(),
                nodeCategories = filteredCategories.ToArray()
            };
        }

        static private List<string> GetNodeCategories(NodeMethodAttribute[] nodeMethodsAttribute)
        {
            List<string> categories = new List<string>();
            foreach (NodeMethodAttribute attribute in nodeMethodsAttribute)
            {
                if (!categories.Contains(attribute.category))
                {
                    categories.Add(attribute.category);
                }
            }

            return categories;
        }

        public void LoadNodeCategories()
        {
            m_categoriesData = new Dictionary<string, NodeCategory>();
            NodeCategory[] nodeCategories = NodeCategory.FindAllCategories();
            foreach(NodeCategory category in nodeCategories)
            {
                if(category != null) m_categoriesData.Add(category.name, category);
            }
        }
        #endregion

        protected void OnGUI()
        {
            InitializeIfNeeded();

            //GUI.EndGroup();

            DrawLeftPanel();
            DrawRightPanel();
            DrawTopPanel();
            DrawMainPanel();
            DrawPanelResizers();

            ProcessEvent(Event.current);
            if (GUI.changed) Repaint();

            //GUI.BeginGroup(new Rect());
        }

        #region Left panel
        protected void DrawLeftPanel()
        {
            EditorGUI.DrawRect(leftPanelRect, s_sidePanelsColor);
            GUILayout.BeginArea(leftPanelRect);
            m_leftPanelScroolViewPosition = EditorGUILayout.BeginScrollView(m_leftPanelScroolViewPosition);

            DrawTriggersList();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected void DrawTriggersList()
        {
            EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);

            if (triggers != null)
            {
                int triggerCount = triggers.Length;
                for (int i = 0; i < triggerCount; i++)
                {
                    if(triggers[i] != null)
                    {
                        DrawTriggerLink(triggers[i]);
                    }
                }
            }

            GUILayout.Space(16);
            DrawCreateTrigger();
        }

        protected void DrawTriggerLink(Trigger trigger)
        {
            switch (DrawLabeledIconButtons(trigger.name, true, ButtonAnswer.Modify, ButtonAnswer.Delete))
            {
                case ButtonAnswer.Modify:
                    m_selectedTrigger = trigger;
                    break;
                case ButtonAnswer.Delete:
                    TriggersManager.RemoveTrigger(trigger);
                    SetSceneDirty();
                    if (trigger == m_selectedTrigger)
                    {
                        m_selectedTrigger = null;
                        m_selectedRoutine = null;
                    }
                    break;
            }
        }

        protected void DrawCreateTrigger()
        {
            m_newTriggerName = EditorGUILayout.TextField(m_newTriggerName);
            EditorGUI.BeginDisabledGroup(!TriggersManager.TriggerNameIsValid(m_newTriggerName));
            if(GUILayout.Button("Create trigger"))
            {
                TriggersManager.CreateTrigger(m_newTriggerName);
                SetSceneDirty();
                m_newTriggerName = string.Empty;
                GUI.FocusControl("Dummy");
            }
            EditorGUI.EndDisabledGroup();
        }
        #endregion

        #region Right panel
        protected void DrawRightPanel()
        {
            EditorGUI.DrawRect(rightPanelRect, s_sidePanelsColor);
            GUILayout.BeginArea(rightPanelRect);
            m_rightPanelScroolViewPosition = EditorGUILayout.BeginScrollView(m_rightPanelScroolViewPosition);
            EditorGUI.BeginDisabledGroup(m_selectedRoutine == null);

            DrawNodeBrowser();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected void DrawNodeBrowser()
        {
            EditorGUILayout.LabelField("Node browser", EditorStyles.boldLabel);

            if(m_selectedRoutine != null)
            {
                string matchingMethodText = (m_selectedRoutine.type == Routine.RoutineType.Action) ? "Only show actions" : "Only show tests";
                m_onlyShowMatchingMethod = EditorGUILayout.Toggle(matchingMethodText, m_onlyShowMatchingMethod);
            }

            GUIContent[] categories = GetCategoriesAsGUIContents();
            m_currentNodeCategoryIndex = EditorGUILayout.Popup(m_currentNodeCategoryIndex, categories, GetCategoryGUIStyle(), GUILayout.Height(32));
            if (m_currentNodeCategoryIndex >= currentSet.nodeMethodsAttributes.Length) m_currentNodeCategoryIndex = 0;

            NodeMethodAttribute[] displayedNodes = currentSet.nodeMethodsAttributes[m_currentNodeCategoryIndex];
            MethodInfo nodeToAddMethod = null;

            for (int i = 0; i < displayedNodes.Length; i++)
            {
                if(DrawLabeledIconButtons(displayedNodes[i].name, true, ButtonAnswer.Add) == ButtonAnswer.Add)
                {
                    nodeToAddMethod = currentSet.nodeMethods[m_currentNodeCategoryIndex][i];
                }
            }

            if(nodeToAddMethod != null)
            {
                m_selectedRoutine.AddNode(nodeToAddMethod);
                SetSceneDirty();
            }
        }
        #endregion

        #region Top panel
        private void DrawTopPanel()
        {
            EditorGUI.DrawRect(topPanelRect, s_sidePanelsColor);
            GUILayout.BeginArea(topPanelRect);
            m_topPanelScroolViewPosition = EditorGUILayout.BeginScrollView(m_topPanelScroolViewPosition);

            DrawSelectedTriggerHeader();
            DrawRoutineLists();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected void DrawSelectedTriggerHeader()
        {
            EditorGUILayout.LabelField("Routines", EditorStyles.boldLabel);

            if (m_selectedTrigger == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();

            string newTriggerName = EditorGUILayout.TextField(m_selectedTrigger.name);
            if (newTriggerName != m_selectedTrigger.name)
            {
                m_selectedTrigger.name = newTriggerName;
                SetSceneDirty();
            }
            m_selectedTrigger.enabled = EditorGUILayout.ToggleLeft("Enabled at start", m_selectedTrigger.enabled);

            EditorGUILayout.EndHorizontal();
        }

        protected void DrawRoutineLists()
        {
            if (m_selectedTrigger == null)
            {
                return;
            }

            DrawRoutineList(Routine.RoutineType.Event, ref m_routineFoldoutEvents);
            DrawRoutineList(Routine.RoutineType.Condition, ref m_routineFoldoutConditions);
            DrawRoutineList(Routine.RoutineType.Action, ref m_routineFoldoutActions);

            GUILayout.Space(16);
            DrawCreateRoutine();
        }

        protected void DrawRoutineList(Routine.RoutineType listType, ref bool foldout)
        {
            foldout = EditorGUILayout.Foldout(foldout, listType.ToString() + "s");
            if (foldout)
            {
                DrawRoutineList(listType);
            }
        }

        protected void DrawRoutineList(Routine.RoutineType listType)
        {
            EditorGUI.indentLevel++;

            float rectOffset = (3 + (int)listType) * 18f + 2f;
            int previousCount = 0;
            for(int i = 0; i < (int)listType; i++)
            {
                Routine[] tmpRoutines = m_selectedTrigger.GetRoutines((Routine.RoutineType)i);
                previousCount += (tmpRoutines == null) ? 0 : tmpRoutines.Length;
            }
            rectOffset += 20f * previousCount;

            Routine[] routines = m_selectedTrigger.GetRoutines(listType);
            if (routines != null)
            {
                for (int i = 0; i < routines.Length; i++)
                {
                    Routine routine = routines[i];
                    DrawRoutineLine(routine, rectOffset + i * 20f, i);
                }
            }

            EditorGUI.indentLevel--;
        }

        protected void DrawRoutineLine(Routine routine, float rectOffset, int routineIndex)
        {
            NodeValue routineLastConnector = routine.finalNode.inputs[0].connection;
            Texture2D routineIcon = null;
            if (routineLastConnector != null)
            {
                MethodInfo nodeMethodInfo = routineLastConnector.node.methodInfo;
                if(nodeMethodInfo == null)
                {
                    routine.RemoveNode(routineLastConnector.node);
                    return;
                }
                NodeMethodAttribute nodeMethodAttribute = routineLastConnector.node.nodeMethodAttribute;
                if (nodeMethodAttribute == null)
                {
                    routine.RemoveNode(routineLastConnector.node);
                    return;
                }

                routineIcon = GetCategoryIcon(nodeMethodAttribute.category);
            }
            GUIContent routineNameGUIContent = new GUIContent(routine.name, routineIcon);

            float width = topPanelRect.width;
            Color routineColor = (routine == m_selectedRoutine) ? s_topPanelSelectedRoutineColor : s_topPanelRoutineColor;

            Rect routineRect = new Rect(16, rectOffset, width - 18, 18);
            EditorGUI.DrawRect(routineRect, routineColor);
            routineRect.position += Vector2.one * 2;
            routineRect.size -= Vector2.one * 4;
            EditorGUI.DrawRect(routineRect, Color.Lerp(routineColor, s_sidePanelsColor, 0.7f));

            switch (DrawLabeledIconButtons(routineNameGUIContent, false, ButtonAnswer.Modify, ButtonAnswer.Duplicate, ButtonAnswer.MoveDown, ButtonAnswer.MoveUp, ButtonAnswer.Delete))
            {
                case ButtonAnswer.Modify:
                    m_selectedRoutine = routine;
                    break;
                case ButtonAnswer.Duplicate:
                    m_selectedTrigger.DuplicateRoutine(routine);
                    SetSceneDirty();
                    break;
                case ButtonAnswer.MoveDown:
                    m_selectedTrigger.SwapRoutines(routineIndex, (routineIndex + 1) % m_selectedTrigger.GetRoutines(routine.type).Length, routine.type);
                    SetSceneDirty();
                    break;
                case ButtonAnswer.MoveUp:
                    m_selectedTrigger.SwapRoutines(routineIndex, (routineIndex == 0) ? m_selectedTrigger.GetRoutines(routine.type).Length - 1 : routineIndex - 1, routine.type);
                    SetSceneDirty();
                    break;
                case ButtonAnswer.Delete:
                    if (routine == m_selectedRoutine) m_selectedRoutine = null;
                    m_selectedTrigger.RemoveRoutine(routine);
                    SetSceneDirty();
                    break;
            }

            GUILayout.Space(2);
        }

        protected void DrawCreateRoutine()
        {
            m_newRoutineName = EditorGUILayout.TextField("Create routine :", m_newRoutineName);
            EditorGUI.BeginDisabledGroup((m_newRoutineName.Length == 0) || m_selectedTrigger.ContainsRoutine(m_newRoutineName));

            EditorGUILayout.BeginHorizontal();

            DrawCreateRoutine(Routine.RoutineType.Event);
            DrawCreateRoutine(Routine.RoutineType.Condition);
            DrawCreateRoutine(Routine.RoutineType.Action);

            EditorGUI.BeginDisabledGroup(m_selectedRoutine == null);
            if (GUILayout.Button("Rename selected"))
            {
                Undo.RegisterCompleteObjectUndo(m_selectedRoutine, "Rename routine");
                Undo.FlushUndoRecordObjects();
                m_selectedRoutine.name = m_newRoutineName;
                m_newRoutineName = string.Empty;
                SetSceneDirty();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        protected void DrawCreateRoutine(Routine.RoutineType routineType)
        {
            if (GUILayout.Button("Create " + routineType.ToString()))
            {
                m_selectedTrigger.CreateRoutine(m_newRoutineName, routineType);
                m_newRoutineName = string.Empty;
                GUI.FocusControl("Dummy");
            }
        }
        #endregion

        #region Main panel
        protected void DrawMainPanel()
        {
            EditorGUI.DrawRect(mainPanelRect, s_nodeEditorColor);
            GUILayout.BeginArea(mainPanelRect);

            RoutineView.Draw(m_selectedRoutine);

            GUILayout.EndArea();
        }
        #endregion

        #region Resizers
        protected void DrawPanelResizers()
        {
            EditorGUI.DrawRect(leftPanelResizerLineRect, Color.black);
            EditorGUIUtility.AddCursorRect(leftPanelResizerRect, MouseCursor.ResizeHorizontal);

            EditorGUI.DrawRect(rightPanelResizerLineRect, Color.black);
            EditorGUIUtility.AddCursorRect(rightPanelResizerRect, MouseCursor.ResizeHorizontal);

            EditorGUI.DrawRect(topPanelResizerLineRect, Color.black);
            EditorGUIUtility.AddCursorRect(topPanelResizerRect, MouseCursor.ResizeVertical);
        }

        protected void ResizeLeftPanel(Vector2 mousePosition)
        {
            leftPanelWidthRatio = mousePosition.x / position.width;
            if (leftPanelWidthRatio < s_minRatio) leftPanelWidthRatio = s_minRatio;
            if (leftPanelWidthRatio > (1 - s_minRatio - rightPanelWidthRatio)) leftPanelWidthRatio = 1 - s_minRatio - rightPanelWidthRatio;
            Repaint();
        }

        protected void ResizeRightPanel(Vector2 mousePosition)
        {
            rightPanelWidthRatio = 1 - (mousePosition.x / position.width);
            if (rightPanelWidthRatio < s_minRatio) rightPanelWidthRatio = s_minRatio;
            if (rightPanelWidthRatio > (1 - s_minRatio - leftPanelWidthRatio)) rightPanelWidthRatio = 1 - s_minRatio - leftPanelWidthRatio;
            Repaint();
        }

        protected void ResizeTopPanel(Vector2 mousePosition)
        {
            topPanelHeightRatio = mousePosition.y / position.height;
            if (topPanelHeightRatio < s_minRatio) topPanelHeightRatio = s_minRatio;
            if (topPanelHeightRatio > (1 - s_minRatio)) topPanelHeightRatio = 1 - s_minRatio;
            Repaint();
        }
        #endregion

        #region GUI Events
        protected void ProcessEvent(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (leftPanelResizerRect.Contains(e.mousePosition))
                    {
                        m_resizingLeftPanel = true;
                    }
                    else if(rightPanelResizerRect.Contains(e.mousePosition))
                    {
                        m_resizingRightPanel = true;
                    }
                    else if(topPanelResizerRect.Contains(e.mousePosition))
                    {
                        m_resizingTopPanel = true;
                    }
                    break;
                case EventType.MouseUp:
                    m_resizingLeftPanel = false;
                    m_resizingRightPanel = false;
                    m_resizingTopPanel = false;
                    break;
            }

            if (m_resizingLeftPanel) ResizeLeftPanel(e.mousePosition);
            if (m_resizingRightPanel) ResizeRightPanel(e.mousePosition);
            if (m_resizingTopPanel) ResizeTopPanel(e.mousePosition);
        }
        #endregion

        #region Helpers
        private Texture2D LoadIcon(string iconName)
        {
            return EditorGUIUtility.Load("Assets/Editor/Triggers editor/Icons/" + iconName) as Texture2D;
        }

        #region Buttons
        private GUIContent GetButtonContent(ButtonAnswer button)
        {
            return new GUIContent(m_buttonIcons[button], m_buttonTooltips[button]);
        }

        private ButtonAnswer DrawLabeledIconButtons(string name, bool labelOnRight, params ButtonAnswer[] buttons)
        {
            GUIContent nameGUIContent = new GUIContent(name);
            return DrawLabeledIconButtons(nameGUIContent, labelOnRight, buttons);
        }

        private ButtonAnswer DrawLabeledIconButtons(GUIContent nameGUIContent, bool labelOnRight, params ButtonAnswer[] buttons)
        {
            return DrawLabeledIconButtons(nameGUIContent, GUIStyle.none, labelOnRight, 16, 16, buttons);
        }

        private ButtonAnswer DrawLabeledIconButtons(GUIContent name, GUIStyle style, bool labelOnRight, int iconSize, int buttonsSize, params ButtonAnswer[] buttons)
        {
            EditorGUILayout.BeginHorizontal();
            ButtonAnswer result = ButtonAnswer.NoButton;
            if(!labelOnRight) EditorGUILayout.LabelField(name, style, GUILayout.Height(iconSize));
            for (int i = 0; i < buttons.Length; i++)
            {
                if (GUILayout.Button(GetButtonContent(buttons[i]), GUILayout.Height(buttonsSize), GUILayout.Width(buttonsSize * 1.8f)))
                {
                    result = buttons[i];
                }
            }
            if (labelOnRight) EditorGUILayout.LabelField(name, style, GUILayout.Height(iconSize));
            EditorGUILayout.EndHorizontal();
            return result;
        }
        #endregion

        #region Right panel
        private Texture2D GetCategoryIcon(string category)
        {
            category = NodeCategory.CategoryFileName(category);
            if (m_categoriesData.ContainsKey(category))
            {
                return m_categoriesData[category].icon;
            }

            //Fallback
            return m_buttonIcons[ButtonAnswer.Modify];
        }

        protected GUIStyle GetCategoryGUIStyle()
        {
            GUIStyle result = new GUIStyle(EditorStyles.popup);
            result.fixedHeight = 32;
            result.stretchHeight = true;
            result.padding = new RectOffset(4, 4, 2, 2);
            result.border = new RectOffset(8, 10, 8, 8);
            result.focused.background = null;

            return result;
        }

        protected GUIContent[] GetCategoriesAsGUIContents()
        {
            if(m_categoriesData == null)
            {
                LoadNodeCategories();
            }

            List<GUIContent> result = new List<GUIContent>();
            for(int i = 0; i < currentSet.nodeCategories.Length; i++)
            {
                result.Add(new GUIContent(currentSet.nodeCategories[i], GetCategoryIcon(currentSet.nodeCategories[i])));
            }
            return result.ToArray();
        }

        static public void SetSceneDirty()
        {
            if (Application.isPlaying) return;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        #endregion

        #region Unused
        static public void DrawRoundedRect(Rect rect, float cornerSize, Color penColor, Color fillColor)
        {
            Color previousColor = Handles.color;

            Vector2 cornerNW = new Vector2(rect.xMin, rect.yMin);
            Vector2 cornerNE = new Vector2(rect.xMax, rect.yMin);
            Vector2 cornerSE = new Vector2(rect.xMax, rect.yMax);
            Vector2 cornerSW = new Vector2(rect.xMin, rect.yMax);

            // Draw Inside
            Handles.color = fillColor;
            Handles.DrawSolidArc(cornerNW + new Vector2(cornerSize, cornerSize), Vector3.forward, Vector3.left, 90, cornerSize);
            Handles.DrawSolidArc(cornerNE + new Vector2(-cornerSize, cornerSize), Vector3.forward, Vector3.down, 90, cornerSize);
            Handles.DrawSolidArc(cornerSE + new Vector2(-cornerSize, -cornerSize), Vector3.forward, Vector3.right, 90, cornerSize);
            Handles.DrawSolidArc(cornerSW + new Vector2(cornerSize, -cornerSize), Vector3.forward, Vector3.up, 90, cornerSize);

            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(new Rect(rect.xMin + cornerSize, rect.yMin, rect.width - cornerSize * 2, rect.height), fillColor, Color.clear);
            Handles.DrawSolidRectangleWithOutline(new Rect(rect.xMin, rect.yMin + cornerSize, rect.width, rect.height - cornerSize * 2), fillColor, Color.clear);

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
        #endregion

        #region Accessors
        #region GUI
        static public TriggerEditorWindow window
        {
            get
            {
                if(s_window == null)
                {
                    CreateWindow();
                }

                return s_window;
            }
        }

        #region Panels
        #region Panel ratios
        public float leftPanelWidthRatio
        {
            get { return EditorPrefs.GetFloat("TriggerEditor_LeftPanelRatio"); }
            set { EditorPrefs.SetFloat("TriggerEditor_LeftPanelRatio", value); }
        }

        public float rightPanelWidthRatio
        {
            get { return EditorPrefs.GetFloat("TriggerEditor_RightPanelRatio"); }
            set { EditorPrefs.SetFloat("TriggerEditor_RightPanelRatio", value); }
        }

        public float topPanelHeightRatio
        {
            get { return EditorPrefs.GetFloat("TriggerEditor_TopPanelHeightRatio"); }
            set { EditorPrefs.SetFloat("TriggerEditor_TopPanelHeightRatio", value); }
        }
        #endregion

        #region Rects
        public Rect mainPanelRect
        {
            get { return new Rect(leftPanelWidthRatio * position.width, position.height * topPanelHeightRatio, (1f - leftPanelWidthRatio - rightPanelWidthRatio) * position.width, position.height * (1 - topPanelHeightRatio)); }
        }

        #region Left Panel
        public Rect leftPanelRect
        {
            get { return new Rect(0f, 0f, leftPanelWidthRatio * position.width, position.height); }
        }

        public Rect leftPanelResizerRect
        {
            get
            {
                return new Rect(leftPanelWidthRatio * position.width - s_resizerWidth / 2f, 0f, s_resizerWidth, position.height);
            }
        }

        public Rect leftPanelResizerLineRect
        {
            get
            {
                return new Rect(leftPanelWidthRatio * position.width, 0f, 1f, position.height);
            }
        }
        #endregion

        #region Right panel
        public Rect rightPanelRect
        {
            get { return new Rect((1 - rightPanelWidthRatio) * position.width, 0f, rightPanelWidthRatio * position.width, position.height); }
        }

        public Rect rightPanelResizerRect
        {
            get
            {
                return new Rect((1 - rightPanelWidthRatio) * position.width - s_resizerWidth / 2f, 0f, s_resizerWidth, position.height);
            }
        }

        public Rect rightPanelResizerLineRect
        {
            get
            {
                return new Rect((1 - rightPanelWidthRatio) * position.width, 0f, 1f, position.height);
            }
        }
        #endregion

        #region Top panel
        public Rect topPanelRect
        {
            get { return new Rect(leftPanelWidthRatio * position.width, 0f, (1f - leftPanelWidthRatio - rightPanelWidthRatio) * position.width, position.height * topPanelHeightRatio); }
        }

        public Rect topPanelResizerRect
        {
            get
            {
                return new Rect(leftPanelWidthRatio * position.width, position.height * topPanelHeightRatio - s_resizerWidth / 2f, (1f - leftPanelWidthRatio - rightPanelWidthRatio) * position.width, s_resizerWidth);
            }
        }

        public Rect topPanelResizerLineRect
        {
            get
            {
                return new Rect(leftPanelWidthRatio * position.width, position.height * topPanelHeightRatio, (1f - leftPanelWidthRatio - rightPanelWidthRatio) * position.width, 1f);
            }
        }
        #endregion

        #endregion

        #endregion
        #endregion
        #region Data
        static public Trigger[] triggers
        {
            get { return TriggersManager.triggers; }
        }

        private NodeSetData currentSet
        {
            get
            {
                if (!m_onlyShowMatchingMethod || (m_selectedRoutine == null)) return m_defaultNodeSet;
                else if (m_selectedRoutine.type == Routine.RoutineType.Action) return m_actionNodeSet;
                else return m_testNodeSet;
            }
        }
        #endregion
        #endregion
    }
}
