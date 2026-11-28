using Microsoft.AspNetCore.Mvc;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Dtos;

namespace InventoryRD.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductDto>> GetAll(string? search = null, int? categoryId = null, int? supplierId = null, bool? isActive = null)
        {
            var result = _productService.GetAll(search, categoryId, supplierId, isActive);

            return Ok(result.Data);
        }

        [HttpGet("low-stock")]
        public ActionResult<IEnumerable<ProductDto>> GetLowStock()
        {
            var result = _productService.GetLowStock();

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public ActionResult<ProductDto> GetById(int id)
        {
            var result = _productService.GetById(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public ActionResult<int> Create(CreateProductDto request)
        {
            var result = _productService.Create(request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(new { Id = result.Data });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateProductDto request)
        {
            var result = _productService.Update(id, request);
            if (!result.Success)
            {
                if (result.Message == "The selected product was not found.")
                {
                    return NotFound(result.Message);
                }

                if (result.Message == "The product changed during this operation. Try again.")
                {
                    return Conflict(result.Message);
                }

                return BadRequest(result.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _productService.Delete(id);
            if (!result.Success)
            {
                if (result.Message == "The selected product was not found.")
                {
                    return NotFound(result.Message);
                }

                if (result.Message == "The product changed during this operation. Try again.")
                {
                    return Conflict(result.Message);
                }

                return BadRequest(result.Message);
            }

            return NoContent();
        }
    }
}
