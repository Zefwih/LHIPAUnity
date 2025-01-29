using System;
using System.Linq;
using UnityEngine;
using SharpWave;

/// <summary>
/// Calculate the Low-to-High Index of the pupil amplitude (LHIPA) based on signal of pupil diameters.
/// The LHIPA is a measure of cognitive load based on analyzing high-frequency changes in pupil diameter irrespective of sampling rate.
/// It is calculated by determining the power in a high-frequency band of the pupil signal using Symlet16-Wavelet transformation.
///
/// Contribution and legal rights follow here...
/// 
/// </summary>
public static class LHIPA
{
    /*
     * So far best correction approach: Use of Threshold for ModMax-Function
     * Used to cut off too small modulus maximus values that are produced by rounding and calculation inaccuracies.
     * Use this correction parameter to achieve LHIPA values close to Python results
     * and to compensate for the differences between wavelet scales in PyWavelet and SharpWave.
     *
     * The modMaxThreshold cuts off modulus maxima values that are too small and should not be recognized in LHIPA calculation.
     * 
     * First tests showed best results with a small value between 0.01 and 0.1:
     * 
        Regression line equation when using modMaxCorrectionThreshold = 0:
        Regression line equation
        Ŷ = 0.04307 + 0.1361X
        Reporting linear regression in APA style
        Unity predicted Python, R2 = .31, F(1,98) = 43.08, p < .001.
        β = .14, p < .001, α = 0.043, p < .001.
        
        Regression line equation when using modMaxCorrectionThreshold = 0.05:
        Regression line equation
        Ŷ = 0.06454 + 0.09607X
        Reporting linear regression in APA style
        Unity predicted Python, R2 = .056, F(1,98) = 5.86, p = .017.
        β = .096, p = .017, α = 0.065, p < .001.

        Regression line equation when using modMaxCorrectionThreshold = 0.1:
        Regression line equation
        Ŷ = 0.06546 + 0.09694X
        Reporting linear regression in APA style
        Unity predicted Python, R2 = .049, F(1,98) = 5.06, p = .027.
        β = .097, p = .027, α = 0.065, p < .001.
     */
    // static float modMaxCorrectionThreshold = 0.1f; //-> moved to method parameter
    
    
    /*
     Alternative correction approach with less reliable results:
     Use correction parameters to correct the complete LHIPA score.
     
     Relationship between Python and Unity values:
     Unity≈2.94⋅Python+0.138
     */
    
    /*
     * Test Results: Differences between Python and Unity Values
     * Multiplicative Relationship:
     * Test Result: 2.9380320871257264
     * SD: 3.948704764022458
     */
    static float multiplicativeCorrectionParameter = 0;
    
