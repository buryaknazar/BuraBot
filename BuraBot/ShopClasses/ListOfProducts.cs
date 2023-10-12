using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuraBot.ShopClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using BuraBot.tools;

namespace BuraBot.ShopClasses
{
    [Serializable]
    class ListOfProducts
    {
        public static List<Product> products { get; set; }

        public ListOfProducts()
        {
            products = new List<Product>();
        }

        public int GetSize()
        {
            return products.Count();
        }

        public void AddProduct(Product product, InteractionContext ctx) 
        {  
            string sqlAddCommand1 = $"INSERT INTO Product(product_name, product_description, product_price, product_imageUrl, category_id) Values('{product.Name}','{product.Description}', {product.Price}, '{product.ImageUrl}' ,{tools.Tools.FromProductCategoryToValue(product.Category)})";
            SqlCommand cmd = new SqlCommand(sqlAddCommand1, Program.DB.connection);
            cmd.ExecuteNonQuery();

            string sqlReadCommand = $"SELECT Product.id FROM Product WHERE Product.product_name like '{product.Name}'";
            SqlCommand cmdRead = new SqlCommand(sqlReadCommand, Program.DB.connection);

            int product_id = 0;

            using (SqlDataReader reader = cmdRead.ExecuteReader())
            {
                while (reader.Read())
                {
                    product_id = Convert.ToInt32(reader["id"]);
                }
            }

            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            currentUserList += $",{product_id}";

            string sqlAddCommand = $"UPDATE Card SET List = '{currentUserList}' WHERE Card.USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdAdd = new SqlCommand(sqlAddCommand, Program.DB.connection);
            cmdAdd.ExecuteNonQuery();

            int currentTotalLosed = 0;

            string sqlUserInfoReadCommand = $"SELECT * FROM UserInfo WHERE username like '{ctx.User.Username}'";
            SqlCommand cmdRead3 = new SqlCommand(sqlUserInfoReadCommand, Program.DB.connection);

            using (SqlDataReader reader = cmdRead3.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentTotalLosed = Convert.ToInt32(reader["total_losed"]);
                }
            }

            currentTotalLosed -= product.Price;

            string sqlUserInfoInsertCommand = $"UPDATE UserInfo SET total_losed = {currentTotalLosed} WHERE username like '{ctx.User.Username}'";
            SqlCommand cmdInsert = new SqlCommand(sqlUserInfoInsertCommand, Program.DB.connection);
            cmdInsert.ExecuteNonQuery();
        }

        public void SellProduct(InteractionContext ctx, int productID)
        {
            if(IsInUserList(ctx, productID))
            {
                int product_price = 0;

                string sqlReadProductPrice = $"SELECT product_price FROM Product WHERE id = {productID}";
                SqlCommand cmdReadPrice = new SqlCommand(sqlReadProductPrice, Program.DB.connection);

                using (SqlDataReader reader = cmdReadPrice.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        product_price = Convert.ToInt32(reader["product_price"]);
                    }
                }


                int currentTotalEarned = 0;
                int currentTotalLosed = 0;

                string sqlUserInfoReadCommand = $"SELECT * FROM UserInfo WHERE username like '{ctx.User.Username}'";
                SqlCommand cmdRead3 = new SqlCommand(sqlUserInfoReadCommand, Program.DB.connection);

                using (SqlDataReader reader = cmdRead3.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        currentTotalEarned = Convert.ToInt32(reader["total_earned"]);
                        currentTotalLosed = Convert.ToInt32(reader["total_losed"]);
                    }
                }

                if (currentTotalLosed < 0)
                {
                    currentTotalLosed += product_price;
                    if (currentTotalLosed > 0)
                    {
                        currentTotalEarned += currentTotalLosed;
                    }
                }
                else
                {
                    currentTotalEarned += product_price;
                }

                string sqlUserInfoInsertCommand = $"UPDATE UserInfo SET total_earned = {currentTotalEarned}, total_losed = {currentTotalLosed} WHERE username like '{ctx.User.Username}'";
                SqlCommand cmdInsert = new SqlCommand(sqlUserInfoInsertCommand, Program.DB.connection);
                cmdInsert.ExecuteNonQuery();

