using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TechNode : MonoBehaviour
{
    [SerializeField] private TechID m_techID;
    private TechTree m_root;
    private TechNode m_previousNode;
    [SerializeField] private BioResources m_baseResearchCost;
    private Material m_material;

    private TechNode[] m_children;
    private int m_depth;

    private void Awake()
    {
        m_previousNode = transform.parent.GetComponent<TechNode>();
        m_root = GetComponentInParent<TechTree>();
        m_material = GetComponent<Renderer>().material;

        InitDepth();
    }

    private void InitDepth()
    {
        List<TechNode> children = new List<TechNode>();
        foreach (Transform child in transform)
        {
            TechNode node = child.GetComponent<TechNode>();
            if (node)
            {
                children.Add(node);
            }
        }
        m_children = children.ToArray();

        m_depth = 0;
        for (int i = 0; i < m_children.Length; i++)
        {
            m_depth = Mathf.Max(m_depth, m_children[i].depth + 1);
        }
    }

    #region Animations

    private IEnumerator ResearchCoroutine()
    {
        float time = 0;
        while (time < m_root.researchAnimationTime)
        {
            time += Time.deltaTime;
            m_material.SetFloat("_Factor", time / m_root.researchAnimationTime);
            yield return null;
        }
        m_material.SetFloat("_Factor", 1);
    }

    #endregion

    #region Public functions
    public bool Affordable()
    {
        return GameManager.singleton.CanAffordResources(researchCost);
    }

    public bool Research()
    {
        if (!researchable)
        {
            return false;
        }

        if (!GameManager.singleton.PayResources(researchCost))
        {
            SoundManager.PlayErrorSoundEffect();
            return false;
        }

        StartCoroutine(ResearchCoroutine());
        TechTree.IncreaseTechLevel(m_techID);
        SoundManager.PlayResearchSoundEffect();

        return true;
    }

    public BioResources researchCost
    {
        get { return m_baseResearchCost * (1 -  GameManager.singleton.techReductionFactor); }
    }

    public bool maxed
    {
        get { return TechTree.TechMaxed(m_techID); }
    }

    public TechID techID
    {
        get { return m_techID; }
    }

    public int maxRank
    {
        get { return TechTree.MaxTechRank(m_techID); }
    }

    public int rank
    {
        get { return TechTree.TechRank(m_techID); }
    }

    public bool unlocked
    {
        get { return ((m_previousNode == null) || m_previousNode.maxed); }
    }

    public bool researchable
    {
        get { return !maxed && unlocked; }
    }

    public int depth
    {
        get { return m_depth; }
    }

    public TechNode[] tree
    {
        get
        {
            List<TechNode> result = new List<TechNode>();
            for (int i = 0; i < m_children.Length; i++)
            {
                TechNode[] tmp = m_children[i].tree;
                for (int j = 0; j < tmp.Length; j++)
                {
                    result.Add(tmp[j]);
                }
                result.Add(m_children[i]);
            }

            //if(!result.Contains(this))
            //{
            //    result.Add(this);
            //}
            return result.ToArray();
        }
    }

    public float hiddenRatio
    {
        get { return m_material.GetFloat("_HiddenFactor"); }
        set { m_material.SetFloat("_HiddenFactor", value); }
    }
    #endregion

    [ContextMenu("Update Node Name")]
    private void UpdateNodeName()
    {
        name = "TechNode " + m_techID;
    }
}
