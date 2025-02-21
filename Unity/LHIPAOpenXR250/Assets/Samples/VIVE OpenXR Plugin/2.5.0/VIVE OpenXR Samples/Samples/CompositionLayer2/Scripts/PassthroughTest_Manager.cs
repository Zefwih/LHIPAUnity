// Copyright HTC Corporation All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;
using VIVE.OpenXR.Samples;

using XrPassthroughHTC = VIVE.OpenXR.Passthrough.XrPassthroughHTC;

public class PassthroughTest_Manager : MonoBehaviour
{
    public GameObject projectedMeshAnchor = null, hmd = null, rigOrigin = null;
    public Slider alphaSlider, scaleSlider;
    public Text alphaValueText, scaleValueText;

    public CompositionLayer classicPassthroughNotificaltionPrompt = null, projectedPassthroughNotificationPrompt = null;

    private bool scaleChanged = false, positionChanged = false, orientationChanged = false, spaceTypeChanged = false;
    private bool passthroughActive 
    { 
        get
        {
            List<XrPassthroughHTC> currentLayerIDs = PassthroughAPI.GetCurrentPassthroughLayerIDs();
            if (currentLayerIDs != null && currentLayerIDs.Contains(activePassthroughID)) //Layer is active
            {
                //Debug.Log("passthroughActive: true");
                return true;
            }
            //Debug.Log("passthroughActive: false");
            return false;
        }
    }
    private PassthroughLayerForm currentActiveLayerForm = PassthroughLayerForm.Planar;
    private LayerType currentActiveLayerType = LayerType.Overlay;
    private ProjectedPassthroughSpaceType currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
    private XrPassthroughHTC activePassthroughID = 0;

    private float scale = 1f, scaleModifier = 1f, alpha = 1f;
    private Vector3[] quadVertices = { new Vector3 (-1f, -1f, 0.0f),
                                       new Vector3 (1f, -1f, 0.0f),
                                       new Vector3 (1f, 1f, 0.0f),
                                       new Vector3 (-1f, 1f, 0.0f) };
    private int[] quadIndicies = { 0, 1, 2, 0, 2, 3 };

    // Start is called before the first frame update
    void Start()
    {
        alphaSlider.value = alpha;
        scaleSlider.value = scaleModifier;

        if (hmd == null)
        {
            hmd = Camera.main.gameObject;
        }
    }

