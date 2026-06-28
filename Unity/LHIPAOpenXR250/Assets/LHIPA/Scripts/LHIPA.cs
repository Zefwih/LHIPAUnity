using System;
using System.Linq;
using UnityEngine;

namespace lhipa
{
    /// <summary>
    /// Calculate the Low/High Index of Pupillary Activity (LHIPA) from a signal of pupil diameters.
    /// The LHIPA is a wavelet-based measure of cognitive load derived from the ratio of low- and
    /// high-frequency bands of pupil-diameter oscillation. It is expected to DECREASE with increasing
    /// cognitive load (the reverse of the IPA).
    ///
    /// For more details on LHIPA, refer to:
    /// Duchowski, A. T., et al.
    /// "The Low/High Index of Pupillary Activity (LHIPA)".
    /// CHI Conference on Human Factors in Computing Systems, 2020.
    ///
    /// Implementation notes
    /// --------------------
    /// This is a faithful C# port of the paper's Python reference (Listing 1), which relies on
    /// PyWavelets:
    ///   cD_H = pywt.downcoef('d', d, 'sym16', 'per', level = 1)     // high-frequency detail band
    ///   cD_L = pywt.downcoef('d', d, 'sym16', 'per', level = lof)   // low-frequency detail band
    /// Both bands are *detail* (high-pass) coefficients of a Mallat (pyramidal) discrete wavelet
    /// transform with periodization ('per') boundary handling. To reproduce pywt exactly we implement
    /// the periodization DWT directly with the sym16 decomposition filters (see <see cref="DwtPerStep"/>),
    /// rather than going through a wavelet *packet* transform whose packet layout does not correspond to
    /// the Mallat detail sub-bands. The single-level convolution
    ///   out[i] = sum_k filt[k] * x[(2*i + L/2 - k) mod N]
    /// reproduces pywt.downcoef('a'/'d', x, 'sym16', 'per', level = 1) to 0.0 numerical error
    /// (verified against PyWavelets 1.8.0 for sym16, L = 32).
    ///
    /// Because the previous wavelet-packet implementation did not match pywt, it relied on empirical
    /// correction parameters; those are no longer needed and have been removed.
    /// </summary>
    public static class LHIPA
    {
        /// <summary>Length of the sym16 decomposition filter (mother wavelength).</summary>
        private const int FilterLength = 32;

        /// <summary>
        /// Minimum number of samples required for a meaningful multi-level decomposition
        /// (FilterLength * 8). Callers should guard against shorter inputs.
        /// </summary>
        public const int MinSamples = FilterLength * 8; // 256

        // sym16 decomposition filters, taken verbatim from pywt.Wavelet('sym16').dec_lo / .dec_hi
        // (PyWavelets 1.8.0). Used directly so the transform matches pywt bit-for-bit.
        private static readonly double[] DecLo =
        {
            6.230006701220761e-06, -3.113556407621969e-06, -0.00010943147929529757, 2.8078582128442894e-05,
            0.0008523547108047095, -0.0001084456223089688, -0.0038809122526038786, 0.0007182119788317892,
            0.012666731659857348, -0.0031265171722710075, -0.031051202843553064, 0.004869274404904607,
            0.032333091610663785, -0.06698304907021778, -0.034574228416972504, 0.39712293362064416,
            0.7565249878756971, 0.47534280601152273, -0.054040601387606135, -0.15959219218520598,
            0.03072113906330156, 0.07803785290341991, -0.0035102750683740089, -0.024952758046290123,
            0.001359844742484172, 0.0069377611308027096, -0.00022211647621176323, -0.0013387206066921965,
            3.656592483348223e-05, 0.00016545679579108483, -5.396483179315242e-06, -1.0797982104319795e-05
        };

        private static readonly double[] DecHi =
        {
            1.0797982104319795e-05, -5.396483179315242e-06, -0.00016545679579108483, 3.656592483348223e-05,
            0.0013387206066921965, -0.00022211647621176323, -0.0069377611308027096, 0.001359844742484172,
            0.024952758046290123, -0.0035102750683740089, -0.07803785290341991, 0.03072113906330156,
            0.15959219218520598, -0.054040601387606135, -0.47534280601152273, 0.7565249878756971,
            -0.39712293362064416, -0.034574228416972504, 0.06698304907021778, 0.032333091610663785,
            -0.004869274404904607, -0.031051202843553064, 0.0031265171722710075, 0.012666731659857348,
            -0.0007182119788317892, -0.0038809122526038786, 0.0001084456223089688, 0.0008523547108047095,
            -2.8078582128442894e-05, -0.00010943147929529757, 3.113556407621969e-06, 6.230006701220761e-06
        };

