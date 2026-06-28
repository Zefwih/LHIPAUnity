# LHIPA verification harness

This folder verifies that the Unity C# implementation
(`Unity/LHIPAOpenXR250/Assets/LHIPA/Scripts/LHIPA.cs`) reproduces the
**CHI 2020 reference** implementation of the LHIPA (Duchowski et al., 2020,
Listing 1), which is based on [PyWavelets](https://pywavelets.readthedocs.io/).

The check compiles the **real** `LHIPA.cs` outside Unity (via a tiny `UnityEngine`
shim) and compares its output, on an identical battery of signals, against the
`pywt` reference computed in Python.

## How to run

```bash
# Requirements: Python 3 + numpy + PyWavelets, and a Roslyn csc.exe (Visual Studio / Build Tools)
pip install numpy PyWavelets
bash run_verification.sh        # set CSC=/path/to/csc.exe if auto-detection fails
```

## Files

| file | role |
|------|------|
| `build_reference.py` | The paper's exact `pywt` reference (`lhipa_reference`) **and** the algorithm ported to C# (`lhipa_newmodel`). Validates that a periodization-DWT cascade reproduces `pywt.downcoef` to <1e-9, and that the ported model equals the paper reference. Emits the shared test battery (`signals.csv`) and `reference.json`. |
| `shim.cs` | Minimal `UnityEngine` shim (`Debug`, `Mathf`) so `LHIPA.cs` compiles/runs outside Unity. `Mathf` mirrors Unity exactly (`float` results via `(float)System.Math.*`). |
| `harness.cs` | Driver: runs `LHIPA.CalculateLHIPA` on `signals.csv`. When built with `-define:LHIPA_TEST` it also dumps the normalized `cD_H`/`cD_L` bands via the test-only seam `LHIPA.ComputeBandsForTest`. |
| `compare.py` | Recomputes the reference on the **same float32-cast inputs** the C# receives and reports band accuracy + end-to-end LHIPA (`OLD` column appears only if `old_results.csv` is present). |

## Method notes

* The single-level periodization convolution `out[i] = Σ_k filt[k]·x[(2i + L/2 − k) mod N]`
  (offset `L/2 = 16` for sym16, `L = 32`) reproduces
  `pywt.downcoef('a'/'d', x, 'sym16', 'per', level=1)` to **0.0** error.
* Odd-length signals: like `pywt` periodization, `LHIPA.cs` extends an odd-length signal by
  repeating its last sample (one extra), giving `ceil(N/2)` coefficients. The DWT runs on the
  **raw** signal length (no power-of-two padding), exactly as the paper calls `pywt.downcoef`
  on the recorded samples — so the port is faithful for arbitrary (non-power-of-two) lengths,
  which is what real pupil recordings always are.
* Both the single step and the full Mallat cascade were checked against `pywt.downcoef` for
  **every length from 256 to 6000** (incl. odd intermediate levels): worst-case error 3.6e-15.
* The reference is recomputed on float32-cast inputs so the only thing under test is the
  algorithm, not single- vs double-precision input.
* The test battery mixes power-of-two (512/1024/2048) and realistic non-power-of-two lengths
  (500, 1500, 2700 = 30 s @ 90 Hz, 5001 ≈ 10 s @ 500 Hz).

## Verified result (PyWavelets 1.8.0, numpy 2.4.x, sym16)

```
WAVELET BANDS  : NEW C# cD_H / cD_L vs pywt.downcoef   -> worst-case error 3.3e-16 (machine eps)
END-TO-END     : NEW C# LHIPA vs paper reference        -> max |diff| 0.0, corr +1.000000  (15/15 signals)
(For comparison, the previous wavelet-packet implementation: corr +0.57, max |diff| 25.3.)
RESULT: PASS
```
