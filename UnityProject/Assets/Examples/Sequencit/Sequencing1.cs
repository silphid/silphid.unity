using System;
using DG.Tweening;
using Silphid.Extensions;
using Silphid.Sequencit;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Rx = UniRx;
using Sequence = Silphid.Sequencit.Sequence;

public class Sequencing1 : MonoBehaviour
{
    // Connections

    public Button StartButton;
    public Button CancelButton;
    public Text Text;
    public GameObject Cube;

    // Configs

    public float RotateDuration = 0.5f;
    public float MoveDuration = 0.4f;
    public float ShowHideTextDuration = 0.4f;
    public float FakeLoadDuration = 3.4f;
    public Vector3 NormalCubePosition;
    public Vector3 LoadingCubePosition;
    public Vector3 NormalTextPosition;
    public Vector3 LoadingTextPosition;

    // Private fields

    private readonly SerialDisposable _serialDisposable = new SerialDisposable();
    private readonly BoolReactiveProperty _isLoading = new BoolReactiveProperty();
    private readonly BoolReactiveProperty _isCancelling = new BoolReactiveProperty();

	internal void Awake()
	{
        // Bind buttons

        // Observables that control whether the buttons are enabled or not.
	    var canStart = _isLoading.Not();
	    var canCancel = _isLoading.And(_isCancelling.Not());

        // BindTo() is a Silphid extension that subscribes the button to a canExecute observable (that determines
        // whether the button is interactable or not) and to its click event/observable (in order to trigger an action).
        // It then returns an IDisposable that can be used to remove the binding, which we then AddTo(this) (so that
        // it gets automatically disposed when this component is destroyed). 
        StartButton.BindTo(canStart, StartLoading).AddTo(this);   
        CancelButton.BindTo(canCancel, CancelLoading).AddTo(this);
    }

    private void StartLoading()
    {
        _isLoading.Value = true;

        // SerialDisposable always disposes previous disposable when assigning new value
        _serialDisposable.Disposable =
            Sequence.Start(seq =>
            {
                // AddParallel() is a shorthand that creates a Parallel from given observables and adds it to sequence.
                // There are multiple overloads of this method, but this is the most compact, when all your methods are
                // returning observables.  Note that we are using the Method Group syntax for even more compact code.
                // For example, instead of passing a "() => ShowText()" lambda, which has the same signature as ShowText(),
                // we can only specify the "ShowText" method group.  The important thing to remember is that the ShowText()
                // method is *not* invoked immediately here, exactly as for lambdas.
                seq.AddParallel(MoveCubeToLoadingPosition, ShowText);

                // The TakeUntil() Rx operator allows to start both the RotateCubeIndefinitely() and LoadGreeting() operations,
                // but it terminates the infinite animation as soon as LoadGreeting() emits a value or completes. The Add() extension
                // method supports any type T for IObservable<T>, but it disregards all emitted values, so it is our responsability
                // to act upon meaningful values, as we are doing here with the Do() Rx operator to log the loaded greeting as a
                // side-effect.
                seq.Add(RotateCubeIndefinitely()
                    .TakeUntil(LoadGreeting().Do(x => Debug.Log($"Greeting loaded: {x}"))));

                // This is a more verbose, but much more flexible overload of AddParallel(), because it passes the new parallel
                // object to the lambda and you therefore have access to all its extensions methods (like AddSequence() in this case).
                seq.AddParallel(p =>
                {
                    p.AddSequence(s =>
                    {
                        s.Add(ResetCubeRotation);
                        s.AddDelay(0.25f);
                        s.Add(MoveCubeToNormalPosition);
                    });

                    p.Add(HideText);
                });

                // Actions are instantaneous steps in a sequence and, as such, do not return an observable.
                // They are very handy when synchronous code must be executed at a specific point in a sequence.
                seq.AddAction(Complete);
            });
    }

    private void CancelLoading()
    {
        _isCancelling.Value = true;

        // SerialDisposable always disposes previous disposable when assigning a new value to it
        _serialDisposable.Disposable =
            Sequence.Start(seq =>
            {
                seq.AddParallel(MoveCubeToNormalPosition, ResetCubeRotation, HideText);
                seq.AddAction(Complete);
            });
    }

    private Rx.IObservable<string> LoadGreeting() =>
        Observable
            .Timer(TimeSpan.FromSeconds(FakeLoadDuration))
            .Select(_ => "Hello World!");

    private void Complete()
    {
        _isLoading.Value = false;
        _isCancelling.Value = false;
    }

    // Rotate cube

    private Rx.IObservable<Unit> RotateCubeIndefinitely() =>
        Sequence.Create(
                () => RotateCube(Vector3.up * 180),
                () => RotateCube(Vector3.right * 180),
                () => RotateCube(Vector3.forward * 180))
            .Repeat();

    private Rx.IObservable<Unit> RotateCube(Vector3 angle) =>
        Cube.transform.DOLocalRotate(angle, RotateDuration).SetEase(Ease.InOutCubic).ToObservable();

    private Rx.IObservable<Unit> ResetCubeRotation() =>
        Cube.transform.DOLocalRotate(Vector3.zero, RotateDuration).SetEase(Ease.InOutCubic).ToObservable();

    // Move cube

    private Rx.IObservable<Unit> MoveCubeToLoadingPosition() => MoveCubeTo(LoadingCubePosition);
    private Rx.IObservable<Unit> MoveCubeToNormalPosition() => MoveCubeTo(NormalCubePosition);
    private Rx.IObservable<Unit> MoveCubeTo(Vector3 position) =>
        Cube.transform.DOLocalMove(position, MoveDuration).SetEase(Ease.InOutCubic).ToObservable();

    // Show or hide text

    private Rx.IObservable<Unit> ShowText() => ShowHideText(LoadingTextPosition, 1);
    private Rx.IObservable<Unit> HideText() => ShowHideText(NormalTextPosition, 0);
    private Rx.IObservable<Unit> ShowHideText(Vector3 position, float alpha) =>
        Parallel.Create(
            () => Text.GetComponent<CanvasGroup>().DOFadeTo(alpha, ShowHideTextDuration).SetEase(Ease.InOutCubic).ToObservable(),
            () => Text.transform.DOLocalMove(position, ShowHideTextDuration).SetEase(Ease.InOutCubic).ToObservable());
}
