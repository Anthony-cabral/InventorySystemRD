using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;
using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, ISupplierRepository supplierRepository, IStockMovementRepository stockMovementRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public ServiceResult<IEnumerable<ProductDto>> GetAll(string? search = null, int? categoryId = null, int? supplierId = null, bool? isActive = null)
        {
            var items = _productRepository.GetAll(search, categoryId, supplierId, isActive);
            var response = items.Select(item => new ProductDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                CategoryId = item.CategoryId,
                SupplierId = item.SupplierId,
                CategoryName = _categoryRepository.GetById(item.CategoryId)?.Name ?? string.Empty,
                SupplierName = _supplierRepository.GetById(item.SupplierId)?.Name ?? string.Empty,
                IsActive = item.IsActive
            }).ToList();

            return new ServiceResult<IEnumerable<ProductDto>>
            {
                Success = true,
                Data = response
            };
        }

        public ServiceResult<IEnumerable<ProductDto>> GetLowStock()
        {
            var result = GetAll(isActive: true);
            result.Data = result.Data!.Where(product => product.Stock <= 5).ToList();

            return result;
        }

        public ServiceResult<ProductDto> GetById(int id)
        {
            var item = _productRepository.GetById(id);
            if (item == null)
            {
                return new ServiceResult<ProductDto>
                {
                    Success = false,
                    Message = "The selected product was not found."
                };
            }

            var response = new ProductDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                CategoryId = item.CategoryId,
                SupplierId = item.SupplierId,
                CategoryName = _categoryRepository.GetById(item.CategoryId)?.Name ?? string.Empty,
                SupplierName = _supplierRepository.GetById(item.SupplierId)?.Name ?? string.Empty,
                IsActive = item.IsActive
            };

            return new ServiceResult<ProductDto>
            {
                Success = true,
                Data = response
            };
        }

        public ServiceResult<int> Create(CreateProductDto request)
        {
            var validation = ValidateCreate(request);

            if (!validation.Success)
            {
                return validation;
            }

            var item = new Products
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Price = request.Price.GetValueOrDefault(),
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                SupplierId = request.SupplierId,
                IsActive = request.IsActive
            };

            try
            {
                _productRepository.Create(item);
                if (item.Stock > 0)
                {
                    _stockMovementRepository.Create(new StockMovements
                    {
                        Product = item,
                        MovementType = "Entry",
                        Quantity = item.Stock,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Description = "Opening stock"
                    });
                }

                _productRepository.SaveChanges();
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The selected category or supplier no longer exists."
                };
            }

            return new ServiceResult<int>
            {
                Success = true,
                Data = item.Id
            };
        }

        public ServiceResult<bool> Update(int id, UpdateProductDto request)
        {
            if (id != request.Id)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected product does not match the information sent."
                };
            }

            var validation = ValidateUpdate(request);

            if (!validation.Success)
            {
                return validation;
            }

            var existing = _productRepository.GetById(id);
            if (existing == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected product was not found."
                };
            }

            if (request.Stock.HasValue && request.Stock.Value != existing.Stock)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "Stock can only be changed through inventory movements."
                };
            }

            existing.Name = request.Name.Trim();
            existing.Description = request.Description?.Trim() ?? string.Empty;
            existing.Price = request.Price.GetValueOrDefault();
            existing.CategoryId = request.CategoryId;
            existing.SupplierId = request.SupplierId;
            existing.IsActive = request.IsActive;

            try
            {
                _productRepository.Update(existing);
                _productRepository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The product changed during this operation. Try again."
                };
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected category or supplier no longer exists."
                };
            }
            return new ServiceResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        public ServiceResult<bool> Delete(int id)
        {
            var existing = _productRepository.GetById(id);
            if (existing == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected product was not found."
                };
            }

            if (_stockMovementRepository.HasProduct(id))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The product has inventory movements."
                };
            }

            try
            {
                _productRepository.Delete(existing);
                _productRepository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The product changed during this operation. Try again."
                };
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The product has inventory movements."
                };
            }

            return new ServiceResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        private ServiceResult<int> ValidateCreate(CreateProductDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The name is required."
                };
            }

            if (request.Name.Trim().Length > 120)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The name cannot exceed 120 characters."
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

            if (request.Price is not decimal price || price < 0 || price > 9999999999999999.99m || decimal.Round(price, 2) != price)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "A price between zero and 9999999999999999.99 with up to two decimal places is required."
                };
            }

            if (request.Stock < 0)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The stock cannot be negative."
                };
            }

            if (request.CategoryId <= 0 || _categoryRepository.GetById(request.CategoryId) == null)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The selected category was not found."
                };
            }

            if (request.SupplierId <= 0 || _supplierRepository.GetById(request.SupplierId) == null)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "The selected supplier was not found."
                };
            }

            return new ServiceResult<int>
            {
                Success = true
            };
        }

        private ServiceResult<bool> ValidateUpdate(UpdateProductDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The name is required."
                };
            }

            if (request.Name.Trim().Length > 120)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The name cannot exceed 120 characters."
                };
            }

            if (request.Description?.Trim().Length > 500)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The description cannot exceed 500 characters."
                };
            }

            if (request.Price is not decimal price || price < 0 || price > 9999999999999999.99m || decimal.Round(price, 2) != price)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "A price between zero and 9999999999999999.99 with up to two decimal places is required."
                };
            }

            if (request.Stock < 0)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The stock cannot be negative."
                };
            }

            if (request.CategoryId <= 0 || _categoryRepository.GetById(request.CategoryId) == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected category was not found."
                };
            }

            if (request.SupplierId <= 0 || _supplierRepository.GetById(request.SupplierId) == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected supplier was not found."
                };
            }

            return new ServiceResult<bool>
            {
                Success = true
            };
        }
    }
}
