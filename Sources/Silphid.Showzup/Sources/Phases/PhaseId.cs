using Silphid.Extensions.DataTypes;

namespace Silphid.Showzup
{
    public class PhaseId : ObjectEnum<PhaseId>
    {
        public static readonly PhaseId Present = new PhaseId();
        public static readonly PhaseId Load = new PhaseId();
        public static readonly PhaseId Transition = new PhaseId();
        public static readonly PhaseId Hide = new PhaseId();
        public static readonly PhaseId Show = new PhaseId();
        public static readonly PhaseId Deconstruct = new PhaseId();
        public static readonly PhaseId Construct = new PhaseId();
    }
}