                RemoveProduct(productID, ctx);
            }
            else
            {
                throw new Exception();
            }
        }

        public bool IsInUserList(InteractionContext ctx, int productID)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (string id in UserListSplited)
            {
                if (id == productID.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveProduct(int productID, InteractionContext ctx)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            bool IsInUserIdList = false;
            string productIdToDelete = "";

            for(int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach(string id in UserListSplited)
            {
                if(id == productID.ToString())
                {
                    productIdToDelete = id;
                    UserListSplited.Remove(id);
                    IsInUserIdList = true;
                    break;
                }
            }

            string sqlDeleteProduct = $"DELETE FROM Product WHERE Product.id = {Convert.ToInt32(productIdToDelete)}";

            if (IsInUserIdList)
            {
                SqlCommand cmdDelete = new SqlCommand(sqlDeleteProduct, Program.DB.connection);
                cmdDelete.ExecuteNonQuery();

                string newUserList = "";

                foreach(string id in UserListSplited)
                {
                    newUserList += $",{id}";
                }

                string sqlUpdateCommand = $"UPDATE Card SET List = '{newUserList}' WHERE Card.USERKEY like '{ctx.User.Username}'";
                SqlCommand cmdAdd = new SqlCommand(sqlUpdateCommand, Program.DB.connection);
                cmdAdd.ExecuteNonQuery();


                foreach(Product p in products)
                {
                    if(p.ID == productID)
                    {
                        products.Remove(p);
                        break;
                    }
                }
            }
        }

        public void EditProduct(InteractionContext ctx, long productID, string product_name, string product_description, ProductCategory product_category, long product_price, string product_imageUrl)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach(var id in UserListSplited)
            {
                if(id == productID.ToString())
                {
                    string sqlUpdateProduct = $"UPDATE Product SET product_name = '{product_name}', product_description = '{product_description}', product_price = {Convert.ToInt32(product_price)}, product_imageUrl = '{product_imageUrl}', category_id = {Tools.FromProductCategoryToValue(product_category)} WHERE id = {productID}";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateProduct, Program.DB.connection);
                    cmdUpdate.ExecuteNonQuery();
                    break;
                }
            }
        }

        public void EditProductName(InteractionContext ctx, int productID, string NewProductName)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (var id in UserListSplited)
            {
                if (id == productID.ToString())
                {
                    string sqlUpdateProduct = $"UPDATE Product SET product_name = '{NewProductName}' WHERE id = {productID}";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateProduct, Program.DB.connection);
                    cmdUpdate.ExecuteNonQuery();
                    break;
                }
            }
        }

        public void EditProductPrice(InteractionContext ctx, int productID, long NewProductPrice)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (var id in UserListSplited)
            {
                if (id == productID.ToString())
                {
                    string sqlUpdateProduct = $"UPDATE Product SET product_price = {NewProductPrice} WHERE id = {productID}";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateProduct, Program.DB.connection);
                    cmdUpdate.ExecuteNonQuery();
                    break;
                }
            }
        }

        public void EditProductCategory(InteractionContext ctx, int productID, ProductCategory NewProductCategory)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (var id in UserListSplited)
            {
                if (id == productID.ToString())
                {
                    string sqlUpdateProduct = $"UPDATE Product SET category_id = {Tools.FromProductCategoryToValue(NewProductCategory)} WHERE id = {productID}";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateProduct, Program.DB.connection);
                    cmdUpdate.ExecuteNonQuery();
                    break;
                }
            }
        }

        public void EditProductImage(InteractionContext ctx, int productID, string NewProductImageUrl)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (var id in UserListSplited)
            {
                if (id == productID.ToString())
                {
                    string sqlUpdateProduct = $"UPDATE Product SET product_imageUrl = '{NewProductImageUrl}' WHERE id = {productID}";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateProduct, Program.DB.connection);
                    cmdUpdate.ExecuteNonQuery();
                    break;
                }
            }
        }

        public void EditProductDescription(InteractionContext ctx, int productID, string NewProductDescription)
        {
            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (var id in UserListSplited)
            {
                if (id == productID.ToString())
                {
                    string sqlUpdateProduct = $"UPDATE Product SET product_description = '{NewProductDescription}' WHERE id = {productID}";
                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateProduct, Program.DB.connection);
                    cmdUpdate.ExecuteNonQuery();
                    break;
                }
            }
        }

        public List<Product> ReturnFilteredListByName(string NameToCompare)
        {
            List<Product> ListToReturn = new List<Product>();

            foreach(Product p in products)
            {
                if (p.Name.ToLower() == NameToCompare.ToLower())
                    ListToReturn.Add(p);
            }

            return ListToReturn;
        }

        public List<Product> ReturnFilteredListByPrice(long PriceToCompare)
        {
            List<Product> ListToReturn = new List<Product>();

            foreach (Product p in products)
            {
                if (p.Price == PriceToCompare)
                    ListToReturn.Add(p);
            }

            return ListToReturn;
        }

        public List<Product> ReturnFilteredListByCategory(ProductCategory CategoryToCompare)
        {
            List<Product> ListToReturn = new List<Product>();

            foreach (Product p in products)
            {
                if (p.Category == CategoryToCompare)
                    ListToReturn.Add(p);
            }

            return ListToReturn;
        }

        public void DeleteAllProducts(InteractionContext ctx)
        {
            products.Clear();

            string currentUserList = "";

            string sqlReadCommand2 = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdRead2 = new SqlCommand(sqlReadCommand2, Program.DB.connection);

            using (SqlDataReader reader = cmdRead2.ExecuteReader())
            {
                while (reader.Read())
                {
                    currentUserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = currentUserList.Split(',').ToList();

            for (int i = 0; i < UserListSplited.Count; i++)
            {
                if (UserListSplited[i] == "")
                {
                    UserListSplited.RemoveAt(i);
                    i--;
                }
            }

            foreach (string id in UserListSplited)
            {
                string sqlDeleteProduct = $"DELETE FROM Product WHERE Product.id = {Convert.ToInt32(id)}";
                SqlCommand cmdDelete = new SqlCommand(sqlDeleteProduct, Program.DB.connection);
                cmdDelete.ExecuteNonQuery();
            }

            string sqlUpdateCommand = $"UPDATE Card SET List = '' WHERE Card.USERKEY like '{ctx.User.Username}'";
            SqlCommand cmdAdd = new SqlCommand(sqlUpdateCommand, Program.DB.connection);
            cmdAdd.ExecuteNonQuery();
        }

        public void LoadProducts(InteractionContext ctx)
        {
            string sqlReadUser = $"SELECT List FROM Card WHERE USERKEY like '{ctx.User.Username}'";
            SqlCommand sqlReadUserCmd = new SqlCommand(sqlReadUser, Program.DB.connection);

            string UserList = "";

            using (SqlDataReader reader = sqlReadUserCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserList = reader["List"].ToString();
                }
            }

            List<string> UserListSplited = UserList.Split(',').ToList();

            UserListSplited.RemoveAt(0);

            products.Clear();

            foreach (string s in UserListSplited)
            {
                string sqlCommand = $"SELECT Category.title, Product.product_name, Product.product_description, Product.product_price, Product.id, Product.product_imageUrl FROM Product, Category WHERE Product.id = {Convert.ToInt32(s)} AND Product.category_id = Category.id";

                SqlCommand cmd = new SqlCommand(sqlCommand, Program.DB.connection);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int product_id = Convert.ToInt32(reader["id"]);
                        string product_name = reader["product_name"].ToString();
                        string product_description = reader["product_description"].ToString();
                        string product_imageUrl = reader["product_imageUrl"].ToString();
                        int product_price = Convert.ToInt32(reader["product_price"]);
                        ProductCategory product_category = tools.Tools.ToProductCategory(reader["title"].ToString());
                        Product toAdd = new Product(product_name, product_category, product_price, product_imageUrl, product_description);
                        toAdd.ID = product_id;
                        products.Add(toAdd);
                    }
                }
            }
        }
    }
}