    private float joyStickThreshold = 0.5f;
    // Update is called once per frame
    void Update()
    {
        //Read input to control passthrough
        if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.X)) //Destroy Passthrough
        {
            DisablePassthrough();
        }
        if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.B)) //Set Passthrough as Overlay
        {
            SetPassthroughToOverlay();
        }
        if (VRSInputManager.instance.GetButtonDown(VRSButtonReference.A)) //Set Passthrough as Underlay
        {
            SetPassthroughToUnderlay();
        }

        //Control Position
        Vector2 leftJoyStickValues = VRSInputManager.instance.GetAxis(VRSHandFlag.Left);
        if (leftJoyStickValues.x > joyStickThreshold) //Move Right
        {
            Vector3 position = projectedMeshAnchor.transform.position;
            position = new Vector3(position.x + 0.05f, position.y, position.z);
            projectedMeshAnchor.transform.position = position;
            positionChanged = true;
        }
        else if (leftJoyStickValues.x < -joyStickThreshold) //Move Left
        {
            Vector3 position = projectedMeshAnchor.transform.position;
            position = new Vector3(position.x - 0.05f, position.y, position.z);
            projectedMeshAnchor.transform.position = position;
            positionChanged = true;
        }

        if (leftJoyStickValues.y > joyStickThreshold) //Move Forward
        {
            Vector3 position = projectedMeshAnchor.transform.position;
            position = new Vector3(position.x, position.y, position.z + 0.05f);
            projectedMeshAnchor.transform.position = position;
            positionChanged = true;
        }
        else if (leftJoyStickValues.y < -joyStickThreshold) //Move Backward
        {
            Vector3 position = projectedMeshAnchor.transform.position;
            position = new Vector3(position.x, position.y, position.z - 0.05f);
            projectedMeshAnchor.transform.position = position;
            positionChanged = true;
        }

        //Control Rotation
        Vector2 rightJoyStickValues = VRSInputManager.instance.GetAxis(VRSHandFlag.Right);
        if (rightJoyStickValues.x > joyStickThreshold) //rotate Right
        {
            projectedMeshAnchor.transform.RotateAround(projectedMeshAnchor.transform.position, Vector3.up, -1f);
            orientationChanged = true;
        }
        else if (rightJoyStickValues.x < -joyStickThreshold) //rotate Left
        {
            projectedMeshAnchor.transform.RotateAround(projectedMeshAnchor.transform.position, Vector3.up, 1f);
            orientationChanged = true;
        }

        if (rightJoyStickValues.y > joyStickThreshold) //rotate up
        {
            projectedMeshAnchor.transform.RotateAround(projectedMeshAnchor.transform.position, Vector3.right, 1f);
            orientationChanged = true;
        }
        else if (rightJoyStickValues.y < -joyStickThreshold) //rotate down
        {
            projectedMeshAnchor.transform.RotateAround(projectedMeshAnchor.transform.position, Vector3.right, -1f);
            orientationChanged = true;
        }

        if (passthroughActive && currentActiveLayerForm == PassthroughLayerForm.Projected) //Only process transform updates when projected passthrough is active
        {
            if (scaleChanged)
            {
                PassthroughAPI.SetProjectedPassthroughScale(activePassthroughID, new Vector3(scale * scaleModifier, scale * scaleModifier, scale * scaleModifier));
                scaleChanged = false;
            }

            if (spaceTypeChanged)
            {
                PassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, currentActiveSpaceType);
                spaceTypeChanged = false;
            }

            if (positionChanged)
            {
                switch(currentActiveSpaceType)
                {
                    case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                        Vector3 relativePosition = hmd.transform.InverseTransformPoint(projectedMeshAnchor.transform.position);
                        PassthroughAPI.SetProjectedPassthroughMeshPosition(activePassthroughID, relativePosition, false);
                        break;
                    case ProjectedPassthroughSpaceType.Worldlock:
                    default:
                        PassthroughAPI.SetProjectedPassthroughMeshPosition(activePassthroughID, projectedMeshAnchor.transform.position);
                        break;
                }
                positionChanged = false;
            }

            if (orientationChanged)
            {
                switch (currentActiveSpaceType)
                {
                    case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                        Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * projectedMeshAnchor.transform.rotation.normalized;
                        PassthroughAPI.SetProjectedPassthroughMeshOrientation(activePassthroughID, relativeRotation, false);
                        break;
                    case ProjectedPassthroughSpaceType.Worldlock:
                    default:
                        PassthroughAPI.SetProjectedPassthroughMeshOrientation(activePassthroughID, projectedMeshAnchor.transform.rotation);
                        break;
                }
                orientationChanged = false;
            }
        }
    }

    public void CreateClassicOverlayPassthrough()
    {
        CreateClassicPassthrough(currentActiveLayerType);
    }

    private void CreateClassicPassthrough(LayerType layerType)
    {
        if (passthroughActive) return;

        PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, layerType, PassthroughSessionDestroyed, alpha);

        if (activePassthroughID != 0)
        {
            currentActiveLayerForm = PassthroughLayerForm.Planar;
            currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
        }
    }

    public void CreateQuadOverlayPassthrough()
    {
        CreateQuadProjectedPassthrough(currentActiveLayerType);
    }

    private void CreateQuadProjectedPassthrough(LayerType layerType)
    {
        if (passthroughActive) return;
             
        PassthroughAPI.CreateProjectedPassthrough(out activePassthroughID, layerType, PassthroughSessionDestroyed, alpha);

        if (activePassthroughID != 0)
        {
            currentActiveLayerForm = PassthroughLayerForm.Projected;
            currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
            SetQuadMesh();
        }
    }

    private void PassthroughSessionDestroyed(XrPassthroughHTC passthrough) //Handle destruction of passthrough layer when OpenXR session is destroyed
    {
        PassthroughAPI.DestroyPassthrough(passthrough);
        activePassthroughID = 0;
    }

    public void SetQuadMesh()
    {
        if (passthroughActive && activePassthroughID != 0 && currentActiveLayerForm == PassthroughLayerForm.Projected)
        {
            PassthroughAPI.SetProjectedPassthroughMesh(activePassthroughID, quadVertices, quadIndicies, false);
            switch (currentActiveSpaceType)
            {
                case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                    Vector3 relativePosition = hmd.transform.InverseTransformPoint(projectedMeshAnchor.transform.position);
                    Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * projectedMeshAnchor.transform.rotation.normalized;
                    PassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, relativePosition, relativeRotation, new Vector3(scale * scaleModifier, scale * scaleModifier, scale * scaleModifier), false);
                    break;
                case ProjectedPassthroughSpaceType.Worldlock:
                default:
                    PassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, projectedMeshAnchor.transform.position, projectedMeshAnchor.transform.rotation, new Vector3(scale * scaleModifier, scale * scaleModifier, scale * scaleModifier));
                    break;
            }
        }
    }

    public void SetUnityMesh(Mesh mesh)
    {
        if (passthroughActive && activePassthroughID != 0 && currentActiveLayerForm == PassthroughLayerForm.Projected)
        {
            PassthroughAPI.SetProjectedPassthroughMesh(activePassthroughID, mesh.vertices, mesh.triangles, true);
            switch (currentActiveSpaceType)
            {
                case ProjectedPassthroughSpaceType.Headlock: //Apply HMD offset
                    Vector3 relativePosition = hmd.transform.InverseTransformPoint(projectedMeshAnchor.transform.position);
                    Quaternion relativeRotation = Quaternion.Inverse(hmd.transform.rotation).normalized * projectedMeshAnchor.transform.rotation.normalized;
                    PassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, relativePosition, relativeRotation, new Vector3(scale * scaleModifier, scale * scaleModifier, scale * scaleModifier), false);
                    break;
                case ProjectedPassthroughSpaceType.Worldlock:
                default:
                    PassthroughAPI.SetProjectedPassthroughMeshTransform(activePassthroughID, currentActiveSpaceType, projectedMeshAnchor.transform.position, projectedMeshAnchor.transform.rotation, new Vector3(scale * scaleModifier, scale * scaleModifier, scale * scaleModifier));
                    break;
            }
        }
    }

    public void SetPassthroughToOverlay()
    {
        if (passthroughActive && activePassthroughID != 0)
        {
            PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Overlay);
            currentActiveLayerType = LayerType.Overlay;
        }
    }

    public void SetPassthroughToUnderlay()
    {
        if (passthroughActive && activePassthroughID != 0)
        {
            PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
            currentActiveLayerType = LayerType.Underlay;
        }
    }

    public void SetHeadLock()
    {
        if (passthroughActive && activePassthroughID != 0 && currentActiveLayerForm == PassthroughLayerForm.Projected)
        {
            if (PassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, ProjectedPassthroughSpaceType.Headlock))
            {
                positionChanged = orientationChanged = spaceTypeChanged = true;

                projectedMeshAnchor.transform.SetParent(hmd.transform);

                currentActiveSpaceType = ProjectedPassthroughSpaceType.Headlock;
            }
        }
    }

    public void SetWorldLock()
    {
        if (passthroughActive && activePassthroughID != 0 && currentActiveLayerForm == PassthroughLayerForm.Projected)
        {
            if (PassthroughAPI.SetProjectedPassthroughSpaceType(activePassthroughID, ProjectedPassthroughSpaceType.Worldlock))
            {
                positionChanged = orientationChanged = spaceTypeChanged = true;

                projectedMeshAnchor.transform.SetParent(null);

                currentActiveSpaceType = ProjectedPassthroughSpaceType.Worldlock;
            }
        }
    }

    private void DisablePassthrough()
    {
        if (passthroughActive && activePassthroughID != 0)
        {
            PassthroughAPI.DestroyPassthrough(activePassthroughID);
            activePassthroughID = 0;
        }
    }

    public void OnAlphaSliderValueChange(float newAlpha)
    {
        alphaValueText.text = newAlpha.ToString();
        alpha = newAlpha;
        if (passthroughActive && activePassthroughID != 0)
        {
            PassthroughAPI.SetPassthroughAlpha(activePassthroughID, newAlpha);
        }
    }

    public void OnScaleSliderValueChange(float newScaleModifier)
    {
        scaleValueText.text = newScaleModifier.ToString();
        if (passthroughActive && activePassthroughID != 0)
        {
            scaleModifier = newScaleModifier;
            scaleChanged = true;
        }
    }


}
