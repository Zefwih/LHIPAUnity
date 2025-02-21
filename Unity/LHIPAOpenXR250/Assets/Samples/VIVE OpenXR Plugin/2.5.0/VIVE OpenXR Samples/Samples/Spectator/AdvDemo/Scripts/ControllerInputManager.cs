// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR;
using VIVE.OpenXR.Samples.Spectator.AdvDemo.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: ControllerInputManager.cs
    /// Role: Manager (Singleton)
    /// Responsibility: Unify to manage the controller input
    /// </summary>
    public class ControllerInputManager : MonoBehaviour
    {
        // Singleton
        private static ControllerInputManager _instance;
        public static ControllerInputManager Instance => _instance;

        #region Public varibles of left/right controller (Unity XR Input System)

        // Left/right hand controller (the entry point that get all sensor values)
        public InputDevice LeftController { get; private set; }
        public InputDevice RightController { get; private set; }

        #endregion

        #region Unity Lifecycle functions

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void OnDestroy()
        {
            InputDevices.deviceConnected -= InputDevicesConnected;
            InputDevices.deviceDisconnected -= InputDevicesDisconnected;
        }

        private void Start()
        {
            InputDevices.deviceConnected += InputDevicesConnected;
            InputDevices.deviceDisconnected += InputDevicesDisconnected;
            InitLeftRightController();
        }

        #endregion

        #region Callback functions

        private void InputDevicesConnected(InputDevice device)
        {
            Debug.Log("InputDevicesConnected");
            Debug.Log($"Device name is {device.name}");
            Debug.Log($"Device characteristics is {device.characteristics}");

            if (XRDeviceProcessHelper.IsEqualDeviceCharacteristics(
                    device.characteristics,
                    InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller))
            {
                LeftController = device;
            }
            else if (XRDeviceProcessHelper.IsEqualDeviceCharacteristics(
                         device.characteristics,
                         InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller))
            {
                RightController = device;
            }
        }

        private void InputDevicesDisconnected(InputDevice device)
        {
            Debug.Log("InputDevicesDisconnected");
            Debug.Log($"Device name is {device.name}");
            Debug.Log($"Device characteristics is {device.characteristics}");
        }

        #endregion

        #region Public functions for checking the state of left/right controller

        public bool IsLeftControllerConnected()
        {
            return LeftController.isValid;
        }

        public bool IsRightControllerConnected()
        {
            return RightController.isValid;
        }

        #endregion

        #region Private functions for initializing left/right controller

        private void InitLeftController()
        {
            var inputDevices = XRDeviceProcessHelper.GetLeftControllers();
            if (inputDevices != null && inputDevices.Count > 0)
            {
                // Just consider the first controller we found
                LeftController = inputDevices[0];
            }
        }

        private void InitRightController()
        {
            var inputDevices = XRDeviceProcessHelper.GetRightControllers();
            if (inputDevices != null && inputDevices.Count > 0)
            {
                // Just consider the first controller we found
                RightController = inputDevices[0];
            }
        }

        private void InitLeftRightController()
        {
            InitLeftController();
            InitRightController();
        }

        #endregion
    }
}