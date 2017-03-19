using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellsManager : MonoBehaviour
{
    static private CellsManager s_singleton;

    private List<Cell> m_cells = new List<Cell>(50);
    [SerializeField] private Cell[] m_registeredCells;

    [Header("Tech : Fight")]
    [SerializeField] private float m_staminaBuffFactor = 0.1f;
    [SerializeField] private float m_strengthBuffFactor = 0.1f;
    [SerializeField] private float m_speedBuffFactor = 0.1f;
    [SerializeField] private float m_regenerationBuffFactor = 0.05f;
    [SerializeField] private float m_armorBuffFactor = 1;
    [SerializeField] private float m_cooperationBuffFactor = 0.05f;
    [SerializeField] private float m_leechBuffFactor = 0.1f;
    [SerializeField] private float m_berserkBuffFactor = 0.25f;
    [SerializeField] private float m_immortalBuffFactor = 0.1f;
    private float m_staminaBuff;
    private float m_strengthBuff;
    private float m_speedBuff;
    private float m_regenerationBuff;
    private float m_armorBuff;
    private float m_cooperationBuff;
    private float m_leechBuff;
    private float m_berserkBuff;
    private float m_immortalBuff;

    [Header("Tech : Adapt")]
    [SerializeField] private float m_biomassEconomyFactor = 0.1f;
    [SerializeField] private Transform[] m_reinforcementsSpawns;
    [SerializeField] private float m_reinforcementChancesPerSecond = 0.1f;
    private List<Cell> m_reinforcements = new List<Cell>();

    private void Awake()
    {
        s_singleton = this;
    }

    private void Update()
    {
        //if(GameManager.singleton.invasionPhase)
        //{
        //    float chancesToSummonReinforcement = Time.deltaTime * m_reinforcementChancesPerSecond * TechTree.TechRank(TechID.Adapt_Backups);
        //    if(Random.Range(0f, 1f) < chancesToSummonReinforcement)
        //    {
        //        SpawnReinforcement();
        //    }
        //}
    }

    private void UpdateBuffs()
    {
        m_staminaBuff = TechTree.TechRank(TechID.Fight_Stamina) * m_staminaBuffFactor;
        m_strengthBuff = TechTree.TechRank(TechID.Fight_Strength) * m_strengthBuffFactor;
        m_speedBuff = TechTree.TechRank(TechID.Fight_Speed) * m_speedBuffFactor;
        m_regenerationBuff = TechTree.TechRank(TechID.Fight_Regeneration) * m_regenerationBuffFactor;
        m_armorBuff = TechTree.TechRank(TechID.Fight_Armor) * m_armorBuffFactor;
        m_cooperationBuff = TechTree.TechRank(TechID.Fight_Cooperation) * m_cooperationBuffFactor;
        m_leechBuff = TechTree.TechRank(TechID.Fight_Leech) * m_leechBuffFactor;
        m_berserkBuff = TechTree.TechRank(TechID.Fight_Berserk) * m_berserkBuffFactor;
        m_immortalBuff = TechTree.TechRank(TechID.Fight_Immortals) * m_immortalBuffFactor;
    }

    public void SpawnCells()
    {
        List<CellPlaceholder> placeholders = CellPlaceholderManager.singleton.placeholders;
        m_cells.Clear();
        UpdateBuffs();
        for (int i = 0; i < placeholders.Count; i++)
        {
            CellPlaceholder cellPlaceholder = placeholders[i];
            Cell cell = Instantiate(cellPlaceholder.cell);

            cell.name = string.Format("Cell n°{0}", i);
            cell.transform.SetParent(transform);
            cell.transform.position = cellPlaceholder.transform.position;
            cell.transform.rotation = cellPlaceholder.transform.rotation;

            cell.AssignCellRenderer(cellPlaceholder.cellRenderer);

            m_cells.Add(cell);
        }
    }

    private void SpawnReinforcement()
    {
        Cell cell = Instantiate(m_registeredCells[0]);
        CellRenderer cellRenderer = Instantiate(cell.cellRendererPrefab);

        cell.name = string.Format("Reinforcement Cell {0}", m_reinforcements.Count);
        cell.transform.SetParent(transform);
        cell.transform.position = m_reinforcementsSpawns[Random.Range(0, m_reinforcementsSpawns.Length)].position;

        cell.AssignCellRenderer(cellRenderer);

        m_reinforcements.Add(cell);
    }

    public void RemoveCells()
    {
        m_cells.Remove(null);

        while(m_cells.Count > 0)
        {
            Destroy(m_cells[0]);
            m_cells.RemoveAt(0);
        }

        m_reinforcements.Remove(null);

        while (m_reinforcements.Count > 0)
        {
            Destroy(m_reinforcements[0]);
            m_cells.RemoveAt(0);
        }
    }

    public int livingCellsCount
    {
        get
        {
            int result = 0;

            foreach(Cell cell in m_cells)
            {
                if(cell && !cell.dead)
                {
                    result++;
                }
            }

            return result;
        }
    }

    static public CellsManager singleton
    {
        get { return s_singleton; }
    }

    #region Tech
    #region Buffs
    static public float staminaBuff
    {
        get { return s_singleton.m_staminaBuff; }
    }
    static public float strengthBuff
    {
        get { return s_singleton.m_strengthBuff; }
    }
    static public float speedBuff
    {
        get { return s_singleton.m_speedBuff; }
    }
    static public float regenerationBuff
    {
        get { return s_singleton.m_regenerationBuff; }
    }
    static public float armorBuff
    {
        get { return s_singleton.m_armorBuff; }
    }
    static public float cooperationBuff
    {
        get { return s_singleton.m_cooperationBuff; }
    }
    static public float leechBuff
    {
        get { return s_singleton.m_leechBuff; }
    }
    static public float berserkBuff
    {
        get { return s_singleton.m_berserkBuff; }
    }
    static public float immortalBuff
    {
        get { return s_singleton.m_immortalBuff; }
    }
    #endregion

    static public float biomassEconomyFactor
    {
        get { return s_singleton.m_biomassEconomyFactor * TechTree.TechRank(TechID.Adapt_Economy); }
    }
    #endregion

    static public Cell[] registeredCells
    {
        get { return s_singleton.m_registeredCells; }
    }

    static public int registeredCellsCount
    {
        get { return s_singleton.m_registeredCells.Length; }
    }
}
