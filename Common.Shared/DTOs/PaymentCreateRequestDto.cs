namespace Common.Shared.DTOs;

public record PaymentCreateRequestDto
{
    public string OrderCode { get; set; }
    public decimal TotalPrice { get; set; }
}