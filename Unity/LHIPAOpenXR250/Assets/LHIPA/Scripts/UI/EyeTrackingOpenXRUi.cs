using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;

namespace lhipa
{

    /// <summary>
    /// UI Methods for testing the LHIPA methods in VR with the LHIPAOpenXRTracking class in Unity.
    /// </summary>
    public class EyeTrackingOpenXRUi : MonoBehaviour
    {
        [Header("References to UI objects")]

        // HMD 
        public Transform hmdTransform;

        public float distanceFromHMD = 10f; // in metres

        // Indicators UI elements
        public Image calculationIndicatorImage;
        public TextMeshProUGUI calculationIndicatorText;
        public Image recordingIndicatorImage;
        public TextMeshProUGUI recordingIndicatorText;

        // LHIPA UI elements
        public Image calculationLHIPAImage;
        public TextMeshProUGUI calculationLHIPAText;
        public TextMeshProUGUI calculationIntervalText;
        public Image recordingLHIPAImage;
        public TextMeshProUGUI recordingLHIPAText;
        public TextMeshProUGUI durationText;
        public TextMeshProUGUI modMaxThresholdText;

        private LHIPAOpenXRTracking tracker;
        private bool buttonPressed = false;

        private void Start()
        {
            // Get reference to script
            tracker = transform.GetComponent<LHIPAOpenXRTracking>();

            // Check VR connection
            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                Debug.Log("Connected with VR HMD.");
            }
            else
            {
                Debug.LogWarning("No VR HMD found!");
            }

            // Get reference to VR camera
            if (hmdTransform == null && Camera.main != null)
            {
                hmdTransform = Camera.main.transform; // use normal main camera if none is defined
            }

            // Change HMD position
            if (hmdTransform != null)
            {
                PositionInfoPlane();
            }
            else
            {
                Debug.LogError("No main camera found in scene!");
            }
        }

        // Change position of this object in relation to VR camera
        private void PositionInfoPlane()
        {
            // Distance to player
            Vector3 forwardDirection = hmdTransform.forward;
            Vector3 newPosition = hmdTransform.position + forwardDirection * distanceFromHMD;

            // Adapt height
            newPosition.y = hmdTransform.position.y;

            // Rotate to player
            Quaternion newRotation = Quaternion.LookRotation(newPosition - hmdTransform.position);

            transform.position = newPosition;
            transform.rotation = newRotation;
        }

        private void Update()
        {
            // Check for keyboard presses:
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (!buttonPressed)
                {
                    StartCoroutine(HandleKeyboardPress(0.5f));
                    tracker.StartLHIPALiveCalculation(tracker.calculationInterval);
                    calculationIndicatorImage.color = Color.green;
                    calculationIndicatorText.text = "CALCULATING";
                }
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (!buttonPressed)
                {
                    StartCoroutine(HandleKeyboardPress(0.5f));
                    tracker.StopLHIPALiveCalculation();
                    calculationIndicatorImage.color = Color.red;
                    calculationIndicatorText.text = "NOT CALCULATING";
                }
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                if (!buttonPressed)
                {
                    StartCoroutine(HandleKeyboardPress(0.5f));
                    tracker.StartEyeTrackingRecording();
                    recordingIndicatorImage.color = Color.green;
                    recordingIndicatorText.text = "RECORDING";
                    durationText.text = " ... ";
                    recordingLHIPAText.text = "...";
                }
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                if (!buttonPressed)
                {
                    StartCoroutine(HandleKeyboardPress(0.5f));
                    float[] results = tracker.StopEyeTrackingRecordingAndCalculateLHIPA();
                    recordingIndicatorImage.color = Color.red;
                    recordingIndicatorText.text = "NOT RECORDING";
                    float currentLhipaValue = results[0];
                    recordingLHIPAText.text = currentLhipaValue.ToString("F3");
                    durationText.text = "Duration: " + results[1].ToString("F2") + " s\nSampling Rate: " +
                                        results[2].ToString("F2");
                    if (currentLhipaValue <= 0)
                    {
                        recordingLHIPAImage.color = Color.yellow;
                    }
                    else if (currentLhipaValue >= 1)
                    {
                        recordingLHIPAImage.color = Color.blue;
                    }
                    else
                    {
                        recordingLHIPAImage.color = Color.Lerp(Color.yellow, Color.blue, currentLhipaValue);
                    }
                }
            }

            // Update text fields
            calculationIntervalText.text = "Calculation Interval: " + tracker.calculationInterval + " s";
            modMaxThresholdText.text = "Modulus Maximus Correction Threshold: " + tracker.modMaxCorrectionThreshold;

            if (tracker.IsCalculatingRightNow() && tracker.GetCurrentLiveLHIPA() != -1f)
            {
                float currentLhipaValue = tracker.GetCurrentLiveLHIPA();
                calculationLHIPAText.text = currentLhipaValue.ToString("F3");
                if (currentLhipaValue <= 0)
                {
                    calculationLHIPAImage.color = Color.yellow;
                }
                else if (currentLhipaValue >= 1)
                {
                    calculationLHIPAImage.color = Color.blue;
                }
                else
                {
                    calculationLHIPAImage.color = Color.Lerp(Color.yellow, Color.blue, currentLhipaValue);
                }
            }
        }

        // Wait a specified time until next keyboard press is allowed
        private IEnumerator HandleKeyboardPress(float waitTime)
        {
            buttonPressed = true;
            yield return new WaitForSeconds(waitTime);
            buttonPressed = false;
        }
    }
}