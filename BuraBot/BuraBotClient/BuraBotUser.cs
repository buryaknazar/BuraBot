using BuraBot.ShopClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuraBot.BuraBotClient
{
    class BuraBotUser
    {
        public int ID { get; set; }
        public string username { get; set; }
        public int totalCashEarned { get; set; }
        public int totalCashLosed { get; set; }

        public ListOfProducts UserProducts { get; set; }

        public BuraBotUser(int ID, string username, int totalCashEarned, int totalCashLosed)
        {
            this.ID = ID;
            this.username = username;
            this.totalCashEarned = totalCashEarned;
            this.totalCashLosed = totalCashLosed;

            UserProducts = new ListOfProducts();
        }

    }
}
