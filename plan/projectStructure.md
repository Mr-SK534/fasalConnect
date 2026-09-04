farmer-marketplace/
│
├── frontend/ # React (Vite) — unchanged
│ └── ... (same structure as before)
│
├── backend/ # ASP.NET Core Web API
│ ├── FarmerMarketplace.Api/ # Main API project
│ │ ├── Controllers/ # equivalent of FastAPI routers
│ │ │ ├── AuthController.cs
│ │ │ ├── ProductsController.cs
│ │ │ ├── OrdersController.cs
│ │ │ ├── UsersController.cs
│ │ │ ├── FpoController.cs
│ │ │ ├── ForecastController.cs
│ │ │ ├── RoutesController.cs
│ │ │ ├── PaymentsController.cs
│ │ │ └── WhatsAppController.cs
│ │ │
│ │ ├── Program.cs # entry point, DI setup, middleware
│ │ ├── appsettings.json # config (non-secret defaults)
│ │ ├── appsettings.Development.json
│ │ └── FarmerMarketplace.Api.csproj
│ │
│ ├── FarmerMarketplace.Domain/ # Entities (equivalent of models/)
│ │ ├── Entities/
│ │ │ ├── User.cs
│ │ │ ├── Product.cs
│ │ │ ├── Order.cs
│ │ │ ├── OrderItem.cs
│ │ │ ├── SalesHistory.cs
│ │ │ ├── Route.cs
│ │ │ └── Payment.cs
│ │ ├── Enums/
│ │ │ ├── UserRole.cs # Farmer, FpoAdmin, Buyer, Admin
│ │ │ └── OrderStatus.cs
│ │ └── FarmerMarketplace.Domain.csproj
│ │
│ ├── FarmerMarketplace.Application/ # Business logic (equivalent of services/)
│ │ ├── DTOs/ # equivalent of Pydantic schemas
│ │ │ ├── UserDto.cs
│ │ │ ├── ProductDto.cs
│ │ │ ├── OrderDto.cs
│ │ │ ├── ForecastDto.cs
│ │ │ └── PaymentDto.cs
│ │ ├── Services/
│ │ │ ├── IAuthService.cs / AuthService.cs
│ │ │ ├── IProductService.cs / ProductService.cs
│ │ │ ├── IOrderService.cs / OrderService.cs
│ │ │ ├── IDemandForecastService.cs / DemandForecastService.cs # ML.NET
│ │ │ ├── IRouteOptimizerService.cs / RouteOptimizerService.cs # OR-Tools
│ │ │ ├── IOrderAggregatorService.cs / OrderAggregatorService.cs # bulk toggle logic
│ │ │ ├── IPaymentService.cs / PaymentService.cs # Razorpay
│ │ │ └── INotificationService.cs / NotificationService.cs # Twilio
│ │ ├── Interfaces/
│ │ │ └── IRepository.cs # generic repository interface
│ │ └── FarmerMarketplace.Application.csproj
│ │
│ ├── FarmerMarketplace.Infrastructure/ # Data access (equivalent of db/)
│ │ ├── Data/
│ │ │ ├── AppDbContext.cs # EF Core DbContext
│ │ │ └── Migrations/ # EF Core migrations (auto-generated)
│ │ ├── Repositories/
│ │ │ ├── UserRepository.cs
│ │ │ ├── ProductRepository.cs
│ │ │ └── OrderRepository.cs
│ │ ├── Security/
│ │ │ ├── JwtTokenGenerator.cs
│ │ │ └── PasswordHasher.cs
│ │ └── FarmerMarketplace.Infrastructure.csproj
│ │
│ ├── FarmerMarketplace.Tests/ # Unit/integration tests
│ │ ├── AuthServiceTests.cs
│ │ ├── ProductServiceTests.cs
│ │ ├── OrderServiceTests.cs
│ │ └── FarmerMarketplace.Tests.csproj
│ │
│ ├── FarmerMarketplace.sln # Solution file (ties all projects together)
│ ├── .env / .env.example # for secrets not in appsettings (or use User Secrets)
│ └── Dockerfile
│
├── data/
│ └── seed_sales_history.csv
│
├── docs/
│ ├── architecture.md
│ ├── financial_model.md
│ ├── api_contract.md
│ └── figma_link.md
│
├── farmer_marketplace_calculator.py # standalone tool, stays Python (independent of backend choice)
├── docker-compose.yml
├── .gitignore
└── README.md
