using System.Net;
using Common.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentProcessController:ControllerBase
{

    [HttpPost("Create")]
    public IActionResult Create(PaymentCreateRequestDto request)
    {
        const decimal balance = 1000;
        if (request.TotalPrice > balance)
        {
            return BadRequest(ResponseDto<PaymentCreateResponseDto>.Fail(HttpStatusCode.BadRequest.GetHashCode(),"Yetersiz bakiye"));
        }

        return Ok(ResponseDto<PaymentCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(),new PaymentCreateResponseDto{Description = "Ödeme başarılı"}));
    }
}