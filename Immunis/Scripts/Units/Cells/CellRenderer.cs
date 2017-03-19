using UnityEngine;
using System.Collections;

public enum CellRenderingMode
{
    Default,
    Selected,
    OtherSelected,
    Affordable,
    NotAffordable
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class CellRenderer : MonoBehaviour
{
    [SerializeField] private Renderer m_renderer;
    private Material m_defaultMaterial;
    [SerializeField] private float m_baseRadius = 0.1f;
    [SerializeField] private Vector3 m_center = Vector3.zero;
    [Header("Materials")]
    [SerializeField] private Material m_selectedMaterial;
    [SerializeField] private Material m_otherSelectedMaterial;
    [SerializeField] private Material m_affordableMaterial;
    [SerializeField] private Material m_notAffordableMaterial;

    [SerializeField] private UnitDecorum m_decorum;

	private void Awake ()
    {
        m_defaultMaterial = m_renderer.material;

        m_decorum.Init(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + m_center, radius);
    }

    public void SetCellRenderingMode(CellRenderingMode mode)
    {
        Material material = m_defaultMaterial;
        switch(mode)
        {
            case CellRenderingMode.OtherSelected:
                material = m_otherSelectedMaterial;
                break;
            case CellRenderingMode.Selected:
                material = m_selectedMaterial;
                break;
            case CellRenderingMode.NotAffordable:
                material = m_notAffordableMaterial;
                break;
            case CellRenderingMode.Affordable:
                material = m_affordableMaterial;
                break;
        }
        m_renderer.material = material;
    }

    public float radius
    {
        get { return m_baseRadius * transform.localScale.x; }
    }

    public UnitDecorum decorum
    {
        get { return m_decorum; }
    }
}
