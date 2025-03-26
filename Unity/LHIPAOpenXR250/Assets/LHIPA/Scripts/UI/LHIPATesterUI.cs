using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace lhipa
{

    /// <summary>
    /// UI Methods for using the LHIPATester class in Unity.
    /// </summary>
    public class LHIPATesterUI : MonoBehaviour
    {
        [Header("References to UI objects")]

        // Toggles
        public Toggle showDetailedCalculationLogToggle;

        // Text Fields
        public TMP_InputField calculationIntervalInputField;
        public TMP_InputField modMaxCorrectionInputField;

        // LHIPA Display
        public TextMeshProUGUI currentLHIPAValueText;
        public Image currentLHIPAValueImage;

        private LHIPATester lhipaTester;
        private float currentLhipaValue = 0f;

        private void Start()
        {
            // Find EyeTrackSimulation reference
            lhipaTester = transform.GetComponent<LHIPATester>();

            // Write initial values into Text Fields and Toggles
            showDetailedCalculationLogToggle.isOn = lhipaTester.consoleLog;

            calculationIntervalInputField.text = lhipaTester.durationInSeconds.ToString();
            modMaxCorrectionInputField.text = lhipaTester.modMaxCorrectionThreshold.ToString();

            // Event-Listener for Toggles and Text Fields
            showDetailedCalculationLogToggle.onValueChanged.AddListener(UpdateShowDetailedCalculationLog);
            calculationIntervalInputField.onEndEdit.AddListener(UpdateCalculationInterval);
            modMaxCorrectionInputField.onEndEdit.AddListener(UpdateModMaxThreshold);
        }

        public void UseInputFileButtonPressed()
        {
            currentLHIPAValueImage.color = Color.white;
            currentLHIPAValueText.text = $"LHIPA results logged to {lhipaTester.TestLHIPAAndLog(true)}";
        }

        public void GenerateInputButtonPressed()
        {
            currentLHIPAValueImage.color = Color.white;
            currentLHIPAValueText.text = $"LHIPA results logged to {lhipaTester.TestLHIPAAndLog(false)}";
        }

        public void UseSingleArrayButtonPressed()
        {
            currentLhipaValue = LHIPA.CalculateLHIPA(lhipaTester.eyeDiametersExampleArray,
                lhipaTester.durationInSeconds, lhipaTester.modMaxCorrectionThreshold, lhipaTester.consoleLog);
            currentLHIPAValueText.text = currentLhipaValue.ToString("F3");
            if (currentLhipaValue <= 0)
            {
                currentLHIPAValueImage.color = Color.yellow;
            }
            else if (currentLhipaValue >= 1)
            {
                currentLHIPAValueImage.color = Color.blue;
            }
            else
            {
                currentLHIPAValueImage.color = Color.Lerp(Color.yellow, Color.blue, currentLhipaValue);
            }
        }

        public void UseSinusArrayButtonPressed()
        {
            currentLhipaValue = LHIPA.CalculateLHIPA(lhipaTester.GenerateSinusArray(256),
                lhipaTester.durationInSeconds, lhipaTester.modMaxCorrectionThreshold, lhipaTester.consoleLog);
            currentLHIPAValueText.text = currentLhipaValue.ToString("F3");
            if (currentLhipaValue <= 0)
            {
                currentLHIPAValueImage.color = Color.yellow;
            }
            else if (currentLhipaValue >= 1)
            {
                currentLHIPAValueImage.color = Color.blue;
            }
            else
            {
                currentLHIPAValueImage.color = Color.Lerp(Color.yellow, Color.blue, currentLhipaValue);
            }
        }

        private void UpdateShowDetailedCalculationLog(bool value)
        {
            lhipaTester.consoleLog = value;
        }

        private void UpdateCalculationInterval(string input)
        {
            if (float.TryParse(input, out float value) && value > 0)
            {
                lhipaTester.durationInSeconds = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for durationInSeconds. Value must be non-negative and not 0!");
                calculationIntervalInputField.text = lhipaTester.durationInSeconds.ToString();
            }
        }

        private void UpdateModMaxThreshold(string input)
        {
            if (float.TryParse(input, out float value) && value >= 0)
            {
                lhipaTester.modMaxCorrectionThreshold = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for modMaxCorrectionThreshold. Value must be non-negative!");
                modMaxCorrectionInputField.text = lhipaTester.modMaxCorrectionThreshold.ToString();
            }
        }
    }
}
