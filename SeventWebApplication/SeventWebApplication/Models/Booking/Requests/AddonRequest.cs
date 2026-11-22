using System;



namespace SeventWebApplication.Models.Booking.Request

{
    public record AddonRequest
    {
        public string AddonType { get; init; } = null!;
        public int Quantity { get; init; } = 1;

        // Göra addontype required samt range check på quantity, t.ex. 1-5, beroende på addon type??? 

    }