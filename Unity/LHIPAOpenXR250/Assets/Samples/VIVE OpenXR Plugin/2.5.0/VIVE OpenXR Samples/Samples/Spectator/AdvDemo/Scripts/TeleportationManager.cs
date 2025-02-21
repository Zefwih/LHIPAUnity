// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.XR;
#if XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: TeleportationManager.cs
    /// Role: Manager (Singleton)
    /// Responsibility: Manage the character teleportation
    /// </summary>
    public partial class TeleportationManager : MonoBehaviour
    {
#if XR_INTERACTION_TOOLKIT
        // Singleton
        private static TeleportationManager _instance;
        private static ControllerInputManager ControllerInputManager => ControllerInputManager.Instance;

        #region Private const warning and error message

        private const string TeleportationProviderNotFoundErrorMessage =
            "TeleportationProvider is null, please setup the related component correctly in the Unity Editor and try it again.";

        private const string TeleportLabelNotFoundErrorMessage =
            "TeleportLabel is null, please setup the related component correctly in the Unity Editor and try it again.";

        private const string TeleportRayNotFoundErrorMessage =
            "TeleportRay is null, please setup the related component correctly in the Unity Editor and try it again.";

        private const string UiOnTeleportControllerNotFoundErrorMessage =
            "UiOnTeleportController is null, cannot disable the UI on teleport controller.";

        #endregion

        #region Private varibles related to doing the teleportion

        // Record the previous value of the primary button on the left controller
        private bool PreviousLeftHandPrimaryButtonForTeleportValue { get; set; }

        [field: SerializeField] private TeleportationProvider TeleportationProvider { get; set; }

        #endregion

        #region Teleport ray and teleport label prefab (private varibles)

        [field: SerializeField] private XRRayInteractor TeleportRay { get; set; }
        [field: SerializeField] private GameObject TeleportLabelPrefab { get; set; }
        private GameObject TeleportLabel { get; set; }

        #endregion

        #region UI object disable when teleportation is activated (private varibles)

        [field: SerializeField] private GameObject UiOnTeleportController { get; set; }

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

        private void Start()
        {
            PreviousLeftHandPrimaryButtonForTeleportValue = false;
            if (TeleportLabelPrefab != null)
            {
                // Create the teleport label on the scene
                TeleportLabel = Instantiate(TeleportLabelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                TeleportLabel.SetActive(false);
            }

            // Disable the teleport ray and teleport label
            SetTeleportRayComponentAndLabelObject(false);
        }

        private void Update()
        {
            // Check if the left controller is connected or not
            if (ControllerInputManager is null || !ControllerInputManager.IsLeftControllerConnected())
            {
                return;
            }

            // Since we define the value of the primary button on the left controller
            // to determine whether teleport is activated or not, we need to check the
            // value of the primary button
            ControllerInputManager.LeftController.TryGetFeatureValue(
                CommonUsages.primaryButton, out bool leftControllerPrimaryButtonForTeleportValue);

            // Set the teleport ray and teleport label active according to the value of the primary button
            SetTeleportRayComponentAndLabelObject(leftControllerPrimaryButtonForTeleportValue);

            // If the value of the primary button on the left controller is not changed,
            // which means the user is still pressing or not pressing the primary button.
            if (PreviousLeftHandPrimaryButtonForTeleportValue != leftControllerPrimaryButtonForTeleportValue)
            {
                PreviousLeftHandPrimaryButtonForTeleportValue = leftControllerPrimaryButtonForTeleportValue;

                if (leftControllerPrimaryButtonForTeleportValue)
                {
                    // Just return if the user change the primary button value from false to true.
                    // We only need to do the teleportation when the user release the primary button. (from true to false)
                    return;
                }

                if (TeleportationProvider == null)
                {
                    Debug.LogError(TeleportationProviderNotFoundErrorMessage);
                }
                else if (TeleportRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    Teleport(hit.point);
                }

                SetTeleportRayComponentAndLabelObject(false);
            }
        }

        #endregion

        #region Main functions for teleportation

        private void SetTeleportRayComponentAndLabelObject(in bool canTeleport)
        {
            // Check if the teleport ray component is null
            if (TeleportRay == null)
            {
                Debug.LogError(TeleportRayNotFoundErrorMessage);
            }
            else
            {
                // Set teleport ray
                TeleportRay.enabled = canTeleport;
            }

            if (UiOnTeleportController == null)
            {
                Debug.LogError(UiOnTeleportControllerNotFoundErrorMessage);
            }
            else
            {
                // Disable the UI on teleport controller when teleport ray is activated
                UiOnTeleportController.SetActive(!canTeleport);
            }

            // Check if the teleport label prefab is null
            if (TeleportLabel == null)
            {
                Debug.LogError(TeleportLabelNotFoundErrorMessage);
            }
            else
            {
                // Set teleport label prefab
                TeleportLabel.SetActive(canTeleport);
            }

            if (TeleportRay == null || TeleportLabel == null || !canTeleport)
            {
                // If TeleportRay = null or TeleportLabel = null or teleport is in-activated,
                // we do not need to update the position of the teleport label
                return;
            }

            if (TeleportRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                // Update the position of the teleport label
                TeleportLabel.transform.position = hit.point;
            }
            else
            {
                // Disable the teleport label if the teleport ray does not hit anything
                TeleportLabel.SetActive(false);
            }
        }

        private void Teleport(in Vector3 position)
        {
            var request = new TeleportRequest
            {
                destinationPosition = position
            };

            TeleportationProvider.QueueTeleportRequest(request);
        }

        #endregion
#endif
    }
}