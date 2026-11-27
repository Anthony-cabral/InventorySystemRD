using Microsoft.AspNetCore.Mvc;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Dtos;

namespace InventoryRD.API.Controllers
{
    [ApiController]
    [Route("api/suppliers")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<SupplierDto>> GetAll()
        {
            var result = _supplierService.GetAll();

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public ActionResult<SupplierDto> GetById(int id)
        {
            var result = _supplierService.GetById(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public ActionResult<int> Create(CreateSupplierDto request)
        {
            var result = _supplierService.Create(request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(new { Id = result.Data });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateSupplierDto request)
        {
            var result = _supplierService.Update(id, request);
            if (!result.Success)
            {
                if (result.Message == "The selected supplier was not found.")
                {
                    return NotFound(result.Message);
                }

                return BadRequest(result.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _supplierService.Delete(id);
            if (!result.Success)
            {
                if (result.Message == "The selected supplier was not found.")
                {
                    return NotFound(result.Message);
                }

                return BadRequest(result.Message);
            }

            return NoContent();
        }
    }
}
