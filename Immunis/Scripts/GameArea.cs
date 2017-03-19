using UnityEngine;
using System.Collections.Generic;

public class GameArea : MonoBehaviour
{
    static private Dictionary<GamePhase, GameArea> s_gameAreas = new Dictionary<GamePhase, GameArea>();
    [SerializeField] private GamePhase m_phase;
    [Header("Pivots")]
    [SerializeField] private Transform m_sceneRoomScaleTransform;
    [SerializeField] private Transform m_scenePreviewCameraPivot;
    [SerializeField] private Transform m_scenePreviewTargetPosition;
    [Header("Render settings")]
    [SerializeField] private Color m_ambientSkyColor = Color.white;
    [SerializeField] private Color m_ambientEquatorColor = Color.white;
    [SerializeField] private Color m_ambientGroundColor = Color.white;

    private void Awake()
    {
        s_gameAreas.Add(m_phase, this);
    }

    public Transform sceneRoomScaleTransform
    {
        get { return m_sceneRoomScaleTransform; }
    }

    public Transform scenePreviewCameraPivot
    {
        get { return m_scenePreviewCameraPivot; }
    }

    public Transform scenePreviewTargetPosition
    {
        get { return m_scenePreviewTargetPosition; }
    }

    [ContextMenu("Apply Render settings")]
    public void ApplyAmbientLightning()
    {
        RenderSettings.ambientSkyColor = m_ambientSkyColor;
        RenderSettings.ambientEquatorColor = m_ambientEquatorColor;
        RenderSettings.ambientGroundColor = m_ambientGroundColor;
    }

    static public GameArea GetGameArea(GamePhase phase)
    {
        return s_gameAreas[phase];
    }
}
