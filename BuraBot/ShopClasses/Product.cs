using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuraBot.ShopClasses
{
    enum ProductCategory
    {
        Sport = 0,
        Food = 1,
        PCEquipment = 2
    }

    class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ProductCategory Category { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public Product(string name, ProductCategory category, int price, string imageUrl, string description)
        {
            Name = name;
            Category = category;
            Price = price;
            ImageUrl = imageUrl;
            Description = description;
        }
    }
}
