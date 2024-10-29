using System.Diagnostics;
using System.Text.Json;
using Common.Shared.Events;
using MassTransit;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer:IConsumer<OrderCreateEvent>
{
    public Task Consume(ConsumeContext<OrderCreateEvent> context)
    {
        Task.Delay(2000);
        Activity.Current.SetTag("message.body", JsonSerializer.Serialize(context.Message));

        return Task.CompletedTask;
    }
}