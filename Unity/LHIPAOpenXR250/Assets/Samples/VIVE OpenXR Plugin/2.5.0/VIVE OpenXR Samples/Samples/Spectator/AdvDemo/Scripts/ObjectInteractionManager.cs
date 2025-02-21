// Copyright HTC Corporation All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.XR;
#if XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif
using VIVE.OpenXR.Toolkits.Spectator;
using VIVE.OpenXR.Toolkits.Spectator.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: ObjectInteractionManager.cs
    /// Role: Manager (Singleton)
    /// Responsibility: Manage the interaction in the spectator camera demo scene
    /// </summary>
    public class ObjectInteractionManager : MonoBehaviour
    {
        // Singleton
        private static ObjectInteractionManager _instance;
        public static ObjectInteractionManager Instance => _instance;

        private static SpectatorCameraManager SpectatorCameraManager => SpectatorCameraManager.Instance;
        private static ControllerInputManager ControllerInputManager => ControllerInputManager.Instance;
        private static UIManager UIManager => UIManager.Instance;

        # region Const value range of object rotation, zoom and movement speed

        private const float ZoomActiveMin = .1f;
        public const float ROTATION_INTERACTION_SPEED_MIN = 1f;
        public const float ROTATION_INTERACTION_SPEED_MAX = 3f;
        public const float MOVEMENT_INTERACTION_SPEED_MIN = .01f;
        public const float MOVEMENT_INTERACTION_SPEED_MAX = .1f;
        public const float ZOOM_INTERACTION_SPEED_MIN = 1f;
        public const float ZOOM_INTERACTION_SPEED_MAX = 5f;

        # endregion

        #region Default value definition

        private const ThumbstickEffectObject ThumbstickEffectOptionDefault = ThumbstickEffectObject.Rotation;

        private const bool IsAffectYAxisWhenThumbstickMoveDefault = false;
        private const float RotationInteractionSpeedDefault = 1f;
        private const float MovementInteractionSpeedDefault = .05f;
        private const float ZoomInteractionSpeedDefault = 1f;

        #endregion

#if XR_INTERACTION_TOOLKIT
		// The ray interactor that is used to select objects
		[field: SerializeField] private XRRayInteractor SelectObjectInteractor { get; set; }
#endif

        // The effect change on object when change thumbstick
        [field: SerializeField]
        public ThumbstickEffectObject ThumbstickEffectOption { get; private set; } = ThumbstickEffectOptionDefault;

        // The effect change on object from y-axis when change thumbstick horizontally
        [field: SerializeField]
        public bool IsAffectYAxisWhenThumbstickMove { get; private set; } = IsAffectYAxisWhenThumbstickMoveDefault;

        // The speed of rotation when change thumbstick
        [field: SerializeField, Range(ROTATION_INTERACTION_SPEED_MIN, ROTATION_INTERACTION_SPEED_MAX)]
        public float RotationInteractionSpeed { get; private set; } = RotationInteractionSpeedDefault;

        // The speed of movement when change thumbstick
        [field: SerializeField, Range(MOVEMENT_INTERACTION_SPEED_MIN, MOVEMENT_INTERACTION_SPEED_MAX)]
        public float MovementInteractionSpeed { get; private set; } = MovementInteractionSpeedDefault;

        // The speed of zoom when change thumbstick
        [field: SerializeField, Range(ZOOM_INTERACTION_SPEED_MIN, ZOOM_INTERACTION_SPEED_MAX)]
        public float ZoomInteractionSpeed { get; private set; } = ZoomInteractionSpeedDefault;

        [HideInInspector] public bool isInteractingObject;

        #region Unity lifecycle event function

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

        private void Start()
        {
            isInteractingObject = false;

#if XR_INTERACTION_TOOLKIT
			if (SelectObjectInteractor == null)
			{
				Debug.LogError("The ray interactor that is used to select objects is not set.");
			}
#endif
        }

        private void Update()
        {
            #region Check input device is valid

            if (ControllerInputManager is null ||
                !ControllerInputManager.IsRightControllerConnected())
            {
                return;
            }

            #endregion

            #region Check is holding the trigger button on the right controller or not

            ControllerInputManager.RightController.TryGetFeatureValue(
                CommonUsages.triggerButton, out isInteractingObject);
            if (isInteractingObject is false)
            {
                // If user is not perform hold the trigger button on the right controller, just return
                return;
            }

            #endregion

            #region Interactive spectator camera

            if (SpectatorCameraManager is null)
            {
                Debug.LogWarning("Cannot interactive with spectator camera " +
                                 "because the spectator camera manager is null.");
                return;
            }

            ControllerInputManager.RightController.TryGetFeatureValue(
                CommonUsages.primary2DAxis, out Vector2 rightControllerThumbstickVector2);

            // Run the interactive action according to the source of the spectator camera
            switch (SpectatorCameraManager.CameraSourceRef)
            {
                case SpectatorCameraHelper.CameraSourceRef.Hmd:
                {
                    // Interactive spectator camera logic according to the option
                    // of the thumbstick effect to change the spectator camera.
                    switch (ThumbstickEffectOption)
                    {
                        // If CameraSourceRef is Hmd, the position and rotation of spectator camera
                        // is controlled by HMD, so if the option of the thumbstick effect is "Rotation"
                        // or "Position", just ignore the operation.
                        case ThumbstickEffectObject.Rotation:
                        case ThumbstickEffectObject.Position:
                        {
                            Debug.LogWarning("The spectator camera position/rotation now is controlled by " +
                                             "HMD. Ignore the position/rotation operation by thumbstick");
                        }
                            break;
                        case ThumbstickEffectObject.FOV:
                        {
                            ZoomSpectatorCamera(rightControllerThumbstickVector2);
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                    break;
                case SpectatorCameraHelper.CameraSourceRef.Tracker:
                {
                    switch (ThumbstickEffectOption)
                    {
                        case ThumbstickEffectObject.Rotation:
                        {
                            RotateObject(rightControllerThumbstickVector2);
                        }
                            break;
                        case ThumbstickEffectObject.Position:
                        {
                            MoveObject(rightControllerThumbstickVector2);
                        }
                            break;
                        case ThumbstickEffectObject.FOV:
                        {
                            ZoomSpectatorCamera(rightControllerThumbstickVector2);
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            #endregion
        }

        #endregion

        #region Public function of changing interaction option

        public void ChangeThumbstickEffect2Position()
        {
            ThumbstickEffectOption = ThumbstickEffectObject.Position;
        }

        public void ChangeThumbstickEffect2Rotation()
        {
            ThumbstickEffectOption = ThumbstickEffectObject.Rotation;
        }

        public void ChangeThumbstickEffect2FOV()
        {
            ThumbstickEffectOption = ThumbstickEffectObject.FOV;
        }

        public void EnableAffectYAxisWhenThumbstickHorizontalMove()
        {
            IsAffectYAxisWhenThumbstickMove = true;
        }

        public void DisableAffectYAxisWhenThumbstickHorizontalMove()
        {
            IsAffectYAxisWhenThumbstickMove = false;
        }

        public void ChangeInteractionSpeed(in ThumbstickEffectObject thumbstickEffectOption, in float value)
        {
            switch (thumbstickEffectOption)
            {
                case ThumbstickEffectObject.Position:
                {
                    MovementInteractionSpeed = value;
                }
                    break;
                case ThumbstickEffectObject.Rotation:
                {
                    RotationInteractionSpeed = value;
                }
                    break;
                case ThumbstickEffectObject.FOV:
                {
                    ZoomInteractionSpeed = value;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Private function of object interaction

        private void ZoomSpectatorCamera(in Vector2 thumbstickValue)
        {
            // Change one axis at the one time.
            float zoomValue = thumbstickValue.y;
            if (Math.Abs(zoomValue) < ZoomActiveMin)
            {
                // If the thumbstick value is too small, ignore the zoom operation.
                return;
            }

            zoomValue *= ZoomInteractionSpeed;

            if (SpectatorCameraManager is null)
            {
                return;
            }

            if (SpectatorCameraManager.IsCameraSourceAsHmd())
            {
                SpectatorCameraManager.VerticalFov += zoomValue;

                if (UIManager != null)
                {
                    // Once set the FOV value to the spectator camera, update the related field on UI
                    UIManager.UpdateSpectatorCameraVerticalFOVUiData(
                        SpectatorCameraManager.VerticalFov);
                }
            }
            else if (SpectatorCameraManager.IsCameraSourceAsTracker())
            {
                SpectatorCameraManager.FollowSpectatorCameraTracker.VerticalFov += zoomValue;

                if (UIManager != null)
                {
                    // Once set the FOV value to the spectator camera, update the related field on UI
                    UIManager.UpdateSpectatorCameraVerticalFOVUiData(
                        SpectatorCameraManager.FollowSpectatorCameraTracker.VerticalFov);
                }
            }
        }

        private void MoveObject(in Vector2 thumbstickValue)
        {
            if (SpectatorCameraManager is null)
            {
                return;
            }

            float distance;
            bool isMoveX = false;
            // Change one axis at the one time.
            if (Math.Abs(thumbstickValue.x) > Math.Abs(thumbstickValue.y))
            {
                // If the thumbstick x value is bigger than thumbstick y value.
                distance = thumbstickValue.x;
                isMoveX = true;
            }
            else
            {
                distance = thumbstickValue.y;
            }

            distance *= MovementInteractionSpeed;

            if (SpectatorCameraManager.IsCameraSourceAsTracker())
            {
                if (isMoveX)
                {
                    SpectatorCameraManager.FollowSpectatorCameraTracker.Position +=
                        SpectatorCameraManager.FollowSpectatorCameraTracker.Rotation *
                        new Vector3(distance, 0, 0);
                }
                else
                {
                    // Is possible to affect y-axis or z-axis

                    Vector3 moveVector = IsAffectYAxisWhenThumbstickMove
                        // If affect y-axis as true, the value of "thumbstickValue.y" will affect y-axis.
                        ? new Vector3(0, distance, 0)
                        // Otherwise, the value of "thumbstickValue.y" will affect z-axis.
                        : new Vector3(0, 0, distance);

                    SpectatorCameraManager.FollowSpectatorCameraTracker.Position +=
                        SpectatorCameraManager.FollowSpectatorCameraTracker.Rotation * moveVector;
                }
            }
        }

        private void RotateObject(in Vector2 thumbstickValue)
        {
            if (SpectatorCameraManager is null)
            {
                return;
            }

            float angle;
            bool isRotateX = false;
            // Change one axis at the one time.
            if (Math.Abs(thumbstickValue.x) > Math.Abs(thumbstickValue.y))
            {
                angle = -thumbstickValue.x;
                isRotateX = true;
            }
            else
            {
                angle = thumbstickValue.y;
            }

            angle *= RotationInteractionSpeed;

            if (SpectatorCameraManager.IsCameraSourceAsTracker())
            {
                if (isRotateX)
                {
                    SpectatorCameraManager.FollowSpectatorCameraTracker.transform.Rotate(
                        angle, 0, 0, Space.Self);
                }
                else
                {
                    // Is possible to affect y-axis or z-axis
                    if (IsAffectYAxisWhenThumbstickMove)
                    {
                        SpectatorCameraManager.FollowSpectatorCameraTracker.transform.Rotate(
                            0, angle, 0, Space.Self);
                    }
                    else
                    {
                        SpectatorCameraManager.FollowSpectatorCameraTracker.transform.Rotate(
                            0, 0, angle, Space.Self);
                    }
                }
            }
        }

        #endregion

        #region Enum related to object interaction

        public enum ThumbstickEffectObject
        {
            Position, // effect object Position when change thumbstick
            Rotation, // effect object Rotation when change thumbstick
            FOV // effect spectator camera FOV when change thumbstick (P.S. interactable object will ignore this option)
        }

        #endregion
    }
}