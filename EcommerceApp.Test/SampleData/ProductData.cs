using Ecommerce.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Test.SampleData
{
    public class ProductData
    {
        public static List<Product> GetProducts() => new()
        {
            new Product
            {
                Id = 1,
                Name = "Test 1",
                Description = "Some description",
                Price = 1,
                PictureUrl = "/images/products/boot-ang1.png",
                Type = "React",
                Brand = "Hat",
                Quantity = 1
            },
            new Product
            {
                Id = 2,
                Name = "Test 2",
                Description = "Some description",
                Price = 1,
                PictureUrl = "/images/products/boot-ang1.png",
                Type = "React",
                Brand = "Boots",
                Quantity = 1
            }
        };

        public static Product GetProduct(int id, string name) => new()
        {
            Id = id,
            Name = name,
            Description = "Some description",
            Price = 1,
            PictureUrl = "/images/products/boot-ang1.png",
            Type = "React",
            Brand = "Boots",
            Quantity = 1
        };

        public static List<string> GetBrands() => ["Angular", "React"];

        public static List<string> GetTypes() => ["Boots", "Hats"];
    }
}
