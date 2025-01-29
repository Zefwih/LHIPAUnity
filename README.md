# LHIPAUnity
Calculate the Low-to-High Index of the pupil amplitude (LHIPA) based on signal of pupil diameters in Unity.

## Folders:

### Unity\LHIPA\Assets\Scripts
- * *LHIPA.cs* * LHIPA algorithm in Unity
- * *Testing* * Scripts to test LHIPA algorithm with different input signals (* *LHIPATester.cs* *) or by simulating eye tracking (* *EyeTrackSimulation.cs* *)
- * *UI* * Scripts to run * *Testing* *-scripts together with prefabs defined in * *Prefabs* *-folder
- * *SharpWave* * Updated version of SharpWave including Symlet16-wavelet with Python coeffitients
- * *pupil_diameter_signals.json* * Example pupil diameter input arrays

### Unity\LHIPA\Assets\Scenes
- * *LHIPATestingScene.unity* * Test LHIPA algorithm with different input signals
- * *EyeTrackingSimulation.unity* * Simulate eye tracking to test LHIPA algorithm

### Python
- * *lhipa.py* * LHIPA Python algorithm
- * *lhipa_test.py* * Executable Python Code to test LHIPA algorithm
- * *pupil_diameter_signals.json* * Example pupil diameter input arrays

### TestResults
- * *LHIPATestResults* * LHIPA calculation results of Python algorithm and Unity algorithm with the use of different ModMaxThresholds
- * *WaveletTestResults* * Rerult Arrays of Symlet16-Decomposition in Python and Unity
- * *InputSignalsForTests.json* * Example pupil diameter input arrays used to calculate test results
