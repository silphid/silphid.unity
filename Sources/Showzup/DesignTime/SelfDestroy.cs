using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    public class SelfDestroy : MonoBehaviour
    {
        public void Awake()
        {
            gameObject.Destroy();
        }
    }
}