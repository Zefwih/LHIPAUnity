// Copyright HTC Corporation All Rights Reserved.

using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VIVE.OpenXR.Samples.Spectator.AdvDemo.Helper;

namespace VIVE.OpenXR.Samples.Spectator.AdvDemo
{
    /// <summary>
    /// Name: UIHint.cs
    /// Role: General script
    /// Responsibility: Perform the action when pointer is enter/exit/down/up/click
    /// </summary>
    public class UIHint : MonoBehaviour,
        IPointerEnterHandler
    // // For other action, feel free to uncomment the following code
    // // and then implement the corresponding interface
    // ,IPointerExitHandler
    // ,IPointerDownHandler
    // ,IPointerUpHandler
    // ,IPointerClickHandler
    {
        #region Const warning/error message

        private const string UiHintNotFoundWarningMessage =
            "Can not find the corresponding ui component hint.";

        private const string PageObjectNotFoundForHintTextObjectWarningMessage =
            "Can not find the corresponding page object for hint text object.";

        #endregion

        private static UIManager UIManager => UIManager.Instance;

        public void OnPointerEnter(PointerEventData eventData)
        {
            string uiComponentHint = string.Empty;

            foreach (var item in UIManager.UIHintStaticResource.UiComponentKeywordAndHintPair)
            {
                if (GameObjectProcessHelper.CheckGameObjectNameIsContainTargetString(transform, item.Key))
                {
                    uiComponentHint = UIManager.UIHintStaticResource.UiComponentKeywordAndHintPair[item.Key];
                }
            }

            if (string.IsNullOrEmpty(uiComponentHint))
            {
                Debug.LogWarning(UiHintNotFoundWarningMessage);
                return;
            }

            for (int i = 0; i < UIManager.UIHintStaticResource.PageNameAndPageRootObjectPair.Count; i++)
            {
                var item = UIManager.UIHintStaticResource.PageNameAndPageRootObjectPair.ElementAt(i);
                if (!transform.IsChildOf(item.Value.transform))
                {
                    // if the index is the last one, it means the page object is not found
                    if (i == UIManager.UIHintStaticResource.PageNameAndPageRootObjectPair.Count - 1)
                    {
                        Debug.LogWarning(PageObjectNotFoundForHintTextObjectWarningMessage);
                    }

                    continue;
                }

                Debug.Log($"Find the corresponding page {item.Value}");
                Debug.Log($"Show the UI hint: {uiComponentHint}");
                UIManager.UIHintStaticResource.PageNameAndHintTextObjectPair[item.Key].GetComponent<TextMeshProUGUI>()
                    .text = uiComponentHint;
                break;
            }
        }

        // // For other action, feel free to uncomment the following code
        // // and then implement the corresponding interface
        // public void OnPointerExit(PointerEventData eventData)
        // {
        // }
        //
        // public void OnPointerDown(PointerEventData eventData)
        // {
        // }
        //
        // public void OnPointerUp(PointerEventData eventData)
        // {
        // }
        //
        // public void OnPointerClick(PointerEventData eventData)
        // {
        // }
    }
}