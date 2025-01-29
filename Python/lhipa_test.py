import math
import pywt
import numpy as np
import csv
import os
import json

def modmax(d):
    """
    Find local maxima of signal `d` based on Modulus-Maxima-Method.
    """
    # Compute signal modulus
    m = [math.fabs(val) for val in d]

    # Find local maxima
    t = [0.0] * len(d)
    for i in range(len(d)):
        ll = m[i - 1] if i >= 1 else m[i]
        oo = m[i]
        rr = m[i + 1] if i < len(d) - 1 else m[i]

        if (ll <= oo and oo >= rr) and (ll < oo or oo > rr):
            # Compute magnitude
            t[i] = math.sqrt(d[i]**2)
        else:
            t[i] = 0.0

    print(f"Debug (modmax): Modulus maxima = {t}")
    return t

def lhipa(d, duration_in_seconds=1.0):
    """
    Calculate the Low-to-High Index of the pupil amplitude (LHIPA) based on signal `d`.
    """
    print(f"Debug (lhipa): Input Signal = {d}")
    print(f"Debug (lhipa): Sampling Rate = {duration_in_seconds}")

    # Initialize wavelet
    wavelet = pywt.Wavelet('sym16')
    
    # Get Python Filter Coefficients
    print(f"LowPass Coefficients = {pywt.Wavelet('sym16').dec_lo}")
    print(f"HighPass Coefficients = {pywt.Wavelet('sym16').dec_hi}")
    
    max_level = pywt.dwt_max_level(len(d), filter_len=wavelet.dec_len)
    print(f"Debug (lhipa): Max Decomposition Level = {max_level}")

    # Set high and low frequency band indices
    hif, lof = 1, max_level // 2
    print(f"Debug (lhipa): High-Frequency Level = {hif}, Low-Frequency Level = {lof}")

    # Get detail coefficients
    cD_H = pywt.downcoef('d', d, 'sym16', 'per', level=hif)
    cD_L = pywt.downcoef('d', d, 'sym16', 'per', level=lof)
    print(f"Debug (lhipa): cD_H = {cD_H}")
    print(f"Debug (lhipa): cD_L = {cD_L}")

    # Normalize coefficients
    cD_H = [x / math.sqrt(2**hif) for x in cD_H]
    cD_L = [x / math.sqrt(2**lof) for x in cD_L]
    print(f"Debug (lhipa): Normalized cD_H = {cD_H}")
    print(f"Debug (lhipa): Normalized cD_L = {cD_L}")

    # Compute LH:HF ratio
    cD_LH = []
    for i in range(len(cD_L)):
        hf_index = int(((2**lof) / (2**hif)) * i)
        if hf_index < len(cD_H):
            cD_LH.append(cD_L[i] / cD_H[hf_index])
        else:
            cD_LH.append(0.0)
    print(f"Debug (lhipa): cD_LH = {cD_LH}")

    # Detect modulus maxima
    cD_LHm = modmax(cD_LH)

    # Thresholding using universal threshold
    λuniv = np.std(cD_LHm) * math.sqrt(2.0 * np.log2(len(cD_LHm)))
    cD_LHt = pywt.threshold(cD_LHm, λuniv, mode='less')
    print(f"Debug (lhipa): Thresholded cD_LH = {cD_LHt}")
    
    # Calculating Sampling Rate
    sampling_rate = len(d)/duration_in_seconds
    
    # Number of Modulus-Maxima
    ctr = sum(1 for x in cD_LHt if math.fabs(x) > 0)
    
    print(f"Debug (lhipa): Maxima Count = {ctr} , Signal Duration = {duration_in_seconds} seconds , Sampling Rate = {sampling_rate}")
    
    # Compute LHIPA
    LHIPA = (float(ctr) / duration_in_seconds) / sampling_rate
    print(f"Debug (lhipa): LHIPA = {LHIPA}")

    return LHIPA


