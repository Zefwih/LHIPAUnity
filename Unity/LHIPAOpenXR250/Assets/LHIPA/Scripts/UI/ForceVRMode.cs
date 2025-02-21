using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class ForceVRMode : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckXRSubsystems());
    }

    IEnumerator CheckXRSubsystems()
    {
        Debug.Log("Checking available XR subsystems...");

        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displaySubsystems);

        if (displaySubsystems.Count == 0)
        {
            Debug.LogError("No XR Display Subsystem found! OpenXR might not be initialized.");
            yield break;
        }

        foreach (var subsystem in displaySubsystems)
        {
            Debug.Log($"Found XR Display Subsystem: {subsystem.SubsystemDescriptor.id}");
        }

        XRDisplaySubsystem xrDisplay = displaySubsystems[0];

        if (!xrDisplay.running)
        {
            Debug.Log("Trying to start XR Display Subsystem...");
            xrDisplay.Start();
        }

        yield return new WaitForSeconds(2);

        if (xrDisplay.running)
        {
            Debug.Log("XR Display Subsystem successfully started!");
        }
        else
        {
            Debug.LogError("Failed to start XR Display Subsystem. Check OpenXR settings.");
        }

        Debug.Log("Checking XR General Settings...");
        XRGeneralSettings xrGeneralSettings = XRGeneralSettings.Instance;
        if (xrGeneralSettings == null || xrGeneralSettings.Manager == null)
        {
            Debug.LogError("XR General Settings or XR Manager is NULL. OpenXR is not initialized!");
            yield break;
        }

        if (xrGeneralSettings.Manager.activeLoader == null)
        {
            Debug.LogError("No active XR Loader found. OpenXR is not properly set up!");
        }
        else
        {
            Debug.Log($"Active XR Loader: {xrGeneralSettings.Manager.activeLoader.name}");
        }
    }
}
