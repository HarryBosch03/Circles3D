using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Circles3D.Runtime.Player
{
    public class Screenshotinator : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current.f12Key.wasPressedThisFrame)
            {
                var dir = "Screenshots";
                var path = $"{dir}/Screenshot{dir}{DateTime.Now:yyyy-mm-dd hh-mm-ss}.png";
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log($"Screenshot Saved at {Path.GetFullPath(path)}");
            }
        }
    }
}