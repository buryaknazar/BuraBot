using BuraBot.ShopClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.Data.SqlClient;

namespace BuraBot.tools
{
    internal class Tools
    {
        public static ProductCategory ToProductCategory(string productCategory)
        {
            ProductCategory category = new ProductCategory();

            if (productCategory == "PCEquipment")
                category = ProductCategory.PCEquipment;
            if (productCategory == "Food")
                category = ProductCategory.Food;
            if (productCategory == "Sport")
                category = ProductCategory.Sport;

            return category;
        }

        public static ProductCategory ToProductCategory(int productCategory)
        {
            ProductCategory category = new ProductCategory();

            if (productCategory == 0)
                category = ProductCategory.Sport;
            if (productCategory == 1)
                category = ProductCategory.Food;
            if (productCategory == 2)
                category = ProductCategory.PCEquipment;

            return category;
        }

        public static int FromProductCategoryToValue(ProductCategory category)
        {
            int result = 0;

            if (category == ProductCategory.Sport)
                result = 0;
            if (category == ProductCategory.Food)
                result = 1;
            if (category == ProductCategory.PCEquipment)
                result = 2;

            return result;
        }

        public static bool CheckIfUserAlreadyLogged(CommandContext ctx)
        {
            string sqlReadUser = $"SELECT USERKEY FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand sqlReadUserCmd = new SqlCommand(sqlReadUser, Program.DB.connection);

            try
            {
                using (SqlDataReader reader = sqlReadUserCmd.ExecuteReader()) {}

                return false;
            }
            catch
            {
                return false;
            }

        }
    }
}