    /*
     * Test Results: Differences between Python and Unity Values
     * Additive Relationship:
     * Test Result: 0.13813145
     * SD: 0.212177
     */
    static float additiveCorrectionParameter = 0f;

    
    /// <summary>
    /// Calculate the Low-to-High Index of the pupil amplitude (LHIPA) based on signal of pupil diameters.
    /// </summary>
    /// <param name="pupilData">Array of pupil diameters in float format. Array must contain at least 32*8 values.</param>
    /// <param name="durationInSeconds">Time in seconds between first and last pupil data array element.</param>
    /// <param name="modMaxCorrectionThreshold">Modulus maximus correction threshold to compensate for the differences between wavelet scales in Python and Unity.</param>
    /// <param name="debugLog">Print all intermediate results of each calculation on console.</param>
    /// <returns>
    /// LHIPA as float value between 0 and 1
    /// </returns>
    public static float CalculateLHIPA(float[] pupilData, float durationInSeconds, float modMaxCorrectionThreshold = 0f, bool debugLog = false)
    {
        if (pupilData == null || pupilData.Length == 0 || durationInSeconds <= 0)
            throw new ArgumentException("Incorrect input. Please check if you submitted a correct pupilData array and a non negative durationInSeconds.");
        
        if(debugLog) Debug.Log( $"Input Signal: [{string.Join(" | ", pupilData)}]");
        
        // 1. Wavelet Decomposition (Low- und High-Frequency Bands)
        
        // SharpWave Initialization
        SharpWave.Wavelet symlet16 = new Symlet16();

        // Check if the input array has the correct length
        if (pupilData.Length < symlet16.MOTHERWAVELENGTH * 8)
        {
            throw new InvalidOperationException("The pupil diameter input is too short for Wavelet Calculation. " +
                                                "Please provide a pupil data array with at least " 
                                                + (symlet16.MOTHERWAVELENGTH * 8) +" pupil diameter values!");
        }

        // Extend signal: periodic boundary condition + power of 2
        double[] extendedSignal = ExtendSignalToBinaryWithPeriodic(pupilData.Select(x => (double)x).ToArray());
        
        // Maximum possible decomposition depth
        int maxLevel = (int)Math.Log(pupilData.Length / symlet16.MOTHERWAVELENGTH, 2);
        if(debugLog) Debug.Log( "max decomposition level: " + maxLevel);

        // Define High- und Low-Frequency-Level
        int hif = 1;
        int lof = maxLevel / 2;
        if(debugLog) Debug.Log( "lof level: " + lof);

        // Total Decomposition with extended Signal
        WaveletPacketTransform waveletTransform = new WaveletPacketTransform(symlet16);
        double[][] decompositionMatrix = waveletTransform.decompose(extendedSignal);

        // High-Frequency-Band (hif)
        double[] cD_H = ExtractHighPassCoefficients(decompositionMatrix[hif], hif);

        // Low-Frequency-Band (lof)
        double[] cD_L = ExtractLowPassCoefficients(decompositionMatrix[lof], lof);

        // Convert results back zu float[]
        float[] highFreq = cD_H.Select(x => (float)x).ToArray();
        float[] lowFreq = cD_L.Select(x => (float)x).ToArray();

        //ArrayLogger.LogArrayToFile(highFreq, "Assets/high_frequency_wavelet_unity.csv");
        //ArrayLogger.LogArrayToFile(lowFreq, "Assets/low_frequency_wavelet_unity.csv");

        if(debugLog) Debug.Log( "1. Detail coefficients average high - low: " + highFreq.Average() + " - " + lowFreq.Average());
        if(debugLog) Debug.Log( $"highFreq: [{string.Join(" | ", highFreq)}]");
        if(debugLog) Debug.Log( $"lowFreq: [{string.Join(" | ", lowFreq)}]");

        // 2. Normalization der Coefficients
        int hfLevel = 1, lfLevel = maxLevel / 2;
        float[] normalizedHigh = NormalizeByScale(highFreq, hfLevel);
        float[] normalizedLow = NormalizeByScale(lowFreq, lfLevel);
        
        if(debugLog) Debug.Log( "2. Normalized coefficients data average high - low: " + normalizedHigh.Average() + " - " + normalizedLow.Average());
        if(debugLog) Debug.Log( $"normalizedHigh: [{string.Join(" | ", normalizedHigh)}]");
        if(debugLog) Debug.Log( $"normalizedLow: [{string.Join(" | ", normalizedLow)}]");

        // 3. Calculation of LF/HF-Ratio
        int ratioLength = Math.Min(normalizedLow.Length, normalizedHigh.Length * (int)Mathf.Pow(2, lfLevel - hfLevel));
        float[] ratio = new float[ratioLength];

        float scaleFactor = Mathf.Pow(2, lfLevel - hfLevel); // (2^lof) / (2^hif)
        for (int i = 0; i < ratioLength; i++)
        {
            int highIndex = (int)(i / scaleFactor);
            if (highIndex < normalizedHigh.Length)
            {
                ratio[i] = normalizedLow[i] / (normalizedHigh[highIndex] + 1e-6f); // +1e-6 to prevent division with 0
            }
            else
            {
                ratio[i] = 0; // fill the rest of the array
            }
        }
        
        if(debugLog) Debug.Log( "3. ratio length: " + ratioLength + " - ratio average: " + ratio.Average());
        if(debugLog) Debug.Log( $"[{string.Join(" | ", ratio)}]");

        // 4. Modulus-Maxima-Detection
        float[] modulusMaxima = ModMax(ratio, modMaxCorrectionThreshold);
        
        if(debugLog) Debug.Log( "4. modulus maxima length: " + modulusMaxima.Length + " - modulus maxima average: " + modulusMaxima.Average());
        if(debugLog) Debug.Log( $"[{string.Join(" | ", modulusMaxima)}]");

        // 5. Calculation of Threshold (universal Threshold)
        float lambdaUniv = Mathf.Sqrt(2.0f * Mathf.Log(modulusMaxima.Length) / Mathf.Log(2.0f)) * StandardDeviation(modulusMaxima);
        
        if(debugLog) Debug.Log( "5. lambdaUniv " + lambdaUniv);

        // 6. Use Threshold ("less"-mode)
        float[] thresholded = UniversalThreshold(modulusMaxima, lambdaUniv);
        
        if(debugLog) Debug.Log( "6. Universal thresholded length: " + thresholded.Length + " - Universal thresholded average: " + thresholded.Average());
        if(debugLog) Debug.Log( $"[{string.Join(" | ", thresholded)}]");
        
        // 7. LHIPA Calculation
        
        // Sampling rate calculation
        float samplingRate = pupilData.Length / durationInSeconds;
        
        // Count Modulus Maxima
        int maximaCount = 0;
        foreach (float value in thresholded)
        {
            if (Mathf.Abs(value) > 0)
            {
                maximaCount++;
            }
        }

        if(debugLog) Debug.Log( "7. maximaCount: " + maximaCount + ", Duration: " + durationInSeconds+ ", Sampling Rate: " + samplingRate);
        
        // LHIPA = Number of Maxima per second depended on sampling rate
        
        float lhipa = (maximaCount / durationInSeconds) / samplingRate;

        if(debugLog) Debug.Log( "LHIPA: " + lhipa);
        
        // 8. LHIPA-Correction:
        lhipa = multiplicativeCorrectionParameter != 0 ? 
            lhipa / multiplicativeCorrectionParameter - (lhipa > additiveCorrectionParameter ? additiveCorrectionParameter : 0) 
            : lhipa - (lhipa > additiveCorrectionParameter ? additiveCorrectionParameter : 0);
        
        if(debugLog) Debug.Log( "Corrected LHIPA: " + lhipa);
        
        return lhipa;
    }

