using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;

public class Passthrough_manager : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    //public Slider slider;

    private VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID = 0;
    private LayerType currentActiveLayerType = LayerType.Underlay;

    static UInt32 count = 0;
    static XrPassthroughConfigurationImageRateHTC[] arrays = new XrPassthroughConfigurationImageRateHTC[2];
    static XrPassthroughConfigurationImageRateHTC config_rate = new XrPassthroughConfigurationImageRateHTC();
    static XrPassthroughConfigurationImageQualityHTC config_quality = new XrPassthroughConfigurationImageQualityHTC();

    static bool inited = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Passthrough_manager EnumeratePassthroughImageRatesHTC");
        XrResult result = XR_HTC_passthrough_configuration.EnumeratePassthroughImageRatesHTC(imageRateCapacityInput: 0, imageRateCountOutput: ref count, imageRates: arrays);
        Debug.Log("Passthrough_manager EnumeratePassthroughImageRatesHTC = " + count);
        if (result == XrResult.XR_SUCCESS)
        {
            Array.Resize(ref arrays, (int)count);
            List<String> Options = new List<String>();
            result = XR_HTC_passthrough_configuration.EnumeratePassthroughImageRatesHTC(imageRateCapacityInput: count, imageRateCountOutput: ref count, imageRates: arrays);

            if (result == XrResult.XR_SUCCESS)
            {
                for (int i = 0; i < count; i++)
                {
                    Debug.Log("Passthrough_manager EnumeratePassthroughImageRatesHTC index " + i + " srcImageRate = " + arrays[i].srcImageRate);
                    Debug.Log("Passthrough_manager EnumeratePassthroughImageRatesHTC index " + i + " dstImageRate = " + arrays[i].dstImageRate);
                    Options.Add(new String(arrays[i].srcImageRate + "/" + arrays[i].dstImageRate));
                }
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(Options);
            Debug.Log("Passthrough_manager count = " + dropdown.options.Count);
        }
        else
        {
            Debug.Log("Passthrough_manager EnumeratePassthroughImageRatesHTC result = " + result);
        }
        config_rate.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_RATE_HTC;
        config_rate.next = IntPtr.Zero;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageRateHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config_rate, ptr, false);
        if (XR_HTC_passthrough_configuration.GetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            config_rate = (XrPassthroughConfigurationImageRateHTC)Marshal.PtrToStructure(ptr, typeof(XrPassthroughConfigurationImageRateHTC));
            Debug.Log("Passthrough_manager get srcImageRate = " + config_rate.srcImageRate);
            Debug.Log("Passthrough_manager get dstImageRate = " + config_rate.dstImageRate);
        }
        Marshal.FreeHGlobal(ptr);

        config_quality.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_QUALITY_HTC;
        config_quality.next = IntPtr.Zero;

        int size2 = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageQualityHTC));
        IntPtr ptr2 = Marshal.AllocHGlobal(size2);

        Marshal.StructureToPtr(config_quality, ptr2, false);
        if (XR_HTC_passthrough_configuration.GetPassthroughConfigurationHTC(ptr2) == XrResult.XR_SUCCESS)
        {
            config_quality = (XrPassthroughConfigurationImageQualityHTC)Marshal.PtrToStructure(ptr2, typeof(XrPassthroughConfigurationImageQualityHTC));
            Debug.Log("Passthrough_manager get scale = " + config_quality.scale);
        }
        Marshal.FreeHGlobal(ptr2);
        //slider.value = config_quality.scale;

        dropdown.onValueChanged.AddListener(DropdownValueChanged);
    }

    static private void OnImageQualityChanged(float fromQuality, float toQuality)
	{
        Debug.Log("Passthrough_manager from scale = " + fromQuality);
        Debug.Log("Passthrough_manager to scale = " + toQuality);
    }

    static private void OnImageRateChanged(float fromSrcImageRate, float fromDestImageRate, float toSrcImageRate, float toDestImageRate)
    {
        Debug.Log("Passthrough_manager fromSrcImageRate = " + fromSrcImageRate);
        Debug.Log("Passthrough_manager fromDestImageRate = " + fromDestImageRate);
        Debug.Log("Passthrough_manager toSrcImageRate = " + toSrcImageRate);
        Debug.Log("Passthrough_manager toDestImageRate = " + toDestImageRate);
    }

    static public void Set_Quality1()
	{
        VivePassthroughImageQualityChanged.Listen(OnImageQualityChanged);

        XrPassthroughConfigurationImageQualityHTC config = new XrPassthroughConfigurationImageQualityHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_QUALITY_HTC;
        config.next = IntPtr.Zero;
        config.scale = 0.25f;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageQualityHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set scale = " + config.scale);
        }
        Marshal.FreeHGlobal(ptr);
    }

    static public void Set_Quality2()
    {
        VivePassthroughImageQualityChanged.Listen(OnImageQualityChanged);

        XrPassthroughConfigurationImageQualityHTC config = new XrPassthroughConfigurationImageQualityHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_QUALITY_HTC;
        config.next = IntPtr.Zero;
        config.scale = 0.50f;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageQualityHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set scale = " + config.scale);
        }
        Marshal.FreeHGlobal(ptr);
    }

    static public void Set_Quality3()
    {
        VivePassthroughImageQualityChanged.Listen(OnImageQualityChanged);

        XrPassthroughConfigurationImageQualityHTC config = new XrPassthroughConfigurationImageQualityHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_QUALITY_HTC;
        config.next = IntPtr.Zero;
        config.scale = 0.75f;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageQualityHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set scale = " + config.scale);
        }
        Marshal.FreeHGlobal(ptr);
    }

    static public void Set_Quality4()
    {
        VivePassthroughImageQualityChanged.Listen(OnImageQualityChanged);

        XrPassthroughConfigurationImageQualityHTC config = new XrPassthroughConfigurationImageQualityHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_QUALITY_HTC;
        config.next = IntPtr.Zero;
        config.scale = 1.00f;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageQualityHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set scale = " + config.scale);
        }
        Marshal.FreeHGlobal(ptr);
    }

    static public void DropdownValueChanged(int idx)
    {
        VivePassthroughImageRateChanged.Listen(OnImageRateChanged);

        Debug.Log("Passthrough_manager dropdown value = " + idx);
        XrPassthroughConfigurationImageRateHTC config = new XrPassthroughConfigurationImageRateHTC();
        config.type = XrStructureType.XR_TYPE_PASSTHROUGH_CONFIGURATION_IMAGE_RATE_HTC;
        config.next = IntPtr.Zero;
        config.srcImageRate = arrays[idx].srcImageRate;
        config.dstImageRate = arrays[idx].dstImageRate;

        int size = Marshal.SizeOf(typeof(XrPassthroughConfigurationImageRateHTC));
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(config, ptr, false);
        if (XR_HTC_passthrough_configuration.SetPassthroughConfigurationHTC(ptr) == XrResult.XR_SUCCESS)
        {
            Debug.Log("Passthrough_manager set srcImageRate = " + config.srcImageRate);
            Debug.Log("Passthrough_manager set dstImageRate = " + config.dstImageRate);
        }
        Marshal.FreeHGlobal(ptr);
    }

    // Update is called once per frame
    void Update()
    {
        if (!inited)
		{
            inited = true;
        }
    }

    void StartPassthrough()
    {
        PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
    }

    void OnDestroyPassthroughFeatureSession(VIVE.OpenXR.Passthrough.XrPassthroughHTC passthroughID)
    {
        PassthroughAPI.DestroyPassthrough(passthroughID);
        activePassthroughID = 0;
    }

    public void EnablePassthrough()
	{
        if (activePassthroughID == 0)
        {
            StartPassthrough();
        }
    }

    public void DisablePassthrough()
	{
        if (activePassthroughID != 0)
        {
            PassthroughAPI.DestroyPassthrough(activePassthroughID);
            activePassthroughID = 0;
        }
        //dropdown.onValueChanged.RemoveListener(DropdownValueChanged);
    }
}
