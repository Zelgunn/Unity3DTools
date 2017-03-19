using UnityEngine;
using System.Collections;

public class PreviewManager : MonoBehaviour
{
    static private PreviewManager s_singleton;
    [SerializeField] private Camera m_scenePreviewCamera;
    [SerializeField] private ScenePreviewTarget m_scenePreviewTarget;

    private void Awake ()
    {
        s_singleton = this;
    }

    public IEnumerator PreviewSceneCoroutine(GamePhase from, GamePhase to)
    {
        m_scenePreviewCamera.gameObject.SetActive(true);
        m_scenePreviewTarget.gameObject.SetActive(true);

        Transform cameraPivot = GameArea.GetGameArea(to).scenePreviewCameraPivot;
        m_scenePreviewCamera.transform.position = cameraPivot.position;

        m_scenePreviewTarget.Reset(GameArea.GetGameArea(from).scenePreviewTargetPosition);

        while(!m_scenePreviewTarget.triggered)
        {
            m_scenePreviewCamera.transform.rotation = cameraPivot.rotation * GameManager.camera.transform.rotation;
            yield return null;
        }

        m_scenePreviewCamera.gameObject.SetActive(false);
        m_scenePreviewTarget.transform.SetParent(transform);
        m_scenePreviewTarget.gameObject.SetActive(false);
    }

    static public PreviewManager singleton
    {
        get { return s_singleton; }
    }
}
