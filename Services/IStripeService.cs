namespace AppartementReservationAPI.Services
{
    public interface IStripeService
    {
        string webhookSecret { get; }
        Task<string> CreateCheckoutSession(int reservationId, string description, decimal amount, string customerEmail);
        Task<Stripe.Checkout.Session> GetSession(string sessionId);
    }
}