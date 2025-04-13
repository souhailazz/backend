using System;
using System.Threading.Tasks;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.IO;


namespace AppartementReservationAPI.Services
{
    public class StripeService: IStripeService
    {
        private readonly string _apiKey;
        private readonly string _webhookSecret;
        private readonly string _frontendUrl;
public string webhookSecret => _webhookSecret;

        public StripeService(IConfiguration configuration)
        {
            _apiKey = configuration["Stripe:SecretKey"];
            _webhookSecret = configuration["Stripe:WebhookSecret"];
            _frontendUrl = configuration["App:FrontendUrl"];
            
            StripeConfiguration.ApiKey = _apiKey;
        }

        public async Task<string> CreateCheckoutSession(int reservationId, string description, decimal amount, string customerEmail)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            UnitAmount = (long)(amount * 100), // Stripe uses cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Apartment Reservation",
                                Description = description
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_frontendUrl}/payment-success?session_id={{CHECKOUT_SESSION_ID}}&reservation_id={reservationId}",
                CancelUrl = $"{_frontendUrl}/payment-cancel?reservation_id={reservationId}",
                CustomerEmail = customerEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "ReservationId", reservationId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            
            return session.Url;
        }

        public async Task<Session> GetSession(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }
    }
}