namespace Werm.Core.Security
{
    public interface IPasswordHasher
    {
        PasswordCredential Create(string password);
        bool Verify(string password, PasswordCredential credential);
    }
}
