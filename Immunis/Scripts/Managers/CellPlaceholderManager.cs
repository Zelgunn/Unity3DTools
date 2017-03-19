using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellPlaceholderManager : MonoBehaviour
{
    static private CellPlaceholderManager s_singleton;
    private int m_selectedCellIndex = 0;
    private int m_selectedCellUpgradeIndex = 0;
    private List<CellPlaceholder> m_placeholders = new List<CellPlaceholder>();
    private CellPlaceholder m_tempPlaceholder = null;
    private CellPlaceholder m_targetedCellPlaceholder;
    private CellPlaceholder m_persistingTargetedCellPlaceholder;

    private void Awake ()
    {
        s_singleton = this;
    }

	private void Update ()
    {
        if(!PlayerCursor.singleton)
        {
            return;
        }

        bool managerIsActive = GameManager.singleton.preparationPhase;
        managerIsActive &= (PlayerCursor.singleton.target == gameObject);
        ShowTempPlaceholder(managerIsActive);

        if (!managerIsActive)
        {
            return;
        }

        UpdateTargetPlaceholder();
        UpdateCellSelection();
        UpdateCellUpgradeSelection();
    }

    private void ShowTempPlaceholder(bool show)
    {
        CellPlaceholder targetedCellPlaceholder = GetTargetedCellPlaceholder();
        bool targetingExistingCell = targetedCellPlaceholder != null;

        if (m_tempPlaceholder)
        {
            m_tempPlaceholder.gameObject.SetActive(show && PlayerCursor.singleton.hasTarget && !targetingExistingCell);
        }
    }

    private void UpdateTargetPlaceholder()
    {
        if (m_tempPlaceholder == null)
        {
            CreateTempPlaceholder();
        }
        UpdatePlaceholders();

        if (InputsManager.singleton.rightTriggerDown || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (m_targetedCellPlaceholder == null)
            {
                BuildCell();
            }
            else
            {
                ModifyPlaceholder();
            }
        }
    }

    private void UpdateCellSelection()
    {
        if (!ControllerUI.singleton.cellMenu)
        {
            return;
        }

        int cellSelectionDelta = InputsManager.singleton.leftPadPressVerticalValue;
        m_selectedCellIndex = (m_selectedCellIndex + cellSelectionDelta) % CellsManager.registeredCellsCount;
        if(m_selectedCellIndex < 0)
        {
            m_selectedCellIndex = CellsManager.registeredCellsCount - 1;
        }
    }

    private void UpdateCellUpgradeSelection()
    {
        if(!ControllerUI.singleton.upgradeMenu)
        {
            return;
        }

        if((m_targetedCellPlaceholder != null) && (m_persistingTargetedCellPlaceholder != m_targetedCellPlaceholder))
        {
            m_selectedCellUpgradeIndex = 0;
            m_persistingTargetedCellPlaceholder = m_targetedCellPlaceholder;
        }

        if(!m_persistingTargetedCellPlaceholder || !m_persistingTargetedCellPlaceholder.cell.hasUpgrade)
        {
            return;
        }

        int cellSelectionDelta = InputsManager.singleton.leftPadPressVerticalValue;
        m_selectedCellUpgradeIndex = (m_selectedCellUpgradeIndex + cellSelectionDelta) % m_persistingTargetedCellPlaceholder.cell.upgrades.Length;
        if(m_selectedCellUpgradeIndex < 0)
        {
            m_selectedCellIndex = m_persistingTargetedCellPlaceholder.cell.upgrades.Length - 1;
        }
    }

    private void CreateTempPlaceholder()
    {
        GameObject tempPlaceholderGameObject = new GameObject("Temporary Cell Placeholder");
        m_tempPlaceholder = tempPlaceholderGameObject.AddComponent<CellPlaceholder>();
        m_tempPlaceholder.SetCell(CellsManager.registeredCells[m_selectedCellIndex]);
        tempPlaceholderGameObject.transform.SetParent(transform);
    }

    private void UpdatePlaceholders()
    {
        m_targetedCellPlaceholder = GetTargetedCellPlaceholder();
        bool targetingExistingCell = m_targetedCellPlaceholder != null;

        if (m_tempPlaceholder && m_tempPlaceholder.isActiveAndEnabled)
        {
            Cell tempCell = CellsManager.registeredCells[m_selectedCellIndex];
            m_tempPlaceholder.transform.position = PlayerCursor.singleton.targetPosition;
            m_tempPlaceholder.SetCell(tempCell);
            m_tempPlaceholder.cellRenderer.SetCellRenderingMode(tempCell.Affordable()? CellRenderingMode.Affordable : CellRenderingMode.NotAffordable);
            Vector3 flattenForward = GameManager.camera.transform.forward;
            flattenForward.y = 0;
            m_tempPlaceholder.transform.forward = flattenForward;
        }

        for (int i = 0; i < m_placeholders.Count; i++)
        {
            CellPlaceholder cellPlaceholder = m_placeholders[i];
            //bool displayAlt = targetingExistingCell ? targetedCellPlaceholder != cellPlaceholder : PlayerCursor.singleton.hasTarget;
            CellRenderingMode cellRenderingMode = CellRenderingMode.Default;
            if (targetingExistingCell)
            {
                if (m_targetedCellPlaceholder == cellPlaceholder)
                {
                    cellRenderingMode = CellRenderingMode.Selected;
                }
                else
                {
                    cellRenderingMode = CellRenderingMode.OtherSelected;
                }
            }
            else
            {
                if (PlayerCursor.singleton.hasTarget)
                {
                    cellRenderingMode = CellRenderingMode.OtherSelected;
                }
            }

            cellPlaceholder.cellRenderer.SetCellRenderingMode(cellRenderingMode);
        }
    }

    private void BuildCell()
    {
        if(!GameManager.singleton.PayResources(m_tempPlaceholder.cell.cost))
        {
            SoundManager.PlayErrorSoundEffect();
            return;
        }

        SoundManager.PlayCellSpawnSoundEffect();

        m_tempPlaceholder.gameObject.name = string.Format("Cell Placeholder n°{0}", m_placeholders.Count);
        m_placeholders.Add(m_tempPlaceholder);
        m_tempPlaceholder = null;
    }

    private void ModifyPlaceholder()
    {
        if (!m_persistingTargetedCellPlaceholder || !m_persistingTargetedCellPlaceholder.cell.hasUpgrade)
        {
            return;
        }

        Cell cellUpgrade = m_persistingTargetedCellPlaceholder.cell.upgrades[m_selectedCellUpgradeIndex];
        if(!GameManager.singleton.PayResources(cellUpgrade.cost))
        {
            SoundManager.PlayErrorSoundEffect();
            return;
        }

        SoundManager.PlayCellSpawnSoundEffect();
        m_persistingTargetedCellPlaceholder.SetCell(cellUpgrade);
    }

    public CellPlaceholder GetTargetedCellPlaceholder()
    {
        Vector3 targetPosition = PlayerCursor.singleton.targetPosition;
        targetPosition.y = 0;
        float tempRadius = m_tempPlaceholder == null ? 0 : m_tempPlaceholder.cellRenderer.radius;

        for (int i = 0; i < m_placeholders.Count; i++)
        {
            Vector3 cellRendererPosition = m_placeholders[i].cellRenderer.transform.position;
            cellRendererPosition.y = 0;

            if ((targetPosition - cellRendererPosition).magnitude < (m_placeholders[i].cellRenderer.radius + tempRadius))
            {
                return m_placeholders[i];
            }
        }
        return null;
    }

    public void ReclaimCellRenderers()
    {
        foreach(CellPlaceholder placeholder in m_placeholders)
        {
            placeholder.ReclaimCellRenderer();
        }
    }

    public bool cellBoundsContainsPoint(Bounds cellBounds, Vector3 point)
    {
        Vector3 projectedBoundsCenter = cellBounds.center;
        projectedBoundsCenter.y = 0;
        cellBounds.center = projectedBoundsCenter;
        point.y = 0;
        return cellBounds.Contains(point);
    }

    public List<CellPlaceholder> placeholders
    {
        get { return m_placeholders; }
    }

    static public CellPlaceholderManager singleton
    {
        get { return s_singleton; }
    }

    #region Selections
    #region Cell selection
    static public Cell selectedCell
    {
        get { return CellsManager.registeredCells[s_singleton.m_selectedCellIndex]; }
    }

    static public Cell nextSelectedCell
    {
        get
        {
            Cell[] registredCells = CellsManager.registeredCells;
            int tmp = (s_singleton.m_selectedCellIndex + 1) % registredCells.Length;
            return registredCells[tmp];
        }
    }

    static public Cell previousSelectedCell
    {
        get
        {
            Cell[] registredCells = CellsManager.registeredCells;
            int tmp = s_singleton.m_selectedCellIndex - 1;
            if (tmp < 0) tmp = registredCells.Length - 1;
            return registredCells[tmp];
        }
    }
    #endregion
    #region Upgrade selection
    static public bool selectedCellHasUpgrade
    {
        get { return selectedCell.hasUpgrade; }
    }

    static public Cell selectedCellUpgrade
    {
        get
        {
            if (!selectedCellHasUpgrade) return null;
            return selectedCell.upgrades[s_singleton.m_selectedCellUpgradeIndex];
        }
    }

    static public Cell nextSelectedCellUpgrade
    {
        get
        {
            if (!selectedCellHasUpgrade) return null;
            Cell[] cellUpgrades = selectedCell.upgrades;
            int tmp = (s_singleton.m_selectedCellUpgradeIndex + 1) % cellUpgrades.Length;
            return cellUpgrades[tmp];
        }
    }

    static public Cell previousSelectedCellUpgrade
    {
        get
        {
            if (!selectedCellHasUpgrade) return null;
            Cell[] cellUpgrades = selectedCell.upgrades;
            int tmp = s_singleton.m_selectedCellUpgradeIndex - 1;
            if (tmp < 0) tmp = cellUpgrades.Length - 1;
            return cellUpgrades[tmp];
        }
    }
    #endregion
    #endregion
}
