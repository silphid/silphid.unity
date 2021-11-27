using Silphid.Showzup.Layout;
using UnityEngine;

namespace Silphid.Showzup.Test.Layout
{
    public class BoxIntegrationTest : MonoBehaviour
    {
        public BoxComponent Box1;
        public BoxComponent Box2;
        public BoxComponent Box3;

        private void Start()
        {
            Box1.Bind(Alignment.Min);
            Box2.Bind(Alignment.Min);
            Box3.Bind(Alignment.Min);
            
            Box2.XMin.BindTo(Box1.XMin);
            Box2.YMin.BindTo(Box1.YMax, 32);
            Box2.Width.BindTo(Box1.Width);
            Box2.Height.BindTo(Box1.Height);

            Box3.XMin.BindTo(Box2.XMin);
            Box3.YMin.BindTo(Box2.YMax, 32);
            Box3.Width.BindTo(Box2.Width);
            Box3.Height.BindTo(Box2.Height);
        }
    }
}
