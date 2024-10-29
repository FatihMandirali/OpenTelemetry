using Common.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Stock.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StockController:ControllerBase
{
    private readonly StockService _stockService;

    public StockController(StockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost]
    [Route("CheckAndPaymentStart")]
    public async Task<IActionResult> CheckAndPaymentStart(StockCheckAndPaymentProcessRequestDto request)
    {
        var result = await _stockService.CheckAndPaymentProcess(request);
        return new ObjectResult(result) { StatusCode = result.StatusCode };
        //HttpStatus değerini result.Status değerine atar..... ****
    }
}