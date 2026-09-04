# Farmer Marketplace — Team Zenith (SIH Project)

A direct farmer-to-consumer marketplace with demand forecasting, route optimization, and split payments.

## Tech Stack

| Layer | Tech |
|---|---|
| Frontend | React (Vite) + React Router, Tailwind CSS v4, shadcn/ui, Leaflet, Recharts, react-i18next |
| Backend | ASP.NET Core Web API (.NET 10) — single flat project (Controllers → Services → Data) |
| Database | PostgreSQL (via Entity Framework Core + Npgsql) |
| Auth | JWT |
| Password Hashing | BCrypt.Net-Next |
| Demand Forecasting | ML.NET (SSA time-series) |
| Route Optimization | Google.OrTools |
| Payments | Razorpay (.NET SDK, test mode) + Razorpay Route (split payments) |
| Notifications | Twilio WhatsApp API (sandbox) |

## Required Versions

| Tool | Version |
|---|---|
| .NET SDK | **10.0.x** |
| Node.js | **20.x** (see `.nvmrc`) |
| PostgreSQL | Any recent version (hosted via Railway/Render — no local install needed) |

## First-Time Setup (do this once after cloning)

### 1. Backend

```bash
cd backend
dotnet --version    # confirm 10.x
dotnet restore
dotnet build         # should say "Build succeeded"
```

Create `appsettings.Development.json` inside `backend/FarmerMarketplace.Api/` (ask a backend lead for real secrets — never commit them):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "FarmerMarketplace",
    "ExpireMinutes": 1440
  },
  "Razorpay": {
    "KeyId": "rzp_test_...",
    "KeySecret": "...",
    "WebhookSecret": "..."
  },
  "Twilio": {
    "AccountSid": "...",
    "AuthToken": "...",
    "WhatsAppNumber": "+14155238886"
  }
}
```

### 2. Frontend

```bash
cd frontend
npm install
cp .env.example .env
```

### 3. Run both (two separate terminals)

```bash
# Terminal 1 — backend
cd backend
dotnet run --project FarmerMarketplace.Api

# Terminal 2 — frontend
cd frontend
npm run dev
```

Frontend: http://localhost:5173
Backend Swagger docs: check console output on `dotnet run` — usually `http://localhost:5000/swagger`

## Installing .NET 10 SDK (if you don't have it)

**Windows:** https://dotnet.microsoft.com/download

**WSL/Ubuntu:**
```bash
sudo snap install dotnet --classic
```
If you hit a `libunwind` error:
```bash
sudo apt install libunwind8 -y
```
If you hit an ICU/globalization error during `dotnet build`:
```bash
sudo apt install libicu-dev -y
```

**Mac:**
```bash
brew install --cask dotnet-sdk
```

## Common Setup Issues (already solved once — don't re-debug these)

| Error | Fix |
|---|---|
| `npx tailwindcss init -p` fails ("could not determine executable") | Tailwind v4 removed this command. Use `npm install -D @tailwindcss/vite` and add the plugin to `vite.config.js` — no config file needed, `index.css` just has `@import "tailwindcss";` |
| `dotnet: command not found` (in WSL specifically) | Windows and WSL are separate environments. Install .NET inside WSL too: `sudo snap install dotnet --classic` |
| `Failed to load libcoreclr.so ... libunwind-x86_64.so.8` | Missing system library. Fix: `sudo apt install libunwind8 -y` |
| `Couldn't find a valid ICU package` during `dotnet build` | Missing globalization library. Fix: `sudo apt install libicu-dev -y` |
| `There are no versions available for the package 'Razorpay.Api'` | Wrong NuGet package name — it's **`Razorpay`**, not `Razorpay.Api` (that's the C# namespace, not the package ID) |
| `dotnet-ef` installs but command not found | Not on PATH. Run: `export PATH="$PATH:$HOME/.dotnet/tools"` and add to `~/.bashrc` to persist |
| Git commit/status very slow on WSL | You're on a Windows-mounted drive (`/mnt/d/...`). Move the project into WSL's native filesystem (`~/your-project`) for much faster performance |

If you hit something not on this list, check with the team before spending hours debugging — someone may have already solved it.

## Project Structure

```
farmer-marketplace/
├── frontend/
│   └── src/
│       ├── pages/            farmer/, buyer/, admin/, fpo-admin/, auth/
│       ├── layouts/          DashboardLayout.jsx (shared sidebar + topbar)
│       ├── components/       common/, product/, cart/, order/, payment/, forecast/, map/, fpo/
│       ├── context/          AuthContext, CartContext, LanguageContext
│       ├── services/         API layer (Axios calls per feature)
│       ├── routes/           AppRoutes.jsx — route definitions + role guards
│       ├── hooks/, locales/, styles/, utils/
│       └── App.jsx, main.jsx, config.js, i18n.js
│
├── backend/
│   └── FarmerMarketplace.Api/     single flat .NET project
│       ├── Controllers/            Auth, Products, Orders, Forecast, Routes, Payments, Admin, WhatsApp
│       ├── Models/                 User, Product, Order, OrderItem, SalesHistory, Route, Payment
│       ├── DTOs/                   request/response shapes per feature
│       ├── Interfaces/             service contracts (IAuthService, IProductService, etc.)
│       ├── Services/               business logic implementations
│       ├── Data/                   AppDbContext + Migrations
│       ├── Security/               JwtService, PasswordHasher
│       ├── Middleware/             ExceptionMiddleware (centralized error handling)
│       └── Program.cs, appsettings.json
│
├── data/                      seed_sales_history.csv
├── docs/                      architecture.md, api_contract.md, financial_model.md, figma_link.md
├── farmer_marketplace_calculator.py   standalone financial model tool (Python, independent of backend)
├── docker-compose.yml
└── README.md
```

## Team Roles

| Person | Owns |
|---|---|
| Backend | Controllers, Services, Data (EF Core), Security (JWT), Auth, Products, Orders, Forecast, Routes, Payments, WhatsApp |
| Frontend | Pages, Layouts, Components, Context, Services (API calls), i18n, role-based routing |

## Git Workflow

- `main` — stable, working code only
- Branch per feature: `git checkout -b feature/product-listing`
- PR before merging into `main` — other teammate reviews
- Never commit secrets (`appsettings.Development.json`, `.env`) — already in `.gitignore`

## Database Migrations (EF Core)

Since the backend is now a single flat project, both flags point to the same project:
```bash
cd backend
dotnet ef migrations add InitialCreate --project FarmerMarketplace.Api --startup-project FarmerMarketplace.Api
dotnet ef database update --project FarmerMarketplace.Api --startup-project FarmerMarketplace.Api
```

## API Contract

See `docs/api_contract.md` for the full list of endpoints, request/response shapes, and auth requirements. Agree on any shape changes there before changing code on either side — a silent shape mismatch breaks the frontend without an obvious error.

## Environment Variables / Secrets Needed

- PostgreSQL connection string
- JWT secret key
- Razorpay test key ID + secret + webhook secret
- Twilio Account SID + Auth Token + WhatsApp sandbox number

Ask a team lead for real values — never commit them to the repo.
