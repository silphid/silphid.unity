using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Silphid.Extensions
{
    public static class DOTweenExtensions
    {
        public static Tween TweenTo(this ReactiveProperty<float> This, float endValue, float duration) =>
            DOTween.To(() => This.Value, x => This.Value = x, endValue, duration);

        public static Tween DOFadeOut(this Graphic This, float duration)
        {
            if (!This.enabled)
                duration = 0;

            return DOTween.ToAlpha(() => This.color, x => This.color = x, 0, duration).SetTimeScaleIndependent();
        }

        public static Tween DOFadeOutAndHide(this Graphic This, float duration)
        {
            if (!This.enabled)
                duration = 0;

            return
                DOTween.ToAlpha(() => This.color, x => This.color = x, 0, duration)
                    .OnComplete(() => This.enabled = false)
                    .SetTimeScaleIndependent();
        }

        public static Tween DOShowAndFadeIn(this Graphic This, float duration)
        {
            if (This.enabled)
                duration = 0;

            This.enabled = true;
            return DOTween.ToAlpha(() => This.color, x => This.color = x, 1, duration).SetTimeScaleIndependent();
        }

        public static TweenerCore<float, float, FloatOptions> DOFadeTo(this CanvasGroup This, float target,
            float duration)
        {
            return DOTween.To(() => This.alpha, value => This.alpha = value, target, duration).SetTarget(This);
        }

        public static TweenerCore<float, float, FloatOptions> DOFadeIn(this CanvasGroup This, float duration)
        {
            return This.DOFadeTo(1, duration);
        }

        public static TweenerCore<float, float, FloatOptions> DOFadeOut(this CanvasGroup This, float duration)
        {
            return This.DOFadeTo(0, duration);
        }


        public static Tweener DOFade(this Renderer This, float endValue, float duration)
        {
            var spriteRenderer = GetSpriteRenderer(This);
            return
                DOTween.ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, endValue, duration)
                    .SetUpdate(UpdateType.Normal, true);
        }

        public static Tween DOFadeOut(this Renderer This, float duration)
        {
            var spriteRenderer = GetSpriteRenderer(This);
            if (!spriteRenderer.enabled && spriteRenderer.color.a.IsAlmostZero())
                duration = 0;

            return
                DOTween.ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, 0, duration)
                    .OnComplete(() => { spriteRenderer.enabled = false; })
                    .SetTimeScaleIndependent();
        }

        public static Tween DOFadeOutAndHide(this Renderer This, float duration)
        {
            var spriteRenderer = GetSpriteRenderer(This);
            if (!spriteRenderer.enabled && spriteRenderer.color.a.IsAlmostZero())
                duration = 0;

            return
                DOTween.ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, 0, duration)
                    .OnComplete(() => { spriteRenderer.gameObject.SetActive(false); })
                    .SetTimeScaleIndependent();
        }

        public static Tween DOShowAndFadeIn(this Renderer This, float duration)
        {
            var spriteRenderer = GetSpriteRenderer(This);
            if (spriteRenderer.enabled && spriteRenderer.color.a.IsAlmostEqualTo(1f))
                duration = 0;

            This.enabled = true;
            spriteRenderer.gameObject.SetActive(true);
            return
                DOTween.ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, 1, duration)
                    .SetTimeScaleIndependent();
        }

        public static Tween SetTimeScaleIndependent(this Tween This, bool isIndependent = true)
        {
            return This.SetUpdate(isIndependent);
        }

        public static Sequence SetTimeScaleIndependent(this Sequence This, bool isIndependent = true)
        {
            return This.SetUpdate(isIndependent);
        }

        private static SpriteRenderer GetSpriteRenderer(Renderer renderer)
        {
            var spriteRenderer = renderer as SpriteRenderer;
            if (spriteRenderer == null)
                throw new NotSupportedException("Renderer must be a SpriteRenderer");
            return spriteRenderer;
        }

        public static IDisposable AsDisposable(this Tween This, bool complete = false) => 
            Disposable.Create(() => This.Kill(complete));
    }
}