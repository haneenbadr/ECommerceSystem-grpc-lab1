using Grpc.Core;
using PaymentService;

namespace PaymentService.Services
{
    public class PaymentServiceImpl : PaymentGrpc.PaymentGrpcBase
    {
        private readonly ILogger<PaymentServiceImpl> _logger;

        public PaymentServiceImpl(ILogger<PaymentServiceImpl> logger)
        {
            _logger = logger;
        }

        public override Task<DeductBalanceResponse> DeductBalance(DeductBalanceRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Deducting the amount of {request.Amount} from user account number {request.UserId}");

          
            if (request.Amount < 5000)
            {
                return Task.FromResult(new DeductBalanceResponse
                {
                    IsSuccess = true,
                    Message = "The amount has been successfully deducted from the balance!"
                });
            }
            else
            {
                return Task.FromResult(new DeductBalanceResponse
                {
                    IsSuccess = false,
                    Message = "The transaction failed: insufficient funds!"
                });
            }
        }
    }
}