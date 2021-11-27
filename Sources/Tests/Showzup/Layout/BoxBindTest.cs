using NUnit.Framework;
using Silphid.Showzup.Layout;
using UnityEngine;

namespace Silphid.Showzup.Test.Layout
{
    [TestFixture]
    public class BoxBindTest
    {
        private IBox _box1;
        private IBox _box2;

        private static readonly Rect _rect1 = new Rect(123, 456, 100, 200);
        private static readonly Rect _rect2 = new Rect(321, 654, 300, 400);

        private static readonly Vector2 Offset = new Vector2(555, 777);
        private static readonly Vector2 NewPosition1 = new Vector2(232, 343);
        private static readonly Vector2 NewPosition2 = new Vector2(2320, 3430);
        private static readonly Vector2 NewSize2 = new Vector2(454, 878);

        private void SetUp(Alignment alignment)
        {
            SetUp(alignment, alignment);
        }

        private void SetUp(Alignment alignment1, Alignment alignment2)
        {
            _box1 = new Box(_rect1);
            _box2 = new Box(_rect2);

            _box1.Bind(alignment1);
            _box2.Bind(alignment2);
        }

        [Test]
        public void InitialValuesPreservedAtConstructionTime()
        {
            SetUp(Alignment.Min);

            _box1.Is(_rect1);
            _box2.Is(_rect2);
        }

        [Test]
        public void AlignmentMin_SettingMinPropagatesToMaxThenToOtherBoundedBox()
        {
            SetUp(Alignment.Min);

            _box2.XMin.BindTo(_box1.XMax, Offset.x);
            _box2.YMin.BindTo(_box1.YMax, Offset.y);

            _box1.SetMin(NewPosition1);

            _box1.Is(NewPosition1, _rect1.size);
            _box2.Is(NewPosition1 + _rect1.size + Offset, _rect2.size);
        }

        [Test]
        public void AlignmentMin_SettingMinKeepsSizeAndAffectsMax()
        {
            SetUp(Alignment.Min);

            _box1.SetMin(NewPosition1);
            _box1.Is(NewPosition1, _rect1.size);
        }

        [Test]
        public void AlignmentMin_SettingMaxKeepsMinAndAffectsSize()
        {
            SetUp(Alignment.Min);

            _box1.SetMax(NewPosition1);
            _box1.Is(_rect1.min, NewPosition1 - _rect1.min);
        }

        [Test]
        public void AlignmentMin_SettingSizeKeepsMinAndAffectsMax()
        {
            SetUp(Alignment.Min);

            _box1.SetSize(NewSize2);
            _box1.Is(_rect1.min, NewSize2);
        }

        [Test]
        public void AlignmentMax_SettingMinKeepsMaxAndAffectsSize()
        {
            SetUp(Alignment.Max);

            _box1.SetMin(NewPosition1);
            _box1.Is(NewPosition1, _rect1.max - NewPosition1);
        }

        [Test]
        public void AlignmentMax_SettingMaxKeepsSizeAndAffectsMin()
        {
            SetUp(Alignment.Max);

            _box1.SetMax(NewPosition1);
            _box1.Is(NewPosition1 - _rect1.size, _rect1.size);
        }

        [Test]
        public void AlignmentMax_SettingSizeKeepsMaxAndAffectsMin()
        {
            SetUp(Alignment.Max);

            _box1.SetSize(NewSize2);
            _box1.Is(_rect1.max - NewSize2, NewSize2);
        }

        [Test]
        public void AlignmentStretch_SettingMinKeepsMaxAndAffectsSize()
        {
            SetUp(Alignment.Stretch);

            _box1.SetMin(NewPosition1);
            _box1.Is(NewPosition1, _rect1.max - NewPosition1);
        }

        [Test]
        public void AlignmentStretch_SettingMaxKeepsMinAndAffectsSize()
        {
            SetUp(Alignment.Stretch);

            _box1.SetMax(NewPosition1);
            _box1.Is(_rect1.min, NewPosition1 - _rect1.min);
        }

        [Test]
        public void AlignmentStretch_SettingSizeKeepsMinAndAffectsMax()
        {
            SetUp(Alignment.Stretch);

            _box1.SetSize(NewSize2);
            _box1.Is(_rect1.min, NewSize2);
        }

        [Test]
        public void AlignmentMin_SettingMaxAltersSize()
        {
            SetUp(Alignment.Min);

            _box2.XMin.BindTo(_box1.XMax, Offset.x);
            _box2.YMin.BindTo(_box1.YMax, Offset.y);

            _box2.SetMax(NewPosition2);

            _box1.Is(_rect1);
            _box2.Is(_rect1.max + Offset, NewPosition2 - (_rect1.max + Offset));
        }
    }
}