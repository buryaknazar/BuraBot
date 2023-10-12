using BuraBot.config;
using BuraBot.ShopClasses;
using System.Data.SqlClient;

namespace BuraBot.DBReader
{
    internal class DataBaseReader
    {
        private string connectionString;
        public SqlConnection connection { get; set; }

        public async Task ConnectDb()
        {
            var jsonReader = new JSONDBReader();
            await jsonReader.ReadJSON();

            this.connectionString = jsonReader.connectionString;

            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch
            {
                Console.WriteLine("Error!");
            }
        }
    }
}
