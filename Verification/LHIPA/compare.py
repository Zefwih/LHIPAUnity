"""
Step 4: Compare OLD C#, NEW C#, and the pywt paper-reference on the identical inputs.
The reference is recomputed on the SAME float32-cast signals the C# harness receives,
so any remaining difference is algorithmic, not input-precision.
"""
import json, os, math
import numpy as np
from build_reference import lhipa_reference  # paper's exact pywt implementation

HERE = os.path.dirname(os.path.abspath(__file__))

def read_signals():
    sigs = {}
    for line in open(os.path.join(HERE, "signals.csv")):
        line = line.strip()
        if not line:
            continue
        tok = line.split(",")
        name = tok[0]; sr = float(tok[1])
        # mimic C# float[] : parse to float32 then widen to float64 (C# does (double)floatVal)
        vals = np.array([float(t) for t in tok[2:]], dtype=np.float32).astype(np.float64)
        sigs[name] = (sr, vals)
    return sigs

def read_csv(path):
    out = {}
    for line in open(path):
        line = line.strip()
        if not line:
            continue
        name, v = line.rsplit(",", 1)
        out[name] = float(v)
    return out

sigs = read_signals()
_old_path = os.path.join(HERE, "old_results.csv")
old = read_csv(_old_path) if os.path.exists(_old_path) else None
new = read_csv(os.path.join(HERE, "new_results.csv"))
new_bands = json.load(open(os.path.join(HERE, "new_bands.json")))

# reference on the float32-cast inputs
ref, ref_bands = {}, {}
for name, (sr, s) in sigs.items():
    tt = len(s) / sr
    val, cH, cL, lof = lhipa_reference(s, tt)
    ref[name] = val
    ref_bands[name] = (cH, cL)

names = list(sigs.keys())

print("=" * 96)
print("WAVELET BAND ACCURACY  (NEW C# cD_H/cD_L  vs  pywt.downcoef detail bands)")
print("=" * 96)
print(f"{'signal':22} {'len(cD_H)':>9} {'len(cD_L)':>9} {'maxErr cD_H':>14} {'maxErr cD_L':>14}")
worst_band = 0.0
for name in names:
    cH_ref, cL_ref = ref_bands[name]
    cH_new = np.array(new_bands[name]["cD_H"]); cL_new = np.array(new_bands[name]["cD_L"])
    eH = np.max(np.abs(cH_new - cH_ref)) if len(cH_new) == len(cH_ref) else float('nan')
    eL = np.max(np.abs(cL_new - cL_ref)) if len(cL_new) == len(cL_ref) else float('nan')
    worst_band = max(worst_band, eH, eL)
    print(f"{name:22} {len(cH_new):>9} {len(cL_new):>9} {eH:>14.3e} {eL:>14.3e}")
print(f"\n  -> worst-case band error across all signals: {worst_band:.3e}")

print()
print("=" * 96)
print("END-TO-END LHIPA")
print("=" * 96)
old_hdr = f"{'OLD C#':>10} " if old is not None else ""
print(f"{'signal':22} {old_hdr}{'NEW C#':>10} {'REFERENCE':>12}   {'NEW-ref':>10}")
for name in names:
    d = abs(new[name] - ref[name])
    old_col = f"{old[name]:>10.4f} " if old is not None else ""
    print(f"{name:22} {old_col}{new[name]:>10.4f} {ref[name]:>12.4f}   {d:>10.2e}")

nw = np.array([new[n] for n in names]); rf = np.array([ref[n] for n in names])
def corr(a, b): return float(np.corrcoef(a, b)[0, 1])
print()
if old is not None:
    o = np.array([old[n] for n in names])
    print(f"  OLD vs REFERENCE : max|diff|={np.max(np.abs(o-rf)):.4f}  corr={corr(o,rf):+.3f}  "
          f"(OLD range {o.min():.3f}..{o.max():.3f})")
print(f"  NEW vs REFERENCE : max|diff|={np.max(np.abs(nw-rf)):.3e}  corr={corr(nw,rf):+.6f}")
print()
if worst_band < 1e-4 and np.max(np.abs(nw-rf)) < 1e-3:
    print("  RESULT: NEW C# reproduces the pywt paper reference (bands + end-to-end LHIPA).  PASS")
else:
    print("  RESULT: discrepancy remains - investigate.  CHECK")
