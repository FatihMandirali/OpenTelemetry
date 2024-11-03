using System.Net;
using Common.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentProcessController:ControllerBase
{
    private readonly ILogger<PaymentProcessController> _logger;

    public PaymentProcessController(ILogger<PaymentProcessController> logger)
    {
        _logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create(PaymentCreateRequestDto request)
    {
        const decimal balance = 1000;
        if (request.TotalPrice > balance)
        {
            //NOT: {@test} şeklimnde isimlendirme yaparsak eğer elastic tarafı bu değişkeni indexler,field olarak algılar ve böylece direkt test:1 şeklinde arama yapabilir hale geliriz. 
            _logger.LogWarning("Bakiye yetersiz. OrderCode:{@orderCode}",request.OrderCode);
            return BadRequest(ResponseDto<PaymentCreateResponseDto>.Fail(HttpStatusCode.BadRequest.GetHashCode(),"Yetersiz bakiye"));
        }
        
        //NOT: {@test} şeklimnde isimlendirme yaparsak eğer elastic tarafı bu değişkeni indexler,field olarak algılar ve böylece direkt test:1 şeklinde arama yapabilir hale geliriz. 
        _logger.LogInformation("Ödeme işlemi başarıyla gerçekleşmiştir. OrderCode:{@orderCode}",request.OrderCode);
        return Ok(ResponseDto<PaymentCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(),new PaymentCreateResponseDto{Description = "Ödeme başarılı"}));
    }
}