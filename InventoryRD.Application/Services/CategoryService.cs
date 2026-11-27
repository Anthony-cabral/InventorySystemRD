using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;
using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public ServiceResult<IEnumerable<CategoryDto>> GetAll()
        {
            var items = _categoryRepository.GetAll();
            var response = items.Select(item => new CategoryDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                IsActive = item.IsActive
            }).ToList();

            return new ServiceResult<IEnumerable<CategoryDto>>
            {
                Success = true,
                Data = response
            };
        }

        public ServiceResult<CategoryDto> GetById(int id)
        {
            var item = _categoryRepository.GetById(id);
            if (item == null)
            {
                return new ServiceResult<CategoryDto>
                {
                    Success = false,
                    Message = "The selected category was not found."
                };
            }

            var response = new CategoryDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                IsActive = item.IsActive
            };

            return new ServiceResult<CategoryDto>
            {
                Success = true,
                Data = response
            };
        }

        public ServiceResult<int> Create(CreateCategoryDto request)
        {
            var validation = ValidateCreate(request);

            if (!validation.Success)
            {
                return validation;
            }

            if (_categoryRepository.NameExists(request.Name.Trim()))
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "This category name is already registered."
                };
            }

            var item = new Categories
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                IsActive = request.IsActive
            };

            try
            {
                _categoryRepository.Create(item);
                _categoryRepository.SaveChanges();
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "This category name is already registered."
                };
            }

            return new ServiceResult<int>
            {
                Success = true,
                Data = item.Id
            };
        }

        public ServiceResult<bool> Update(int id, UpdateCategoryDto request)
        {
            if (id != request.Id)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected category does not match the information sent."
                };
            }

            var validation = ValidateUpdate(request);

            if (!validation.Success)
            {
                return validation;
            }

            var existing = _categoryRepository.GetById(id);
            if (existing == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected category was not found."
                };
            }

            if (_categoryRepository.NameExists(request.Name.Trim(), id))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "This category name is already registered."
                };
            }

            existing.Name = request.Name.Trim();
            existing.Description = request.Description?.Trim() ?? string.Empty;
            existing.IsActive = request.IsActive;

            try
            {
                _categoryRepository.Update(existing);
                _categoryRepository.SaveChanges();
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "This category name is already registered."
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
            var existing = _categoryRepository.GetById(id);
            if (existing == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected category was not found."
                };
            }

            if (_productRepository.HasCategory(id))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The category has associated products."
                };
            }

            try
            {
                _categoryRepository.Delete(existing);
                _categoryRepository.SaveChanges();
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The category has associated products."
                };
            }

            return new ServiceResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        private ServiceResult<int> ValidateCreate(CreateCategoryDto request)
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

            return new ServiceResult<int>
            {
                Success = true
            };
        }

        private ServiceResult<bool> ValidateUpdate(UpdateCategoryDto request)
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

            return new ServiceResult<bool>
            {
                Success = true
            };
        }
    }
}
