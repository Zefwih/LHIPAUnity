using System;
using UnityEngine;
using System.IO;
using System.Linq;
using VIVE.OpenXR;
using VIVE.OpenXR.EyeTracker;

namespace lhipa
{

    /// <summary>
    /// Class with use cases how to use LHIPA together with eye tracking.
    /// Contains examples for LHIPA live calculation and for collecting pupil data.
    /// This class uses the OpenXR extension for HTC eye tracking devices.
    /// </summary>
    public class LHIPAOpenXRTracking : MonoBehaviour
    {
        [Header("Eye Diameter Logging Parameters")] [Tooltip("Path to output logfile with LHIPA values.")]
        public string outputFilePath = "Assets/LHIPA/pupil_data";

        [Header("General LHIPA Calculation Parameters")]
        [Tooltip(
            "Modulus maximus correction threshold to compensate for the differences between wavelet scales in Python and Unity.")]
        public float modMaxCorrectionThreshold = 0f;

        [Tooltip("Print all intermediate results of each calculation on console.")]
        public bool showDetailedCalculationLog = true;

        [Header("LHIPA Live Calculation Parameters")]
        [Tooltip("Time in seconds until the next LHIPA live calculation.")]
        public float calculationInterval = 10f;

        // Variables for background handling
        private bool isCalculating = false;
        private bool isRecording = false;
        private string filePath;

        private int
            maxSamples = 10000000; // Maximal number of von dates in pupilDiameter array (storage space limitation)

        private float[] pupilDiameters;
        private int currentIndex = 0;
        private float startTime;

        private float[] pupilDiametersLive;
        private int currentIndexLive = 0;
        private float startTimeLive;

        private float currentLiveLHIPAValue = -1f;

        /// <summary>
        /// Start live calculation of LHIPA all calculationInterval seconds with use of OpenXR eye tracking data.
        /// </summary>
        public void StartLHIPALiveCalculation(float calculationIntervalInSeconds)
        {
            if (calculationIntervalInSeconds >= 1)
            {
                calculationInterval = calculationIntervalInSeconds;
            }
            else
            {
                Debug.LogError("Calculation Interval should not be smaller than 1!");
                return;
            }

            if (isCalculating) return;
            isCalculating = true;
            pupilDiametersLive = new float[maxSamples];
            currentIndexLive = 0;
            startTimeLive = Time.time;
            Debug.Log("LHIPA live calculation started.");
        }

        /// <summary>
        /// Start live calculation of LHIPA.
        /// </summary>
        public void StopLHIPALiveCalculation()
        {
            if (!isCalculating) return;
            isCalculating = false;
            Debug.Log("LHIPA live calculation stopped.");
        }

        /// <summary>
        /// Start recording of pupil diameter with use of OpenXR eye tracking data.
        /// </summary>
        public void StartEyeTrackingRecording()
        {
            if (isRecording) return;
            isRecording = true;
            pupilDiameters = new float[maxSamples];
            currentIndex = 0;
            startTime = Time.time;
            Debug.Log("Eye tracking recording started.");
        }

        /// <summary>
        /// Stop recording of pupil diameter data. Log pupil diameter data into outputFilePath. Calculate LHIPA with recorded data.
        /// </summary>
        /// <returns>
        /// Float array with three values: [0] LHIPA value, [1] duration of recording in seconds, [2] sampling rate of recording
        /// </returns>
        public float[] StopEyeTrackingRecordingAndCalculateLHIPA()
        {
            if (!isRecording) return new[] { -1f, -1f };
            isRecording = false;
            // Create log file
            filePath = outputFilePath + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".csv";
            // Write eye diameter array into logfile
            float[] dataSlice = pupilDiameters.Take(currentIndex).ToArray();
            SaveFloatArrayToJson(dataSlice, filePath);
            // Calculate LHIPA from recorded data and return it
            float duration = Time.time - startTime;
            float samplingRate = dataSlice.Length / duration;
            float resultLHIPAvalue = LHIPA.CalculateLHIPA(dataSlice, duration, modMaxCorrectionThreshold,
                showDetailedCalculationLog);
            Debug.Log("Result LHIPA value: " + resultLHIPAvalue + ", Duration of recording: " + duration +
                      ", Sampling rate: " + samplingRate);
            return new[]
            {
                resultLHIPAvalue, duration, samplingRate
            }; // float: 0. value: LHIPA, 1. value: duration, 2. value: sampling rate
        }

        // Save float array into given file path
        private static void SaveFloatArrayToJson(float[] array, string filePath)
        {
            string json = $"[{string.Join(",", array)}]";
            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log("Eye tracking recording stopped. Results logged to " + filePath);
            }
            catch (IOException e)
            {
                Debug.LogError("Error when trying to save pupil data: " + e.Message);
            }
        }

        /// <summary>
        /// Returns true when pupil data is recorded right now.
        /// </summary>
        public bool IsRecordingRightNow()
        {
            return isRecording;
        }

        /// <summary>
        /// Returns true when pupil data is calculated right now.
        /// </summary>
        public bool IsCalculatingRightNow()
        {
            return isCalculating;
        }

        /// <summary>
        /// Returns current LHIPA live calculation value. Should only be used when isCalculating = true.
        /// </summary>
        public float GetCurrentLiveLHIPA()
        {
            return currentLiveLHIPAValue;
        }

        private void Update()
        {
            if (isCalculating || isRecording)
            {
                // Call eye tracking data from OpenXR
                bool success = XR_HTC_eye_tracker.Interop.GetEyePupilData(out var pupilData);

                if (!success)
                {
                    Debug.LogWarning("No eye tracking data available.");
                    return;
                }

                // Read current pupil diameter
                XrSingleEyePupilDataHTC leftPupil = pupilData[(int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC];
                XrSingleEyePupilDataHTC rightPupil = pupilData[(int)XrEyePositionHTC.XR_EYE_POSITION_RIGHT_HTC];

                float pupilLeft = leftPupil.isDiameterValid ? leftPupil.pupilDiameter : -1f;
                float pupilRight = rightPupil.isDiameterValid ? rightPupil.pupilDiameter : -1f;

                // Calculate average of left and right pupil diameter
                float currentDiameter = (pupilLeft + pupilRight) / 2;

                Debug.Log($"Pupil diameter: {currentDiameter} mm");

                if (isRecording)
                {
                    // Keep current value in array
                    if (currentIndex < maxSamples)
                    {
                        pupilDiameters[currentIndex++] = currentDiameter;
                    }
                }

                if (isCalculating)
                {
                    // Keep current value in array
                    if (currentIndexLive < maxSamples)
                    {
                        pupilDiametersLive[currentIndexLive++] = currentDiameter;
                    }

                    // Calculate LHIPA all calculationInterval seconds
                    if (Time.time - startTimeLive >= calculationInterval)
                    {
                        float duration = Time.time - startTime;
                        float[] dataSlice = pupilDiametersLive.Take(currentIndex).ToArray();
                        currentLiveLHIPAValue = LHIPA.CalculateLHIPA(dataSlice, duration, modMaxCorrectionThreshold,
                            showDetailedCalculationLog);

                        Debug.Log(
                            "----------------- Current LHIPA Value: " + currentLiveLHIPAValue +
                            " ---------------------------");

                        // Clear pupil diameter array
                        Array.Clear(pupilDiametersLive, 0, currentIndexLive);
                        currentIndexLive = 0;
                        startTimeLive = Time.time;
                    }

                }

            }
        }
    }
}