using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmerMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppController(
            IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        [HttpPost("incoming")]
        public async Task<IActionResult> Incoming(
            WhatsAppIncomingDto request)
        {
            await _whatsAppService.ReceiveMessageAsync(request);

            return Ok(new
            {
                success = true
            });
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send(
            WhatsAppSendDto request)
        {
            await _whatsAppService.SendMessageAsync(request);

            return Ok(new
            {
                success = true
            });
        }
    }
}