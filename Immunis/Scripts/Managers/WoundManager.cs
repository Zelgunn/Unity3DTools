using UnityEngine;
using System.Collections;

public class WoundManager : MonoBehaviour
{
    static private WoundManager s_singleton;
    [SerializeField] private Transform m_spray;
    [SerializeField] private ParticleSystem m_sprayParticleSystem;
    [SerializeField] private Transform[] m_corners;
    [SerializeField] private int m_particleCount = 20;
    [SerializeField] private int m_particlesToCleanWound = 30;
    private int m_particlesOnWound = 0;
    [SerializeField] private LayerMask m_handLayerMask;
    [SerializeField] private Collider m_woundCollider;
    [SerializeField] private GameObject m_sprayEffectPrefab;

	private void Awake ()
    {
        s_singleton = this;
	}

    private void Update()
    {
        if (!GameManager.singleton.woundPhase)
        {
            return;
        }

        m_spray.LookAt(PlayerCursor.singleton.targetPosition);

        if(InputsManager.singleton.rightTriggerDown || Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(SprayCoroutine());
        }
    }

    private IEnumerator SprayCoroutine()
    {
        m_sprayParticleSystem.Play();
        // Spray area !
        yield return new WaitForSeconds(0.25f);
        RaycastHit hit;
        for (int i = 0; i < m_particleCount; i++)
        {
            Ray ray = RandomRayInCone();
            if (Physics.Raycast(ray, out hit, 20, m_handLayerMask.value))
            {
                GameObject sprayEffect = Instantiate(m_sprayEffectPrefab);
                sprayEffect.transform.position = hit.point;
                sprayEffect.transform.SetParent(transform);

                if(hit.collider == m_woundCollider)
                {
                    m_particlesOnWound++;
                }
            }
        }
    }

    public Ray RandomRayInCone()
    {
        Vector3 direction = Vector3.zero;

        for (int i = 0; i < m_corners.Length; i++)
        {
            direction += m_corners[i].forward * Random.Range(0f, 1f);
        }

        Ray ray = new Ray(m_sprayParticleSystem.transform.position, direction);

        return ray;
    }

    private void OnDrawGizmosSelected()
    {
        foreach(Transform corner in m_corners)
        {
            Gizmos.DrawRay(corner.transform.position, corner.transform.forward);
        }
    }

    public bool woundClean
    {
        get { return m_particlesOnWound >= m_particlesToCleanWound; }
    }

    static public WoundManager singleton
    {
        get { return s_singleton; }
    }
}
