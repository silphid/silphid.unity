using UnityEngine.SceneManagement;

namespace Silphid.Injexit
{
    public static class IInjectorExtensions
    {
        public static void InjectScene(this IInjector This, Scene scene) =>
            This.Inject(scene.GetRootGameObjects());
    }
}