using System;
using System.Linq;
using UnityEngine;

namespace lhipa
{

    /// <summary>
    /// Class to simulate the measurement of eye diameter data that can be used to test the LHIPA algorithm.
    /// </summary>
    public class EyeTrackSimulation : MonoBehaviour
    {
        [Header("Simulation of Eye Tracking Parameters")] [Tooltip("Average pupil diameter in mm.")]
        public float baselineDiameter = 3.5f;

        [Tooltip("Time in seconds between change of low and high eye diameter values.")]
        public float simulationSwitchInterval = 1f;

        [Tooltip("Number of simulated measurements per second.")]
        public float samplingRate = 256;

        [Tooltip("Start eye tracking simulation together with scene.")]
        public bool startWithScene = true;

        [Header("LHIPA Calculation Parameters")] [Tooltip("Time in seconds until the next LHIPA calculation.")]
        public float calculationInterval = 5f;

        [Tooltip(
            "Modulus maximus correction threshold to compensate for the differences between wavelet scales in Python and Unity.")]
        public float modMaxCorrectionThreshold = 0f;

        [Tooltip("Print all intermediate results of each calculation on console.")]
        public bool showDetailedCalculationLog = true;

        [Header("LHIPA Result Value")] [Tooltip("Output of current LHIPA value calculated by the LHIPA algorithm.")]
        public float currentLHIPAValue = 0f;

        // Variables used for eye tracking simulation
        private float[] pupilDiameters;

        private int
            maxSamples = 100000; // Maximal number of von dates in pupilDiameter array (storage space limitation)

        private int currentIndex = 0;
        private float startTime;
        private float lastSampleTime;
        float samplingInterval;
        private bool simulationOngoing = false;
        private float simulationPhase = 0f; // fixed per session; a per-sample random phase would be noise
        
        private const int FilterLength = 32; // Symlet16

        private float lowFreqHz;   // target frequency for Low-Frequency-Phase (lof)
        private float highFreqHz;  // target frequency for High-Frequency-Phase (hif)

        private void Start()
        {
            ComputeTargetFrequencies();
            if (startWithScene) StartSimulation();
        }

        /// <summary>
        /// Start eye tracking simulation and LHIPA calculation.
        /// </summary>
        public void StartSimulation()
        {
            pupilDiameters = new float[maxSamples];
            startTime = Time.time;
            samplingInterval = 1f / samplingRate;
            lastSampleTime = -samplingInterval;
            simulationPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f); // one stable phase per session
            simulationOngoing = true;
        }

        /// <summary>
        /// Stop eye tracking simulation and LHIPA calculation.
        /// </summary>
        public void StopSimulation()
        {
            simulationOngoing = false;
        }

        private void Update()
        {
            if (!simulationOngoing) return;

            // Read simulated pupil data
            float currentDiameter = GetPupilDiameterFromEyeTracker();
            float currentTime = Time.time;
            samplingInterval = 1f / samplingRate;

            // Add eye diameter if enough time since last sampling according to sampling rate
            if (currentTime - lastSampleTime >= samplingInterval)
            {
                if (currentIndex < maxSamples)
                {
                    pupilDiameters[currentIndex++] = currentDiameter;
                }

                lastSampleTime = currentTime;
            }

            // Calculate LHIPA all calculationInterval seconds
            if (Time.time - startTime >= calculationInterval)
            {
                float duration = Time.time - startTime;
                float[] dataSlice = pupilDiameters.Take(currentIndex).ToArray();

                if (dataSlice.Length >= LHIPA.MinSamples)
                {
                    currentLHIPAValue = LHIPA.CalculateLHIPA(dataSlice, duration, modMaxCorrectionThreshold,
                        showDetailedCalculationLog);

                    Debug.Log(
                        "----------------- Current LHIPA Value: " + currentLHIPAValue + " ---------------------------");
                }
                else
                {
                    Debug.LogWarning($"Not enough samples for LHIPA (need >= {LHIPA.MinSamples}, " +
                                     $"got {dataSlice.Length}). Increase samplingRate or calculationInterval.");
                }

                // Clear simulation data
                Array.Clear(pupilDiameters, 0, currentIndex);
                currentIndex = 0;
                startTime = Time.time;
            }
        }
        
        /// <summary>
        /// Compute new frequency targets dependent on sampling rate and pupil change interval.
        /// </summary>
        public void ComputeTargetFrequencies()
        {
            int sampleCount = Mathf.RoundToInt(calculationInterval * samplingRate);
            int maxLevel = Mathf.FloorToInt(Mathf.Log(sampleCount / (float)(FilterLength - 1), 2f));
            int lof = Mathf.Max(1, maxLevel / 2);

            // hif: [fs/4, fs/2]
            highFreqHz = samplingRate * 3f / 8f;

            // lof: [fs/2^(lof+1), fs/2^lof]
            float lofLow = samplingRate / Mathf.Pow(2, lof + 1);
            float lofHigh = samplingRate / Mathf.Pow(2, lof);
            lowFreqHz = (lofLow + lofHigh) / 2f;

            Debug.Log($"Simulator: hif≈{highFreqHz:F1} Hz, lof≈{lowFreqHz:F1} Hz (N={sampleCount}, maxLevel={maxLevel}, lof-Level={lof})");
        }

        /// <summary>
        /// Eye Tracking Simulation Method that changes between high and low eye diameter values
        /// </summary>
        /// <returns>
        /// Random Eye Diameter Value
        /// </returns>
        public float GetPupilDiameterFromEyeTracker()
        {
            // Simulated pupil diameters with frequency variations; cycle length = simulationSwitchInterval * 2.
            // Use a phase that is fixed for the session (set in StartSimulation) so the sine waves are
            // continuous instead of being re-randomized every sample (which would just produce noise).
            float time = Time.time % (simulationSwitchInterval * 2);
            if (time < simulationSwitchInterval)
            {
                // High-Frequency-Phase
                return baselineDiameter + 0.01f * Mathf.Sin(2f * Mathf.PI * highFreqHz * time + simulationPhase);
            }
            else
            {
                // Low-Frequency-Phase
                return baselineDiameter + 0.05f * Mathf.Sin(2f * Mathf.PI * lowFreqHz * time + simulationPhase);
            }
        }

    }
}