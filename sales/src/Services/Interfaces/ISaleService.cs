using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using sales.src.Models;
using sales.src.Models.DTOs;

namespace sales.src.Services.Interfaces
{
    public interface ISaleService
    {
        Task CreateSale(string id);
        public Task AddItemsToSale(string id, List<SaleItemRequestDTO> saleRequest);
        Task<Sale> GetSaleById(string id);
        Task<List<Sale>> GetAllSales();
        Task UpdateSale(string id, Sale updatedSale);
        Task DeleteSale(string id);
    }
}
