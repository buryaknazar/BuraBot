using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuraBot.BuraBotClient
{
    class BuraBotUsersList
    {
        List<BuraBotUser> Users { get; set; }

        public BuraBotUsersList()
        {
            Users = new List<BuraBotUser>();
        }

        public void LoadUsers()
        {
            Users.Clear();

            string sqlReadUsers = $"SELECT * FROM Card";
            SqlCommand cmdRead = new SqlCommand(sqlReadUsers, Program.DB.connection);

            using (SqlDataReader reader = cmdRead.ExecuteReader())
            {
                while (reader.Read())
                {
                    int user_id = Convert.ToInt32(reader["id"]);
                    string username = reader["USERKEY"].ToString();
                    int totalEarned = Convert.ToInt32(reader["total_earned"]);
                    int totalLosed = Convert.ToInt32(reader["total_losed"]);

                    BuraBotUser NewUser = new BuraBotUser(user_id, username, totalEarned, totalLosed);
                    Users.Add(NewUser);
                }
            }
        }
    }
}
