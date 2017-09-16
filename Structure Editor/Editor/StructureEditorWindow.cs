using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class StructureEditorWindow : EditorWindow
{
    static private StructureEditorWindow s_window;
    [NonSerialized] private bool m_initialized = false;

    //[SerializeField] private string m_name = "Name";
    //[SerializeField] private Vector3 m_size = new Vector3(5, 3, 5);
    //[SerializeField] private int[] m_links = new int[4];
    //[SerializeField] private int m_id;
    //[SerializeField] private int m_roomCreationPrefabIndex;
    //[SerializeField] private int m_roomCreationLinkIndex;
    //[SerializeField] private RoomData.LinkAnchor m_roomCreationLinkAnchor;

    [SerializeField] private int m_skinIndex = 0;

    [MenuItem("Window/Structure")]
    private static void Init()
    {
        window.Show();
    }

    [MenuItem("Finalize/Lights")]
    private static void BakeLights()
    {
        Lightmapping.Bake();
    }

    [MenuItem("Finalize/Navigation")]
    private static void BakeNavMesh()
    {
        SerializedObject settingsObject = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);

        SerializedProperty agentRadius = settingsObject.FindProperty("m_BuildSettings.agentRadius");
        agentRadius.floatValue = 0.2f;

        SerializedProperty agentHeight = settingsObject.FindProperty("m_BuildSettings.agentHeight");
        agentHeight.floatValue = 1.7f;

        settingsObject.ApplyModifiedProperties();
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
    }

    [MenuItem("Finalize/Scene")]
    private static void FinalizeScene()
    {
        BakeNavMesh();
        BakeLights();

        string currentScenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
        string currentSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { currentScenePath };
        buildPlayerOptions.locationPathName = currentSceneName + ".exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    private void OnGUI()
    {
        InitIfNeeded();
        DrawCreateRoomGUI();
        MyDebug();
    }

    private void InitIfNeeded()
    {
        if (m_initialized) return;

        m_initialized = true;
    }

    private void MyDebug()
    {
        if(!GUILayout.Button("Debug"))
        {
            return;
        }

        //Debug.Log(RoomManager.roomsGraph.GetSubparts().Length);
    }

    private void DrawCreateRoomGUI()
    {
        DrawHeader();
        RoomManager.roomsGraph.RemoveAllNull();
        DrawStructureParameters();

        Editor.CreateEditor(tmpRoom).OnInspectorGUI();

        EditorGUI.BeginDisabledGroup(!CanCreateRoom());
        if (GUILayout.Button(createRoomButtonGUIContent))
        {
            RoomManager.singleton.CreateRoom();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void DrawHeader()
    {
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        EditorGUILayout.LabelField("Room creation", titleStyle, GUILayout.MinHeight(20));

        RoomManager.singleton.tmpRoomModelIndex = EditorGUILayout.Popup("Room model", RoomManager.singleton.tmpRoomModelIndex, RoomManager.singleton.GetPrefabRoomsNames());

        EditorGUILayout.LabelField("Room parameters", EditorStyles.boldLabel);
        tmpRoom.name = EditorGUILayout.TextField("Name", tmpRoom.name);

        m_skinIndex = EditorGUILayout.Popup("Room skin", m_skinIndex, AssetHelpers.GetSkinsNames(RoomManager.GetRoomSkins()));
        tmpRoom.skin = RoomManager.GetRoomSkins()[m_skinIndex];
    }

    private void DrawStructureParameters()
    {
        if (RoomManager.singleton.GetHasExistingRooms())
        {
            RoomManager.singleton.tmpRoomAnchorIndex = EditorGUILayout.Popup("Build room next to", RoomManager.singleton.tmpRoomAnchorIndex, RoomManager.singleton.GetExistingRoomsNames());
            RoomManager.singleton.tmpRoomRelation = (Room.Direction) EditorGUILayout.EnumPopup("Anchor", RoomManager.singleton.tmpRoomRelation);
        }
    }

    #region GUI Helpers
    private bool CreateRoomNameTaken()
    {
        return RoomNameTaken(tmpRoom.name);
    }

    private bool CreateRoomNameValid()
    {
        return tmpRoom.name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
    }

    private bool CanCreateRoom()
    {
        return !CreateRoomNameTaken() & CreateRoomNameValid();
    }

    private GUIContent createRoomButtonGUIContent
    {
        get
        {
            string tooltip;
            if(CreateRoomNameTaken())
            {
                tooltip = "The name of the room is already taken";
            }
            else if(!CreateRoomNameValid())
            {
                tooltip = "The name of the room contains invalid characters";
            }
            else
            {
                tooltip = "Create a room with current parameters";
            }

            GUIContent guiContent = new GUIContent("Create", tooltip);
            return guiContent;
        }
    }
    #endregion

    public Room tmpRoom
    {
        get { return RoomManager.singleton.tmpRoom; }
    }

    #region Existing rooms helpers
    private bool RoomNameTaken(string name)
    {
        string[] existingRoomNames = RoomManager.singleton.GetExistingRoomsNames();
        if (existingRoomNames == null) return false;
        for (int i = 0; i < existingRoomNames.Length; i++)
        {
            if (name == existingRoomNames[i])
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Window helpers
    static public StructureEditorWindow window
    {
        get
        {
            if(s_window == null)
            {
                s_window = GetWindow<StructureEditorWindow>("Structure");
            }

            return s_window;
        }
    }
    #endregion
}
