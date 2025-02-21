// Copyright HTC Corporation All Rights Reserved.

#if UNITY_EDITOR
using UnityEditor;
using VIVE.OpenXR.Toolkits.Spectator.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: TeleportationManager.Editor.cs
    /// Role: General script only use in Unity Editor
    /// Responsibility: Display the TeleportationManager.cs in Unity Inspector
    /// </summary>
    public partial class TeleportationManager
    {
        [CustomEditor(typeof(TeleportationManager))]
        public class TeleportationManagerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                // Just return if not "TeleportationManager" class
                if (!(target is TeleportationManager))
                {
                    return;
                }

                EditorHelper.ShowDefaultInspector(serializedObject);
            }
        }
    }
}
#endif