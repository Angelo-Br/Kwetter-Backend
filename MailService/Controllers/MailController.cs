using MailService.DBContexts;
using MailService.DTO;
using Microsoft.AspNetCore.Mvc;
using RabbitMQLibrary;

namespace MailService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly MailServiceDatabaseContext _dbContext;
        private readonly IMessageProducer _messageProducer;

        /// <summary>
        /// Constructer is used for receiving the database context at the creation of the UserController.
        /// </summary>
        /// <param name="dbContext">Context of the database</param>
        public MailController(MailServiceDatabaseContext dbContext, IMessageProducer messageProducer)
        {
            _dbContext = dbContext;
            _messageProducer = messageProducer;
        }

        /// <summary>
        /// Uses Rabbitmq to send a mail to the userservice microservice
        /// </summary>
        /// <param name="mailModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateMail(MailModel mailModel)
        {
            MailModel mail = new()
            {
                MailName = mailModel.MailName,
            };

            // sent the message to the rabbitmq server.
            await _messageProducer.PublishMessageAsync("mailmessage", mail);
            return Ok(new { });
        }
    }
}
