using Microsoft.AspNetCore.Mvc;
using InventoryRD.Application.Contract;
using InventoryRD.Application.Dtos;

namespace InventoryRD.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoryDto>> GetAll()
        {
            var result = _categoryService.GetAll();

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public ActionResult<CategoryDto> GetById(int id)
        {
            var result = _categoryService.GetById(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost]
        public ActionResult<int> Create(CreateCategoryDto request)
        {
            var result = _categoryService.Create(request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(new { Id = result.Data });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateCategoryDto request)
        {
            var result = _categoryService.Update(id, request);
            if (!result.Success)
            {
                if (result.Message == "The selected category was not found.")
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
            var result = _categoryService.Delete(id);
            if (!result.Success)
            {
                if (result.Message == "The selected category was not found.")
                {
                    return NotFound(result.Message);
                }

                return BadRequest(result.Message);
            }

            return NoContent();
        }
    }
}
