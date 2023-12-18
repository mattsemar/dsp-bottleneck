using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Bottleneck.UI
{
    public class BottleneckProductEntryElement
    {
        public UIButton precursorButton;
        public UIButton successorButton;
    }

    public static class Util
    {
        public static UIButton CopyButton(UIProductEntry uiProductEntry, UIButton button, Vector2 positionDelta, int entryDataItemId, Action<int> action,
            Sprite btnSprite)
        {
            var rectTransform = button.GetComponent<RectTransform>();
            var copied = Object.Instantiate(rectTransform, uiProductEntry.transform, false);
            var copiedImage = copied.transform.GetComponent<Image>();
            copiedImage.sprite = btnSprite;
            copiedImage.fillAmount = 0;

            copied.anchorMin = rectTransform.anchorMin;
            copied.anchorMax = rectTransform.anchorMax;
            copied.sizeDelta = rectTransform.sizeDelta * 0.75f;
            copied.anchoredPosition = rectTransform.anchoredPosition + positionDelta;
            var mainActionButton = copied.GetComponentInChildren<UIButton>();
            if (mainActionButton != null)
            {
                var productName = GetItemName(entryDataItemId);
                mainActionButton.tips.tipTitle = $"{productName} made on";
                mainActionButton.tips.tipText = "";
                mainActionButton.button.onClick.RemoveAllListeners();
                mainActionButton.button.onClick.AddListener(() => action.Invoke(1));
                mainActionButton.highlighted = false;
                mainActionButton.Init();
            }

            return mainActionButton;
        }

        private static string GetItemName(int itemId)
        {
            return LDB.items.Select(itemId).Name.Translate();
        }
    }
}