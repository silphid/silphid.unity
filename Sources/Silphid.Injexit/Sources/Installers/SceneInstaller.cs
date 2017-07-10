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
        public void Start()
        {
            var parentInstallers = AllScenesRootGameObjects
                .OfComponent<TParent>()
                .ToArray();
            
            if (parentInstallers.Length == 0)
                throw new InvalidOperationException($"No parent installer of type {typeof(TParent).Name} found for scene installer {GetType().Name}. Expecting one and only one.");
            if (parentInstallers.Length > 1)
                throw new InvalidOperationException($"Multiple parent installers of type {typeof(TParent).Name} found for scene installer {GetType().Name}. Expecting one and only one.");
            
            Container = parentInstallers[0].Container.CreateChild();
            
            Debug.Log($"Installing {GetType().Name}");

            Install();
            InjectScene();
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