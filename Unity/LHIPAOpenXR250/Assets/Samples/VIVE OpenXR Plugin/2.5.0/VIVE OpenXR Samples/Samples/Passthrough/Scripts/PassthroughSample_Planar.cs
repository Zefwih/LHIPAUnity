// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

using VIVE.OpenXR.Passthrough;
using VIVE.OpenXR.Samples;

namespace VIVE.OpenXR.CompositionLayer.Samples.Passthrough
{
    public class PassthroughSample_Planar : MonoBehaviour
    {
        private OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;

        private void Update()
        {
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.B)) //Set Passthrough as Overlay
            {
                SetPassthroughToOverlay();
            }
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.A)) //Set Passthrough as Underlay
            {
                SetPassthroughToUnderlay();
            }
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.GripR))
            {
                if (activePassthroughID == 0)
                {
                    StartPassthrough();
                }
            }
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.GripL))
            {
                if(activePassthroughID != 0)
                {
                    PassthroughAPI.DestroyPassthrough(activePassthroughID);
                    activePassthroughID = 0;
                }
            }
        }

        public void SetPassthroughToOverlay()
        {
            if (activePassthroughID != 0)
            {
                PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Overlay);
                currentActiveLayerType = LayerType.Overlay;
            }
        }

        public void SetPassthroughToUnderlay()
        {
            if (activePassthroughID != 0)
            {
                PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
                currentActiveLayerType = LayerType.Underlay;
            }
        }

        void StartPassthrough()
        {
            PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
        }

        void OnDestroyPassthroughFeatureSession(OpenXR.Passthrough.XrPassthroughHTC passthroughID)
        {
            PassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }
    }
}
