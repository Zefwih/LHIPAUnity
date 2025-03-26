using UnityEngine;
using UnityEditor;

namespace lhipa
{
    // The custom editor class should be in an Editor folder within Assets
    [CustomEditor(typeof(LHIPATester))]
    public class LHIPATesterEditor : Editor
    {
        Texture2D logoTexture;

        void OnEnable()
        {
            // Load the logo texture from the Images folder within the Assets directory
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LHIPA/Scripts/Editor/ResearchThemeBanner.png");

            if (logoTexture == null)
            {
                Debug.Log("Failed to load logo texture.");
            }

        }

        public override void OnInspectorGUI()
        {
            // If the logo texture has been loaded, draw it at the top of the inspector
            if (logoTexture != null)
            {
                GUILayout.Space(5);
                float logoWidth = EditorGUIUtility.currentViewWidth;
                float logoHeight = logoWidth / logoTexture.width * logoTexture.height;
                Rect logoRect = new Rect(10, 10, logoWidth, logoHeight);
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
                GUILayout.Space(logoHeight);
            }

            // Draw the default inspector after the logo
            DrawDefaultInspector();
        }
    }
    
    [CustomEditor(typeof(LHIPATesterUI))]
    public class LHIPATesterUIEditor : Editor
    {
        Texture2D logoTexture;

        void OnEnable()
        {
            // Load the logo texture from the Images folder within the Assets directory
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LHIPA/Scripts/Editor/ResearchThemeBanner.png");

            if (logoTexture == null)
            {
                Debug.Log("Failed to load logo texture.");
            }

        }

        public override void OnInspectorGUI()
        {
            // If the logo texture has been loaded, draw it at the top of the inspector
            if (logoTexture != null)
            {
                GUILayout.Space(5);
                float logoWidth = EditorGUIUtility.currentViewWidth;
                float logoHeight = logoWidth / logoTexture.width * logoTexture.height;
                Rect logoRect = new Rect(10, 10, logoWidth, logoHeight);
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
                GUILayout.Space(logoHeight);
            }

            // Draw the default inspector after the logo
            DrawDefaultInspector();
        }
    }
    
    [CustomEditor(typeof(EyeTrackSimulation))]
    public class EyeTrackSimulationEditor : Editor
    {
        Texture2D logoTexture;

        void OnEnable()
        {
            // Load the logo texture from the Images folder within the Assets directory
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LHIPA/Scripts/Editor/ResearchThemeBanner.png");

            if (logoTexture == null)
            {
                Debug.Log("Failed to load logo texture.");
            }

        }

        public override void OnInspectorGUI()
        {
            // If the logo texture has been loaded, draw it at the top of the inspector
            if (logoTexture != null)
            {
                GUILayout.Space(5);
                float logoWidth = EditorGUIUtility.currentViewWidth;
                float logoHeight = logoWidth / logoTexture.width * logoTexture.height;
                Rect logoRect = new Rect(10, 10, logoWidth, logoHeight);
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
                GUILayout.Space(logoHeight);
            }

            // Draw the default inspector after the logo
            DrawDefaultInspector();
        }
    }
    
    [CustomEditor(typeof(EyeTrackingSimulationUI))]
    public class EyeTrackingSimulationUIEditor : Editor
    {
        Texture2D logoTexture;

        void OnEnable()
        {
            // Load the logo texture from the Images folder within the Assets directory
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LHIPA/Scripts/Editor/ResearchThemeBanner.png");

            if (logoTexture == null)
            {
                Debug.Log("Failed to load logo texture.");
            }

        }

        public override void OnInspectorGUI()
        {
            // If the logo texture has been loaded, draw it at the top of the inspector
            if (logoTexture != null)
            {
                GUILayout.Space(5);
                float logoWidth = EditorGUIUtility.currentViewWidth;
                float logoHeight = logoWidth / logoTexture.width * logoTexture.height;
                Rect logoRect = new Rect(10, 10, logoWidth, logoHeight);
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
                GUILayout.Space(logoHeight);
            }

            // Draw the default inspector after the logo
            DrawDefaultInspector();
        }
    }

}