using System.Data;

namespace Werm.Data.Connection
{
    public interface IWermDbConnectionFactory
    {
        IDbConnection OpenConnection();
    }
}
