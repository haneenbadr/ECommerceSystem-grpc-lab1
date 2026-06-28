using InventoryService;
using PaymentService;
using OrderService.Services;

namespace OrderService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register gRPC services
            builder.Services.AddGrpc();

            // Register CORS policy for gRPC-Web and external JS client requests
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
                });
            });

            builder.Services.AddGrpcClient<InventoryGrpc.InventoryGrpcClient>(o =>
            {
                o.Address = new Uri("https://localhost:7124"); 
            });

           
            builder.Services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o =>
            {
                o.Address = new Uri("https://localhost:7244");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Use CORS before gRPC-Web and endpoints
            app.UseCors("AllowAll");

            // Enable gRPC-Web middleware
            app.UseGrpcWeb();

            app.UseAuthorization();

            app.MapControllers();

            // Map gRPC service with CORS and gRPC-Web enabled
            app.MapGrpcService<OrderGrpcServiceImpl>()
               .EnableGrpcWeb()
               .RequireCors("AllowAll");

            app.Run();
        }
    }
}
