// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VIVE.OpenXR.Samples.Spectator.AdvDemo.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: UIHintStaticResource.cs
    /// Role: Singleton class without MonoBehaviour
    /// Responsibility: The static resource for UIHint.cs usage.
    /// 
    /// This design pattern can reduce the memory usage because
    /// all UIHint class will share the same resource.
    /// </summary>
    public class UIHintStaticResource
    {
        // Singleton
        private static UIHintStaticResource _instance;

        public static UIHintStaticResource Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UIHintStaticResource();
                }

                return _instance;
            }
        }

        private const string PageNameNotFoundForHintTextObjectWarningMessage =
            "Can not find the corresponding page name for hint text object.";

        // The key word in gameObject (which showing the UI hint) name
        private const string UIHintGameObjectKeyWords = "Description Text";

        #region Page name keywords

        private const string MainPageKeyWords = "Main Page";
        private const string InteractionSettingPageKeyWords = "Interaction Setting Page";
        private const string SpectatorCameraSettingPageKeyWords = "Spectator Camera Setting Page";

        #endregion

        #region UI component keywords and corresponding hint

        #region Main Page

        private const string BackButtonKeyWords = "Back Button";
        private const string BackButtonHint = " ";

        private const string SpectatorCameraSettingButtonKeyWords = "Spectator Camera Setting Button";

        private const string SpectatorCameraSettingButtonHint =
            "Click the button to open the spectator camera setting page.";

        private const string InteractionSettingButtonKeyWords = "Interaction Setting Button";
        private const string InteractionSettingButtonHint = "Click the button to open the interaction setting page.";

        private const string LicenseButtonKeyWords = "License Button";
        private const string LicenseButtonHint = "Click the button to open the license page.";

        private const string ExitButtonKeyWords = "Exit Button";
        private const string ExitButtonHint = "Click the button to exit the application.";

        #endregion

        #region Spectator camera setting page

        private const string CaptureButtonKeyWords = "360 Capture Button";
        private const string CaptureButtonHint = "Click the button to capture 3D-360 image.";

        private const string HmdSourceButtonKeyWords = "HMD Source Button";

        private const string HmdSourceButtonHint = "Choose the \"HMD\" source for spectator camera. " +
                                                   "The spectator camera position and rotation will follow the " +
                                                   "HMD.";

        private const string TrackerSourceButtonKeyWords = "Tracker Source Button";

        private const string TrackerSourceButtonHint = "Choose the \"Tracker\" source for spectator camera. " +
                                                       "The spectator camera position and rotation will be " +
                                                       "follow the tracker.";

        private const string TrackerDropdownKeyWords = "Tracker Dropdown";

        private const string TrackerDropdownHint = "Choose the tracker for spectator camera. " +
                                                   "Which tracker will be used for the spectator camera follow?";

        private const string LayerMaskDropdownKeyWords = "Layer Mask Dropdown";

        private const string LayerMaskDropdownHint = "Set the layer mask for spectator camera. " +
                                                     "The spectator camera will only render the object which is " +
                                                     "in the layer mask.\nP.S. Green = On, Red = Off.";

        private const string FOVSliderKeyWords = "FOV Slider";
        private const string FOVSliderHint = "Set the vertical FOV for spectator camera.";

        private const string SaveSettingButtonKeyWords = "Save Setting Button";
        private const string SaveSettingButtonHint = "Save the current spectator camera setting.";

        private const string ReloadSettingButtonKeyWords = "Reload Setting Button";
        private const string ReloadSettingButtonHint = "Reload the last time saved spectator camera setting.";

        private const string ResetToFactorySettingButtonKeyWords = "Reset To Factory Setting Button";
        private const string ResetToFactorySettingButtonHint = "Reset the spectator camera setting to factory setting.";

        private const string ResetToUserBuildSettingButtonKeyWords = "Reset To User Build Setting Button";

        private const string ResetToUserBuildSettingButtonHint =
            "Reset the spectator camera setting to user setup on the Unity Editor.";

        #endregion

        #region Interaction setting page

        private const string ThumbstickEffectPositionButtonKeyWords = "Position Button";

        private const string ThumbstickEffectPositionButtonHint =
            "Choose \"Position\" as the affected variable if move the thumbstick.";

        private const string ThumbstickEffectRotationButtonKeyWords = "Rotation Button";

        private const string ThumbstickEffectRotationButtonHint =
            "Choose \"Rotation\" as the affected variable if move the thumbstick.";

        private const string ThumbstickEffectFOVButtonKeyWords = "FOV Button";

        private const string ThumbstickEffectFOVButtonHint =
            "Choose \"FOV\" as the affected variable if move the thumbstick.";

        private const string ThumbstickEffectAffectYAxisToggleKeyWords = "Affect Y Axis Toggle";

        private const string ThumbstickEffectAffectYAxisToggleHint =
            "Affect the Y-axis under the \"Position\" or \"Rotation\" mode of the \"Thumbstick Effect Option\" or not.";

        private const string MovementSpeedSliderKeyWords = "Movement Speed Slider";

        private const string MovementSpeedSliderHint =
            "Set the movement speed of the object when rotating the thumbstick.";

        private const string RotationSpeedSliderKeyWords = "Rotation Speed Slider";

        private const string RotationSpeedSliderHint =
            "Set the rotation speed of the object when rotating the thumbstick.";

        private const string ZoomSpeedSliderKeyWords = "Zoom Speed Slider";
        private const string ZoomSpeedSliderHint = "Set the zoom speed of the object when rotating the thumbstick.";

        #endregion

        #endregion

        private readonly List<string> _pageKeyWords = new List<string>
        {
            MainPageKeyWords,
            InteractionSettingPageKeyWords,
            SpectatorCameraSettingPageKeyWords,
        };

        public readonly Dictionary<string, string> UiComponentKeywordAndHintPair = new Dictionary<string, string>
        {
            { SpectatorCameraSettingButtonKeyWords, SpectatorCameraSettingButtonHint },
            { InteractionSettingButtonKeyWords, InteractionSettingButtonHint },
            { LicenseButtonKeyWords, LicenseButtonHint },
            { ExitButtonKeyWords, ExitButtonHint },

            { BackButtonKeyWords, BackButtonHint },

            { CaptureButtonKeyWords, CaptureButtonHint },
            { HmdSourceButtonKeyWords, HmdSourceButtonHint },
            { TrackerSourceButtonKeyWords, TrackerSourceButtonHint },
            { TrackerDropdownKeyWords, TrackerDropdownHint },
            { LayerMaskDropdownKeyWords, LayerMaskDropdownHint },
            { FOVSliderKeyWords, FOVSliderHint },
            { SaveSettingButtonKeyWords, SaveSettingButtonHint },
            { ReloadSettingButtonKeyWords, ReloadSettingButtonHint },
            { ResetToFactorySettingButtonKeyWords, ResetToFactorySettingButtonHint },
            { ResetToUserBuildSettingButtonKeyWords, ResetToUserBuildSettingButtonHint },

            { ThumbstickEffectPositionButtonKeyWords, ThumbstickEffectPositionButtonHint },
            { ThumbstickEffectRotationButtonKeyWords, ThumbstickEffectRotationButtonHint },
            { ThumbstickEffectFOVButtonKeyWords, ThumbstickEffectFOVButtonHint },
            { ThumbstickEffectAffectYAxisToggleKeyWords, ThumbstickEffectAffectYAxisToggleHint },
            { MovementSpeedSliderKeyWords, MovementSpeedSliderHint },
            { RotationSpeedSliderKeyWords, RotationSpeedSliderHint },
            { ZoomSpeedSliderKeyWords, ZoomSpeedSliderHint },
        };

        private static GameObjectProcessHelper.GameObjectWithComponentKeyWords<TextMeshProUGUI>
            _gameObjectWithComponentKeyWords;

        public Dictionary<string, GameObject> PageNameAndHintTextObjectPair { get; }
        public Dictionary<string, GameObject> PageNameAndPageRootObjectPair { get; }

        private UIHintStaticResource()
        {
            _gameObjectWithComponentKeyWords =
                GameObjectProcessHelper.GameObjectWithComponentKeyWords<TextMeshProUGUI>.Instance;
            _gameObjectWithComponentKeyWords.GameObjectKeyWords = UIHintGameObjectKeyWords;

            PageNameAndHintTextObjectPair = new Dictionary<string, GameObject>();
            PageNameAndPageRootObjectPair = new Dictionary<string, GameObject>();

            FindCorrespondingPageNameAndHintTextObjectPair();
            ResetAllHintTextObject();
        }

        /// <summary>
        /// Find the corresponding page name and hint text object and its own root pair.
        /// </summary>
        private void FindCorrespondingPageNameAndHintTextObjectPair()
        {
            Debug.Log(
                $"Find {_gameObjectWithComponentKeyWords.GameObjectList.Count} " +
                "GameObject with \"TextMeshProUGUI\" component and its name contain " +
                $"{UIHintGameObjectKeyWords} key word in the scene");

            foreach (var item in _gameObjectWithComponentKeyWords.GameObjectList)
            {
                GameObject upper = item;
                bool isMatch = false;
                while (!isMatch)
                {
                    foreach (var pageKeyWord in _pageKeyWords)
                    {
                        if (GameObjectProcessHelper.CheckGameObjectNameIsContainTargetString(upper.transform,
                                pageKeyWord))
                        {
                            Debug.Log("Init PageNameAndHintTextObjectPair and PageNameAndPageRootObjectPair");
                            PageNameAndHintTextObjectPair.Add(pageKeyWord, item);
                            PageNameAndPageRootObjectPair.Add(pageKeyWord, upper);
                            isMatch = true;
                            break;
                        }
                    }

                    if (!isMatch)
                    {
                        if (upper.transform.parent != null)
                        {
                            upper = upper.transform.parent.gameObject;
                        }
                        else
                        {
                            Debug.LogWarning(PageNameNotFoundForHintTextObjectWarningMessage);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset all hint text object to empty string
        /// </summary>
        private void ResetAllHintTextObject()
        {
            foreach (var item in PageNameAndHintTextObjectPair)
            {
                item.Value.GetComponent<TextMeshProUGUI>().text = string.Empty;
            }
        }
    }
}