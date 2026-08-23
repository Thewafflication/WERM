namespace Werm.Core.Domain
{
    public sealed class Product
    {
        public Product(
            string plu,
            string description,
            string ingredientsStatement,
            bool safeHandlingRequired,
            bool isActive)
        {
            Plu = DomainText.Required(plu, nameof(plu));
            Description = DomainText.Required(description, nameof(description));
            IngredientsStatement = DomainText.Optional(ingredientsStatement);
            SafeHandlingRequired = safeHandlingRequired;
            IsActive = isActive;
        }

        public string Plu { get; private set; }
        public string Description { get; private set; }
        public string IngredientsStatement { get; private set; }
        public bool SafeHandlingRequired { get; private set; }
        public bool IsActive { get; private set; }
    }
}
