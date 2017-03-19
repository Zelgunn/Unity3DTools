using UnityEngine;
using System.Collections;

public class PlayerCursor : MonoBehaviour
{
    static private PlayerCursor s_singleton;
    [SerializeField] private Transform m_cursorEndBone;
    [SerializeField] private Vector3 m_cursorDirection = new Vector3(0, -0.5f, 2);
    [SerializeField] private Renderer m_renderer;
    [SerializeField] private LayerMask m_targetLayer;
    private Vector3 m_targetPosition;
    private GameObject m_target;

	private void Awake ()
    {
        s_singleton = this;
    }

    private void Update()
    {
        //transform.Rotate(Vector3.right, -10 * Time.deltaTime * Input.GetAxis("Vertical"), Space.Self);

        if(GameManager.singleton.invasionPhase)
        {
            UpdateNoRaycast();
        }
        else
        {
            UpdateRaycast();
        }

        m_cursorEndBone.position = m_targetPosition;
    }

    private void UpdateRaycast()
    {

        Ray ray = new Ray(transform.position, transform.TransformDirection(m_cursorDirection));
        RaycastHit hit;
        //
        bool hasTarget = Physics.Raycast(ray, out hit, 20, m_targetLayer.value);
        m_renderer.enabled = hasTarget;

        if (!hasTarget)
        {
            m_target = null;
        }
        else
        {
            m_targetPosition = hit.point;
            m_target = hit.collider.gameObject;
        }
    }

    private void UpdateNoRaycast()
    {
        m_renderer.enabled = true;
        m_target = null;
        m_targetPosition = transform.position + transform.TransformDirection(m_cursorDirection) * 10;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, transform.TransformDirection(m_cursorDirection) * 20);
    }

    public GameObject target
    {
        get { return m_target; }
    }

    public bool hasTarget
    {
        get { return m_target != null; }
    }

    public Vector3 targetPosition
    {
        get { return m_targetPosition; }
    }

    static public PlayerCursor singleton
    {
        get { return s_singleton; }
    }
}
