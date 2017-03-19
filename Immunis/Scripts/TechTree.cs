using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TechID
{
    Fight_Stamina,
    Fight_Strength,
    Fight_Speed,
    Fight_Regeneration,
    Fight_Armor,
    Fight_Cooperation,
    Fight_Leech,
    Fight_Berserk,
    Fight_Immortals,

    Evol_Regeneration,
    Evol_Thorns,
    Evol_Anticipation,
    Evol_Defense,
    Evol_Fortress,
    Evol_Patriot,
    Evol_Bastion,
    Evol_DoubleHanded,
    Evol_NoWaste,

    Adapt_AntiBacteria,
    Adapt_AntiEColi,
    Adapt_AntiPollen,
    Adapt_Economy,
    Adapt_CellDivision,
    Adapt_BiomassRecovery,
    Adapt_FastEvolution,
    Adapt_Backups
}

public class TechTree : MonoBehaviour
{
    static private Dictionary<TechID, int> s_techRanks = new Dictionary<TechID, int>(26);
    static private List<TechTree> s_techTrees = new List<TechTree>(1);
   [SerializeField] private float m_showAnimationTime = 0.25f;
    [SerializeField] private float m_researchAnimationTime = 0.25f;

    private bool m_shown = false;
    private bool m_animationInProgress = false;

    private TechNode[] m_nodes;
    private TechNode[][] m_nodesByDepth;
    private TechNode m_rootNode;

    private int m_selectedNodeIndex = 0;
    private TechNode[] m_researchableTechNodes;

    private void Awake ()
    {
        m_rootNode = GetComponentInChildren<TechNode>();
        s_techTrees.Add(this);
    }

    private void Start()
    {
        ComputeNodesByDepth();
        UpdateResearchableTechNodes();
        Show(false, 0);
    }

    #region Initialisation
    private void ComputeNodesByDepth()
    {
        m_nodes = m_rootNode.tree;
        List<TechNode> fix = new List<TechNode>(m_nodes);
        fix.Add(m_rootNode);
        m_nodes = fix.ToArray();
        m_nodesByDepth = new TechNode[m_rootNode.depth + 1][];
        List<TechNode>[] resultList = new List<TechNode>[m_nodesByDepth.Length];

        for (int i = 0; i < resultList.Length; i++)
        {
            resultList[i] = new List<TechNode>();
        }

        for (int i = 0; i < m_nodes.Length; i++)
        {
            int nodeDepth = m_nodes[i].depth;
            resultList[nodeDepth].Add(m_nodes[i]);
        }

        for (int i = 0; i < resultList.Length; i++)
        {
            m_nodesByDepth[i] = resultList[i].ToArray();
        }

        TechNode[] tmp = new TechNode[1];
        tmp[0] = m_rootNode;
        m_nodesByDepth[m_nodesByDepth.Length - 1] = tmp;
    }
    #endregion

    private void Update()
    {
        bool show = ControllerUI.singleton && (ControllerUI.singleton.menu == ControllerUI.ControllerUIMenus.TechTreeMenu) && GameManager.singleton.preparationPhase;

        if (show != m_shown)
        {
            Show(show, m_showAnimationTime);
        }

        if(!m_shown)
        {
            return;
        }

        int tmp = InputsManager.singleton.leftPadPressVerticalValue;
        if (tmp != 0)
        {
            m_selectedNodeIndex = (m_selectedNodeIndex + tmp) % m_researchableTechNodes.Length;
            if (m_selectedNodeIndex < 0) m_selectedNodeIndex = m_researchableTechNodes.Length - 1;
        }

        if (InputsManager.singleton.leftTriggerDown)
        {
            selectedTechNode.Research();
        }
    }

    #region Show / Hide
    public void Show(bool show, float duration)
    {
        if (m_animationInProgress)
        {
            return;
        }

        if (show)
        {
            StartCoroutine(ShowCoroutine(duration));
        }
        else
        {
            StartCoroutine(HideCoroutine(duration));
        }
    }

    private IEnumerator ShowCoroutine(float duration)
    {
        m_animationInProgress = true;

        TechNode[][] nodes = m_nodesByDepth;
        for (int i = nodes.Length; i > 0; i--)
        {
            TechNode[] nodesAtDepth = nodes[i - 1];
            float time = 0;

            while (time < duration)
            {
                float ratio = time / duration;
                for (int j = 0; j < nodesAtDepth.Length; j++)
                {
                    nodesAtDepth[j].hiddenRatio = ratio;
                }
                time += Time.deltaTime;
                if (time < duration)
                    yield return null;
            }

            for (int j = 0; j < nodesAtDepth.Length; j++)
            {
                nodesAtDepth[j].hiddenRatio = 2;
            }
        }

        m_animationInProgress = false;
        m_shown = true;
    }

    private IEnumerator HideCoroutine(float duration)
    {
        m_animationInProgress = true;

        TechNode[][] nodes = m_nodesByDepth;
        for (int i = 0; i < nodes.Length; i++)
        {
            TechNode[] nodesAtDepth = nodes[i];
            float time = 0;

            while (time < duration)
            {
                float ratio = 1 - time / duration;
                for (int j = 0; j < nodesAtDepth.Length; j++)
                {
                    nodesAtDepth[j].hiddenRatio = ratio;
                }
                time += Time.deltaTime;
                if (time < duration)
                    yield return null;
            }

            for (int j = 0; j < nodesAtDepth.Length; j++)
            {
                nodesAtDepth[j].hiddenRatio = -1;
            }
        }

        m_animationInProgress = false;
        m_shown = false;
    }