        /// <summary>
        /// Calculate the Low/High Index of Pupillary Activity (LHIPA) from a signal of pupil diameters.
        /// </summary>
        /// <param name="pupilData">Array of pupil diameters. Must contain at least <see cref="MinSamples"/> values.</param>
        /// <param name="durationInSeconds">Elapsed time in seconds between the first and last sample.</param>
        /// <param name="modMaxCorrectionThreshold">
        /// Optional small-magnitude cutoff for modulus-maxima detection (default 0 = paper behaviour).
        /// Values below the threshold are discarded; use a small value (e.g. 0.01) to suppress maxima that
        /// arise purely from numerical noise.
        /// </param>
        /// <param name="debugLog">Print all intermediate results to the console.</param>
        /// <returns>The LHIPA value (count of surviving modulus maxima per second).</returns>
        public static float CalculateLHIPA(float[] pupilData, float durationInSeconds,
            float modMaxCorrectionThreshold = 0f, bool debugLog = false)
        {
            if (pupilData == null || pupilData.Length == 0 || durationInSeconds <= 0)
                throw new ArgumentException(
                    "Incorrect input. Please supply a non-empty pupilData array and a positive durationInSeconds.");

            if (pupilData.Length < MinSamples)
                throw new InvalidOperationException(
                    "The pupil diameter input is too short for wavelet calculation. " +
                    "Please provide at least " + MinSamples + " pupil diameter values.");

            if (debugLog) Debug.Log($"Input Signal: [{string.Join(" | ", pupilData)}]");

            // Work in double precision, on the raw signal length (the reference calls pywt.downcoef
            // directly on the recorded samples). Odd lengths are handled inside DwtPerStep exactly as
            // pywt's periodization does, so no power-of-two padding is needed.
            double[] signal = pupilData.Select(x => (double)x).ToArray();

            // Decomposition levels, matching pywt.dwt_max_level(len, dec_len) = floor(log2(len/(dec_len-1))).
            int maxLevel = (int)Math.Floor(Math.Log(pupilData.Length / (double)(FilterLength - 1), 2.0));
            int hif = 1;
            int lof = Math.Max(1, maxLevel / 2);
            if (debugLog) Debug.Log($"maxLevel: {maxLevel}, hif: {hif}, lof: {lof}");

            // Detail (high-pass) coefficients at the two octaves, normalized by 1/sqrt(2^level) as in the paper.
            double[] cD_H = NormalizeByScale(DetailAtLevel(signal, hif), hif);
            double[] cD_L = NormalizeByScale(DetailAtLevel(signal, lof), lof);

            if (debugLog)
                Debug.Log($"cD_H (len {cD_H.Length}) avg {cD_H.Average()} | cD_L (len {cD_L.Length}) avg {cD_L.Average()}");

            // LF/HF ratio: iterate the shorter low band and index the longer high band by 2^(lof-hif)*i,
            // exactly as the reference: cD_LH[i] = cD_L[i] / cD_H[(2^lof / 2^hif) * i].
            int factor = 1 << (lof - hif);
            double[] ratio = new double[cD_L.Length];
            for (int i = 0; i < cD_L.Length; i++)
            {
                int highIndex = factor * i;
                ratio[i] = highIndex < cD_H.Length ? cD_L[i] / cD_H[highIndex] : 0.0;
            }

            if (debugLog) Debug.Log($"ratio length: {ratio.Length}, avg: {ratio.Average()}");

            // Modulus maxima of the ratio signal.
            double[] modulusMaxima = ModMax(ratio, modMaxCorrectionThreshold);

            // Universal threshold: lambda = std(modmax) * sqrt(2 * log2(n)).
            double lambdaUniv = StandardDeviation(modulusMaxima) *
                                Math.Sqrt(2.0 * Math.Log(modulusMaxima.Length, 2.0));

            // 'less' thresholding: keep values <= lambda, zero values above it.
            double[] thresholded = UniversalThreshold(modulusMaxima, lambdaUniv);

            // Count surviving maxima.
            int maximaCount = 0;
            foreach (double value in thresholded)
                if (Math.Abs(value) > 0) maximaCount++;

            // LHIPA = surviving maxima per second (reference: ctr / tt).
            float lhipa = (float)(maximaCount / (double)durationInSeconds);

            if (debugLog)
                Debug.Log($"maximaCount: {maximaCount}, duration: {durationInSeconds}, LHIPA: {lhipa}");

            return lhipa;
        }

