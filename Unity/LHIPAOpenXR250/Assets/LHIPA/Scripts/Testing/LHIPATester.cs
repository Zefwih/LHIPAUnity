using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace lhipa
{

    /// <summary>
    /// Class with methods to test the LHIPA function either with single float arrays or with a list of arrays.
    /// </summary>
    public class LHIPATester : MonoBehaviour
    {
        [Tooltip("Duration of recorded or simulated eye tracking data in seconds.")]
        public float durationInSeconds = 1.0f;

        [Tooltip(
            "Modulus maximus correction threshold to compensate for the differences between wavelet scales in Python and Unity.")]
        public float modMaxCorrectionThreshold = 0f;

        [Tooltip("Path to file with recorded eye diameter data.")]
        public string inputFilePath = "Assets/LHIPA/pupil_diameter_signals.json";

        [Tooltip("Path to output logfile with LHIPA values.")]
        public string outputFilePath = "Assets/LHIPA/lhipa_results";

        [Tooltip("Print all intermediate results of each calculation on console.")]
        public bool consoleLog = true;

        private void Start()
        {
            // Use logfile of eye diameter arrays
            //TestLHIPAAndLog(true);

            // Generate new example eye pupil arrays
            //TestLHIPAAndLog(false);

            // Test a single eye diameter array
            //Debug.Log("LHIPA test result: " + (LHIPA.CalculateLHIPA(eyeDiametersExampleArray, durationInSeconds, modMaxCorrectionThreshold, consoleLog)));

            // Test with sinus array
            //Debug.Log("LHIPA test result: " + (LHIPA.CalculateLHIPA(GenerateSinusArray(256), durationInSeconds, modMaxCorrectionThreshold, consoleLog)));
        }

        // Load json-file with eye diameter input arrays
        private List<float[]> LoadTestArraysFromFile()
        {
            string jsonContent = File.ReadAllText(inputFilePath);
            return JsonConvert.DeserializeObject<List<float[]>>(jsonContent);
        }

        /// <summary>
        /// Generate 100 deterministic pupil eye diameter test arrays with increasing number of pupil size changes that can be used as input for LHIPA algorithm.
        /// </summary>
        /// <returns>
        /// List of float arrays with pupil diameters.
        /// </returns>
        public List<float[]> GenerateTestArrays()
        {
            System.Random random = new System.Random(42); // Seed for deterministic behavior
            List<float[]> testArrays = new List<float[]>();

            for (int i = 0; i < 100; i++)
            {
                int numPoints = 32 * 16;
                float[] array = new float[numPoints];
                float[] increasingPeriod = Enumerable.Range(0, numPoints)
                    .Select(x => 1.0f / ((x + 1) * (i + 1) / 100f + 1)).ToArray(); // Increasing min-max intervals
                float cumulativeSum = 0;

                for (int j = 0; j < numPoints; j++)
                {
                    cumulativeSum += increasingPeriod[j];
                    float baseFrequency = Mathf.Sin(cumulativeSum); // Sinusoidal with varying periods
                    float amplitudeVariation = (float)(0.8 + (random.NextDouble() * 0.4)); // Amplitude variation
                    float baseline = (float)(3.5 + (random.NextDouble() * 3.5)); // Baseline range
                    float noise = (float)(random.NextDouble() * 0.1 - 0.05); // Small noise
                    array[j] = Mathf.Round((baseline + amplitudeVariation * baseFrequency + noise) * 100) /
                               100; // Round to 2 decimals
                }

                testArrays.Add(array);
            }

            return testArrays;
        }

        /// <summary>
        /// Test CalculateLHIPA method with a set of eye diameter input arrays and log result into file defined in 'outputFilePath'.
        /// </summary>
        /// <param name="useInputFile">Set true to use input file defined in 'inputFilePath'. Set false for using 'GenerateTestArrays()' method to generate new input arrays on the fly.</param>
        /// <returns>
        /// Filename of LHIPA logfile.
        /// </returns>
        public string TestLHIPAAndLog(bool useInputFile)
        {
            List<float[]> testArrays = useInputFile ? LoadTestArraysFromFile() : GenerateTestArrays();

            string newPathFile = outputFilePath + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".csv";

            using (StreamWriter writer = new StreamWriter(newPathFile, true))
            {
                writer.WriteLine("LHIPA Result");

                for (int i = 0; i < testArrays.Count; i++)
                {
                    float[] testArray = testArrays[i];
                    float inputAverage = Mathf.Round(testArray.Average() * 100) / 100; // Round to 2 decimals
                    float lhipaResult =
                        Mathf.Round(LHIPA.CalculateLHIPA(testArray, durationInSeconds, modMaxCorrectionThreshold,
                            consoleLog) * 1000000) / 1000000; // Round to 6 decimals
                    writer.WriteLine($"{lhipaResult}");
                }
            }

            Debug.Log($"LHIPA results logged to {newPathFile}");

            return newPathFile;
        }

        /// <summary>
        /// Help Function: Generate Sinus Array for periodic testing.
        /// </summary>
        public float[] GenerateSinusArray(int numPoints)
        {
            float[] sinWave = new float[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                double x = 2 * Math.PI * i / numPoints; // x-Werte von 0 bis 2π
                sinWave[i] = (float)Math.Sin(x);
            }

            return sinWave;
        }

        [Tooltip("Pupil Diameter Example Input Array for fast testing.")]
        public float[] eyeDiametersExampleArray = new float[]
        {
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f,
            3.2f, 2.8f, 3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.6f,
            3.6f, 2.7f, 3.5f, 2.8f, 3.4f, 2.9f, 3.3f, 2.8f,
            3.2f, 2.9f, 3.1f, 3.0f, 3.2f, 2.8f, 3.3f, 2.7f,
            3.4f, 2.9f, 3.5f, 2.6f, 3.6f, 2.7f, 3.5f, 2.8f,
            3.4f, 2.9f, 3.3f, 2.8f, 3.2f, 2.9f, 3.1f, 3.0f,
            3.3f, 2.7f, 3.4f, 2.9f, 3.5f, 2.8f, 3.6f, 2.7f,
            3.5f, 2.9f, 3.4f, 3.0f, 3.3f, 2.8f, 3.2f, 2.9f,
            3.1f, 2.8f, 3.2f, 3.0f, 3.3f, 2.7f, 3.4f, 2.9f
        };
    }
}