    public bool shown
    {
        get { return m_shown; }
        set { Show(value, m_showAnimationTime); }
    }

    #endregion

    public float researchAnimationTime
    {
        get { return m_researchAnimationTime; }
    }

    public int depth
    {
        get { return m_nodesByDepth.Length; }
    }

    static public void IncreaseTechLevel(TechID techID)
    {
        if (!s_techRanks.ContainsKey(techID))
        {
            s_techRanks.Add(techID, 0);
        }

        int tmp = s_techRanks[techID];

        s_techRanks[techID]++;
        if (s_techRanks[techID] > MaxTechRank(techID)) s_techRanks[techID] = MaxTechRank(techID);
        print(s_techRanks[techID]);

        if(s_techRanks[techID] != tmp)
        {
            OnResearchDone(techID, s_techRanks[techID]);
        }
    }

    static public int TechRank(TechID techID)
    {
        if(!s_techRanks.ContainsKey(techID))
        {
            return 0;
        }

        return s_techRanks[techID];
    }

    static public bool TechMaxed(TechID techID)
    {
        return TechRank(techID) == MaxTechRank(techID);
    }

    static public int MaxTechRank(TechID techID)
    {
        int maxTechRank = 0;

        switch(techID)
        {
            case TechID.Fight_Stamina:
                maxTechRank = 3;
                break;
            case TechID.Fight_Strength: 
                maxTechRank = 3;
                break;
            case TechID.Fight_Speed: 
                maxTechRank = 3;
                break;
            case TechID.Fight_Regeneration: 
                maxTechRank = 1;
                break;
            case TechID.Fight_Armor: 
                maxTechRank = 2;
                break;
            case TechID.Fight_Cooperation:
                maxTechRank = 1;
                break;
            case TechID.Fight_Leech: 
                maxTechRank = 3;
                break;
            case TechID.Fight_Berserk: 
                maxTechRank = 1;
                break;
            case TechID.Fight_Immortals:
                maxTechRank = 1;
                break;

            case TechID.Evol_Regeneration : 
                maxTechRank = 1;
                break;
            case TechID.Evol_Thorns:
                maxTechRank = 1;
                break;
            case TechID.Evol_Anticipation: 
                maxTechRank = 3;
                break;
            case TechID.Evol_Defense:
                maxTechRank = 3;
                break;
            case TechID.Evol_Fortress: 
                maxTechRank = 1;
                break;
            case TechID.Evol_Patriot:
                maxTechRank = 1;
                break;
            case TechID.Evol_Bastion:
                maxTechRank = 3;
                break;
            case TechID.Evol_DoubleHanded: 
                maxTechRank = 1;
                break;
            case TechID.Evol_NoWaste:
                maxTechRank = 1;
                break;

            case TechID.Adapt_AntiBacteria : 
                maxTechRank = 1;
                break;
            case TechID.Adapt_AntiEColi:
                maxTechRank = 1;
                break;
            case TechID.Adapt_AntiPollen:
                maxTechRank = 1;
                break;
            case TechID.Adapt_Economy:
                maxTechRank = 2;
                break;
            case TechID.Adapt_CellDivision:
                maxTechRank = 1;
                break;
            case TechID.Adapt_BiomassRecovery:
                maxTechRank = 3;
                break;
            case TechID.Adapt_FastEvolution:
                maxTechRank = 2;
                break;
            case TechID.Adapt_Backups:
                maxTechRank = 1;
                break;
        }

        return maxTechRank;
    }

    static private void OnResearchDone(TechID techID, int rank)
    {
        switch(techID)
        {
            case TechID.Evol_Fortress:
                PlayerCore.singleton.EvolveToFortress();
                break;
        }

        for(int i = 0; i < s_techTrees.Count; i++)
        {
            s_techTrees[i].UpdateResearchableTechNodes();
        }
    }

    private void UpdateResearchableTechNodes()
    {
        List<TechNode> researchableNodes = new List<TechNode>(3);

        for(int i = 0; i < m_nodes.Length; i++)
        {
            if(m_nodes[i].researchable)
            {
                researchableNodes.Add(m_nodes[i]);
            }
        }

        m_researchableTechNodes = researchableNodes.ToArray();
        if(m_selectedNodeIndex >= m_researchableTechNodes.Length)
        {
            m_selectedNodeIndex = 0;
        }
    }

    public TechNode selectedTechNode
    {
        get { return m_researchableTechNodes[m_selectedNodeIndex]; }
    }

    public TechNode nextTechNode
    {
        get
        {
            int tmp = (m_selectedNodeIndex + 1) % m_researchableTechNodes.Length;
            return m_researchableTechNodes[tmp];
        }
    }

    public TechNode previousTechNode
    {
        get
        {
            int tmp = m_selectedNodeIndex - 1;
            if (tmp < 0) tmp = m_researchableTechNodes.Length - 1;
            return m_researchableTechNodes[tmp];
        }
    }
}
