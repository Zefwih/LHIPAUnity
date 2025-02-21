// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using VIVE.OpenXR.Toolkits.Spectator;
using VIVE.OpenXR.Toolkits.Spectator.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: UIManager.cs
    /// Role: Manager (Singleton)
    /// Responsibility: Manage all UI logic in the spectator demo scene
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public partial class UIManager : MonoBehaviour
    {
        // Singleton
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        #region Private const for page content

        #region Spectator Camera Setting Page Constants

        private const string NowCameraSourceRefTextPrefix = "Camera Source Ref: ";

        # region Spectator Camera Layer Mask Dropdown Constants

        private static readonly TMP_Dropdown.OptionData CameraLayerMaskDropdownLastOptionData =
            new TMP_Dropdown.OptionData("Empty");

        private const int CameraLayerMaskDropdownSpriteWidth = 10;
        private const int CameraLayerMaskDropdownSpriteHeight = 10;
        private const float CameraLayerMaskDropdownSpritePixelsPerUnit = 1;
        private static readonly Vector2 CameraLayerMaskDropdownSpritePivot = new Vector2(0.5f, 0.5f);

        private static readonly Color CameraLayerMaskDropdownOnColor = Color.green;
        private static readonly Color CameraLayerMaskDropdownOffColor = Color.magenta;

        #endregion

        #endregion

        #region Interaction Setting Page Constants

        private const string NowInteractionModeTextPrefix = "Interaction Mode: ";
        private const string MovementInteractionSpeedTextPrefix = "Movement Interaction Speed: ";
        private const string RotationInteractionSpeedTextPrefix = "Rotation Interaction Speed: ";
        private const string ZoomInteractionSpeedTextPrefix = "Zoom Interaction Speed: ";
        private const string SpectatorCameraVerticalFOVTextPrefix = "Camera FOV: ";
        private const string SpectatorCameraTrackerSourceTextPrefix = "Tracker Source: ";

        #endregion

        #endregion

        [field: SerializeField] private string LoadSecondSceneName { get; set; }

        #region Private variables for UI audio

        [field: SerializeField, Header("The main camera in XRRig")]
        private Camera XRHeadCamera { get; set; }

        [field: SerializeField, Header("The UI audio clip")]
        private AudioClip UIAudioClip { get; set; }

        #endregion

        #region Main page varibles (private)

        [field: SerializeField, Header("Main page item")]
        private Button SpectatorCameraSettingButton { get; set; }

        [field: SerializeField] private Button InteractionSettingButton { get; set; }
        [field: SerializeField] private Button LoadSecondSceneButton { get; set; }
        [field: SerializeField] private Button LicenseButton { get; set; }
        [field: SerializeField] private Button ExitButton { get; set; }
        [field: SerializeField] private GameObject MainPage { get; set; }
        [field: SerializeField] private GameObject SpectatorCameraSettingPage { get; set; }
        [field: SerializeField] private GameObject InteractionSettingPage { get; set; }
        [field: SerializeField] private GameObject LicensePage { get; set; }

        #endregion

        #region Interaction setting page varibles (private)

        private static ObjectInteractionManager ObjectInteractionManager => ObjectInteractionManager.Instance;

        [field: SerializeField, Header("Interaction setting page item")]
        private Button InteractionSettingPageBackButton { get; set; }

        [field: SerializeField] private TMP_Text ThumbstickEffectOptionText { get; set; }
        [field: SerializeField] private Button ThumbstickEffectPositionButton { get; set; }
        [field: SerializeField] private Button ThumbstickEffectRotationButton { get; set; }
        [field: SerializeField] private Button ThumbstickEffectFOVButton { get; set; }
        [field: SerializeField] private Toggle AffectYAxisToggle { get; set; }
        [field: SerializeField] private TMP_Text MovementInteractionSpeedText { get; set; }
        [field: SerializeField] private Slider MovementInteractionSpeedSlider { get; set; }
        [field: SerializeField] private TMP_Text RotationInteractionSpeedText { get; set; }
        [field: SerializeField] private Slider RotationInteractionSpeedSlider { get; set; }
        [field: SerializeField] private TMP_Text ZoomInteractionSpeedText { get; set; }
        [field: SerializeField] private Slider ZoomInteractionSpeedSlider { get; set; }

        #endregion

        #region Spectator camera setting page varibles (private)

        private SpectatorCameraManager SpectatorCameraManager => SpectatorCameraManager.Instance;

        [field: SerializeField, Header("Spectator camera setting page item")]
        private Button SpectatorCameraSettingPageBackButton { get; set; }

        #region Spectator camera 360 capture varible

        [field: SerializeField] private Button SpectatorCameraCapture360Button { get; set; }

        #endregion

        #region Spectator camera source ref varibles

        [field: SerializeField] private TMP_Text NowCameraSourceRefText { get; set; }
        [field: SerializeField] private Button HmdCameraSourceRefButton { get; set; }
        [field: SerializeField] private Button TrackerCameraSourceRefButton { get; set; }

        #endregion

        #region Spectator camera tracker source varibles

        [field: SerializeField] private TMP_Text NowTrackerSourceRefText { get; set; }
        [field: SerializeField] private TMP_Dropdown TrackerSourceDropdown { get; set; }

        #endregion

        #region Spectator camera layer mask dropdown varibles

        [field: SerializeField] private TMP_Dropdown SpectatorCameraLayerMaskDropdown { get; set; }
        private string[] AllLayerMaskNameArray { get; set; }
        private Sprite CameraLayerMaskOnSprite { get; set; }
        private Sprite CameraLayerMaskOffSprite { get; set; }

        #endregion

        #region Spectator camera vertical FOV varibles

        [field: SerializeField] private TMP_Text SpectatorCameraVerticalFOVText { get; set; }
        [field: SerializeField] private Slider SpectatorCameraVerticalFOVSlider { get; set; }

        #endregion

        #endregion

        #region License page varibles (private)

        [field: SerializeField, Header("License page item")]
        private Button LicensePageBackButton { get; set; }

        #endregion

        public UIHintStaticResource UIHintStaticResource { get; private set; }

        #region Unity lifecycle event functions

        private void Awake()
        {
            #region Singleton

            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            #endregion
        }

        private IEnumerator Start()
        {
            // Hard-coding delay 3 seconds
            yield return new WaitForSeconds(3);

            UIHintStaticResource = UIHintStaticResource.Instance;

            #region Page and some basic button init

            MainPage.SetActive(true);
            SpectatorCameraSettingPage.SetActive(false);
            InteractionSettingPage.SetActive(false);
            LicensePage.SetActive(false);

            // Register back button event
            SpectatorCameraSettingPageBackButton.onClick.AddListener(OnClickBackButton);
            InteractionSettingPageBackButton.onClick.AddListener(OnClickBackButton);
            LicensePageBackButton.onClick.AddListener(OnClickBackButton);

            #endregion

            #region Main page button OnClick register

            SpectatorCameraSettingButton.onClick.AddListener(OnClickSpectatorCameraSettingButton);
            InteractionSettingButton.onClick.AddListener(OnClickInteractionSettingButton);
            LoadSecondSceneButton.onClick.AddListener(OnClickLoadSecondSceneButton);
            LicenseButton.onClick.AddListener(OnClickLicenseButton);
            ExitButton.onClick.AddListener(OnClickExitButton);

            #endregion

            #region Interaction setting page setup

            ThumbstickEffectPositionButton.onClick.AddListener(OnClickThumbstickEffectPositionButton);
            ThumbstickEffectRotationButton.onClick.AddListener(OnClickThumbstickEffectRotationButton);
            ThumbstickEffectFOVButton.onClick.AddListener(OnClickThumbstickEffectFOVButton);
            ThumbstickEffectOptionText.text =
                $"{NowInteractionModeTextPrefix}{ObjectInteractionManager.ThumbstickEffectOption.ToString()}";

            AffectYAxisToggle.isOn = ObjectInteractionManager.IsAffectYAxisWhenThumbstickMove;
            AffectYAxisToggle.onValueChanged.AddListener(OnAffectYAxisToggleValueChanged);

            MovementInteractionSpeedSlider.minValue = ObjectInteractionManager.MOVEMENT_INTERACTION_SPEED_MIN;
            MovementInteractionSpeedSlider.maxValue = ObjectInteractionManager.MOVEMENT_INTERACTION_SPEED_MAX;
            MovementInteractionSpeedSlider.value = ObjectInteractionManager.MovementInteractionSpeed;
            MovementInteractionSpeedSlider.onValueChanged.AddListener(OnMovementInteractionSpeedSliderValueChanged);
            MovementInteractionSpeedText.text =
                $"{MovementInteractionSpeedTextPrefix}{ObjectInteractionManager.MovementInteractionSpeed}";

            RotationInteractionSpeedSlider.minValue = ObjectInteractionManager.ROTATION_INTERACTION_SPEED_MIN;
            RotationInteractionSpeedSlider.maxValue = ObjectInteractionManager.ROTATION_INTERACTION_SPEED_MAX;
            RotationInteractionSpeedSlider.value = ObjectInteractionManager.RotationInteractionSpeed;
            RotationInteractionSpeedSlider.onValueChanged.AddListener(OnRotationInteractionSpeedSliderValueChanged);
            RotationInteractionSpeedText.text =
                $"{RotationInteractionSpeedTextPrefix}{ObjectInteractionManager.RotationInteractionSpeed}";

            ZoomInteractionSpeedSlider.minValue = ObjectInteractionManager.ZOOM_INTERACTION_SPEED_MIN;
            ZoomInteractionSpeedSlider.maxValue = ObjectInteractionManager.ZOOM_INTERACTION_SPEED_MAX;
            ZoomInteractionSpeedSlider.value = ObjectInteractionManager.ZoomInteractionSpeed;
            ZoomInteractionSpeedSlider.onValueChanged.AddListener(OnZoomInteractionSpeedSliderValueChanged);
            ZoomInteractionSpeedText.text =
                $"{ZoomInteractionSpeedTextPrefix}{ObjectInteractionManager.ZoomInteractionSpeed}";

            #endregion

            #region Spectator camera setting page setup

            // SpectatorCameraCapture360Button register OnClick event
            SpectatorCameraCapture360Button.onClick.AddListener(OnClickSpectatorCameraCapture360Button);

            #region CameraSourceRefButton register OnClick event

            HmdCameraSourceRefButton.onClick.AddListener(OnClickHmdCameraSourceRefButton);
            TrackerCameraSourceRefButton.onClick.AddListener(OnClickTrackerCameraSourceRefButton);

            #endregion

            NowCameraSourceRefText.text =
                $"{NowCameraSourceRefTextPrefix}{SpectatorCameraManager.CameraSourceRef.ToString()}";

            var trackerNameList = SpectatorCameraManager.SpectatorCameraTrackerList != null &&
                                  SpectatorCameraManager.SpectatorCameraTrackerList.Count > 0
                ? SpectatorCameraManager.SpectatorCameraTrackerList.Select(item => item.name).ToList()
                : new List<string>();
            TrackerSourceDropdown.options.Clear();
            TrackerSourceDropdown.AddOptions(trackerNameList);
            TrackerSourceDropdown.value =
                SpectatorCameraManager.CameraSourceRef is SpectatorCameraHelper.CameraSourceRef.Hmd
                    ? -1
                    : SpectatorCameraManager.FollowSpectatorCameraTracker is null ||
                      SpectatorCameraManager.SpectatorCameraTrackerList is null ||
                      SpectatorCameraManager.SpectatorCameraTrackerList.Count == 0
                        ? -1
                        : TrackerSourceDropdown.options.IndexOf(
                            TrackerSourceDropdown.options.Find(
                                item =>
                                    item.text == SpectatorCameraManager.FollowSpectatorCameraTracker.name));
            TrackerSourceDropdown.onValueChanged.AddListener(OnSpectatorCameraTrackerSourceDropdownValueChanged);

            NowTrackerSourceRefText.text =
                SpectatorCameraManager.CameraSourceRef is SpectatorCameraHelper.CameraSourceRef.Hmd
                    ? "N/A"
                    : SpectatorCameraManager.FollowSpectatorCameraTracker is null ||
                      SpectatorCameraManager.SpectatorCameraTrackerList is null ||
                      SpectatorCameraManager.SpectatorCameraTrackerList.Count == 0
                        ? "N/A"
                        : $"{SpectatorCameraTrackerSourceTextPrefix}" +
                          $"{SpectatorCameraManager.FollowSpectatorCameraTracker.name}";

            if (SpectatorCameraManager.CameraSourceRef == SpectatorCameraHelper.CameraSourceRef.Hmd ||
                SpectatorCameraManager.SpectatorCameraTrackerList is null ||
                SpectatorCameraManager.SpectatorCameraTrackerList.Count == 0)
            {
                TrackerSourceDropdown.interactable = false;
            }

            if (SpectatorCameraManager.SpectatorCameraTrackerList is null ||
                SpectatorCameraManager.SpectatorCameraTrackerList.Count == 0)
            {
                TrackerCameraSourceRefButton.interactable = false;
            }

            #region LayerMask dropdown setup

            // Get all layer mask name but ignore empty string
            AllLayerMaskNameArray = Enumerable.Range(0, 31).Select(LayerMask.LayerToName)
                .Where(layer => !string.IsNullOrEmpty(layer)).ToArray();

            // Init the dropdown sprite color
            CameraLayerMaskOnSprite = Sprite.Create(
                TextureProcessHelper.CreatePureTexture2DWithColor(CameraLayerMaskDropdownSpriteWidth,
                    CameraLayerMaskDropdownSpriteHeight,
                    CameraLayerMaskDropdownOnColor),
                new Rect(0f, 0f, CameraLayerMaskDropdownSpriteWidth, CameraLayerMaskDropdownSpriteHeight),
                CameraLayerMaskDropdownSpritePivot,
                CameraLayerMaskDropdownSpritePixelsPerUnit);

            CameraLayerMaskOffSprite = Sprite.Create(
                TextureProcessHelper.CreatePureTexture2DWithColor(CameraLayerMaskDropdownSpriteWidth,
                    CameraLayerMaskDropdownSpriteHeight,
                    CameraLayerMaskDropdownOffColor),
                new Rect(0f, 0f, CameraLayerMaskDropdownSpriteWidth, CameraLayerMaskDropdownSpriteHeight),
                CameraLayerMaskDropdownSpritePivot, CameraLayerMaskDropdownSpritePixelsPerUnit);

            // Call UpdateSpectatorCameraLayerMaskUiData in order to update the dropdown value to default value
            // (last one option in dropdown list) but also show the caption text and sprite to the first option.
            // This can make every time click the dropdown will call OnSpectatorCameraLayerMaskDropdownValueChanged
            // even the previous clicked is equal to the current clicked
            UpdateSpectatorCameraLayerMaskUiData(0, LayerMask.NameToLayer(AllLayerMaskNameArray[0]), true);
            SpectatorCameraLayerMaskDropdown.onValueChanged.AddListener(OnSpectatorCameraLayerMaskDropdownValueChanged);

            #endregion

            #region UI setup that related to spectator camera FOV

            SpectatorCameraVerticalFOVSlider.minValue = SpectatorCameraHelper.VERTICAL_FOV_MIN;
            SpectatorCameraVerticalFOVSlider.maxValue = SpectatorCameraHelper.VERTICAL_FOV_MAX;
            if (SpectatorCameraManager.CameraSourceRef == SpectatorCameraHelper.CameraSourceRef.Hmd)
            {
                SpectatorCameraVerticalFOVSlider.value =
                    SpectatorCameraManager.VerticalFov;
            }
            else if (SpectatorCameraManager.CameraSourceRef == SpectatorCameraHelper.CameraSourceRef.Tracker)
            {
                if (SpectatorCameraManager.IsFollowTrackerExist())
                {
                    SpectatorCameraVerticalFOVSlider.value =
                        SpectatorCameraManager.FollowSpectatorCameraTracker.VerticalFov;
                }
            }

            SpectatorCameraVerticalFOVSlider.onValueChanged.AddListener(OnSpectatorCameraVerticalFOVSliderValueChanged);
            SpectatorCameraVerticalFOVText.text =
                $"{SpectatorCameraVerticalFOVTextPrefix}{SpectatorCameraVerticalFOVSlider.value}";

            #endregion

            #endregion
        }

        #endregion

        #region Spectator camera setting page functions

        #region Unity UI event functions

        private void OnSpectatorCameraTrackerSourceDropdownValueChanged(int arg0)
        {
            SpectatorCameraManager.FollowSpectatorCameraTracker =
                SpectatorCameraManager.SpectatorCameraTrackerList[arg0];
            NowTrackerSourceRefText.text =
                $"{SpectatorCameraTrackerSourceTextPrefix}{SpectatorCameraManager.SpectatorCameraTrackerList[arg0].name}";
            UpdateCameraSourceRefUiData(SpectatorCameraHelper.CameraSourceRef.Tracker);
        }

        private void OnSpectatorCameraLayerMaskDropdownValueChanged(int arg0)
        {
            if (arg0 == SpectatorCameraLayerMaskDropdown.options.Count - 1)
            {
                // for dropdown value equal to last option which mean reset the dropdown value to default value
                // in order to make every time click the dropdown will call OnSpectatorCameraLayerMaskDropdownValueChanged
                // even the previous clicked is equal to the current clicked
                return;
            }

            // Get the corresponding layer mask name and convert it to layer mask
            int toggleLayerMaskIndex = LayerMask.NameToLayer(AllLayerMaskNameArray[arg0]);

            LayerMask newLayerMask;
            switch (SpectatorCameraManager.CameraSourceRef)
            {
                case SpectatorCameraHelper.CameraSourceRef.Hmd:
                {
                    newLayerMask =
                        SpectatorCameraHelper.InverseCameraLayer(SpectatorCameraManager.LayerMask,
                            toggleLayerMaskIndex);
                    SpectatorCameraManager.LayerMask = newLayerMask;
                }
                    break;
                case SpectatorCameraHelper.CameraSourceRef.Tracker:
                {
                    newLayerMask =
                        SpectatorCameraHelper.InverseCameraLayer(
                            SpectatorCameraManager.FollowSpectatorCameraTracker.LayerMask, toggleLayerMaskIndex);
                    SpectatorCameraManager.FollowSpectatorCameraTracker.LayerMask = newLayerMask;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Update the layer mask ui data (no need to reload the dropdown list)
            UpdateSpectatorCameraLayerMaskUiData(arg0, toggleLayerMaskIndex);
        }

        private void OnClickSpectatorCameraCapture360Button()
        {
            SpectatorCameraManager.CaptureSpectatorCamera360Photo();
        }

        private void OnClickTrackerCameraSourceRefButton()
        {
            SpectatorCameraManager.CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Tracker;
            UpdateCameraSourceRefUiData(SpectatorCameraManager.CameraSourceRef);
        }

        private void OnClickHmdCameraSourceRefButton()
        {
            SpectatorCameraManager.CameraSourceRef = SpectatorCameraHelper.CameraSourceRef.Hmd;
            UpdateCameraSourceRefUiData(SpectatorCameraManager.CameraSourceRef);
        }

        private void OnSpectatorCameraVerticalFOVSliderValueChanged(float arg0)
        {
            switch (SpectatorCameraManager.CameraSourceRef)
            {
                case SpectatorCameraHelper.CameraSourceRef.Hmd:
                {
                    SpectatorCameraManager.VerticalFov = arg0;
                }
                    break;
                case SpectatorCameraHelper.CameraSourceRef.Tracker:
                {
                    if (SpectatorCameraManager.IsFollowTrackerExist())
                    {
                        SpectatorCameraManager.FollowSpectatorCameraTracker.VerticalFov = arg0;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateSpectatorCameraVerticalFOVUiData(arg0);
        }

        #endregion

        /// <summary>
        /// Init the spectator camera layer mask dropdown list. This only need to be called when need to re-initiate
        /// all the dropdown list data (such as reload/reset setting button is clicked).
        /// </summary>
        private void InitSpectatorCameraLayerMaskDropdown(LayerMask currentLayerMask)
        {
            var allLayerMaskNameOptionData = AllLayerMaskNameArray.Select(layerName => new TMP_Dropdown.OptionData(
                layerName,
                (currentLayerMask & (1 << LayerMask.NameToLayer(layerName))) != 0
                    ? CameraLayerMaskOnSprite
                    : CameraLayerMaskOffSprite
#if UNITY_6000_0_OR_NEWER || UNITY_TEXT_MESH_PRO_3_2_0_PRE_10_OR_NEWER
                , (currentLayerMask & (1 << LayerMask.NameToLayer(layerName))) != 0 
                    ? CameraLayerMaskDropdownOnColor
                    : CameraLayerMaskDropdownOffColor
#endif
                )
            ).ToList();
            allLayerMaskNameOptionData.Add(CameraLayerMaskDropdownLastOptionData);

            SpectatorCameraLayerMaskDropdown.ClearOptions();
            SpectatorCameraLayerMaskDropdown.AddOptions(allLayerMaskNameOptionData);

            UpdateSpectatorCameraLayerMaskUiData(0, LayerMask.NameToLayer(AllLayerMaskNameArray[0]));
        }

        /// <summary>
        /// Update the UI element which related to spectator camera layer mask dropdown
        /// </summary>
        /// <param name="dropdownUpdateIndex">Int. The index in dropdown list which is clicked</param>
        /// <param name="toggleLayerMaskIndex">Int. The corresponding layer index of dropdownUpdateIndex</param>
        /// <param name="reloadListData">Bool. Request to reload the data of dropdown list</param>
        private void UpdateSpectatorCameraLayerMaskUiData(
            in int dropdownUpdateIndex,
            in int toggleLayerMaskIndex,
            in bool reloadListData = false)
        {
            LayerMask currentLayerMask;
            switch (SpectatorCameraManager.CameraSourceRef)
            {
                case SpectatorCameraHelper.CameraSourceRef.Hmd:
                {
                    currentLayerMask = SpectatorCameraManager.LayerMask;
                }
                    break;
                case SpectatorCameraHelper.CameraSourceRef.Tracker:
                {
                    if (SpectatorCameraManager.IsFollowTrackerExist())
                    {
                        currentLayerMask =
                            SpectatorCameraManager.FollowSpectatorCameraTracker.LayerMask;
                    }
                    else
                    {
                        return;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (reloadListData) // reload all data in dropdown list
            {
                // this will also recall UpdateSpectatorCameraLayerMaskUiData once again but with reloadListData = false
                InitSpectatorCameraLayerMaskDropdown(currentLayerMask);
            }
            else // just update the dropdown item in the dropdown list
            {
                // update the dropdown value to default value in order to make every time click the dropdown will call
                // "OnSpectatorCameraLayerMaskDropdownValueChanged" function even the previous clicked is equal to the
                // current clicked
                SpectatorCameraLayerMaskDropdown.value = SpectatorCameraLayerMaskDropdown.options.Count - 1;

                bool isLayerMaskOn = (currentLayerMask & (1 << toggleLayerMaskIndex)) != 0;

                // update the dropdown item in the dropdown list
                SpectatorCameraLayerMaskDropdown.options[dropdownUpdateIndex].image =
                    isLayerMaskOn ? CameraLayerMaskOnSprite : CameraLayerMaskOffSprite;

                // update the dropdown show data
                SpectatorCameraLayerMaskDropdown.captionText.text = AllLayerMaskNameArray[dropdownUpdateIndex];
                // the captionImage(image component) in dropdown component will automatically hide when
                // save the project in unity editor every time (don't know why), so enable it manually
                SpectatorCameraLayerMaskDropdown.captionImage.enabled = true;
                SpectatorCameraLayerMaskDropdown.captionImage.sprite =
                    isLayerMaskOn ? CameraLayerMaskOnSprite : CameraLayerMaskOffSprite;
            }
        }

        /// <summary>
        /// Update the UI element which related to spectator camera source reference
        /// </summary>
        /// <param name="newValue">The camera source reference value</param>
        private void UpdateCameraSourceRefUiData(in SpectatorCameraHelper.CameraSourceRef newValue)
        {
            NowCameraSourceRefText.text = $"{NowCameraSourceRefTextPrefix}{newValue.ToString()}";

            // update layer mask, tracker dropdown interactive, fov ui data according to the new camera source ref
            switch (newValue)
            {
                case SpectatorCameraHelper.CameraSourceRef.Hmd:
                {
                    // update layer mask dropdown
                    InitSpectatorCameraLayerMaskDropdown(SpectatorCameraManager.LayerMask);
                    // update tracker dropdown interactive
                    TrackerSourceDropdown.interactable = false;
                    // update fov ui data
                    SpectatorCameraVerticalFOVSlider.value = SpectatorCameraManager.VerticalFov;
                    SpectatorCameraVerticalFOVText.text =
                        $"{SpectatorCameraVerticalFOVTextPrefix}{SpectatorCameraManager.VerticalFov}";
                }
                    break;
                case SpectatorCameraHelper.CameraSourceRef.Tracker:
                {
                    // update layer mask dropdown
                    InitSpectatorCameraLayerMaskDropdown(SpectatorCameraManager.FollowSpectatorCameraTracker.LayerMask);
                    // update tracker dropdown interactive
                    TrackerSourceDropdown.interactable = true;
                    // update fov ui data
                    if (SpectatorCameraManager.IsFollowTrackerExist())
                    {
                        SpectatorCameraVerticalFOVSlider.value =
                            SpectatorCameraManager.FollowSpectatorCameraTracker.VerticalFov;
                        SpectatorCameraVerticalFOVText.text =
                            $"{SpectatorCameraVerticalFOVTextPrefix}{SpectatorCameraManager.FollowSpectatorCameraTracker.VerticalFov}";
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        /// <summary>
        /// Update the UI element which related to spectator camera vertical FOV
        /// </summary>
        /// <param name="newValue">Float. The FOV value</param>
        public void UpdateSpectatorCameraVerticalFOVUiData(in float newValue)
        {
            SpectatorCameraVerticalFOVSlider.value = newValue;
            SpectatorCameraVerticalFOVText.text = $"{SpectatorCameraVerticalFOVTextPrefix}{newValue}";
        }

        #endregion

        #region Interaction setting page function (Unity UI event functions)

        private void OnClickThumbstickEffectFOVButton()
        {
            ObjectInteractionManager.ChangeThumbstickEffect2FOV();
            ThumbstickEffectOptionText.text =
                $"{NowInteractionModeTextPrefix}{ObjectInteractionManager.ThumbstickEffectOption.ToString()}";
        }

        private void OnClickThumbstickEffectRotationButton()
        {
            ObjectInteractionManager.ChangeThumbstickEffect2Rotation();
            ThumbstickEffectOptionText.text =
                $"{NowInteractionModeTextPrefix}{ObjectInteractionManager.ThumbstickEffectOption.ToString()}";
        }

        private void OnClickThumbstickEffectPositionButton()
        {
            ObjectInteractionManager.ChangeThumbstickEffect2Position();
            ThumbstickEffectOptionText.text =
                $"{NowInteractionModeTextPrefix}{ObjectInteractionManager.ThumbstickEffectOption.ToString()}";
        }

        private void OnAffectYAxisToggleValueChanged(bool arg0)
        {
            if (arg0)
            {
                ObjectInteractionManager.EnableAffectYAxisWhenThumbstickHorizontalMove();
            }
            else
            {
                ObjectInteractionManager.DisableAffectYAxisWhenThumbstickHorizontalMove();
            }
        }

        private void OnZoomInteractionSpeedSliderValueChanged(float arg0)
        {
            ObjectInteractionManager.ChangeInteractionSpeed(
                ObjectInteractionManager.ThumbstickEffectObject.FOV,
                arg0);
            ZoomInteractionSpeedText.text =
                $"{ZoomInteractionSpeedTextPrefix}{ObjectInteractionManager.ZoomInteractionSpeed}";
        }

        private void OnRotationInteractionSpeedSliderValueChanged(float arg0)
        {
            ObjectInteractionManager.ChangeInteractionSpeed(
                ObjectInteractionManager.ThumbstickEffectObject.Rotation,
                arg0);
            RotationInteractionSpeedText.text =
                $"{RotationInteractionSpeedTextPrefix}{ObjectInteractionManager.RotationInteractionSpeed}";
        }

        private void OnMovementInteractionSpeedSliderValueChanged(float arg0)
        {
            ObjectInteractionManager.ChangeInteractionSpeed(
                ObjectInteractionManager.ThumbstickEffectObject.Position,
                arg0);
            MovementInteractionSpeedText.text =
                $"{MovementInteractionSpeedTextPrefix}{ObjectInteractionManager.MovementInteractionSpeed}";
        }

        #endregion

        #region Main page functions (Unity UI event functions)

        private void OnClickSpectatorCameraSettingButton()
        {
            PlayAudio();

            MainPage.SetActive(false);
            SpectatorCameraSettingPage.SetActive(true);
            InteractionSettingPage.SetActive(false);
            LicensePage.SetActive(false);
        }

        private void OnClickInteractionSettingButton()
        {
            PlayAudio();

            MainPage.SetActive(false);
            SpectatorCameraSettingPage.SetActive(false);
            InteractionSettingPage.SetActive(true);
            LicensePage.SetActive(false);
        }

        private void OnClickLoadSecondSceneButton()
        {
            PlayAudio();

            if (DoesSceneExist(LoadSecondSceneName))
            {
                SceneManager.LoadScene(LoadSecondSceneName);
            }
            else
            {
                Debug.LogWarning($"{LoadSecondSceneName} scene not found. Please add it in build setting first.");
            }

            return;

            bool DoesSceneExist(string loadSceneName)
            {
                if (string.IsNullOrEmpty(loadSceneName))
                {
                    return false;
                }

                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    var lastSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
                    var sceneName = scenePath.Substring(lastSlash + 1,
                        scenePath.LastIndexOf(".", StringComparison.Ordinal) - lastSlash - 1);

                    if (string.Compare(loadSceneName, sceneName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void OnClickLicenseButton()
        {
            PlayAudio();

            MainPage.SetActive(false);
            SpectatorCameraSettingPage.SetActive(false);
            InteractionSettingPage.SetActive(false);
            LicensePage.SetActive(true);
        }

        private void OnClickExitButton()
        {
            PlayAudio();
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
			if (Application.productName == "VRTestApp")
			{
				Destroy(SpectatorCameraManager.gameObject);
				
				try
				{
					SceneManager.LoadScene("VRTestApp");
				}
				catch
				{
					Application.Quit();
				}
			}
			else
			{
				Application.Quit();
			}
#endif
        }

        #endregion

        #region General functions

        /// <summary>
        /// Play the audio at the XRHeadCamera position
        /// </summary>
        private void PlayAudio()
        {
            AudioSource.PlayClipAtPoint(UIAudioClip, XRHeadCamera.transform.position);
        }

        /// <summary>
        /// Call when Back to main page
        /// </summary>
        private void OnClickBackButton()
        {
            MainPage.SetActive(true);
            InteractionSettingPage.SetActive(false);
            SpectatorCameraSettingPage.SetActive(false);
            LicensePage.SetActive(false);
        }

        #endregion
    }
}