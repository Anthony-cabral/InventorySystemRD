using Microsoft.EntityFrameworkCore;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;
using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Application.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IProductRepository _productRepository;

        public StockMovementService(IStockMovementRepository stockMovementRepository, IProductRepository productRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _productRepository = productRepository;
        }

        public ServiceResult<IEnumerable<StockMovementDto>> GetAll(int? productId = null)
        {
            var movements = _stockMovementRepository.GetAll(productId).Select(movement => new StockMovementDto
            {
                Id = movement.Id,
                ProductId = movement.ProductId,
                ProductName = movement.Product.Name,
                MovementType = movement.MovementType,
                Quantity = movement.Quantity,
                CreatedAt = movement.CreatedAt,
                Description = movement.Description
            }).ToList();

            return new ServiceResult<IEnumerable<StockMovementDto>>
            {
                Success = true,
                Data = movements
            };
        }

        public ServiceResult<int> Create(CreateStockMovementDto request)
        {
            var validation = ValidateCreate(request);

            if (!validation.Success)
            {
                return validation;
            }

            var product = _productRepository.GetById(request.ProductId);
            if (product == null)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The selected product was not found."
                };
            }

            var stock = request.MovementType == "Entry"
                ? (long)product.Stock + request.Quantity
                : (long)product.Stock - request.Quantity;

            if (stock < 0)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The exit quantity exceeds the available stock."
                };
            }

            if (stock > int.MaxValue)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The resulting stock exceeds the maximum supported quantity."
                };
            }

            var movement = new StockMovements
            {
                ProductId = product.Id,
                MovementType = request.MovementType,
                Quantity = request.Quantity,
                CreatedAt = DateTimeOffset.UtcNow,
                Description = request.Description?.Trim() ?? string.Empty
            };

            product.Stock = (int)stock;

            try
            {
                _stockMovementRepository.Create(movement);
                _stockMovementRepository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The product changed during this operation. Try again."
                };
            }

            return new ServiceResult<int>
            {
                Success = true,
                Data = movement.Id
            };
        }

        private ServiceResult<int> ValidateCreate(CreateStockMovementDto request)
        {
            if (request.ProductId <= 0)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "You need to select a product."
                };
            }

            if (request.MovementType is not ("Entry" or "Exit"))
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The movement type must be Entry or Exit."
                };
            }

            if (request.Quantity <= 0)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The movement quantity must be greater than zero."
                };
            }

            if (request.Description?.Trim().Length > 500)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The description cannot exceed 500 characters."
                };
            }

            return new ServiceResult<int>
            {
                Success = true
            };
        }
    }
}
