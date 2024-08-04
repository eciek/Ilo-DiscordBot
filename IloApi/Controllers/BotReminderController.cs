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
            _logger.LogInformation("BotReminder Received POST!");

            if (reminder == null)
                return BadRequest();

            try
            {
                ReminderHandler.HandleReminder(reminder);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error : {error}", ex);
                return BadRequest(ex);
            }

            _logger.LogInformation("BotReminder Handled POST! Status code: 200");
            return Ok();
        }
    }
}
