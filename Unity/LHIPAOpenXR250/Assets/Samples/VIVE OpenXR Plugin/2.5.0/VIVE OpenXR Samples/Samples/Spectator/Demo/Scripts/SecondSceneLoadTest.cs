// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

namespace VIVE.OpenXR.Samples.Spectator.Demo
{
    public class SecondSceneLoadTest : MonoBehaviour
    {
        private InputDevice Left { get; set; }
        [field: SerializeField] private string LoadSecondSceneName { get; set; }

        private void Start()
        {
            var inputDeviceList = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, inputDeviceList);
            if (inputDeviceList.Count > 0)
            {
                Debug.Log("The input device list > 0. Get the first one as default.");
                Left = inputDeviceList[0];
            }
            else
            {
                Debug.Log("The input device list <= 0");
            }
        }

        private void Update()
        {
            if (!Left.isValid)
            {
                return;
            }

            Left.TryGetFeatureValue(CommonUsages.triggerButton, out bool isClick);
            if (!isClick)
            {
                return;
            }

            if (DoesSceneExist(LoadSecondSceneName))
            {
                SceneManager.LoadScene(LoadSecondSceneName);
            }
            else
            {
                Debug.LogWarning($"{LoadSecondSceneName} scene not found. Please add it in build setting first.");
            }
        }

        /// <summary>
        /// Returns true if the scene 'name' exists and is in your Build settings, false otherwise.
        /// </summary>
        private static bool DoesSceneExist(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var lastSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
                var sceneName = scenePath.Substring(lastSlash + 1,
                    scenePath.LastIndexOf(".", StringComparison.Ordinal) - lastSlash - 1);

                if (string.Compare(name, sceneName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}