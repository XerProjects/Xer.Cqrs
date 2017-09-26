using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.DomainDriven.EventSourcing.Snapshots
{
    public abstract class Snapshot<TAggregate> where TAggregate : EventSourcedAggregate
    {
        TAggregate Data { get; }
        int SnapshotVersion { get; }
        DateTime LastUpdated { get; }

        public Snapshot(TAggregate data, int snapshotVersion, DateTime lastUpdated)
        {
            Data = data;
            SnapshotVersion = snapshotVersion;
            LastUpdated = lastUpdated;
        }
    }
}
