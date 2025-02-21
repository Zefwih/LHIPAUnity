// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;

/*#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif*/
using VIVE.OpenXR.FrameSynchronization;

namespace VIVE.OpenXR.Samples.OpenXRInput
{
    [RequireComponent(typeof(Text))]
    public class UserPresence : MonoBehaviour
    {
        private Text m_Text = null;

        private void Awake()
        {
            m_Text = GetComponent<Text>();
        }
        private void OnEnable()
        {
            GetFrameSynchronizationMode();
        }
        private void Update()
        {
            if (m_Text == null) { return; }

            m_Text.text = "User is " + (IsUserPresent() ? "Present" : "Away") + ", " + m_FrameSynchronizationMode;
            //Debug.LogFormat("VIVE.OpenXR.Samples.OpenXRInput.UserPresence Update() {0}", m_Text.text);
        }

        public bool IsUserPresent()
        {
/*#if UNITY_ANDROID && ENABLE_INPUT_SYSTEM
            if (ProximitySensor.current != null)
            {
                if (!ProximitySensor.current.IsActuated())
                    InputSystem.EnableDevice(ProximitySensor.current);

                return ProximitySensor.current.distance.ReadValue() < 1; // near p-sensor < 1cm
            }
#endif*/
            return XR_EXT_user_presence.Interop.IsUserPresent();
        }

        private string m_FrameSynchronizationMode = "No FS";
        private void GetFrameSynchronizationMode()
        {
            ViveFrameSynchronization feature = OpenXRSettings.Instance.GetFeature<ViveFrameSynchronization>();
            m_FrameSynchronizationMode = feature ? feature.GetSynchronizationMode().ToString() : "No FS";
        }
    }
}