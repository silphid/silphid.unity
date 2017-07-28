using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public interface IScene
    {
        Scene Scene { get; }
        bool IsValid();
    }
}