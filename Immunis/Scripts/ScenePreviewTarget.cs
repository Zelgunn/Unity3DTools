using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class ScenePreviewTarget : MonoBehaviour
{
    private bool m_triggered = false;
    private List<SteamVR_TrackedController> m_controllers = new List<SteamVR_TrackedController>();

    private void Update()
    {
        transform.LookAt(GameManager.camera.transform);

        bool held = false;
        foreach(SteamVR_TrackedController controller in m_controllers)
        {
            held |= controller.triggerPressed;
            if (controller.triggerPressed && (transform.parent == PreviewManager.singleton.transform))
            {
                transform.SetParent(controller.transform);
            }
        }

        if(!held)
        {
            transform.SetParent(PreviewManager.singleton.transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<SteamVR_Camera>() != null)
        {
            m_triggered = true;
            return;
        }

        SteamVR_TrackedController controller = other.GetComponent<SteamVR_TrackedController>();
        if(controller && !m_controllers.Contains(controller))
        {
            m_controllers.Add(controller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SteamVR_TrackedController controller = other.GetComponent<SteamVR_TrackedController>();
        if (controller && m_controllers.Contains(controller))
        {
            m_controllers.Remove(controller);
        }
    }

    public void Reset(Transform copyTransform)
    {
        transform.position = copyTransform.position;
        transform.localScale = copyTransform.lossyScale;
        m_triggered = false;
    }

    public bool triggered
    {
        get { return m_triggered; }
    }
}
