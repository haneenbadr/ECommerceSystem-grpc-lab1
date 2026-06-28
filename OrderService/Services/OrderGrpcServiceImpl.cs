using Grpc.Core;
using InventoryService;
using PaymentService;
using Microsoft.Extensions.Logging;

namespace OrderService.Services
{
    public class OrderGrpcServiceImpl : OrderGrpc.OrderGrpcBase
    {
        private readonly InventoryGrpc.InventoryGrpcClient _inventoryClient;
        private readonly PaymentGrpc.PaymentGrpcClient _paymentClient;
        private readonly ILogger<OrderGrpcServiceImpl> _logger;

        public OrderGrpcServiceImpl(
            InventoryGrpc.InventoryGrpcClient inventoryClient,
            PaymentGrpc.PaymentGrpcClient paymentClient,
            ILogger<OrderGrpcServiceImpl> _logger)
        {
            this._inventoryClient = inventoryClient;
            this._paymentClient = paymentClient;
            this._logger = _logger;
        }

        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Processing PlaceOrder request for Order ID: {OrderId}, User ID: {UserId}", request.Id, request.UserId);

            double totalAmount = 0;
            foreach (var item in request.Items)
            {
                totalAmount += item.Quantity * 150.0;
            }

            try
            {
                // 1. Process payment downstream
                var paymentResult = await _paymentClient.DeductBalanceAsync(new DeductBalanceRequest
                {
                    UserId = request.UserId,
                    Amount = totalAmount
                });

                if (!paymentResult.IsSuccess)
                {
                    _logger.LogWarning("Payment failed for Order ID: {OrderId}. Reason: {Reason}", request.Id, paymentResult.Message);
                    return new PlaceOrderResponse
                    {
                        IsSuccess = false,
                        Message = "Process Payment failed " + paymentResult.Message
                    };
                }

                // 2. Deduct inventory for each item downstream
                foreach (var item in request.Items)
                {
                    var inventoryResult = await _inventoryClient.DeductQuantityAsync(new DeductQuantityRequest
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    });

                    if (!inventoryResult.IsSuccess)
                    {
                        _logger.LogWarning("Inventory deduction failed for Order ID: {OrderId}, Item ID: {ItemId}. Reason: {Reason}", 
                            request.Id, item.ItemId, inventoryResult.Message);
                        return new PlaceOrderResponse
                        {
                            IsSuccess = false,
                            Message = $"Failed to deduct product {item.ItemId} from inventory: " + inventoryResult.Message
                        };
                    }
                }

                _logger.LogInformation("Successfully completed PlaceOrder for Order ID: {OrderId}", request.Id);
                return new PlaceOrderResponse
                {
                    IsSuccess = true,
                    Message = "The request was created successfully, and the gRPC operations were completed successfully!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PlaceOrder for Order ID: {OrderId}", request.Id);
                return new PlaceOrderResponse
                {
                    IsSuccess = false,
                    Message = "An internal error occurred while processing the order: " + ex.Message
                };
            }
        }
    }
}
