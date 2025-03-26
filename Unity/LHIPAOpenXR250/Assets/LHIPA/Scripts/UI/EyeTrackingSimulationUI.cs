using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace lhipa
{

    /// <summary>
    /// UI Methods for testing the LHIPA method with the EyeTrackingSimulation class in Unity.
    /// </summary>
    public class EyeTrackingSimulationUI : MonoBehaviour
    {
        [Header("References to UI objects")]

        // InputFields TextMesh Pro
        public TMP_InputField baselineDiameterInputField;

        public TMP_InputField simulationSwitchIntervalInputField;
        public TMP_InputField calculationIntervalInputField;
        public TMP_InputField modMaxCorrectionInputField;
        public TMP_InputField samplingRateInputField;

        // Toggles
        public Toggle showDetailedCalculationLogToggle;

        // LHIPA Display
        public TextMeshProUGUI currentLHIPAValueText;
        public Image currentLHIPAValueImage;

        private EyeTrackSimulation eyeTrackSimulation;

        private void Start()
        {
            // Find EyeTrackSimulation Reference
            eyeTrackSimulation = transform.GetComponent<EyeTrackSimulation>();

            // IWrite initial values into InputFields and Toggles
            baselineDiameterInputField.text = eyeTrackSimulation.baselineDiameter.ToString();
            simulationSwitchIntervalInputField.text = eyeTrackSimulation.simulationSwitchInterval.ToString();
            calculationIntervalInputField.text = eyeTrackSimulation.calculationInterval.ToString();
            modMaxCorrectionInputField.text = eyeTrackSimulation.modMaxCorrectionThreshold.ToString();
            samplingRateInputField.text = eyeTrackSimulation.samplingRate.ToString();

            showDetailedCalculationLogToggle.isOn = eyeTrackSimulation.showDetailedCalculationLog;

            // Add Event-Listener for onEndEdit on InputFields and Toggles
            baselineDiameterInputField.onEndEdit.AddListener(UpdateBaselineDiameter);
            simulationSwitchIntervalInputField.onEndEdit.AddListener(UpdateSimulationSwitchInterval);
            calculationIntervalInputField.onEndEdit.AddListener(UpdateCalculationInterval);
            modMaxCorrectionInputField.onEndEdit.AddListener(UpdateModMaxThreshold);
            samplingRateInputField.onEndEdit.AddListener(UpdateSamplingRate);

            showDetailedCalculationLogToggle.onValueChanged.AddListener(UpdateShowDetailedCalculationLog);
        }

        private void UpdateBaselineDiameter(string input)
        {
            if (float.TryParse(input, out float value) && value >= 0)
            {
                eyeTrackSimulation.baselineDiameter = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for baselineDiameter. Value must be non-negative!");
                baselineDiameterInputField.text = eyeTrackSimulation.baselineDiameter.ToString();
            }
        }

        private void UpdateSimulationSwitchInterval(string input)
        {
            if (float.TryParse(input, out float value) && value >= 0)
            {
                eyeTrackSimulation.simulationSwitchInterval = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for simulationSwitchInterval. Value must be non-negative!.");
                simulationSwitchIntervalInputField.text = eyeTrackSimulation.simulationSwitchInterval.ToString();
            }
        }

        private void UpdateCalculationInterval(string input)
        {
            if (float.TryParse(input, out float value) && value > 0)
            {
                eyeTrackSimulation.calculationInterval = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for calculationInterval. Value must be non-negative and not 0!");
                calculationIntervalInputField.text = eyeTrackSimulation.calculationInterval.ToString();
            }
        }

        private void UpdateModMaxThreshold(string input)
        {
            if (float.TryParse(input, out float value) && value >= 0)
            {
                eyeTrackSimulation.modMaxCorrectionThreshold = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for modMaxThreshold. Value must be non-negative!.");
                modMaxCorrectionInputField.text = eyeTrackSimulation.modMaxCorrectionThreshold.ToString();
            }
        }

        private void UpdateSamplingRate(string input)
        {
            if (float.TryParse(input, out float value) && value > 0)
            {
                eyeTrackSimulation.samplingRate = value;
            }
            else
            {
                Debug.LogWarning("Invalid input for samplingRate. Value must be non-negative and not 0!.");
                samplingRateInputField.text = eyeTrackSimulation.samplingRate.ToString();
            }
        }


        private void UpdateShowDetailedCalculationLog(bool value)
        {
            eyeTrackSimulation.showDetailedCalculationLog = value;
        }

        private void Update()
        {
            // Get currentLHIPAValue from eyeTrackingSimulation
            float currentLHIPAValue = eyeTrackSimulation.currentLHIPAValue;

            // Round to three decimal numbers
            currentLHIPAValueText.text = currentLHIPAValue.ToString("F3");

            // Change color of background depending on LHIPA value
            if (currentLHIPAValue <= 0)
            {
                currentLHIPAValueImage.color = Color.yellow;
            }
            else if (currentLHIPAValue >= 1)
            {
                currentLHIPAValueImage.color = Color.blue;
            }
            else
            {
                currentLHIPAValueImage.color = Color.Lerp(Color.yellow, Color.blue, currentLHIPAValue);
            }
        }
    }
}