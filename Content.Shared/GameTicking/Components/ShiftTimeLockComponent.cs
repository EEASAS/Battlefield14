using System;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Shared.GameTicking.Components
{
    /// <summary>
    /// Prevents interactions with an entity until the configured shift time has been reached.
    /// </summary>
    [RegisterComponent]
    public sealed partial class ShiftTimeLockComponent : Component
    {
        /// <summary>
        /// The shift time at which interactions become allowed.
        /// This is compared against round duration.
        /// </summary>
        [DataField("shiftTime", customTypeSerializer: typeof(TimespanSerializer))]
        public TimeSpan ShiftTime;
    }
}
