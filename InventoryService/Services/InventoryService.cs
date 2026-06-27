using Grpc.Core;
using InventoryService; 

namespace InventoryService.Services
{
    
    public class InventoryServiceImpl : InventoryGrpc.InventoryGrpcBase
    {
        private readonly ILogger<InventoryServiceImpl> _logger;

        public InventoryServiceImpl(ILogger<InventoryServiceImpl> logger)
        {
            _logger = logger;
        }

       
        public override Task<DeductQuantityResponse> DeductQuantity(DeductQuantityRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Attempting to deduct a quantity of { request.Quantity} for the product with ID{ request.ItemId}");

            
            if (request.Quantity < 100)
            {
                return Task.FromResult(new DeductQuantityResponse
                {
                    IsSuccess = true,
                    Message = "The quantity has been successfully deducted from the warehouse!"
                });
            }
            else
            {
                return Task.FromResult(new DeductQuantityResponse
                {
                    IsSuccess = false,
                    Message = "The operation failed: The requested quantity is not available in stock"!
                });
            }
        }
    }
}
