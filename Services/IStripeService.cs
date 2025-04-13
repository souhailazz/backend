namespace AppartementReservationAPI.Services
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSession(int reservationId, string description, decimal amount, string customerEmail);
        Task<Stripe.Checkout.Session> GetSession(string sessionId);
    }
}
