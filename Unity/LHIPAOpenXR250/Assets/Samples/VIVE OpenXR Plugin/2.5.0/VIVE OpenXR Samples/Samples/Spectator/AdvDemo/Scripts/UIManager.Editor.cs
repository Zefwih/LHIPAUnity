// Copyright HTC Corporation All Rights Reserved.

#if UNITY_EDITOR
using UnityEditor;
using VIVE.OpenXR.Toolkits.Spectator.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: UIManager.Editor.cs
    /// Role: General script only use in Unity Editor
    /// Responsibility: Display the UIManager.cs in Unity Inspector
    /// </summary>
    public partial class UIManager
    {
        [CustomEditor(typeof(UIManager))]
        public class UIManagerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                // just return if not "SpectatorCameraController" class
                if (!(target is UIManager))
                {
                    return;
                }

                EditorHelper.ShowDefaultInspector(serializedObject);
            }
        }
    }
}

#endif