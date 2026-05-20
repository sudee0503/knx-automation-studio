using Microsoft.Data.SqlClient;

namespace Sude.Services
{
    public static class SqlConnectionHelper
    {
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(
                "Server=localhost;Database=knx;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}