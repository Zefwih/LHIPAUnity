// Copyright HTC Corporation All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.UI;  
using VIVE.OpenXR;

using XrFoveationModeHTC = VIVE.OpenXR.Foveation.XrFoveationModeHTC;
using XrFoveationLevelHTC = VIVE.OpenXR.Foveation.XrFoveationLevelHTC;
using XrFoveationConfigurationHTC = VIVE.OpenXR.Foveation.XrFoveationConfigurationHTC;

public class MyFoveatedTest : MonoBehaviour
{
	private float FOVLarge = 57;
    private float FOVSmall = 19;
    private float FOVMiddle = 38;
    private GameObject button01, button02, button03;
	
    public static XrFoveationConfigurationHTC config_left, config_right;
    public static XrFoveationConfigurationHTC[] configs = { config_left, config_right };

    MyFoveatedTest()
    {
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        configs[0].clearFovDegree = FOVLarge;
        configs[0].focalCenterOffset.x = 0.0f;
        configs[0].focalCenterOffset.y = 0.0f;
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        configs[1].clearFovDegree = FOVLarge;
        configs[1].focalCenterOffset.x = 0.0f;
        configs[1].focalCenterOffset.y = 0.0f;
    }

    void Start()
    {
        button01 = GameObject.FindGameObjectWithTag("EnableButton01");
        button02 = GameObject.FindGameObjectWithTag("EnableButton02");
        button03 = GameObject.FindGameObjectWithTag("EnableButton03");
    }

    public void FoveationIsDisable()
	{
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DISABLE_HTC, 0, null);
    }

	public void FoveationIsEnable()
	{
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_FIXED_HTC, 0, null);
    }

    bool FoveationIsDynamic_bit01 = true;
    bool FoveationIsDynamic_bit02 = true;
    bool FoveationIsDynamic_bit04 = true;

    public void FoveationIsDynamic_setbit01(){
        FoveationIsDynamic_bit01 = !FoveationIsDynamic_bit01;
        if (button01 != null)
            button01.GetComponentInChildren<Text>().text = FoveationIsDynamic_bit01 ? "01" : "00";
    }

    public void FoveationIsDynamic_setbit02(){
        FoveationIsDynamic_bit02 = !FoveationIsDynamic_bit02;
        if (button02 != null)
            button02.GetComponentInChildren<Text>().text = FoveationIsDynamic_bit02 ? "02" : "00";
    }

    public void FoveationIsDynamic_setbit04(){
        FoveationIsDynamic_bit04 = !FoveationIsDynamic_bit04;
        if (button03 != null)
           button03.GetComponentInChildren<Text>().text = FoveationIsDynamic_bit04 ? "04" : "00";
    }

    public void FoveationIsDynamic()
    {
        UInt64 flags = (FoveationIsDynamic_bit01 ? ViveFoveation.XR_FOVEATION_DYNAMIC_LEVEL_ENABLED_BIT_HTC : 0x00) |
            (FoveationIsDynamic_bit02 ? ViveFoveation.XR_FOVEATION_DYNAMIC_CLEAR_FOV_ENABLED_BIT_HTC : 0x00) |
            (FoveationIsDynamic_bit04 ? ViveFoveation.XR_FOVEATION_DYNAMIC_FOCAL_CENTER_OFFSET_ENABLED_BIT_HTC : 0x00);
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_DYNAMIC_HTC, 0, null, flags);
    }

    public void LeftClearVisionFOVHigh()
	{
        configs[0].clearFovDegree = FOVLarge;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftClearVisionFOVLow()
	{
        configs[0].clearFovDegree = FOVSmall;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftClearVisionFOVMiddle()
	{
        configs[0].clearFovDegree = FOVMiddle;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftEyePeripheralQualityHigh()
	{
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftEyePeripheralQualityLow()
	{
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void LeftEyePeripheralQualityMiddle()
	{
        configs[0].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightClearVisionFOVHigh()
	{
        configs[1].clearFovDegree = FOVLarge;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightClearVisionFOVLow()
	{
        configs[1].clearFovDegree = FOVSmall;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightClearVisionFOVMiddle()
	{
        configs[1].clearFovDegree = FOVMiddle;

        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightEyePeripheralQualityHigh()
	{
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_HIGH_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightEyePeripheralQualityLow()
	{
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_LOW_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }

	public void RightEyePeripheralQualityMedium()
	{
        configs[1].level = XrFoveationLevelHTC.XR_FOVEATION_LEVEL_MEDIUM_HTC;
        ViveFoveation.ApplyFoveationHTC(XrFoveationModeHTC.XR_FOVEATION_MODE_CUSTOM_HTC, 2, configs);
    }
}
