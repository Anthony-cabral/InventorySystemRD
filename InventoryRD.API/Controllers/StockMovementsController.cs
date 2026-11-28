using Microsoft.AspNetCore.Mvc;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Dtos;

namespace InventoryRD.API.Controllers
{
    [ApiController]
    [Route("api/stockmovements")]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;

        public StockMovementsController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StockMovementDto>> GetAll(int? productId = null)
        {
            var result = _stockMovementService.GetAll(productId);

            return Ok(result.Data);
        }

        [HttpPost]
        public ActionResult<int> Create(CreateStockMovementDto request)
        {
            var result = _stockMovementService.Create(request);
            if (!result.Success)
            {
                if (result.Message == "The product changed during this operation. Try again.")
                {
                    return Conflict(result.Message);
                }

                return BadRequest(result.Message);
            }

            return Ok(new { Id = result.Data });
        }
    }
}
