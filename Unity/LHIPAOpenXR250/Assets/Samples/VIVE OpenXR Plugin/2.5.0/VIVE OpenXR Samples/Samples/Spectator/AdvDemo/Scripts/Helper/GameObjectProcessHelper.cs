// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo.Helper
{
    public static class GameObjectProcessHelper
    {
        public class GameObjectWithComponentKeyWords<T> where T : Component
        {
            private static GameObjectWithComponentKeyWords<T> _instance;

            public static GameObjectWithComponentKeyWords<T> Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        Debug.Log("Init GameObjectWithComponentKeyWords");
                        _instance = new GameObjectWithComponentKeyWords<T>();
                    }

                    return _instance;
                }
            }

            private const string SetUpErrorMessage =
                "Please set GameObjectKeyWords first.";

            public string GameObjectKeyWords { get; set; }

            private List<GameObject> _gameObjectList;

            public List<GameObject> GameObjectList
            {
                get
                {
                    if (string.IsNullOrEmpty(GameObjectKeyWords))
                    {
                        Debug.LogError(SetUpErrorMessage);
                        return null;
                    }

                    if (_gameObjectList == null)
                    {
                        _gameObjectList = GetGameObjectsWithComponentAndKeyWords<T>(GameObjectKeyWords);
                    }
                    else if (!_gameObjectList[0].name.Contains(GameObjectKeyWords))
                    {
                        _gameObjectList.Clear();
                        _gameObjectList = GetGameObjectsWithComponentAndKeyWords<T>(GameObjectKeyWords);
                    }

                    if (_gameObjectList == null)
                    {
                        Debug.LogError(
                            $"Cannot find any GameObject with {typeof(T).FullName} component and its name contain {GameObjectKeyWords} word in the scene.");
                    }

                    return _gameObjectList;
                }
            }
        }

        public static void SetLayerRecursively(Transform root, int targetLayer)
        {
            root.gameObject.layer = targetLayer;
            foreach (Transform child in root)
            {
                SetLayerRecursively(child, targetLayer);
            }
        }

        public static void RemoveComponent<TComponent>(this GameObject go)
        {
            var component = go.GetComponent<TComponent>();

            if (component != null)
            {
                Object.DestroyImmediate(component as Object, true);
            }
        }

        public static Transform FindParentRoot(Transform childTransform)
        {
            if (childTransform.parent == null)
            {
                Debug.Log($"Root is {childTransform.name}");
                return childTransform;
            }

            return FindParentRoot(childTransform.parent);
        }

        public static List<GameObject> GetGameObjectsWithComponent<T>() where T : Component
        {
            var gameObjectsWithComponentT = Object.FindObjectsOfType<T>();
            var result = gameObjectsWithComponentT.Select(item => item.gameObject).ToList();

            return result.Count == 0 ? null : result;
        }

        public static List<GameObject> GetGameObjectsWithComponentAndKeyWords<T>(string keyWords) where T : Component
        {
            var gameObjectsWithComponent = GetGameObjectsWithComponent<T>();
            if (gameObjectsWithComponent == null)
            {
                return null;
            }

            var result = gameObjectsWithComponent.Where(item => item.name.Contains(keyWords))
                .Select(item => item.gameObject).ToList();

            return result.Count == 0 ? null : result;
        }

        public static bool CheckGameObjectNameIsContainTargetString(Transform gameObjectNameNeedToCheck,
            string targetString)
        {
            return gameObjectNameNeedToCheck.name.Contains(targetString);
        }

        public static bool CheckGameObjectNameIsMatchTargetString(Transform gameObjectNameNeedToCheck,
            string targetString)
        {
            return string.Equals(
                gameObjectNameNeedToCheck.name,
                targetString,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}