using System;
using System.Collections.Generic;

namespace MailCheck.Dkim.Entity.Entity
{
    public abstract class StateBase
    {
        public virtual Dictionary<string, DateTime> LastUpdateSources { get; set; } = new Dictionary<string, DateTime>();

        public virtual bool CanUpdate(string source, DateTime timestamp)
        {
            if (LastUpdateSources.TryGetValue(source, out var previousTimeStamp))
            {
                return timestamp > previousTimeStamp;
            }

            return true;
        }

        public virtual void UpdateSource(string source, DateTime timestamp)
        {
            LastUpdateSources[source] = timestamp;
        }
    }
}