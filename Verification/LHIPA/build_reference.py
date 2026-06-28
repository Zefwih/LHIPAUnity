"""
Step 2: Build the validated reference.
 - lhipa_reference(): the paper's exact pywt implementation (Listing 1).
 - lhipa_newmodel(): the algorithm to be ported to C# (validated DWT via conv_per cascade).
 - Confirm lhipa_newmodel == lhipa_reference (the C# port target).
 - Generate the shared test battery -> signals.csv, and reference outputs -> reference.json.
"""
import numpy as np, pywt, math, json, os

HERE = os.path.dirname(os.path.abspath(__file__))
w = pywt.Wavelet('sym16')
DEC_LO = np.array(w.dec_lo)
DEC_HI = np.array(w.dec_hi)
L = len(DEC_LO)            # 32
OFFSET = L // 2           # 16

# ---------- validated periodization single-step DWT (matches pywt mode='per') ----------
def dwt_per_step(x, filt):
    x = np.asarray(x, dtype=float)
    if len(x) % 2 == 1:                 # pywt periodization: extend odd signal by its last sample
        x = np.append(x, x[-1])
    N = len(x); half = N // 2
    out = np.zeros(half)
    for i in range(half):
        s = 0.0
        for k in range(L):
            idx = ((2 * i + OFFSET - k) % N + N) % N
            s += filt[k] * x[idx]
        out[i] = s
    return out

def detail_at_level(x, level):
    """level-`level` detail coeffs, Mallat cascade on the approximation, periodization."""
    cA = np.asarray(x, dtype=float)
    cD = None
    for _ in range(level):
        cD = dwt_per_step(cA, DEC_HI)
        cA = dwt_per_step(cA, DEC_LO)
    return cD

# sanity: cascade reproduces pywt.downcoef('d', level) exactly
def validate_cascade():
    rng = np.random.default_rng(7)
    # include non-power-of-two lengths (with odd intermediate levels) to exercise periodization
    for N in (512, 1024, 2048, 500, 1500, 2700, 5001):
        x = rng.standard_normal(N)
        for lvl in (1, 2, 3):
            mine = detail_at_level(x, lvl)
            ref = pywt.downcoef('d', x, 'sym16', 'per', level=lvl)
            err = np.max(np.abs(mine - ref))
            assert err < 1e-9, (N, lvl, err)
    print("cascade detail_at_level matches pywt.downcoef to <1e-9 for levels 1-3 "
          "(incl. non-power-of-two lengths)  OK")

# ---------- modmax / threshold (faithful to the paper's helper & C# port) ----------
def modmax(d):
    d = np.asarray(d, dtype=float)
    m = np.abs(d); n = len(d); t = np.zeros(n)
    for i in range(n):
        ll = m[i-1] if i >= 1 else m[i]
        oo = m[i]
        rr = m[i+1] if i < n-1 else m[i]
        if (ll <= oo and oo >= rr) and (ll < oo or oo > rr):
            t[i] = math.sqrt(d[i]*d[i])
        else:
            t[i] = 0.0
    return t

# ---------- the paper's reference (pywt downcoef) ----------
def lhipa_reference(d, tt):
    maxlevel = pywt.dwt_max_level(len(d), filter_len=w.dec_len)
    hif, lof = 1, int(maxlevel/2)
    cD_H = pywt.downcoef('d', d, 'sym16', 'per', level=hif).astype(float)
    cD_L = pywt.downcoef('d', d, 'sym16', 'per', level=lof).astype(float)
    cD_H = cD_H / math.sqrt(2**hif)
    cD_L = cD_L / math.sqrt(2**lof)
    factor = (2**lof)//(2**hif)
    cD_LH = np.zeros(len(cD_L))
    for i in range(len(cD_L)):
        cD_LH[i] = cD_L[i] / cD_H[factor*i]
    cD_LHm = modmax(cD_LH)
    lam = np.std(cD_LHm) * math.sqrt(2.0*np.log2(len(cD_LHm)))
    cD_LHt = pywt.threshold(cD_LHm, lam, mode='less')
    ctr = int(np.sum(np.abs(cD_LHt) > 0))
    return float(ctr)/tt, cD_H, cD_L, lof

