using Microsoft.AspNetCore.Mvc;
using InventoryService;
using PaymentService;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly InventoryGrpc.InventoryGrpcClient _inventoryClient;
        private readonly PaymentGrpc.PaymentGrpcClient _paymentClient;

       
        public OrdersController(
            InventoryGrpc.InventoryGrpcClient inventoryClient,
            PaymentGrpc.PaymentGrpcClient paymentClient)
        {
            _inventoryClient = inventoryClient;
            _paymentClient = paymentClient;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            
            double totalAmount = 0;
            foreach (var item in request.Items)
            {
                totalAmount += item.Quantity * 150.0;
            }

         
            var paymentResult = await _paymentClient.DeductBalanceAsync(new DeductBalanceRequest
            {
                UserId = request.UserId,
                Amount = totalAmount
            });

            if (!paymentResult.IsSuccess)
            {
                return BadRequest(new { error = "Process Payment failed " + paymentResult.Message });
            }

            
            foreach (var item in request.Items)
            {
                var inventoryResult = await _inventoryClient.DeductQuantityAsync(new DeductQuantityRequest
                {
                    ItemId = item.ItemId,
                    Quantity = item.Quantity
                });

                if (!inventoryResult.IsSuccess)
                {
                    return BadRequest(new { error = $"Failed to deduct product {item.ItemId} from inventory: " + inventoryResult.Message });
                }
            }

            return Ok(new { message = "The request was created successfully, and the gRPC operations were completed successfully!" });
        }
    }

  
    public class OrderRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}