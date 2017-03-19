using UnityEngine;
using System.Collections;

public class ControllerUI : MonoBehaviour
{
    public enum ControllerUIMenus
    {
        CelluleMenu = 0,
        UpgradeMenu = 1,
        ReadyMenu = 2,
        ResourcesMenu = 3,
        TechTreeMenu = 4,
        MenusCount = 5
    }

    static private ControllerUI s_singleton;
    private ControllerUIMenus m_menu = ControllerUIMenus.CelluleMenu;

	private void Awake ()
    {
        s_singleton = this;
	}
	
	private void Update ()
    {
        int leftSwipe = InputsManager.singleton.leftPadPressHorizontalValue;
        if(leftSwipe != 0)
        {
            ChangeMenu(leftSwipe);
        }
	}

    private void ChangeMenu(int delta)
    {
        m_menu = m_menu + delta;

        if(m_menu == ControllerUIMenus.MenusCount)
        {
            m_menu = 0;
        }

        if(m_menu < 0)
        {
            m_menu = ControllerUIMenus.MenusCount - 1;
        }


        //transform.Rotate(Vector3.up, delta * 360 / (int)ControllerUIMenus.MenusCount, Space.Self);
    }
    public ControllerUIMenus menu
    {
        get { return m_menu; }
        set
        {
            int tmp = (value - m_menu);
            ChangeMenu(tmp);
        }
    }

    public bool readyMenu
    {
        get { return m_menu == ControllerUIMenus.ReadyMenu; }
    }

    public bool techTreeMenu
    {
        get { return m_menu == ControllerUIMenus.TechTreeMenu; }
    }

    public bool cellMenu
    {
        get { return m_menu == ControllerUIMenus.CelluleMenu; }
    }

    public bool resourcesMenu
    {
        get { return m_menu == ControllerUIMenus.ResourcesMenu; }
    }

    public bool upgradeMenu
    {
        get { return m_menu == ControllerUIMenus.UpgradeMenu; }
    }

    static public ControllerUI singleton
    {
        get { return s_singleton; }
    }
}
