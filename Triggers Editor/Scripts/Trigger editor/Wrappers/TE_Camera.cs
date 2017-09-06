using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Camera
    {
        [NodeMethod("Camera", "Set Field of view")]
        static public void SetFOV(Camera camera, float fov)
        {
            camera.fieldOfView = fov;
        }

        [NodeMethod("Camera", "Set clipping (near and far)")]
        static public void SetBothClipping(Camera camera, float nearClipPlane, float farClipPlane)
        {
            camera.nearClipPlane = nearClipPlane;
            camera.farClipPlane = farClipPlane;
        }

        [NodeMethod("Camera", "Set clipping (near)")]
        static public void SetNearClipping(Camera camera, float nearClipPlane)
        {
            camera.nearClipPlane = nearClipPlane;
        }

        [NodeMethod("Camera", "Set clipping (far)")]
        static public void SetFarClipping(Camera camera, float farClipPlane)
        {
            camera.farClipPlane = farClipPlane;
        }

        [NodeMethod("Camera", "Set Orthographic mode")]
        static public void SwitchOrthoPerspec(Camera camera, bool orthographic)
        {
            camera.orthographic = orthographic;
        }

        [NodeMethod("Camera", "Set Orthographic size")]
        static public void SetOrthographicSize(Camera camera, float size)
        {
            camera.orthographicSize = size;
        }
    }
}
