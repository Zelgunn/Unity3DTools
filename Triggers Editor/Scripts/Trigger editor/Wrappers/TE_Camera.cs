using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Camera
    {
        [NodeMethod("Camera", "Set Field of view", NodeMethodType.Action)]
        static public void SetFOV(Camera camera, float fov)
        {
            camera.fieldOfView = fov;
        }

        [NodeMethod("Camera", "Set clipping (near and far)", NodeMethodType.Action)]
        static public void SetBothClipping(Camera camera, float nearClipPlane, float farClipPlane)
        {
            camera.nearClipPlane = nearClipPlane;
            camera.farClipPlane = farClipPlane;
        }

        [NodeMethod("Camera", "Set clipping (near)", NodeMethodType.Action)]
        static public void SetNearClipping(Camera camera, float nearClipPlane)
        {
            camera.nearClipPlane = nearClipPlane;
        }

        [NodeMethod("Camera", "Set clipping (far)", NodeMethodType.Action)]
        static public void SetFarClipping(Camera camera, float farClipPlane)
        {
            camera.farClipPlane = farClipPlane;
        }

        [NodeMethod("Camera", "Set Orthographic mode", NodeMethodType.Action)]
        static public void SwitchOrthoPerspec(Camera camera, bool orthographic)
        {
            camera.orthographic = orthographic;
        }

        [NodeMethod("Camera", "Set Orthographic size", NodeMethodType.Action)]
        static public void SetOrthographicSize(Camera camera, float size)
        {
            camera.orthographicSize = size;
        }
    }
}
