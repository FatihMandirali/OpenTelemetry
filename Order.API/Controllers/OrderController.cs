using Common.Shared.Events;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Mvc;
using Order.API.OrderServices;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController:ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderController(OrderService orderService, IPublishEndpoint publishEndpoint)
    {
        _orderService = orderService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderCreateRequestDto requestDto)
    {
        var res = await _orderService.CreateAsync(requestDto);
        
        #region ExceptionÖrneği için hazırlandı
        //Exception örneği için hazırlandı
        // var a = 10;
        // var b = 0;
        // var result = a / b;
        #endregion
        
        return new ObjectResult(res){StatusCode = res.StatusCode};
    }

    [HttpGet]
    public async Task<IActionResult> SendOrderCreatedEvent()
    {
        //KUYRUĞA MESAJ GÖNDER
        await _publishEndpoint.Publish(new OrderCreateEvent
        {
            OrderCode = Guid.NewGuid().ToString()
        });

        return Ok();
    }
    
}