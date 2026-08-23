namespace Werm.Core.Printing
{
    public interface ILabelDocumentFactory
    {
        ILabelDocument CreateFromTemplate(string templatePath);
    }
}
