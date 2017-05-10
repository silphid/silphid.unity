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
    public Button StartButton;
    public Button CancelButton;
    public Text Text;
    public GameObject Cube;

    public float RotateDuration = 0.5f;
    public float MoveDuration = 0.4f;
    public float ShowHideTextDuration = 0.4f;
    public float FakeLoadDuration = 3.4f;

    public Vector3 NormalCubePosition;
    public Vector3 LoadingCubePosition;

    public Vector3 NormalTextPosition;
    public Vector3 LoadingTextPosition;

    private readonly SerialDisposable _serialDisposable = new SerialDisposable();

	internal void Awake()
	{
	    StartButton.OnClickAsObservable().Subscribe(_ => StartLoading()).AddTo(this);
	    CancelButton.OnClickAsObservable().Subscribe(_ => CancelLoading()).AddTo(this);
	}

    private void StartLoading()
    {
        StartButton.interactable = false;
        CancelButton.interactable = true;

        _serialDisposable.Disposable =
            Sequence.Start(seq =>
            {
                seq.AddParallel(MoveCubeToLoadingPosition, ShowText);
                seq.Add(RotateCubeIndefinitely().TakeUntil(FakeLoad()));
                seq.AddParallel(MoveCubeToNormalPosition, ResetCubeRotation, HideText);
                seq.AddAction(ResetButtons);
            });
    }

    private void CancelLoading()
    {
        CancelButton.interactable = false;

        _serialDisposable.Disposable =
            Sequence.Start(seq =>
            {
                seq.AddParallel(MoveCubeToNormalPosition, ResetCubeRotation, HideText);
                seq.AddAction(ResetButtons);
            });
    }

    private Rx.IObservable<Unit> FakeLoad() =>
        Observable.Timer(TimeSpan.FromSeconds(FakeLoadDuration)).AsSingleUnitObservable();

    private void ResetButtons()
    {
        StartButton.interactable = true;
        CancelButton.interactable = false;
    }

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

    private Rx.IObservable<Unit> MoveCubeToLoadingPosition() => MoveCubeTo(LoadingCubePosition);
    private Rx.IObservable<Unit> MoveCubeToNormalPosition() => MoveCubeTo(NormalCubePosition);
    private Rx.IObservable<Unit> MoveCubeTo(Vector3 position) =>
        Cube.transform.DOLocalMove(position, MoveDuration).SetEase(Ease.InOutCubic).ToObservable();

    private Rx.IObservable<Unit> ShowText() => ShowHideText(LoadingTextPosition, 1);
    private Rx.IObservable<Unit> HideText() => ShowHideText(NormalTextPosition, 0);
    private Rx.IObservable<Unit> ShowHideText(Vector3 position, float alpha) =>
        Parallel.Create(
            () => Text.GetComponent<CanvasGroup>().DOFadeTo(alpha, ShowHideTextDuration).SetEase(Ease.InOutCubic).ToObservable(),
            () => Text.transform.DOLocalMove(position, ShowHideTextDuration).SetEase(Ease.InOutCubic).ToObservable());
}
