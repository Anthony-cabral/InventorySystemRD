using System.Net.Mail;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;
using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IProductRepository _productRepository;

        public SupplierService(ISupplierRepository supplierRepository, IProductRepository productRepository)
        {
            _supplierRepository = supplierRepository;
            _productRepository = productRepository;
        }

        public ServiceResult<IEnumerable<SupplierDto>> GetAll()
        {
            var items = _supplierRepository.GetAll();
            var response = items.Select(item => new SupplierDto
            {
                Id = item.Id,
                Name = item.Name,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber,
                IsActive = item.IsActive
            }).ToList();

            return new ServiceResult<IEnumerable<SupplierDto>>
            {
                Success = true,
                Data = response
            };
        }

        public ServiceResult<SupplierDto> GetById(int id)
        {
            var item = _supplierRepository.GetById(id);
            if (item == null)
            {
                return new ServiceResult<SupplierDto>
                {
                    Success = false,
                    Message = "The selected supplier was not found."
                };
            }

            var response = new SupplierDto
            {
                Id = item.Id,
                Name = item.Name,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber,
                IsActive = item.IsActive
            };

            return new ServiceResult<SupplierDto>
            {
                Success = true,
                Data = response
            };
        }

        public ServiceResult<int> Create(CreateSupplierDto request)
        {
            var validation = ValidateCreate(request);

            if (!validation.Success)
            {
                return validation;
            }

            var item = new Suppliers
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                IsActive = request.IsActive
            };

            _supplierRepository.Create(item);
            _supplierRepository.SaveChanges();

            return new ServiceResult<int>
            {
                Success = true,
                Data = item.Id
            };
        }

        public ServiceResult<bool> Update(int id, UpdateSupplierDto request)
        {
            if (id != request.Id)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected supplier does not match the information sent."
                };
            }

            var validation = ValidateUpdate(request);

            if (!validation.Success)
            {
                return validation;
            }

            var existing = _supplierRepository.GetById(id);
            if (existing == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected supplier was not found."
                };
            }

            existing.Name = request.Name.Trim();
            existing.Email = request.Email.Trim();
            existing.PhoneNumber = request.PhoneNumber.Trim();
            existing.IsActive = request.IsActive;

            _supplierRepository.Update(existing);
            _supplierRepository.SaveChanges();

            return new ServiceResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        public ServiceResult<bool> Delete(int id)
        {
            var existing = _supplierRepository.GetById(id);
            if (existing == null)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The selected supplier was not found."
                };
            }

            if (_productRepository.HasSupplier(id))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The supplier has associated products."
                };
            }

            try
            {
                _supplierRepository.Delete(existing);
                _supplierRepository.SaveChanges();
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 547 })
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "The supplier has associated products."
                };
            }

            return new ServiceResult<bool>
            {
                Success = true,
                Data = true
            };
        }

        private ServiceResult<int> ValidateCreate(CreateSupplierDto request)
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

            if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Trim().Length > 254 || !MailAddress.TryCreate(request.Email.Trim(), out var email) || !string.Equals(email.Address, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "A valid email address is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Trim().Length > 30 || request.PhoneNumber.Count(char.IsAsciiDigit) is < 7 or > 15 || request.PhoneNumber.Trim().IndexOf('+', 1) >= 0 || request.PhoneNumber.Trim().Any(character => !char.IsAsciiDigit(character) && character is not ('+' or '-' or ' ' or '(' or ')' or '.')))
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = "A valid phone number is required."
                };
            }

            return new ServiceResult<int>
            {
                Success = true
            };
        }

        private ServiceResult<bool> ValidateUpdate(UpdateSupplierDto request)
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

            if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Trim().Length > 254 || !MailAddress.TryCreate(request.Email.Trim(), out var email) || !string.Equals(email.Address, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "A valid email address is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || request.PhoneNumber.Trim().Length > 30 || request.PhoneNumber.Count(char.IsAsciiDigit) is < 7 or > 15 || request.PhoneNumber.Trim().IndexOf('+', 1) >= 0 || request.PhoneNumber.Trim().Any(character => !char.IsAsciiDigit(character) && character is not ('+' or '-' or ' ' or '(' or ')' or '.')))
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = "A valid phone number is required."
                };
            }

            return new ServiceResult<bool>
            {
                Success = true
            };
        }
    }
}
