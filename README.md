# LHIPAUnity – Low-High Index of Pupillary Activity (Unity Implementation)
Calculate the Low-to-High Index of the Pupil Amplitude (LHIPA; see https://dl.acm.org/doi/10.1145/3313831.3376394) based on the signal of pupil diameters in Unity.

The **LHIPA (Low-High Index of Pupillary Activity)** is a cognitive workload metric based on fluctuations in pupil diameter.  
It is widely used in cognitive science, human-computer interaction, and psychology to assess mental effort in real-time or in post-processed data analysis.

---

## What is LHIPA?

LHIPA is computed by analyzing **high-frequency changes** in pupil diameter using a **Symlet16-Wavelet transformation**.  
It is independent of sampling rate and provides a standardized measure of cognitive load.

A **higher LHIPA value** indicates increased cognitive demand, while a **lower value** suggests minimal effort or distraction.

### Important:
Values ​​in this implementation can vary significantly depending on the sampling rate and recording duration used. 
A value indicating high cognitive load under settings with a specific sampling rate and recording duration might indicate low cognitive load under different settings. 
Therefore, the **absolute LHIPA value itself is not what matters**; rather, it is the **relative comparison of LHIPA values** ​​calculated using the same settings.

---

## Scientific Background

For more details on LHIPA, refer to:

**Duchowski, A. T., et al.**  
*"The Low/High Index of Pupillary Activity (LHIPA): A Psychophysiological Measure of Cognitive Workload".*  
CHI Conference on Human Factors in Computing Systems, 2020.

---

## Asset Key Features

- ✅ **Plug & Play:** Drag the script into your Unity scene and call the LHIPA function with pupil diameter data—computation happens automatically!  
- ✅ **Cross-platform:** Works on desktop, mobile, and VR applications.  
- ✅ **Live & recorded data:** Supports real-time LHIPA computation and post-analysis of pre-recorded eye-tracking data.  
- ✅ **Configurable settings**  
- ✅ **Well-documented & example scenes:** Includes two demo scenes showcasing LHIPA integration.

---

## Technical Details

### How LHIPA is Calculated
LHIPA is computed by analyzing the **power of high-frequency bands** in the pupil signal using a **Symlet16-Wavelet transformation**.  
This method detects subtle changes in pupil size that correlate with mental workload.

---

### What’s Included in the Asset?

- **LHIPA Calculation Script** – The core script for computing LHIPA  
- **2 Example Scenes:**  
  - **Eye Tracking Simulator:** Simulates real-time LHIPA computation  
  - **LHIPA Testing Scene:** Allows testing with recorded pupil diameter data  
- **2 Prefabs:**  
  - **Eye Tracking Simulator UI** – Example UI for live data processing  
  - **LHIPA Tester UI** – UI for analyzing recorded data  
- **Example pupil diameter dataset:** *pupil_diameter_signals.json*

---

## Technical Requirements

- **Unity 2020.1 or newer**  
- **TextMeshPro** (required for UI elements)  
- **Newtonsoft.Json** (required for reading/writing JSON data)  

You can install both via **Unity Package Manager**:  
`Window → Package Manager → TextMesh Pro`  
`Window → Package Manager → Newtonsoft.Json for Unity`

---

## Usage

`LHIPA.cs` is the main script, and **CalculateLHIPA()** is the method used to compute a LHIPA value.

You simply call `CalculateLHIPA()` with:
- An array of pupil diameters  
- Duration (in seconds)  
- Optional parameters (correction threshold, debug logging)

You can compute LHIPA:
- **In real-time** (with an eye tracker)  
- **Retrospectively** (using recorded data)

---

### CalculateLHIPA() Method Inputs

- `float[] pupilData` – Pupil diameters in mm (minimum length: 256)  
- `float durationInSeconds` – Duration between first and last array element  
- `float modMaxCorrectionThreshold` – Threshold for modulus maxima correction  
- `bool debugLog` – Enables detailed computation logs in the Unity Console  

### Method Output

- `float LHIPA` – Value between 0 and 1 representing cognitive load

---

# Scripts & Methods

## 1. LHIPA.cs

### Description
Core script for computing the **Low-to-High Index of Pupillary Activity (LHIPA)**  
based on a pupil diameter signal.

### modMaxCorrectionThreshold Details
This threshold removes noise in modulus maxima caused by rounding errors.

#### Recommended values:
- **0.01 – 0.1** (recommended and most stable)

### Example Regression Results

| Threshold | R²    | F-Value | β      | α      |
|-----------|-------|---------|--------|--------|
| 0.0       | 0.31  | 43.08   | 0.14   | 0.043  |
| 0.05      | 0.056 | 5.86    | 0.096  | 0.065  |
| 0.1       | 0.049 | 5.06    | 0.097  | 0.065  |

---

## 2. LHIPATester.cs

### Description
A testing script for computing LHIPA values from recorded data or simulated pupil arrays.  
Additional examples for using `CalculateLHIPA()` are included in the **Start()** method.

### Public Variables
- `float durationInSeconds` – Duration of the data sample  
- `float modMaxCorrectionThreshold` – Correction threshold  
- `string inputFilePath` – Path to recorded eye-tracking JSON file  
- `string outputFilePath` – Path for LHIPA logs  
- `bool consoleLog` – Enables console output  
- `float[] eyeDiametersExampleArray` – Example pupil diameter array  

### Public Methods

#### **TestLHIPAAndLog(bool useInputFile)**
Computes LHIPA for test data and logs results.

**Parameters**  
`useInputFile` → true: read JSON file; false: generate simulated arrays  

**Returns**  
`string` → Path to the logfile containing LHIPA values

---

#### **GenerateTestArrays()**
Generates 100 deterministic pupil diameter arrays for testing.

**Returns**  
`List<float[]>` – List of float arrays

---

## 3. EyeTrackSimulation.cs

### Description
Simulates eye-tracking data to test LHIPA calculations in Unity.  
`CalculateLHIPA()` is called live every **calculationInterval** seconds.

### Public Variables
- `float baselineDiameter`  
- `float simulationSwitchInterval`  
- `float samplingRate`  
- `bool startWithScene`  
- `float calculationInterval`  
- `float modMaxCorrectionThreshold`  
- `bool showDetailedCalculationLog`  
- `float currentLHIPAValue`  

### Public Methods

#### **StartSimulation()**
Starts the simulation and live LHIPA computation.

#### **StopSimulation()**
Stops simulation and live computation.

#### **GetPupilDiameterFromEyeTracker()**
Returns:  
`float` – Simulated pupil diameter value

---

# Additional Components

## UI Scripts
Scripts included to handle example UI interactions such as:
- Buttons  
- Text fields  
- Displaying LHIPA results  

These are used in the provided example scenes and prefabs.

---

## SharpWave (Wavelet Library)

This asset includes a **modified version of SharpWave**, with:
- Symlet16 wavelet implementation  
- Updated `decompose()` methods  
- Optimizations for Unity  

Based on SharpWave by **graetz23**:  
https://github.com/graetz23/sharpWave  
MIT License:  
https://github.com/graetz23/sharpWave/blob/master/LICENSE  

Right now, these waveletes are not used for LHIPA calculation anymore. But we keep the modivied wavelets in the repo so that they can be used for comparions between different calculations.

---

## Folders and Files

### `Unity Packages`
- **`LHIPACoreAlgorithmAndTestingPackage.unitypackage`** - Package contains LHIPA algorithm and all testing resources for Unity. For all platforms.
- **`AssetStorePackage_LHIPAForUnity`** - Same as `LHIPACoreAlgorithmAndTestingPackage.unitypackage` but with documentation and more comments. The package contains the LHIPA algorithm and all testing resources for Unity. For all platforms.
- **`LHIPAOpenXRPackage.unitypackage`** - OpenXR implementation of LHIPA with eye tracking. Beta. Only works when the latest VIVE OpenXR is integrated into the project.
- **`LHIPAOpenXR250Package.unitypackage`** - OpenXR 2.5.0 implementation of LHIPA with eye tracking. Beta. Only works when VIVE OpenXR 2.5.0 is integrated into the project.

### `Unity`
- **`LHIPAOpenXR250`** - LHIPA implementation in Unity. For desktop and VR. VR requires VIVE OpenXR 2.5.0 integration. So far, the eye-tracking feature seems not to work with VIVE Pro Eye when using OpenXR.

### `Unity/LHIPAOpenXR250/Assets/LHIPA/Scripts`
- **`LHIPA.cs`** - LHIPA algorithm in C# for use in Unity.
- **SharpWave** - Updated version of SharpWave including the Symlet16 wavelet with Python coefficients. Must be in the project to run `LHIPA.cs`!
- **Testing** - Scripts to test the LHIPA algorithm with different input signals:
  - **`LHIPATester.cs`** - Test LHIPA algorithm with predefined signals.
  - **`EyeTrackSimulation.cs`** - Simulate eye tracking for testing.
- **UI** - Scripts to run testing scripts together with prefabs defined in the **Prefabs** folder.
- **EyeTracking** - OpenXR eye tracking scripts to test LHIPA in VR.
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