    // Symlet Wavelet Helper Methods:

    // Combined extension for periodic boundary conditions and power of 2
    private static double[] ExtendSignalToBinaryWithPeriodic(double[] signal)
    {
        int signalLength = signal.Length;

        // Target: Next power of 2
        int targetLength = NextPowerOfTwo(signalLength);

        // Additional length for periodic extension
        int extensionLength = (targetLength - signalLength) / 2;

        // Create array with new length
        double[] extendedSignal = new double[targetLength];

        // Left border (copy the last `extensionLength` values of signal)
        for (int i = 0; i < extensionLength; i++)
        {
            extendedSignal[i] = signal[signalLength - extensionLength + i];
        }

        // Main signal
        Array.Copy(signal, 0, extendedSignal, extensionLength, signalLength);

        // Right border (copy of first `extensionLength` values of signal)
        for (int i = 0; i < extensionLength; i++)
        {
            extendedSignal[extensionLength + signalLength + i] = signal[i];
        }

        return extendedSignal;
    }

    /*
    // When using forward method instead of decompose method in SharpWave use these methods:
    // Help function for extraction of High-Pass-Coefficients decomposition values
    private static double[] ExtractHighPassCoefficients(double[] transformed, int level)
    {
        int offset = transformed.Length / (int)Math.Pow(2, level);
        int length = offset;
        double[] coefficients = new double[length];
        Array.Copy(transformed, 0, coefficients, 0, length); // High-Pass on beginning of signal
        return coefficients;
    }

    // Help function for extraction of Low-Pass-Coefficients decomposition values
    private static double[] ExtractLowPassCoefficients(double[] transformed, int level)
    {
        int offset = transformed.Length / (int)Math.Pow(2, level);
        int length = offset;
        double[] coefficients = new double[length];
        Array.Copy(transformed, offset, coefficients, 0, length); // Low-Pass at end of signal
        return coefficients;
    }
    */

