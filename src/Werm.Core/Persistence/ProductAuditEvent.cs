using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Werm.Core.Persistence
{
    public sealed class ProductAuditEvent
    {
        public ProductAuditEvent(
            long auditEventId,
            string productPlu,
            long? parentAuditEventId,
            int revisionNumber,
            AuditChangeType changeType,
            DateTimeOffset changedAtUtc,
            string changedBy,
            string changeReason,
            IList<ProductAuditChange> changes)
        {
            AuditEventId = auditEventId;
            ProductPlu = productPlu;
            ParentAuditEventId = parentAuditEventId;
            RevisionNumber = revisionNumber;
            ChangeType = changeType;
            ChangedAtUtc = changedAtUtc;
            ChangedBy = changedBy;
            ChangeReason = changeReason;
            Changes = new ReadOnlyCollection<ProductAuditChange>(changes);
        }

        public long AuditEventId { get; private set; }
        public string ProductPlu { get; private set; }
        public long? ParentAuditEventId { get; private set; }
        public int RevisionNumber { get; private set; }
        public AuditChangeType ChangeType { get; private set; }
        public DateTimeOffset ChangedAtUtc { get; private set; }
        public string ChangedBy { get; private set; }
        public string ChangeReason { get; private set; }
        public ReadOnlyCollection<ProductAuditChange> Changes { get; private set; }
    }
}
