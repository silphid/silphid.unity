using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public class ISceneAdaptor : IScene
    {
        public Scene Scene { get; }

        public ISceneAdaptor(Scene scene)
        {
            Scene = scene;
        }

        public bool IsValid() => Scene.IsValid();
    }
}