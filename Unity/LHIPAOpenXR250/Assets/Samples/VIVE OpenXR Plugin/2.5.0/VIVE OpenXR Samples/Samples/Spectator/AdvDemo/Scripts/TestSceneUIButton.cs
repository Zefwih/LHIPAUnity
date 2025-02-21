// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using VIVE.OpenXR.Toolkits.Spectator;
using VIVE.OpenXR.Toolkits.Spectator.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: TestSceneUIButton.cs
    /// Role: UI script
    /// Responsibility: Define the button click event in the "Spectator_Adv_Demo_Second_Scene_Test" scene
    /// </summary>
    public class TestSceneUIButton : MonoBehaviour
    {
        public void OnClickChangeCameraSourceButton()
        {
            var spectatorCameraManager = SpectatorCameraManager.Instance;
            if (spectatorCameraManager is null)
            {
                return;
            }

            if (spectatorCameraManager.CameraSourceRef is SpectatorCameraHelper.CameraSourceRef.Tracker)
            {
                spectatorCameraManager.CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
            }
            else if (spectatorCameraManager.CameraSourceRef is SpectatorCameraHelper.CameraSourceRef.Hmd)
            {
                if (spectatorCameraManager.SpectatorCameraTrackerList.Count <= 0)
                {
                    return;
                }

                spectatorCameraManager.FollowSpectatorCameraTracker =
                    spectatorCameraManager.SpectatorCameraTrackerList[0];
                spectatorCameraManager.CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Tracker;
            }
        }
    }
}