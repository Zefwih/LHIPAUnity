# LHIPAUnity
Calculate the Low-to-High Index of the Pupil Amplitude (LHIPA) based on the signal of pupil diameters in Unity.

## Folders and Files

### `Unity Packages`
- **`LHIPACoreAlgorithmAndTestingPackage.unitypackage`** - Package contains LHIPA algorithm and all testing resources for Unity. For all plattforms.
- **`AssetStorePackage_LHIPAForUnity`** - Same as `LHIPACoreAlgorithmAndTestingPackage.unitypackage` but with documentation and more comments. Package contains LHIPA algorithm and all testing resources for Unity. For all plattforms.
- **`LHIPAOpenXRPackage.unitypackage`** - OpenXR implementation of LHIPA with eye tracking. Beta. Only works, when latest VIVE OpenXR is integrated into project.
- **`LHIPAOpenXR250Package.unitypackage`** - OpenXR 2.5.0 implementation of LHIPA with eye tracking. Beta. Only works, when VIVE OpenXR 2.5.0 is integrated into project.

### `Unity`
- **`LHIPAOpenXR250`** - LHIPA implementation in Unity. For desktop and VR. VR requires VIVE OpenXR 2.5.0 integration. So far, eye tracking feature seems not to work with VIVE Pro Eye when using OpenXR.

### `Unity/LHIPAOpenXR250/Assets/LHIPA/Scripts`
- **`LHIPA.cs`** - LHIPA algorithm in C# for use in Unity.
- **SharpWave** - Updated version of SharpWave including the Symlet16 wavelet with Python coefficients. Must be in project to run `LHIPA.cs`!
- **Testing** - Scripts to test the LHIPA algorithm with different input signals:
  - **`LHIPATester.cs`** - Test LHIPA algorithm with predefined signals.
  - **`EyeTrackSimulation.cs`** - Simulate eye tracking for testing.
- **UI** - Scripts to run testing scripts together with prefabs defined in the **Prefabs** folder.
- **EyeTracking** - OpenXR eye tracking scrips to test LHIPA in VR.
- **`pupil_diameter_signals.json`** - Example pupil diameter input arrays.

### `Unity/LHIPA/Assets/LHIPA/Scenes`
- **`LHIPATestingScene.unity`** - Test the LHIPA algorithm with different input signals.
- **`EyeTrackingSimulation.unity`** - Simulate eye tracking to test the LHIPA algorithm.
- **`OpenXREyeTrackingExample.unity`** - OpenXR example of LHIPA with HTC eye tracking.

### `Python`
- **`lhipa.py`** - LHIPA Python algorithm.
- **`lhipa_test.py`** - Executable Python code to test the LHIPA algorithm.
- **`pupil_diameter_signals.json`** - Example pupil diameter input arrays.

### `TestResults`
- **LHIPATestResults** - LHIPA calculation results of the Python and Unity algorithms using different ModMaxThresholds.
- **WaveletTestResults** - Result arrays of Symlet16 decomposition in Python and Unity.
- **`InputSignalsForTests.json`** - Example pupil diameter input arrays used to calculate test results.

