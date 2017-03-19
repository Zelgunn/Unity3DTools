using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdaterUI : MonoBehaviour
{
    private struct UpdaterUITransform
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 subScale;
    }

    static private UpdaterUI s_singleton;

    [Header("Invasion UI")]
    [SerializeField] private Transform m_invasionParent;
    [SerializeField] private Text m_bataille_enemy_nbr;
    [SerializeField] private Image m_bataille_enemy_icon;

    [Header("Cell Upgrade UI")]
    [SerializeField] private Transform m_cellUpgradeParent;
    [SerializeField] private Image m_upgrade_current;
    [SerializeField] private Image m_upgrade_next;
    [SerializeField] private Image m_upgrade_previous;
    [SerializeField] private Text m_upgrade_cost;
    [SerializeField] private Text m_upgrade_name;

    [Header("Cell Building UI")]
    [SerializeField] private Transform m_cellBuildingParent;
    [SerializeField] private Image m_cells_current;
    [SerializeField] private Image m_cells_next;
    [SerializeField] private Image m_cells_previous;
    [SerializeField] private Text m_cells_cost;
    [SerializeField] private Text m_cells_name;

    [Header("Resources UI")]
    [SerializeField] private Transform m_resourcesParent;
    [SerializeField] private Text m_biomasse_bioflouz_nbr;

    [Header("Tech Tree UI")]
    [SerializeField] private TechTree m_techTree;
    [SerializeField] private Transform m_techTreeParent;
    [SerializeField] private Text m_selectedTechName;
    [SerializeField] private Text m_selectedTechCost;

    private Transform[] m_displays;
    private Transform[] m_displaysScalers;
    private UpdaterUITransform[] m_updaterUITransforms;
    private ControllerUI.ControllerUIMenus m_lastMenu = ControllerUI.ControllerUIMenus.CelluleMenu;

    private void Awake()
    {
        s_singleton = this;

        m_displays = new Transform[5];
        m_displays[0] = m_cellBuildingParent;
        m_displays[1] = m_cellUpgradeParent;
        m_displays[2] = m_invasionParent;
        m_displays[3] = m_resourcesParent;
        m_displays[4] = m_techTreeParent;

        m_displaysScalers = new Transform[m_displays.Length];
        m_updaterUITransforms = new UpdaterUITransform[m_displays.Length];
        for (int i = 0; i < m_displays.Length; i++)
        {
            m_displaysScalers[i] = m_displays[i].GetChild(0);

            m_updaterUITransforms[i] = new UpdaterUITransform
            {
                localPosition = m_displays[i].localPosition,
                localRotation = m_displays[i].localRotation,
                subScale = m_displaysScalers[i].localScale
            };
        }
    }

    private void Update()
    {
        //UpdateTransform();
        UpdateMenu();

        UpdateInvasionInfos();
        UpdateUpgradeInfos();
        UpdateBuildingInfos();
        UpdateResourcesInfos();
        UpdateTechTreeInfos();
    }

    private void UpdateTransform()
    {
        Vector3 tmp = GameManager.camera.transform.forward;
        tmp.y = 0;
        if (Vector3.Angle(tmp, transform.forward) > 5)
            transform.LookAt(transform.position + tmp);
    }

    private void UpdateMenu()
    {
        if(m_lastMenu == ControllerUI.singleton.menu)
        {
            return;
        }

        m_lastMenu = ControllerUI.singleton.menu;
        for (int i = 0; i < m_displays.Length; i++)
        {
            int tmp = (i - (int)m_lastMenu) % (int)ControllerUI.ControllerUIMenus.MenusCount;
            if (tmp < 0) tmp += (int)ControllerUI.ControllerUIMenus.MenusCount;

            m_displays[i].localPosition = m_updaterUITransforms[tmp].localPosition;
            m_displays[i].localRotation = m_updaterUITransforms[tmp].localRotation;
            m_displaysScalers[i].localScale = m_updaterUITransforms[tmp].subScale;
        }
    }

    private void UpdateInvasionInfos()
    {
        m_bataille_enemy_nbr.text = BacteriasManager.singleton.nextInvasionBacteriasCount.ToString();
        m_bataille_enemy_icon.sprite = BacteriasManager.singleton.nextInvasionConfig.bacteria.icon;
    }

    private void UpdateUpgradeInfos()
    {
        Cell currentUpgrade = CellPlaceholderManager.selectedCellUpgrade;
        Cell nextUpgrade = CellPlaceholderManager.nextSelectedCellUpgrade;
        Cell previousUpgrade = CellPlaceholderManager.previousSelectedCellUpgrade;

        m_upgrade_current.sprite = currentUpgrade ? currentUpgrade.icon : null;
        m_upgrade_next.sprite = nextUpgrade ? nextUpgrade.icon : null;
        m_upgrade_previous.sprite = previousUpgrade ? previousUpgrade.icon : null;
        m_upgrade_cost.text = currentUpgrade ? ((int)currentUpgrade.cost.biomass).ToString() : "";
        m_upgrade_name.text = currentUpgrade ? currentUpgrade.name : "";
    }

    private void UpdateBuildingInfos()
    {
        m_cells_current.sprite = CellPlaceholderManager.selectedCell.icon;
        m_cells_next.sprite = CellPlaceholderManager.nextSelectedCell.icon;
        m_cells_previous.sprite = CellPlaceholderManager.previousSelectedCell.icon;
        m_cells_cost.text = ((int)CellPlaceholderManager.selectedCell.cost.biomass).ToString();
        m_cells_name.text = CellPlaceholderManager.selectedCell.name;
    }

    private void UpdateResourcesInfos()
    {
        m_biomasse_bioflouz_nbr.text = ((int)GameManager.playerResources.biomass).ToString();
    }

    private void UpdateTechTreeInfos()
    {
        m_selectedTechName.text = m_techTree.selectedTechNode.techID.ToString();
        m_selectedTechCost.text = ((int)m_techTree.selectedTechNode.researchCost.biomass).ToString();
    }

    static public void Show(bool show)
    {
        s_singleton.gameObject.SetActive(show);
    }
}
