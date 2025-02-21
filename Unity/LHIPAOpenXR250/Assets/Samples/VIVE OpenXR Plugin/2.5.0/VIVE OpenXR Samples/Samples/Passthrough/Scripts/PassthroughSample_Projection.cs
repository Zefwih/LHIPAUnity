// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

using VIVE.OpenXR.Passthrough;
using VIVE.OpenXR.Samples;

namespace VIVE.OpenXR.CompositionLayer.Samples.Passthrough
{
    public class PassthroughSample_Projection : MonoBehaviour
    {
        public Mesh passthroughMesh = null;
        public Transform passthroughMeshTransform = null;

        public GameObject hmd = null;

        public Text scaleValueText;
        public Slider scaleSlider = null;
        private Vector3 scale = Vector3.one;
        private float scaleModifier = 1f;

        private OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
        private LayerType currentActiveLayerType = LayerType.Underlay;
        private ProjectedPassthroughSpaceType currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;

        private void Start()
        {
            if (hmd == null) hmd = Camera.main.gameObject;

            if (scaleSlider != null) scaleSlider.value = scaleModifier;
        }

        private void Update()
        {
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.B)) //Set Passthrough as Overlay
			{
				SetPassthroughToOverlay();
				if (activePassthroughID != 0)  SetPassthroughMesh();
			}
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.A)) //Set Passthrough as Underlay
			{
				SetPassthroughToUnderlay();
				if (activePassthroughID != 0) SetPassthroughMesh();
			}
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.X)) //Switch to world lock
			{
				SetWorldLock();
				if (activePassthroughID != 0) SetPassthroughMesh();
			}
			if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.Y)) //Switch to head lock
			{
				SetHeadLock();
				if (activePassthroughID != 0) SetPassthroughMesh();
			}

            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.GripR))
            {
                if (passthroughMesh != null && passthroughMeshTransform != null)
                {
                    if (activePassthroughID == 0)
                    {
                        StartPassthrough();
                    }
                }
            }
            if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.GripL))
            {
                if (activePassthroughID != 0)
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

        public void SetHeadLock()
        {
            if (activePassthroughID != 0)
            {
                if (PassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, ProjectedPassthroughSpaceType.Headlock))
                {
                    passthroughMeshTransform.SetParent(hmd.transform);

                    currentActiveSpaceType = ProjectedPassthroughSpaceType.Headlock;
                }
            }
        }

        public void SetWorldLock()
        {
            if (activePassthroughID != 0)
            {
                if (PassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, ProjectedPassthroughSpaceType.Worldlock))
                {
                    passthroughMeshTransform.SetParent(null);

                    currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
                }
            }
        }

        public void OnScaleSliderValueChange(float newScaleModifier)
        {
            scaleValueText.text = newScaleModifier.ToString();
            if (activePassthroughID != 0)
            {
                scaleModifier = newScaleModifier;
				SetPassthroughMesh();
			}
        }

        void StartPassthrough()
        {
            PassthroughAPI.CreateProjectedPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
            SetPassthroughMesh();
        }

        void SetPassthroughMesh()
        {
            PassthroughAPI.SetProjectedPassthroughMesh(activePassthroughID, passthroughMesh.vertices, passthroughMesh.triangles);
            switch (currentActiveSpaceType)
            {
                case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                    Vector3 relativePosition = hmd.transform.InverseTransformPoint(passthroughMeshTransform.transform.position);
                    Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * passthroughMeshTransform.transform.rotation.normalized;
                    PassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, relativePosition, relativeRotation, scale * scaleModifier, false);
                    break;
                case ProjectedPassthroughSpaceType.Worldlock:
                default:
                    PassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, passthroughMeshTransform.transform.position, passthroughMeshTransform.transform.rotation, scale * scaleModifier);
                    break;
            }
        }

        void OnDestroyPassthroughFeatureSession(OpenXR.Passthrough.XrPassthroughHTC passthrough)
        {
            PassthroughAPI.DestroyPassthrough(passthrough);
            activePassthroughID = 0;
        }
    }
}
