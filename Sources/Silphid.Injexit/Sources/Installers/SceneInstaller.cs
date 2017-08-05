using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silphid.Injexit
{
    public abstract class SceneInstaller<TParent> : Installer
        where TParent : IInstaller
    {
        public virtual void Start()
        {
            var parent = GetParentInstaller();
            Container = parent.Container.Child();
            Logger = parent.Logger;
            
            Logger?.Log($"Installing {GetType().Name}");

            OnBind(Container);
            InjectScene();
            OnReady();
        }

        private TParent GetParentInstaller()
        {
            var parentInstallers = AllScenesRootGameObjects
                .OfComponent<TParent>()
                .ToArray();

            if (parentInstallers.Length == 0)
                throw new InvalidOperationException(
                    $"No parent installer of type {typeof(TParent).Name} found for scene installer {GetType().Name}. Expecting one and only one.");
            if (parentInstallers.Length > 1)
                throw new InvalidOperationException(
                    $"Multiple parent installers of type {typeof(TParent).Name} found for scene installer {GetType().Name}. Expecting one and only one.");

            var parentInstaller = parentInstallers[0];
            return parentInstaller;
        }

        private IEnumerable<GameObject> AllScenesRootGameObjects =>
            AllScenes.SelectMany(x => x.GetRootGameObjects());

        private IEnumerable<Scene> AllScenes
        {
            get
            {
                for (var i = 0; i < SceneManager.sceneCount; i++)
                    yield return SceneManager.GetSceneAt(i);
            }
        }
    }
}