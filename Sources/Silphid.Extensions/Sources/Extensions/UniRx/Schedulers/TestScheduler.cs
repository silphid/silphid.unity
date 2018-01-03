// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

using System;

namespace Silphid.Extensions.UniRx.Schedulers
{
    /// <summary>
    /// Virtual time scheduler used for testing applications and libraries built using Reactive Extensions.
    /// </summary>
    public class TestScheduler : VirtualTimeScheduler
    {
        /// <summary>
        /// Schedules an action to be executed at the specified virtual time.
        /// </summary>
        /// <param name="action">Action to be executed.</param>
        /// <param name="dueTime">Absolute virtual time at which to execute the action.</param>
        /// <returns>Disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        public override IDisposable ScheduleAbsolute(long dueTime, Action action)
        {
            if (dueTime < Clock)
                dueTime = Clock;

            return base.ScheduleAbsolute(dueTime, action);
        }
    }
}