# ---------- the model to port to C# (uses validated cascade instead of pywt) ----------
def lhipa_newmodel(d, tt):
    N = len(d)
    maxlevel = int(math.floor(math.log2(N/(w.dec_len-1))))   # == pywt.dwt_max_level
    hif, lof = 1, maxlevel//2
    cD_H = detail_at_level(d, hif) / math.sqrt(2**hif)
    cD_L = detail_at_level(d, lof) / math.sqrt(2**lof)
    factor = (2**lof)//(2**hif)
    cD_LH = np.zeros(len(cD_L))
    for i in range(len(cD_L)):
        cD_LH[i] = cD_L[i] / cD_H[factor*i]
    cD_LHm = modmax(cD_LH)
    lam = np.std(cD_LHm) * math.sqrt(2.0*np.log2(len(cD_LHm)))
    # pywt 'less': keep where data < lam else 0 (modmax output is non-negative)
    cD_LHt = np.where(cD_LHm < lam, cD_LHm, 0.0)
    ctr = int(np.sum(np.abs(cD_LHt) > 0))
    return float(ctr)/tt, cD_H, cD_L

# ---------- deterministic test battery ----------
def make_signals():
    sig = {}
    rng = np.random.default_rng(123)
    # Mix power-of-two AND realistic non-power-of-two lengths (real pupil recordings are never 2^k):
    # 2700 = 30 s @ 90 Hz, 5001 = ~10 s @ 500 Hz, etc. These exercise the odd-length periodization path.
    for N in (512, 1024, 2048, 500, 1500, 2700, 5001):
        t = np.arange(N)
        base = 3.5
        sig[f"sine_lowfreq_{N}"]  = base + 0.3*np.sin(2*np.pi*t/64)
        sig[f"sine_highfreq_{N}"] = base + 0.1*np.sin(2*np.pi*t/6)
        sig[f"mixed_{N}"]         = base + 0.2*np.sin(2*np.pi*t/64) + 0.05*np.sin(2*np.pi*t/5)
        sig[f"noisy_{N}"]         = base + 0.15*np.sin(2*np.pi*t/40) + 0.04*rng.standard_normal(N)
        sig[f"ramp_osc_{N}"]      = base + 0.001*t + 0.08*np.sin(2*np.pi*t/12)
    return sig

def main():
    validate_cascade()
    signals = make_signals()
    SR = 256.0  # samples/second -> tt = N/SR

    # confirm new model reproduces the paper reference exactly on the battery
    max_lhipa_diff = 0.0
    for name, s in signals.items():
        tt = len(s)/SR
        r_ref, hH, hL, lof = lhipa_reference(s, tt)
        r_new, nH, nL = lhipa_newmodel(s, tt)
        max_lhipa_diff = max(max_lhipa_diff, abs(r_ref-r_new))
        assert np.max(np.abs(hH-nH)) < 1e-9 and np.max(np.abs(hL-nL)) < 1e-9, name
    print(f"new-model vs paper-reference: identical bands; max LHIPA diff = {max_lhipa_diff:.3e}  ✔")

    # write shared inputs for the C# harness
    with open(os.path.join(HERE,"signals.csv"),"w") as f:
        for name, s in signals.items():
            f.write(name + "," + str(SR) + "," + ",".join(repr(float(v)) for v in s) + "\n")

    ref = {}
    for name, s in signals.items():
        tt = len(s)/SR
        val, cH, cL, lof = lhipa_reference(s, tt)
        ref[name] = {"tt": tt, "lof": lof, "lhipa_ref": val,
                     "cD_H": [float(v) for v in cH], "cD_L": [float(v) for v in cL]}
    with open(os.path.join(HERE,"reference.json"),"w") as f:
        json.dump(ref, f)
    print(f"wrote signals.csv and reference.json ({len(signals)} signals)")

if __name__ == "__main__":
    main()
