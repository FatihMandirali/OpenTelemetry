using System.Diagnostics;
using System.Net;
using Common.Shared.DTOs;
using Stock.API.Services;

namespace Stock.API;

public class StockService
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<StockService> _logger;

    public StockService(PaymentService paymentService, ILogger<StockService> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    private Dictionary<int, int> GetProductStockList()
    {
        var productStockList = new Dictionary<int, int>();
        productStockList.Add(1,10);
        productStockList.Add(2,20);
        productStockList.Add(3,30);
        return productStockList;
    }

    public async Task<ResponseDto<StockCheckAndPaymentProcessResponseDto>> CheckAndPaymentProcess(StockCheckAndPaymentProcessRequestDto request)
    {
        //Order servicede activiye baggege aklenmişti. Eklenen veriyi stock servicinde yakaladık..
        var userId =Activity.Current.GetBaggageItem("userId");
        var productStocklist = GetProductStockList();
        var stockStatus = new List<(int productId, bool hasStockExist)>();
        foreach (var item in request.OrderItems)
        {
            var hasExistStock = productStocklist.Any(x => x.Key == item.ProductId && x.Value >= item.Count);
            stockStatus.Add((item.ProductId,hasExistStock));
        }

        if (stockStatus.Any(x => x.hasStockExist == false))
        {
            return ResponseDto<StockCheckAndPaymentProcessResponseDto>.Fail(HttpStatusCode.BadRequest.GetHashCode(), "stock yetersiz");
        }
        
        //NOT: {@test} şeklimnde isimlendirme yaparsak eğer elastic tarafı bu değişkeni indexler,field olarak algılar ve böylece direkt test:1 şeklinde arama yapabilir hale geliriz. 
        _logger.LogInformation("Stock ayrıldı.OrderCode:{@orderCode}",request.OrderCode);

        var (isSuccess,failMessage) = await _paymentService.CreateCreate(new PaymentCreateRequestDto{OrderCode = request.OrderCode, TotalPrice = request.OrderItems.Sum(x=>x.UnitPrice)});

        if (isSuccess)
        {
            return ResponseDto<StockCheckAndPaymentProcessResponseDto>.Success(HttpStatusCode.OK.GetHashCode(),new StockCheckAndPaymentProcessResponseDto{Description = "Ödeme süreci tamamlandı"});
        }
        return ResponseDto<StockCheckAndPaymentProcessResponseDto>.Fail(HttpStatusCode.BadRequest.GetHashCode(), failMessage??"Ödeme gerçekleşmedi");
    }
    
    
}