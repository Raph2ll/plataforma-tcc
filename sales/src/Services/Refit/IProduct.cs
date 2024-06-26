using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;

namespace sales.src.Services.Refit
{
    public interface IProduct
    {
        [Get("/api/{id}")]
        Task<ApiResponse<Product>> GetProductById([AliasAs("id")] string id);

        [Put("/api/{id}")]
        Task<ApiResponse<Product>> UpdateProduct([AliasAs("id")] string id, [Body] ProductUpdateRequest request);
    }

    public class Product
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }

    public class ProductUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}