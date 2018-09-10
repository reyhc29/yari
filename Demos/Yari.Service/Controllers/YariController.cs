using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Yari.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YariController : ControllerBase
    {
        private readonly ActionManager actionManager;

        public YariController(ActionManager actionManager)
        {
            this.actionManager = actionManager;

            //registering a method handler
            actionManager.RegisterExecuteHandler("service_call", myServiceCall);
        }

        [HttpPost]
        public IActionResult Post([FromBody]JObject content)
        {
            try
            {
                ActionDescriptor actionDescritor = new ActionDescriptor(content);

                JObject result = actionManager.Execute(actionDescritor);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private JObject myServiceCall(dynamic actionDescriptor)
        {
            JObject result = new JObject("Hello from Yari's service call method!");

            return result;
        }
    }
}
