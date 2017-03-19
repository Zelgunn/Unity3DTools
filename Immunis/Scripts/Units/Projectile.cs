using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    static private Transform s_projectilesParent;
    [SerializeField] private float m_ttl = 5;
    private Rigidbody m_rigidbody;
    private Unit m_source;
    private UnitFaction m_factionMask;

	private void Awake ()
    {
        if(!s_projectilesParent)
        {
            s_projectilesParent = new GameObject("Projectiles").transform;
        }
        m_rigidbody = GetComponent<Rigidbody>();
        transform.SetParent(s_projectilesParent);
        Destroy(gameObject, m_ttl);
    }

    new public Rigidbody rigidbody
    {
        get { return m_rigidbody; }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Unit unit = collision.collider.GetComponent<Unit>();
        if(unit && !((unit.faction & m_factionMask) == unit.faction))
        {
            m_source.DamageUnit(unit);
            Destroy(gameObject);
        }
    }

    public Unit source
    {
        get { return m_source; }
        set { m_source = value; }
    }

    public UnitFaction factionMask
    {
        get { return m_factionMask; }
        set { m_factionMask = value; }
    }
}
