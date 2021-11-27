using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public class SceneAdaptor : IScene
    {
        public Scene Scene { get; }

        public SceneAdaptor(Scene scene)
        {
            Scene = scene;
        }

        public bool IsValid() => Scene.IsValid();
    }
}