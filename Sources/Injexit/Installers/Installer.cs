using System;
using log4net;
using UniRx;
using UnityEngine;

namespace Silphid.Injexit
{
    public abstract class Installer : MonoBehaviour, IInstaller, IContainerProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Installer));

        public IContainer Container { get; protected set; }

        protected abstract IContainer CreateContainer();
        protected abstract void Configure();
        protected virtual void Complete() {}

        protected Installer()
        {
            _finishRefCount = new RefCountDisposable(Disposable.Create(Finish));
        }

        #region Wait

        private readonly RefCountDisposable _finishRefCount;

        protected IDisposable Wait() => _finishRefCount.GetDisposable();

        protected void WaitFor(ICompletable completable)
        {
            var disposable = Wait();
            completable.Subscribe(() => disposable.Dispose());
        }

        #endregion

        public virtual void Start()
        {
            Log.Info($"Creating container for {GetType().Name}...");
            Container = CreateContainer();

            Log.Info("Configuring bindings...");
            Configure();

            _finishRefCount.Dispose();
        }

        private void Finish()
        {
            Log.Info("Instantiating eager singles...");
            Container.InstantiateEagerSingles(Container);

            Log.Info($"Injecting dependencies into {gameObject.scene.name} scene...");
            InjectScene();

            Log.Info($"Completing {GetType().Name}...");
            Complete();

            Log.Info($"Completed {GetType().Name}.");
        }

        protected virtual void InjectScene()
        {
            Container.InjectScene(gameObject.scene);
        }

        private void OnDestroy()
        {
            Container?.Dispose();
        }
    }
}