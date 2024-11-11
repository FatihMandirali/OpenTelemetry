using System.Diagnostics.Metrics;

namespace Metric.API.OpenTelemetry;

public static class OpenTelemetryMetric
{
    private static readonly Meter meter = new Meter("metric.meter.api");
    public static Counter<int> OrderCreatedEventCounter = meter.CreateCounter<int>("order.created.event.count");
    
    public static ObservableCounter<int> OrderCancelledCounter = meter.CreateObservableCounter<int>("order.cancelled.count",()=>new Measurement<int>(Counter.OrderCancelledCounter));

    public static UpDownCounter<int> CurrentStockCounter = meter.CreateUpDownCounter<int>("current.stock.counter",unit:"Sayı");

    public static ObservableUpDownCounter<int> CurrentStockObservableCounter =
        meter.CreateObservableUpDownCounter("current.stock.observable.counter", () => new Measurement<int>(Counter.CurrentStockCount));

    public static ObservableGauge<int> RowKitchenTemp =
        meter.CreateObservableGauge<int>("row.kitchen.temp", () => new Measurement<int>(Counter.KitchenTemp));

    public static Histogram<int> XMethodDuration =
        meter.CreateHistogram<int>("x.method.duraiton",unit:"ms");

}