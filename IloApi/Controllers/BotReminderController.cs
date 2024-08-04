using IloApi.Reminder;
using Microsoft.AspNetCore.Mvc;

namespace IloApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotReminderController(ILogger<BotReminderController> logger) : ControllerBase
    {
        private readonly ILogger<BotReminderController> _logger = logger;

        [HttpPost(Name = "PostReminder")]
        public IActionResult PostReminder([FromBody] ReminderModel reminder)
        {
            if (reminder == null)
                return BadRequest();

            try
            {
                ReminderHandler.HandleReminder(reminder);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error : {error}", ex);
                throw;
            }

            return Ok();
        }
    }
}
