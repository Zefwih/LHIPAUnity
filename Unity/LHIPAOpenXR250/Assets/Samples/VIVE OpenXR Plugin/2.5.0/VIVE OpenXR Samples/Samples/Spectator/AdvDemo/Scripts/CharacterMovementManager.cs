// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: CharacterMovementManager.cs
    /// Role: Manager (Singleton)
    /// Responsibility: Manage the character movement
    /// </summary>
    public partial class CharacterMovementManager : MonoBehaviour
    {
        // Singleton
        private static CharacterMovementManager _instance;
        private ObjectInteractionManager ObjectInteractionManager => ObjectInteractionManager.Instance;
        private ControllerInputManager ControllerInputManager => ControllerInputManager.Instance;

        #region Private const movement setting

        private const float MoveSpeedMin = .01f;
        private const float MoveSpeedMax = .1f;
        private const float MoveSpeedDefault = .03f;
        private const float MoveThreshold = .01f;

        #endregion

        [field: SerializeField, Range(MoveSpeedMin, MoveSpeedMax)]
        private float MoveSpeed { get; set; } = MoveSpeedDefault;

        [field: SerializeField] private GameObject XRRig { get; set; }

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

        private void Update()
        {
            if (ObjectInteractionManager != null)
            {
                // Since we determine whether the user wants to interact with the object by the trigger button,
                // we check whether the trigger button is pressed or not. If pressed, we will ignore the character
                // movement logic and prepare to interact with the object if the user has any action until the
                // trigger button is released.
                if (ObjectInteractionManager.isInteractingObject)
                {
                    return;
                }
            }

            if (ControllerInputManager is null ||
                ControllerInputManager.IsRightControllerConnected() is false)
            {
                return;
            }

            // Get the thumbstick value of the right controller
            ControllerInputManager.RightController.TryGetFeatureValue(
                CommonUsages.primary2DAxis, out Vector2 rightControllerThumbstickVector2);

            // Check the thumbstick value is beyond the threshold or not
            if (Vector2.Distance(rightControllerThumbstickVector2, Vector2.zero) > MoveThreshold)
            {
                MoveCharacter(rightControllerThumbstickVector2);
            }
        }

        #endregion

        private void MoveCharacter(in Vector2 thumbstickValue)
        {
            XRRig.transform.position += XRRig.transform.rotation *
                                        new Vector3(MoveSpeed * thumbstickValue.x, 0, MoveSpeed * thumbstickValue.y);
        }
    }
}