        // ---------------------------------------------------------------------------------------------
        // Wavelet helpers
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// One level of the periodization ('per') discrete wavelet transform with the given filter,
        /// reproducing pywt.downcoef(..., 'per', level = 1):
        ///   out[i] = sum_k filt[k] * x[(2*i + L/2 - k) mod N],   i = 0 .. N/2 - 1.
        /// </summary>
        private static double[] DwtPerStep(double[] x, double[] filt)
        {
            int n = x.Length;
            // pywt periodization extends an odd-length signal by repeating its last sample (so the
            // output has ceil(n/2) coefficients), then convolves/downsamples periodically.
            if ((n & 1) == 1)
            {
                double[] padded = new double[n + 1];
                Array.Copy(x, padded, n);
                padded[n] = x[n - 1];
                x = padded;
                n++;
            }
            int half = n / 2;
            int offset = filt.Length / 2; // L/2 = 16 for sym16
            double[] outCoef = new double[half];
            for (int i = 0; i < half; i++)
            {
                double sum = 0.0;
                for (int k = 0; k < filt.Length; k++)
                {
                    int idx = ((2 * i + offset - k) % n + n) % n;
                    sum += filt[k] * x[idx];
                }
                outCoef[i] = sum;
            }
            return outCoef;
        }

        /// <summary>
        /// Detail (high-pass) coefficients at the requested level via a Mallat cascade: the approximation
        /// is filtered repeatedly and the detail of the final level is returned. Equivalent to
        /// pywt.downcoef('d', x, 'sym16', 'per', level).
        /// </summary>
        private static double[] DetailAtLevel(double[] x, int level)
        {
            double[] approximation = x;
            double[] detail = null;
            for (int l = 0; l < level; l++)
            {
                detail = DwtPerStep(approximation, DecHi);
                approximation = DwtPerStep(approximation, DecLo);
            }
            return detail;
        }

        // Normalize detail coefficients by 1/sqrt(2^level), as in the reference.
        private static double[] NormalizeByScale(double[] data, int level)
        {
            double factor = 1.0 / Math.Sqrt(Math.Pow(2.0, level));
            double[] normalized = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
                normalized[i] = data[i] * factor;
            return normalized;
        }

        // Detection of modulus maxima (faithful port of Duchowski et al.'s modmax helper).
        private static double[] ModMax(double[] d, double threshold)
        {
            int length = d.Length;
            double[] m = new double[length];
            double[] t = new double[length];

            for (int i = 0; i < length; i++)
                m[i] = Math.Abs(d[i]);

            for (int i = 0; i < length; i++)
            {
                double ll = i >= 1 ? m[i - 1] : m[i];          // left neighbour (self for first element)
                double oo = m[i];                               // current
                double rr = i < length - 1 ? m[i + 1] : m[i];  // right neighbour (self for last element)

                if (ll <= oo && oo >= rr && (ll < oo || oo > rr))
                {
                    double mag = Math.Sqrt(d[i] * d[i]); // |d[i]|
                    t[i] = mag < threshold ? 0.0 : mag;  // optional small-noise cutoff
                }
                else
                {
                    t[i] = 0.0;
                }
            }

            return t;
        }

        // Population standard deviation (matches numpy.std default), including the zero entries.
        private static double StandardDeviation(double[] data)
        {
            double mean = data.Average();
            double sumSquaredDiffs = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                double diff = data[i] - mean;
                sumSquaredDiffs += diff * diff;
            }
            return Math.Sqrt(sumSquaredDiffs / data.Length);
        }

        // 'less' thresholding (pywt mode = "less"): keep values with |x| <= threshold, zero the rest.
        private static double[] UniversalThreshold(double[] data, double threshold)
        {
            double[] result = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = Math.Abs(data[i]) > threshold ? 0.0 : data[i];
            return result;
        }

#if LHIPA_TEST
        /// <summary>
        /// Test-only seam (compiled only when LHIPA_TEST is defined): exposes the normalized cD_H / cD_L
        /// detail bands so they can be compared against pywt.downcoef. Not part of the public API.
        /// </summary>
        public static void ComputeBandsForTest(float[] pupilData, out double[] cdH, out double[] cdL)
        {
            double[] signal = pupilData.Select(x => (double)x).ToArray();
            int maxLevel = (int)Math.Floor(Math.Log(pupilData.Length / (double)(FilterLength - 1), 2.0));
            int hif = 1;
            int lof = Math.Max(1, maxLevel / 2);
            cdH = NormalizeByScale(DetailAtLevel(signal, hif), hif);
            cdL = NormalizeByScale(DetailAtLevel(signal, lof), lof);
        }
#endif
    }
}
