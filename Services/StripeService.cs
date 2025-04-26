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
             _apiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        _webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
        _frontendUrl = configuration["App:FrontendUrl"];
            
            StripeConfiguration.ApiKey = _apiKey;
        }

       public async Task<string> CreateCheckoutSession(
    int reservationId,
    string description,
    decimal amount,
    string customerEmail)
{
    // Validate the inputs
    if (string.IsNullOrEmpty(description))
    {
        throw new ArgumentException("Description cannot be null or empty");
    }

    if (string.IsNullOrEmpty(customerEmail))
    {
        throw new ArgumentException("Customer email cannot be null or empty");
    }

    var options = new SessionCreateOptions
    {
        PaymentMethodTypes = new List<string> { "card" },
        LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100),
                    Currency = "eur",
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
        SuccessUrl = "https://clientfrance.netlify.app/payment/success?session_id={CHECKOUT_SESSION_ID}",
        CancelUrl = "https://clientfrance.netlify.app/payment/cancel",
        CustomerEmail = customerEmail, // This will use the email we retrieved
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