#!/usr/bin/env bash
# Reproduce the LHIPA verification: compile the REAL Unity LHIPA.cs (outside Unity, via a tiny
# UnityEngine shim) and confirm it reproduces the CHI 2020 PyWavelets reference bit-for-bit.
#
# Requirements:
#   - Python 3 with numpy + pywt   (pip install numpy PyWavelets)
#   - A C# compiler (Roslyn csc.exe from Visual Studio / Build Tools)
#
# Usage:   bash run_verification.sh
set -euo pipefail
cd "$(dirname "$0")"

LHIPA_CS="../../Unity/LHIPAOpenXR250/Assets/LHIPA/Scripts/LHIPA.cs"

# Locate csc.exe (override by exporting CSC=/path/to/csc.exe)
CSC="${CSC:-}"
if [ -z "$CSC" ]; then
  for c in "/c/Program Files/Microsoft Visual Studio/"*/*/MSBuild/Current/Bin/Roslyn/csc.exe \
           "/c/Program Files (x86)/Microsoft Visual Studio/"*/*/MSBuild/*/Bin/Roslyn/csc.exe; do
    [ -f "$c" ] && CSC="$c" && break
  done
fi
[ -z "$CSC" ] && { echo "csc.exe not found; set CSC=/path/to/csc.exe"; exit 1; }
echo "Using csc: $CSC"

echo "[1/3] Building pywt reference + test battery ..."
PYTHONUTF8=1 python build_reference.py

echo "[2/3] Compiling real LHIPA.cs (with LHIPA_TEST seam) ..."
"$CSC" -nologo -optimize+ -langversion:latest -define:LHIPA_TEST -out:new.exe \
  shim.cs harness.cs "$LHIPA_CS" | grep -v "warning CS" || true
./new.exe signals.csv new_bands.json > new_results.csv

echo "[3/3] Comparing NEW C# against the pywt reference ..."
PYTHONUTF8=1 python compare.py
