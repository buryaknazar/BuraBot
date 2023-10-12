using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using ImageChartsLib;
using System.Data.SqlClient;

namespace BuraBot.charts
{
    class UserCharts
    {
        public string username { get; set; }
        public int totalCashEarned { get; set; }
        public int totalCashLosed { get; set; }

        public void LoadUserInfo(InteractionContext ctx)
        {
            string sqlUserInfoReadCommand = $"SELECT * FROM UserInfo WHERE username like '{ctx.User.Username}'";
            SqlCommand cmdRead3 = new SqlCommand(sqlUserInfoReadCommand, Program.DB.connection);

            using (SqlDataReader reader = cmdRead3.ExecuteReader())
            {
                while (reader.Read())
                {
                    this.username = reader["username"].ToString();
                    this.totalCashEarned = Convert.ToInt32(reader["total_earned"]);
                    this.totalCashLosed = Convert.ToInt32(reader["total_losed"]);
                }
            }
        }

        public int GetNetIncome()
        {
            if(totalCashEarned > totalCashLosed)
            {
                return totalCashEarned + totalCashLosed;
            }
            else
            {
                return 0;
            }
        }

        public string CreateChart(InteractionContext ctx)
        {
            LoadUserInfo(ctx);
            ImageCharts chart = new ImageCharts().cht("bvs").chd($"a:{this.totalCashEarned},{this.totalCashLosed * -1},{GetNetIncome()}").chs("500x400").chtt("User Chart").chl($"{this.totalCashEarned}|{this.totalCashLosed * -1}|{GetNetIncome()}").chf("b0,lg,90,EA469EFF,1,03A9F47C,0.54").chxt("x,y").chxl("0:|Total cash earned|Total cash losed|Net income");


            return chart.toURL();
        }
    }
}
