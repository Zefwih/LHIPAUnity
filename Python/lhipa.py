import math
import pywt
import numpy as np

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

    # Initialize wavelet
    wavelet = pywt.Wavelet('sym16')
    
    # Get Python Filter Coefficients
    max_level = pywt.dwt_max_level(len(d), filter_len=wavelet.dec_len)

    # Set high and low frequency band indices
    hif, lof = 1, max_level // 2

    # Get detail coefficients
    cD_H = pywt.downcoef('d', d, 'sym16', 'per', level=hif)
    cD_L = pywt.downcoef('d', d, 'sym16', 'per', level=lof)

    # Normalize coefficients
    cD_H = [x / math.sqrt(2**hif) for x in cD_H]
    cD_L = [x / math.sqrt(2**lof) for x in cD_L]
    
    # Compute LH:HF ratio
    cD_LH = []
    for i in range(len(cD_L)):
        hf_index = int(((2**lof) / (2**hif)) * i)
        if hf_index < len(cD_H):
            cD_LH.append(cD_L[i] / cD_H[hf_index])
        else:
            cD_LH.append(0.0)

    # Detect modulus maxima
    cD_LHm = modmax(cD_LH)

    # Thresholding using universal threshold
    λuniv = np.std(cD_LHm) * math.sqrt(2.0 * np.log2(len(cD_LHm)))
    cD_LHt = pywt.threshold(cD_LHm, λuniv, mode='less')
    
    # Calculating Sampling Rate
    sampling_rate = len(d)/duration_in_seconds
    
    # Number of Modulus-Maxima
    ctr = sum(1 for x in cD_LHt if math.fabs(x) > 0)
    
    # Compute LHIPA
    LHIPA = (float(ctr) / duration_in_seconds) / sampling_rate

    return LHIPA
