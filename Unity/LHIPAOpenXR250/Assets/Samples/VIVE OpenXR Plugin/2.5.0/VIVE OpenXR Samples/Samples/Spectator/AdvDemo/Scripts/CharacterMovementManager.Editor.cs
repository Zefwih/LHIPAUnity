// Copyright HTC Corporation All Rights Reserved.

#if UNITY_EDITOR
using UnityEditor;
using VIVE.OpenXR.Toolkits.Spectator.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: CharacterMovementManager.Editor.cs
    /// Role: General script only use in Unity Editor
    /// Responsibility: Display the CharacterMovementManager.cs in Unity Inspector
    /// </summary>
    public partial class CharacterMovementManager
    {
        [CustomEditor(typeof(CharacterMovementManager))]
        public class CharacterMovementManagerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                if (!(target is CharacterMovementManager))
                {
                    // Just return if not "CharacterMovementManager" class
                    return;
                }

                EditorHelper.ShowDefaultInspector(serializedObject);
            }
        }
    }
}
#endif