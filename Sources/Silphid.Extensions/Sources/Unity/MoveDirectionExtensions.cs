using UnityEngine.EventSystems;

namespace Silphid.Extensions
{
    public static class MoveDirectionExtensions
    {
        public static MoveDirection Opposite(this MoveDirection This)
        {
            if (This == MoveDirection.Up)
                return MoveDirection.Down;

            if (This == MoveDirection.Down)
                return MoveDirection.Up;

            if (This == MoveDirection.Left)
                return MoveDirection.Right;
            
            if (This == MoveDirection.Right)
                return MoveDirection.Left;

            return This;
        }
    }
}