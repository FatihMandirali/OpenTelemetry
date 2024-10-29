using System.Diagnostics;
using System.Net;
using Common.Shared.DTOs;
using Common.Shared.Events;
using MassTransit;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.RedisServices;
using Order.API.StockServices;

namespace Order.API.OrderServices;

public class OrderService
{
    private readonly AppDbContext _appDbContext;
    private readonly StockService _stockService;
    private readonly RedisService _redisService;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderService(AppDbContext appDbContext, StockService stockService, RedisService redisService, IPublishEndpoint publishEndpoint)
    {
        _appDbContext = appDbContext;
        _stockService = stockService;
        _redisService = redisService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ResponseDto<OrderCreateResponseDto>> CreateAsync(OrderCreateRequestDto requestDto)
    {
        #region Redis
        //USİNG İLE KULLANMAK ZORUNDA DEĞİLİZ ÇÜNKÜ CONFİGURATİONA REDİS İNSTRUMENTATİON KURDUKTAN SONRA OTOMATİK OLARAK REİDS İSTEKLERİNİTRACE EDECEK
        //using (var redisActivity = ActivitySourceProvider.Source.StartActivity("Redis.Request"))
        //{
            await _redisService.GetdB(0).StringSetAsync("userId", requestDto.UserId);
            var redisUserId = await _redisService.GetdB(0).StringGetAsync("userId");
        //}

        #endregion
        
        Activity.Current.SetTag("Asp.Net Core Tag1", "ASP.NET TAG value"); // Ana aktiviteye ekler
        //başına Using koymak zorundayız. Tüm methodu temsil edip çalışması için
        using var activity = ActivitySourceProvider.Source.StartActivity();
        activity?.AddEvent(new ActivityEvent("Sipariş süreci başladı"));
        //İstediğimiz veriyi activiteler arasında taşımak için baggage ekledik...
        activity.SetBaggage("userId", requestDto.UserId.ToString());

        var newOrder = new Order()
        {
            Created = DateTime.Now,
            OrderCode = Guid.NewGuid().ToString(),
            Status = OrderStatus.Success,
            UserId = requestDto.UserId,
            Items = requestDto.Items.Select(x=>new OrderItem
            {
                Count = x.Count,
                ProductId = x.ProductId,
                UnitPrice = x.UnitPrice
            }).ToList()
        };

        _appDbContext.Orders.Add(newOrder);
        await _appDbContext.SaveChangesAsync();
        
        // //KUYRUĞA MESAJ GÖNDER
        // await _publishEndpoint.Publish(new OrderCreateEvent
        // {
        //     OrderCode = newOrder.OrderCode
        // });

        var stockRequest = new StockCheckAndPaymentProcessRequestDto
        {
            OrderCode = newOrder.OrderCode,
            OrderItems = requestDto.Items
        };
        var (isSuccess,failMessage) = await _stockService.CheckStockAndPaymentStartAsync(stockRequest);

        if (!isSuccess)
        {
            return ResponseDto<OrderCreateResponseDto>.Fail(HttpStatusCode.InternalServerError.GetHashCode(), failMessage);
        }
        
        activity?.SetTag("orderUserId", requestDto.UserId);
        activity?.AddEvent(new ActivityEvent("Sipariş süreci tamamlandı"));

        return ResponseDto<OrderCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(),
            new OrderCreateResponseDto { Id = newOrder.Id });
    }
}