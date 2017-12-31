#if DOTWEEN

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
        private static IObservable<Unit> TweenTo<T>(this IReactiveProperty<T> This, Func<Tween> selector, Ease ease = Ease.Linear, bool completeTweenOnDispose = false) =>
            Observable.Create<Unit>(subscriber =>
            {
                var tween = selector()
                    .SetEase(ease)
                    .OnComplete(() =>
                    {
                        subscriber.OnNext(Unit.Default);
                        subscriber.OnCompleted();
                    });

                return Disposable.Create(() => tween.Kill(completeTweenOnDispose));
            });

        public static IObservable<Unit> TweenTo(this IReactiveProperty<float> This, float target, float duration, Ease ease = Ease.Linear, bool completeTweenOnDispose = false) =>
            This.TweenTo(() => DOTween.To(() => This.Value, x => This.Value = x, target, duration), ease, completeTweenOnDispose);

        public static IObservable<Unit> TweenTo(this IReactiveProperty<float?> This, float target, float duration, Ease ease = Ease.Linear, bool completeTweenOnDispose = false) =>
            This.TweenTo(() => DOTween.To(() => This.Value ?? 0f, x => This.Value = x, target, duration), ease, completeTweenOnDispose);

        public static IObservable<Unit> TweenTo(this IReactiveProperty<TimeSpan> This, TimeSpan target, float duration, Ease ease = Ease.Linear, bool completeTweenOnDispose = false) =>
            This.TweenTo(() => DOTween.To(() => This.Value.Ticks, x => This.Value = TimeSpan.FromTicks(x), target.Ticks, duration), ease, completeTweenOnDispose);

        public static IObservable<Unit> TweenTo(this IReactiveProperty<TimeSpan?> This, TimeSpan target, float duration, Ease ease = Ease.Linear, bool completeTweenOnDispose = false) =>
            This.TweenTo(() => DOTween.To(() => This.Value?.Ticks ?? 0, x => This.Value = TimeSpan.FromTicks(x), target.Ticks, duration), ease, completeTweenOnDispose);

        public static Tweener ToAlpha(DOGetter<Color> getter, DOSetter<Color> setter, float endValue, float duration)
        {
            var color = getter();
            return DOTween.To(() => color.a, x => setter(new Color(color.r, color.g, color.b, x)), endValue, duration);
        }

        public static Tween DOFadeOut(this Graphic This, float duration)
        {
            if (!This.enabled)
                duration = 0;

            return ToAlpha(() => This.color, x => This.color = x, 0, duration).SetTimeScaleIndependent();
        }

        public static Tween DOFadeOutAndHide(this Graphic This, float duration)
        {
            if (!This.enabled)
                duration = 0;

            return
                ToAlpha(() => This.color, x => This.color = x, 0, duration)
                    .OnComplete(() => This.enabled = false)
                    .SetTimeScaleIndependent();
        }

        public static Tween DOShowAndFadeIn(this Graphic This, float duration)
        {
            if (This.enabled)
                duration = 0;

            This.enabled = true;
            return ToAlpha(() => This.color, x => This.color = x, 1, duration).SetTimeScaleIndependent();
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
                ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, endValue, duration)
                    .SetUpdate(UpdateType.Normal, true);
        }

        public static Tween DOFadeOut(this Renderer This, float duration)
        {
            var spriteRenderer = GetSpriteRenderer(This);
            if (!spriteRenderer.enabled && spriteRenderer.color.a.IsAlmostZero())
                duration = 0;

            return
                ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, 0, duration)
                    .OnComplete(() => { spriteRenderer.enabled = false; })
                    .SetTimeScaleIndependent();
        }

        public static Tween DOFadeOutAndHide(this Renderer This, float duration)
        {
            var spriteRenderer = GetSpriteRenderer(This);
            if (!spriteRenderer.enabled && spriteRenderer.color.a.IsAlmostZero())
                duration = 0;

            return
                ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, 0, duration)
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
                ToAlpha(() => spriteRenderer.color, x => spriteRenderer.color = x, 1, duration)
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

#endif