using UnityEngine;
using System.Collections;

public struct InputState
{
    public bool triggered;
    public bool padPressed;
    public bool padTouched;
    public Vector2 padAxis;

    public void Update(SteamVR_TrackedController controller)
    {
        triggered = controller.triggerPressed;
        padPressed = controller.padPressed;
        padTouched = controller.padTouched;
        padAxis.x = controller.controllerState.rAxis0.x;
        padAxis.y = controller.controllerState.rAxis0.y;
    }
}

public class InputsManager : MonoBehaviour
{
    static private InputsManager s_singleton;
    [SerializeField] private SteamVR_TrackedController m_leftController;
    [SerializeField] private SteamVR_TrackedController m_rightController;
    private InputState m_leftLastState = new InputState { triggered = false, padPressed = false, padTouched = false, padAxis = Vector2.zero };
    private InputState m_rightLastState = new InputState { triggered = false, padPressed = false, padTouched = false, padAxis = Vector2.zero };

    private bool m_rightTriggerDown;
    private bool m_leftTriggerDown;

    private bool m_rightPadPressed;
    private bool m_leftPadPressed;

    private void Awake ()
    {
        s_singleton = this;
	}
	
	private void Update ()
    {
        m_rightTriggerDown = m_rightController.triggerPressed && !m_rightLastState.triggered;
        m_leftTriggerDown = m_leftController.triggerPressed && !m_leftLastState.triggered;

        m_rightPadPressed = m_rightController.padPressed && !m_rightLastState.padPressed;
        m_leftPadPressed = m_leftController.padPressed && !m_leftLastState.padPressed;

        m_leftLastState.Update(m_leftController);
        m_rightLastState.Update(m_rightController);
    }

    #region Pad swiped

    /*
    public int leftPadSwipedHorizontal
    {
        get
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                return -1;
            }
            if(Input.GetKeyDown(KeyCode.M))
            {
                return 1;
            }

            if(!leftPadTouchUp)
            {
                return 0;
            }

            float delta = m_leftLastState.padAxis.x - m_leftPadAnchor.x;
            if(delta < -0.5f)
            {
                return -1;
            }
            else if(delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public int rightPadSwipedHorizontal
    {
        get
        {
            if (!rightPadTouchUp)
            {
                return 0;
            }

            float delta = m_rightLastState.padAxis.x - m_rightPadAnchor.x;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public int leftPadSwipedVertical
    {
        get
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                return -1;
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                return 1;
            }

            if (!leftPadTouchUp)
            {
                return 0;
            }

            float delta = m_leftLastState.padAxis.x - m_leftPadAnchor.x;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public int rightPadSwipedVertical
    {
        get
        {
            if (!rightPadTouchUp)
            {
                return 0;
            }

            float delta = m_rightLastState.padAxis.y - m_rightPadAnchor.y;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
    */

    #endregion

    #region Pad touch
    public bool leftPadTouched
    {
        get { return m_leftController.padTouched && !m_leftLastState.padTouched; }
    }

    public bool rightPadTouched
    {
        get { return m_rightController.padTouched && !m_rightLastState.padTouched; }
    }

    //public bool padTouched
    //{
    //    get { return leftPadTouched || rightPadTouched; }
    //}

    public bool leftPadTouchUp
    {
        get
        {
            return !m_leftController.padTouched && m_leftLastState.padTouched;
        }
    }

    public bool rightPadTouchUp
    {
        get
        {
            return !m_rightController.padTouched && m_rightLastState.padTouched;
        }
    }
    #endregion

    #region Pad press
    public bool leftPadPressed
    {
        get { return m_leftPadPressed; }
    }

    public bool rightPadPressed
    {
        get { return m_rightPadPressed; }
    }

    //public bool padPressed
    //{
    //    get { return leftPadPressed || rightPadPressed; }
    //}

    public int leftPadPressHorizontalValue
    {
        get
        {
            if (!leftPadPressed)
            {
                return 0;
            }
            float delta = m_leftLastState.padAxis.x;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public int leftPadPressVerticalValue
    {
        get
        {
            if (!leftPadPressed)
            {
                return 0;
            }

            float delta = m_leftLastState.padAxis.y;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public bool leftPressCenter
    {
        get
        {
            return (leftPadPressHorizontalValue == 0) && (leftPadPressVerticalValue == 0) && leftPadPressed;
        }
    }

    public int rightPadPressHorizontalValue
    {
        get
        {
            if (!rightPadPressed)
            {
                return 0;
            }
            float delta = m_rightLastState.padAxis.x;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public int rightPadPressVerticalValue
    {
        get
        {
            if (!rightPadPressed)
            {
                return 0;
            }

            float delta = m_rightLastState.padAxis.y;
            if (delta < -0.5f)
            {
                return -1;
            }
            else if (delta > 0.5f)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public bool rightPressCenter
    {
        get
        {
            return (rightPadPressHorizontalValue == 0) && (rightPadPressVerticalValue == 0) && rightPadPressed;
        }
    }

    #endregion

    #region Trigger
    public bool leftTriggerDown
    {
        get { return m_leftTriggerDown; }
    }

    public bool rightTriggerDown
    {
        get { return m_rightTriggerDown; }
    }

    //public bool triggerDown
    //{
    //    get { return leftTriggerDown || rightTriggerDown; }
    //}

    public bool leftTriggered
    {
        get { return m_leftController.triggerPressed; }
    }

    public bool rightTriggered
    {
        get { return m_rightController.triggerPressed; }
    }

    //public bool triggered
    //{
    //    get { return leftTriggered || rightTriggered; }
    //}
    #endregion

    static public InputsManager singleton
    {
        get { return s_singleton; }
    }
}
