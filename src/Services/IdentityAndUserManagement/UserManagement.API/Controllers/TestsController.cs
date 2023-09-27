using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }
    }
}
