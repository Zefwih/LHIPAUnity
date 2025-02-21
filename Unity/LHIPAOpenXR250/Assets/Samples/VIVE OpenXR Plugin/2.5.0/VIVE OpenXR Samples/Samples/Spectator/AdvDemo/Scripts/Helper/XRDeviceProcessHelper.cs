// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo.Helper
{
    public static class XRDeviceProcessHelper
    {
        private static readonly List<InputDevice> MyInputDevices = new List<InputDevice>();

        private static List<InputDevice> TryInitializeController(InputDeviceCharacteristics characteristics)
        {
            MyInputDevices.Clear();

            InputDevices.GetDevicesWithCharacteristics(characteristics, MyInputDevices);

            return MyInputDevices.Count == 0 ? null : MyInputDevices;
        }

        public static List<UnityEngine.XR.InputDevice> GetLeftControllers()
        {
            var getInputDevices = TryInitializeController(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller);
            if (getInputDevices == null || getInputDevices.Count == 0)
            {
                return null;
            }

            Debug.Log($"Found device number with role (Left | Controller): {getInputDevices.Count}");
            foreach (var item in getInputDevices)
            {
                Debug.Log($"Device name is {item.name}");
                Debug.Log($"Device characteristics is {item.characteristics}");
            }

            return getInputDevices;
        }

        public static List<UnityEngine.XR.InputDevice> GetRightControllers()
        {
            var getInputDevices = TryInitializeController(
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller);
            if (getInputDevices == null || getInputDevices.Count == 0)
            {
                return null;
            }

            Debug.Log($"Found device number with role (Right | Controller): {getInputDevices.Count}");
            foreach (var item in getInputDevices)
            {
                Debug.Log($"Device name is {item.name}");
                Debug.Log($"Device characteristics is {item.characteristics}");
            }

            return getInputDevices;
        }

        public static bool IsEqualDeviceCharacteristics(
            InputDeviceCharacteristics deviceNeedToCheck,
            InputDeviceCharacteristics characteristics)
        {
            return (deviceNeedToCheck & characteristics) == characteristics;
        }
    }
}