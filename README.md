# Farmer Marketplace — Team Zenith (SIH Project)

A direct farmer-to-consumer marketplace with demand forecasting, route optimization, and split payments.

## Tech Stack

| Layer | Tech |
|---|---|
| Frontend | React (Vite) + React Router, Tailwind CSS v4, shadcn/ui, Leaflet, Recharts, react-i18next |
| Backend | ASP.NET Core Web API (.NET 10) — Clean Architecture (Api / Application / Domain / Infrastructure) |
| Database | PostgreSQL (via Entity Framework Core + Npgsql) |
| Auth | JWT |
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

# Confirm .NET SDK is installed:
dotnet --version    # should show 10.x

dotnet restore
dotnet build         # should say "Build succeeded" for all 5 projects
```

Create your `appsettings.Development.json` or `.env` (ask a backend lead for real secrets — never commit them):
- Database connection string (PostgreSQL, from Railway/Render)
- JWT secret key
- Razorpay test keys
- Twilio sandbox credentials

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
Backend Swagger docs (auto-generated): check the console output when `dotnet run` starts — usually `http://localhost:5000/swagger` or `https://localhost:5001/swagger`

## Installing .NET 10 SDK (if you don't have it)

**Windows:** https://dotnet.microsoft.com/download — use the installer directly, don't go through WSL if you're not already working there.

**WSL/Ubuntu:**
```bash
sudo snap install dotnet --classic
```
If you hit a `libunwind` error after installing via snap, run:
```bash
sudo apt install libunwind8 -y
```

**Mac:**
```bash
brew install --cask dotnet-sdk
```

## Common Setup Issues (already solved once — don't re-debug these)

| Error | Fix |
|---|---|
| `npx tailwindcss init -p` fails ("could not determine executable") | Tailwind v4 removed this command. Use `npm install -D @tailwindcss/vite` and add the plugin to `vite.config.js` instead — no config file needed, `index.css` just has `@import "tailwindcss";` |
| `dotnet: command not found` (in WSL specifically) | .NET installed on Windows doesn't carry over to WSL — they're separate environments. Install .NET separately inside WSL: `sudo snap install dotnet --classic` |
| `Failed to load libcoreclr.so ... libunwind-x86_64.so.8: cannot open shared object file` | Missing system library after snap install. Fix: `sudo apt install libunwind8 -y` |
| `Couldn't find a valid ICU package` during `dotnet build` | Missing globalization library. Fix: `sudo apt install libicu-dev -y` (or `libicu76`/similar depending on your Ubuntu version — check with `apt-cache search libicu`) |
| `There are no versions available for the package 'Razorpay.Api'` | Wrong NuGet package name — the package is called **`Razorpay`**, not `Razorpay.Api` (that's the C# namespace, not the package ID). Run: `dotnet add <project> package Razorpay` |
| `dotnet tool install --global dotnet-ef` installs but `dotnet-ef` command not found | Tools folder isn't on PATH yet. Run: `export PATH="$PATH:$HOME/.dotnet/tools"` and add the same line to `~/.bashrc` to make it permanent |
| Git commit/status takes a very long time on WSL | You're working on a Windows-mounted drive (`/mnt/d/...`), which WSL accesses slowly. Move the project into WSL's native filesystem (`~/your-project`) instead for much faster git/build performance |

If you hit something not on this list, check with the team before spending hours debugging — someone may have already solved it.

## Project Structure

```
farmer-marketplace/
├── frontend/                          React (Vite)
├── backend/
│   ├── FarmerMarketplace.Api/          Controllers, Program.cs, config
│   ├── FarmerMarketplace.Domain/       Entities, Enums
│   ├── FarmerMarketplace.Application/  DTOs, Services (forecasting, routing, payments, notifications)
│   ├── FarmerMarketplace.Infrastructure/  EF Core DbContext, Migrations, Repositories, Security
│   └── FarmerMarketplace.Tests/        Unit/integration tests
├── data/                               Seed data for demand forecasting
├── docs/                               Architecture, financial model, API contract
└── farmer_marketplace_calculator.py    Standalone financial model tool (Python, independent of backend)
```

## Team Roles

| Person | Owns |
|---|---|
| P1 | Backend core (auth, products, orders, FPO management) |
| P2 | Forecasting (ML.NET) + Route Optimization (OR-Tools) services |
| P3 | Frontend — Farmer/FPO Admin pages |
| P4 | Frontend — Buyer pages (browse, cart, checkout with bulk toggle) |
| P5 | Payments (Razorpay + Route) + WhatsApp notifications (Twilio) |
| P6 | i18n/multi-language setup + Admin dashboard + map integration |

## Git Workflow

- `main` — stable, working code only
- Create a branch per feature: `git checkout -b feature/product-listing`
- Open a PR before merging into `main` — at least one other teammate reviews
- Never commit secrets (`appsettings.Development.json`, `.env`) — already in `.gitignore`

## Database Migrations (EF Core)

Once entities exist, generate and apply migrations from inside `backend/`:
```bash
dotnet ef migrations add InitialCreate --project FarmerMarketplace.Infrastructure --startup-project FarmerMarketplace.Api
dotnet ef database update --project FarmerMarketplace.Infrastructure --startup-project FarmerMarketplace.Api
```

## Environment Variables / Secrets Needed

- PostgreSQL connection string
- JWT secret key
- Razorpay test key ID + secret
- Razorpay webhook secret
- Twilio Account SID + Auth Token + WhatsApp sandbox number

Ask a team lead for real values — never commit them to the repo.
