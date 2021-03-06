﻿using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Light
    {
        [NodeMethod("Light", "Turn On|Off", NodeMethodType.Action)]
        static public void TurnOnOff(Light light, bool on)
        {
            light.enabled = on;
        }

        [NodeMethod("Light", "Set light intensity", NodeMethodType.Action)]
        static public void SetLightIntensity(Light light, float intensity)
        {
            light.intensity = intensity;
        }
    }
}
