#!/bin/bash
# ============================================================
# Farmer Marketplace — Restructure to Flat Backend + Updated Frontend
# Team Zenith — SIH Project
#
# This script:
#   1. Backs up your current layered .NET backend (doesn't delete it)
#   2. Creates a new SINGLE flat .NET project (no Domain/Application/
#      Infrastructure/Tests split)
#   3. Installs all NuGet packages directly into that one project
#   4. Adds new frontend folders/files (layouts, new components)
#      WITHOUT touching or overwriting any existing frontend files
#
# Requires: .NET 10 SDK installed (dotnet --version)
# Run from: farmer-marketplace/ (project root)
#
# Usage:
#   chmod +x restructure_project.sh
#   ./restructure_project.sh
# ============================================================

set -e

# --- Safety checks ---
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: 'dotnet' command not found. Install .NET 10 SDK first."
    exit 1
fi

if [ ! -d "frontend" ]; then
    echo "ERROR: 'frontend' folder not found. Run this from farmer-marketplace/ (project root)."
    exit 1
fi

echo "Found dotnet SDK version: $(dotnet --version)"
echo ""

# ============================================================
# PART 1: BACKEND — flatten to single project
# ============================================================

if [ -d "backend" ]; then
    BACKUP_NAME="backend_layered_backup_$(date +%Y%m%d_%H%M%S)"
    echo "Backing up existing layered backend/ to $BACKUP_NAME/ ..."
    mv backend "$BACKUP_NAME"
fi

mkdir backend
cd backend

echo ""
echo "Creating single flat .NET project..."

dotnet new sln -n FarmerMarketplace
dotnet new webapi -n FarmerMarketplace.Api
dotnet sln add FarmerMarketplace.Api/FarmerMarketplace.Api.csproj

cd FarmerMarketplace.Api

# ------------------------------------------------------------
# Folder structure
# ------------------------------------------------------------
echo "Creating internal folder structure..."

mkdir -p Controllers
mkdir -p Models
mkdir -p DTOs
mkdir -p Interfaces
mkdir -p Services
mkdir -p Data
mkdir -p Data/Migrations
mkdir -p Security
mkdir -p Middleware

# Placeholder files matching your structure (safe — touch won't overwrite
# real content if you re-run this after adding code, but on first run these
# start empty for you to fill in)
touch Controllers/AuthController.cs
touch Controllers/ProductsController.cs
touch Controllers/OrdersController.cs
touch Controllers/ForecastController.cs
touch Controllers/RoutesController.cs
touch Controllers/PaymentsController.cs
touch Controllers/AdminController.cs
touch Controllers/WhatsAppController.cs

touch Models/User.cs
touch Models/Product.cs
touch Models/Order.cs
touch Models/OrderItem.cs
touch Models/SalesHistory.cs
touch Models/Route.cs
touch Models/Payment.cs

touch DTOs/LoginDto.cs
touch DTOs/RegisterDto.cs
touch DTOs/ProductDto.cs
touch DTOs/OrderDto.cs
touch DTOs/ForecastDto.cs
touch DTOs/PaymentDto.cs
touch DTOs/AdminSummaryDto.cs

touch Interfaces/IAuthService.cs
touch Interfaces/IProductService.cs
touch Interfaces/IOrderService.cs
touch Interfaces/IForecastService.cs
touch Interfaces/IRouteService.cs
touch Interfaces/IPaymentService.cs
touch Interfaces/IWhatsAppService.cs

touch Services/AuthService.cs
touch Services/ProductService.cs
touch Services/OrderService.cs
touch Services/ForecastService.cs
touch Services/RouteService.cs
touch Services/PaymentService.cs
touch Services/WhatsAppService.cs

touch Data/AppDbContext.cs

touch Security/JwtService.cs
touch Security/PasswordHasher.cs

touch Middleware/ExceptionMiddleware.cs

# ------------------------------------------------------------
# Remove the default WeatherForecast template files (not needed)
# ------------------------------------------------------------
rm -f WeatherForecast.cs
rm -f Controllers/WeatherForecastController.cs

# ------------------------------------------------------------
# NuGet packages — all go directly into this one project now
# ------------------------------------------------------------
echo ""
echo "Installing NuGet packages (needs internet access)..."

dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package BCrypt.Net-Next
dotnet add package Microsoft.ML
dotnet add package Microsoft.ML.TimeSeries
dotnet add package Google.OrTools
dotnet add package Razorpay
dotnet add package Twilio

cd ../..

# Re-add dotnet-ef tool reference reminder (already installed globally per earlier setup)
echo ""
echo "Backend restructure complete."
echo ""

# ============================================================
# PART 2: FRONTEND — add new files/folders only (non-destructive)
# ============================================================

echo "Adding new frontend folders/files (existing files untouched)..."

cd frontend/src

# --- New layouts folder ---
mkdir -p layouts
touch layouts/DashboardLayout.jsx

# --- New common components (only creates if missing — won't overwrite) ---
mkdir -p components/common
for f in Sidebar.jsx Topbar.jsx RoleSelector.jsx StatCard.jsx DataTable.jsx; do
    if [ ! -f "components/common/$f" ]; then
        touch "components/common/$f"
        echo "  Created components/common/$f"
    else
        echo "  Skipped components/common/$f (already exists)"
    fi
done

# --- App.css if missing ---
if [ ! -f "App.css" ]; then
    touch App.css
    echo "  Created App.css"
fi

cd ../..

echo ""
echo "============================================================"
echo "Restructure complete."
echo ""
echo "Backend: new flat project at backend/FarmerMarketplace.Api/"
echo "Old layered backend backed up at: $BACKUP_NAME/"
echo "(copy over any real code you'd already written before deleting it)"
echo ""
echo "Frontend: new files added under frontend/src/layouts/ and"
echo "frontend/src/components/common/ — existing files untouched."
echo "============================================================"
echo ""
echo "Next steps:"
echo "  cd backend && dotnet build     # confirm it compiles"
echo "  cd frontend && npm run dev     # confirm frontend still runs"
echo "============================================================"
