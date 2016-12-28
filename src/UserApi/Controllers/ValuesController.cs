using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            //return Content("a");
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}
