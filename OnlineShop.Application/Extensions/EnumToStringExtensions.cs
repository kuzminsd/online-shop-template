using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Application.Extensions;

public static class EnumToStringExtensions
{
    public static string ConvertToString(this OrderStatus orderStatus)
        => orderStatus switch
        {
            OrderStatus.Collecting => "COLLECTING",
            OrderStatus.BookingInProgress => "BOOKING_IN_PROGRESS",
            OrderStatus.Booked => "BOOKED",
            OrderStatus.DeliverySet => "DELIVERY_SET",
            OrderStatus.PaymentInProgress => "PAYMENT_IN_PROGRESS",
            OrderStatus.Payed => "PAYED",
            _ => "unknown"
        };

    public static string ConvertToString(this PaymentStatus paymentStatus)
        => paymentStatus switch
        {
            PaymentStatus.Failed => "FAILED",
            PaymentStatus.Success => "SUCCESS",
            _ => "unknown"
        };
}