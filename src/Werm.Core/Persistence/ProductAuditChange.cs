namespace Werm.Core.Persistence
{
    public sealed class ProductAuditChange
    {
        public ProductAuditChange(
            int sequence,
            string entityType,
            string entityKey,
            string fieldName,
            string oldValue,
            string newValue)
        {
            Sequence = sequence;
            EntityType = entityType;
            EntityKey = entityKey;
            FieldName = fieldName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int Sequence { get; private set; }
        public string EntityType { get; private set; }
        public string EntityKey { get; private set; }
        public string FieldName { get; private set; }
        public string OldValue { get; private set; }
        public string NewValue { get; private set; }
    }
}