# Deterministic array generator function
def generate_test_arrays():
    np.random.seed(42)  # Ensure deterministic behavior
    test_arrays = []
    for i in range(100):
        num_points = 32 * 16
        increasing_period = np.linspace(1, 50, num_points) * (i + 1) / 100  # Increasing min-max intervals
        base_frequency = np.sin(np.cumsum(1 / increasing_period))  # Sinusoidal with varying periods
        amplitude_variation = np.random.uniform(0.8, 1.2)  # Amplitude variation
        baseline = np.random.uniform(3.5, 7.0)  # Baseline range typical for pupil diameter
        noise = np.random.uniform(-0.05, 0.05, num_points)  # Small noise
        array = (baseline + amplitude_variation * base_frequency + noise).round(2)
        test_arrays.append(array)
    return test_arrays

# Call LHIPA function for each test array and log results
def test_lhipa_and_log_to_csv(lhipa_function, duration_in_seconds=1.0, output_file="lhipa_results.csv"):
    test_arrays = generate_test_arrays()
    with open(output_file, mode='w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(["Index", "Input Array Average", "LHIPA Result"])
        
        for index, test_array in enumerate(test_arrays, start=1):
            input_avg = round(np.mean(test_array), 2)  # Round to 2 decimal places
            lhipa_result = round(lhipa_function(test_array, duration_in_seconds=duration_in_seconds), 6)  # Round to 6 decimals
            writer.writerow([index, input_avg, lhipa_result])

# Call LHIPA function for each test array and log results
def test_lhipa_and_log_to_csv_from_file(lhipa_function, duration_in_seconds=1.0, input_file="pupil_diameter_signals.json", output_file="lhipa_results.csv"):
    # Load input data of JSON file
    with open(input_file, 'r') as file:
        test_arrays = json.load(file)
    
    with open(output_file, mode='w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(["LHIPA Result"])
        
        for index, test_array in enumerate(test_arrays, start=1):
            input_avg = round(np.mean(test_array), 2)  # Round to 2 decimal places
            lhipa_result = round(lhipa_function(test_array, duration_in_seconds=duration_in_seconds), 6)  # Round to 6 decimals
            writer.writerow([lhipa_result])
    
    print(f"LHIPA results logged to {output_file}")

def main():
    # Setting simulated duration
    duration_in_seconds = 1.0
    
    # Calculate LHIPA values based on input file with pupil diameter arrays and log them
    test_lhipa_and_log_to_csv_from_file(lhipa)
    
    # Generate example pupil diameter arrays, calculate LHIPA values and log them
    #test_lhipa_and_log_to_csv(lhipa)
    
    # Generate a sinus wave with 256 date points and calculate LHIPA
    #num_points = 256
    #x = np.linspace(0, 2 * np.pi, num_points)  # x-Werte von 0 bis 2π
    #sin_wave = np.sin(x)
    #lhipa_result = lhipa(sin_wave, duration_in_seconds)
    #print(f"LHIPA Result: {lhipa_result}")
    
    # Calculate LHIPA with Test Array (example array with simulated pupil diameters, equal to exampe data in unity)
    eye_diameters = [
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9,
        3.2, 2.8, 3.3, 2.7, 3.4, 2.9, 3.5, 2.6,
        3.6, 2.7, 3.5, 2.8, 3.4, 2.9, 3.3, 2.8,
        3.2, 2.9, 3.1, 3.0, 3.2, 2.8, 3.3, 2.7,
        3.4, 2.9, 3.5, 2.6, 3.6, 2.7, 3.5, 2.8,
        3.4, 2.9, 3.3, 2.8, 3.2, 2.9, 3.1, 3.0,
        3.3, 2.7, 3.4, 2.9, 3.5, 2.8, 3.6, 2.7,
        3.5, 2.9, 3.4, 3.0, 3.3, 2.8, 3.2, 2.9,
        3.1, 2.8, 3.2, 3.0, 3.3, 2.7, 3.4, 2.9
    ]
    #lhipa_result = lhipa(eye_diameters, duration_in_seconds)
    #print(f"LHIPA Result: {lhipa_result}")
    

if __name__ == "__main__":
    main()
