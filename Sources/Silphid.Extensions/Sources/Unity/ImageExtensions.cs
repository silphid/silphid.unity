using System;
using UnityEngine.UI;

namespace Silphid.Extensions
{
    public static class ImageExtensions
    {
        public static void Show(this Image image)
        {
            SetVisible(image, true);
        }

        public static void Hide(this Image image)
        {
            SetVisible(image, false);
        }

        public static void SetVisible(this Image image, bool show)
        {
            image.enabled = show;
            image.color = image.color.WithAlpha(show ? 1 : 0);            
        }

        public static void SetAlpha(this Image image, float alpha)
        {
            image.enabled = (alpha > Single.Epsilon);
            image.color = image.color.WithAlpha(alpha);            
        }
    }
}