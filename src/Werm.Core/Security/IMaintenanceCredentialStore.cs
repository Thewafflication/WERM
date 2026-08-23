namespace Werm.Core.Security
{
    public interface IMaintenanceCredentialStore
    {
        PasswordCredential Get();
        void Create(PasswordCredential credential);
        void Replace(PasswordCredential credential);
    }
}
