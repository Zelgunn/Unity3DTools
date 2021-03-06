﻿using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Inputs
    {
        [NodeMethod("Inputs", "On keyboard down", NodeMethodType.Event, "Key down")]
        static public bool OnKeyCodeDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }

        [NodeMethod("Inputs", "On keyboard", NodeMethodType.Condition, "Key is pressed")]
        static public bool OnKeyCode(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        [NodeMethod("Inputs", "On keyboard up", NodeMethodType.Event, "Key up")]
        static public bool OnKeyCodeUp(KeyCode keyCode)
        {
            return Input.GetKeyUp(keyCode);
        }
    }
}
