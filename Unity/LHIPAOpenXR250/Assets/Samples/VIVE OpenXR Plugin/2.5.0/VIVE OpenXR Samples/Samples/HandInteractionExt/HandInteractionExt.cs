// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;

namespace VIVE.OpenXR.Samples.HandInteractionExt
{
    [RequireComponent(typeof(Text))]
    public class HandInteractionExt : MonoBehaviour
    {
        public bool isLeft = true;

        public InputActionReference pointerPose = null;
        public InputActionReference pointerValue = null;
        public InputActionReference pointerReady = null;

        public InputActionReference gripPose = null;
        public InputActionReference gripValue = null;
        public InputActionReference gripReady = null;

        public InputActionReference pinchPose = null;
        public InputActionReference pinchValue = null;
        public InputActionReference pinchReady = null;

        public InputActionReference pokePose = null;

        private Text m_Text = null;
        private void Start()
        {
            m_Text = GetComponent<Text>();
        }

		void Update()
		{
			if (m_Text == null) { return; }

			m_Text.text = (isLeft ? "Left Pointer: " : "Right Pointer: ");
			m_Text.text += "\n  isTracked: ";
			{
				if (CommonHelper.GetPoseIsTracked(pointerPose, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  trackingState: ";
			{
				if (CommonHelper.GetPoseTrackingState(pointerPose, out InputTrackingState value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  position (";
			{
				if (CommonHelper.GetPosePosition(pointerPose, out Vector3 value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\n  rotation (";
			{
				if (CommonHelper.GetPoseRotation(pointerPose, out Quaternion value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\nValue: ";
			{
				if (CommonHelper.GetAnalog(pointerValue, out float value, out string msg))
				{
					m_Text.text += value.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\nReady: ";
			{
				if (CommonHelper.GetButton(pointerReady, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n\n";

			m_Text.text += (isLeft ? "Left Grip: " : "Right Grip: ");
			m_Text.text += "\n  isTracked: ";
			{
				if (CommonHelper.GetPoseIsTracked(gripPose, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  trackingState: ";
			{
				if (CommonHelper.GetPoseTrackingState(gripPose, out InputTrackingState value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  position (";
			{
				if (CommonHelper.GetPosePosition(gripPose, out Vector3 value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\n  rotation (";
			{
				if (CommonHelper.GetPoseRotation(gripPose, out Quaternion value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\nValue: ";
			{
				if (CommonHelper.GetAnalog(gripValue, out float value, out string msg))
				{
					m_Text.text += value.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\nReady: ";
			{
				if (CommonHelper.GetButton(gripReady, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n\n";

			m_Text.text += (isLeft ? "Left Pinch: " : "Right Pinch: ");
			m_Text.text += "\n  isTracked: ";
			{
				if (CommonHelper.GetPoseIsTracked(pinchPose, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  trackingState: ";
			{
				if (CommonHelper.GetPoseTrackingState(pinchPose, out InputTrackingState value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  position (";
			{
				if (CommonHelper.GetPosePosition(pinchPose, out Vector3 value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\n  rotation (";
			{
				if (CommonHelper.GetPoseRotation(pinchPose, out Quaternion value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\nValue: ";
			{
				if (CommonHelper.GetAnalog(pinchValue, out float value, out string msg))
				{
					m_Text.text += value.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\nReady: ";
			{
				if (CommonHelper.GetButton(pinchReady, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n\n";

			m_Text.text += (isLeft ? "Left Poke: " : "Right Poke: ");
			m_Text.text += "\n  isTracked: ";
			{
				if (CommonHelper.GetPoseIsTracked(pokePose, out bool value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  trackingState: ";
			{
				if (CommonHelper.GetPoseTrackingState(pokePose, out InputTrackingState value, out string msg))
				{
					m_Text.text += value;
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += "\n  position (";
			{
				if (CommonHelper.GetPosePosition(pokePose, out Vector3 value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")\n  rotation (";
			{
				if (CommonHelper.GetPoseRotation(pokePose, out Quaternion value, out string msg))
				{
					m_Text.text += value.x.ToString() + ", " + value.y.ToString() + ", " + value.z.ToString() + ", " + value.w.ToString();
				}
				else
				{
					m_Text.text += msg;
				}
			}
			m_Text.text += ")";
		}
	}
}