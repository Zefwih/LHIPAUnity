// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using VIVE.OpenXR.CompositionLayer;

namespace VIVE.OpenXR.Samples.CompositionLayer
{
    public class Actions : MonoBehaviour
    {
        static private XrSharpeningModeHTC mode = XrSharpeningModeHTC.FAST;
        static private float level = 1.0f;
        static private GameObject button01, button02;

        // Start is called before the first frame update
        void Start()
        {
            button01 = GameObject.FindGameObjectWithTag("SharpenMode");
            button01.GetComponentInChildren<Text>().text = "Mode : ";
            button02 = GameObject.FindGameObjectWithTag("SharpenLevel");
            button02.GetComponentInChildren<Text>().text = "Level : ";
        }

        static public void SetConfig1()
        {
            mode = XrSharpeningModeHTC.FAST;
            button01.GetComponentInChildren<Text>().text = "Mode : " + mode.ToString();
        }

        static public void SetConfig2()
        {
            mode = XrSharpeningModeHTC.NORMAL;
            button01.GetComponentInChildren<Text>().text = "Mode : " + mode.ToString();
        }

        static public void SetConfig3()
        {
            mode = XrSharpeningModeHTC.QUALITY;
            button01.GetComponentInChildren<Text>().text = "Mode : " + mode.ToString();
        }

        static public void SetConfig4()
        {
            mode = XrSharpeningModeHTC.AUTOMATIC;
            button01.GetComponentInChildren<Text>().text = "Mode : " + mode.ToString();
        }

        static public void SetLevel1()
        {
            level = 1.0f;
            button02.GetComponentInChildren<Text>().text = "Level : " + level.ToString();
        }

        static public void SetLevel2()
        {
            level = 0.8f;
            button02.GetComponentInChildren<Text>().text = "Level : " + level.ToString();
        }

        static public void SetLevel3()
        {
            level = 0.5f;
            button02.GetComponentInChildren<Text>().text = "Level : " + level.ToString();
        }

        static public void SetLevel4()
        {
            level = 0.2f;
            button02.GetComponentInChildren<Text>().text = "Level : " + level.ToString();
        }

        static public void Disactivate()
        {
            XR_HTC_composition_layer_extra_settings.Interop.DisableSharpening();
        }

        static public void Activate()
        {
            XR_HTC_composition_layer_extra_settings.Interop.EnableSharpening(mode, level);
        }
    }
}