    // Help function for extraction of High-Pass-Coefficients decomposition values
    private static double[] ExtractHighPassCoefficients(double[] levelData, int level)
    {
        int length = levelData.Length / 2;
        double[] coefficients = new double[length];
        Array.Copy(levelData, 0, coefficients, 0, length); // First half of signal
        return coefficients;
    }
    
    // Help function for extraction of Low-Pass-Coefficients decomposition values
    private static double[] ExtractLowPassCoefficients(double[] levelData, int level)
    {
        int length = levelData.Length / 2;
        double[] coefficients = new double[length];
        Array.Copy(levelData, length, coefficients, 0, length); // Second half of signal
        return coefficients;
    }

    // Normalization of Values
    private static void Normalize(float[] data, float factor)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] /= factor;
        }
    }

    // Calculation of LF/HF-Ratio
    private static float[] ComputeRatio(float[] cD_L, float[] cD_H)
    {
        int size = Mathf.Min(cD_L.Length, cD_H.Length);
        float[] result = new float[size];
        for (int i = 0; i < size; i++)
        {
            result[i] = cD_H[i] == 0 ? 0 : cD_L[i] / cD_H[i];
        }
        return result;
    }

    // Detection of Modulus-Maxima
    private static float[] ModMax(float[] d, float threshold)
    {
        int length = d.Length;
        float[] m = new float[length];
        float[] t = new float[length];

        // 1. Compute signal modulus
        for (int i = 0; i < length; i++)
        {
            m[i] = Math.Abs(d[i]);
        }

        // 2. Identify local maxima
        for (int i = 0; i < length; i++)
        {
            float ll = (i >= 1) ? m[i - 1] : m[i]; // Left neighbor or self for the first element
            float oo = m[i];                      // Current value
            float rr = (i < length - 1) ? m[i + 1] : m[i]; // Right neighbor or self for the last element

            // Check for local maximum
            if ((ll <= oo && oo >= rr) && (ll < oo || oo > rr))
            {
                // Compute magnitude and check against threshold
                float mag = (float)Math.Sqrt(d[i] * d[i]);

                // If the magnitude is below the threshold, set it to 0
                if (mag < threshold)
                {
                    t[i] = 0.0f;
                }
                else
                {
                    t[i] = mag;
                }
            }
            else
            {
                t[i] = 0.0f;
            }
        }

        return t;
    }
    
    // Calculation of standard deviation
    private static float StandardDeviation(float[] data)
    {
        float mean = data.Average();
        float sumSquaredDiffs = data.Sum(x => (x - mean) * (x - mean));
        return Mathf.Sqrt(sumSquaredDiffs / data.Length);
    }
    
    // Apply Universal Thresholding
    private static float[] UniversalThreshold(float[] data, float threshold)
    {
        float[] result = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = Mathf.Abs(data[i]) > threshold ? 0 : data[i];
        }
        return result;
    }
    
    // Calculate the next power of two value for number
    private static int NextPowerOfTwo(int n)
    {
        if (n <= 0)
            throw new ArgumentException("Input must be a positive integer.");
        return (int)Math.Pow(2, Math.Ceiling(Math.Log(n, 2)));
    }
    
    // Helper method: Normalization of float values
    private static float[] NormalizeByScale(float[] data, int level)
    {
        // Normalization Factor: 1 / sqrt(2^level)
        float normalizationFactor = 1f / Mathf.Sqrt(Mathf.Pow(2, level));

        // New array with normalized values
        float[] normalizedData = new float[data.Length];

        // Scale each value
        for (int i = 0; i < data.Length; i++)
        {
            normalizedData[i] = data[i] * normalizationFactor;
        }

        return normalizedData;
    }
}
