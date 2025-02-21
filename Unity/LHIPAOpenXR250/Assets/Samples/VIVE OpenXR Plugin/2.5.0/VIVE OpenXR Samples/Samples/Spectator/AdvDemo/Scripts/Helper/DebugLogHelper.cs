// Copyright HTC Corporation All Rights Reserved.

using System;
using UnityEngine;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo.Helper
{
    public static class DebugHelper
    {
        #region Custom variable

        public static float UpdateFreqInSecond { get; set; } = 5f;

        #endregion

        #region Default varible in UnityEngine.Debug Class

        public static bool IsDebugBuild => Debug.isDebugBuild;

        public static bool DeveloperConsoleVisible
        {
            get => Debug.developerConsoleVisible;
            set => Debug.developerConsoleVisible = value;
        }

        public static ILogger Logger => Debug.unityLogger;

        #endregion

        #region Custom function

        public static void DisableLog()
        {
            Debug.unityLogger.logEnabled = false;
        }

        #endregion

        #region Default function implementation in UnityEngine.Debug Class

        public static void LogWithFrequently(ref float timer, object message, UnityEngine.Object context = null)
        {
            if (timer < UpdateFreqInSecond)
            {
                // time is not reach on the update frequency
                // add now frame time and return
                timer += Time.deltaTime;
                return;
            }

            // time is reach on the update frequency
            // reset timer and then log
            timer = 0f;

            if (context == null)
            {
                Log(message);
            }
            else
            {
                Log(message, context);
            }
        }

        public static void Log(object message)
        {
            Debug.Log(message);
        }

        public static void Log(object message, UnityEngine.Object context)
        {
            Debug.Log(message, context);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        public static void DrawLine(Vector3 start, Vector3 end)
        {
            Debug.DrawLine(start, end);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            Debug.DrawLine(start, end, color);
        }

        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0.0f,
            bool depthTest = true)
        {
            Debug.DrawRay(start, dir, color, duration);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogWarning(object message, UnityEngine.Object context)
        {
            Debug.LogWarning(message, context);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public static void LogException(Exception exception)
        {
            Debug.LogException(exception);
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            Debug.LogException(exception, context);
        }

        #endregion
    }
}