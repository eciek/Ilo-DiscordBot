using IloApi.Reminder;
using Microsoft.AspNetCore.Mvc;

namespace IloApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotReminderController : ControllerBase
    {
        private readonly ILogger<BotReminderController> _logger;

        public BotReminderController(ILogger<BotReminderController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "PostReminder")]
        public IActionResult PostReminder([FromBody] ReminderModel reminder)
        {
            if (reminder == null)
                return BadRequest();

            ReminderHandler.HandleReminder(reminder);

            return Ok();
        }
    }
}
