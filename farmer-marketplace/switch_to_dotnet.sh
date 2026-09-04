#!/bin/bash
# ============================================================
# Farmer Marketplace — Switch Backend from FastAPI to .NET
# Team Zenith — SIH Project
#
# This script:
#   1. Backs up your old Python backend/ folder (doesn't delete it)
#   2. Creates a new .NET 8 Clean Architecture solution in backend/
#   3. Wires up project references
#   4. Installs the NuGet packages we planned (EF Core, JWT, ML.NET,
#      OR-Tools, Razorpay, Twilio)
#
# Requires: .NET 8 SDK installed (check with: dotnet --version)
#
# Usage:
#   chmod +x switch_to_dotnet.sh
#   ./switch_to_dotnet.sh
# ============================================================

set -e

# --- Safety check: confirm dotnet SDK is installed ---
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: 'dotnet' command not found."
    echo "Install the .NET 8 SDK first: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "Found dotnet SDK version: $(dotnet --version)"
echo ""

# --- Confirm we're at the project root (should contain frontend/, backend/) ---
if [ ! -d "frontend" ]; then
    echo "ERROR: 'frontend' folder not found in current directory."
    echo "Run this script from inside farmer-marketplace/ (the project root)."
    exit 1
fi

# --- Back up old Python backend instead of deleting it ---
if [ -d "backend" ]; then
    BACKUP_NAME="backend_python_backup_$(date +%Y%m%d_%H%M%S)"
    echo "Backing up existing backend/ to $BACKUP_NAME/ ..."
    mv backend "$BACKUP_NAME"
fi

mkdir backend
cd backend

# ------------------------------------------------------------
# CREATE SOLUTION + PROJECTS
# ------------------------------------------------------------
echo ""
echo "Creating .NET solution and projects..."

dotnet new sln -n FarmerMarketplace

dotnet new webapi -n FarmerMarketplace.Api
dotnet new classlib -n FarmerMarketplace.Domain
dotnet new classlib -n FarmerMarketplace.Application
dotnet new classlib -n FarmerMarketplace.Infrastructure
dotnet new xunit -n FarmerMarketplace.Tests

dotnet sln add FarmerMarketplace.Api/FarmerMarketplace.Api.csproj
dotnet sln add FarmerMarketplace.Domain/FarmerMarketplace.Domain.csproj
dotnet sln add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj
dotnet sln add FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj
dotnet sln add FarmerMarketplace.Tests/FarmerMarketplace.Tests.csproj

# ------------------------------------------------------------
# WIRE UP PROJECT REFERENCES
# ------------------------------------------------------------
echo ""
echo "Setting up project references..."

dotnet add FarmerMarketplace.Api/FarmerMarketplace.Api.csproj reference \
    FarmerMarketplace.Application/FarmerMarketplace.Application.csproj \
    FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj

dotnet add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj reference \
    FarmerMarketplace.Domain/FarmerMarketplace.Domain.csproj

dotnet add FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj reference \
    FarmerMarketplace.Domain/FarmerMarketplace.Domain.csproj \
    FarmerMarketplace.Application/FarmerMarketplace.Application.csproj

dotnet add FarmerMarketplace.Tests/FarmerMarketplace.Tests.csproj reference \
    FarmerMarketplace.Application/FarmerMarketplace.Application.csproj

# ------------------------------------------------------------
# CREATE FOLDER STRUCTURE INSIDE EACH PROJECT
# ------------------------------------------------------------
echo ""
echo "Creating internal folder structure..."

mkdir -p FarmerMarketplace.Api/Controllers
mkdir -p FarmerMarketplace.Domain/Entities
mkdir -p FarmerMarketplace.Domain/Enums
mkdir -p FarmerMarketplace.Application/DTOs
mkdir -p FarmerMarketplace.Application/Services
mkdir -p FarmerMarketplace.Application/Interfaces
mkdir -p FarmerMarketplace.Infrastructure/Data
mkdir -p FarmerMarketplace.Infrastructure/Data/Migrations
mkdir -p FarmerMarketplace.Infrastructure/Repositories
mkdir -p FarmerMarketplace.Infrastructure/Security

# ------------------------------------------------------------
# NUGET PACKAGES
# ------------------------------------------------------------
echo ""
echo "Installing NuGet packages (this needs internet access)..."

# API project — auth
dotnet add FarmerMarketplace.Api/FarmerMarketplace.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer

# Infrastructure — database
dotnet add FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add FarmerMarketplace.Infrastructure/FarmerMarketplace.Infrastructure.csproj package BCrypt.Net-Next

# Application — forecasting, routing, payments, notifications
dotnet add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj package Microsoft.ML
dotnet add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj package Microsoft.ML.TimeSeries
dotnet add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj package Google.OrTools
dotnet add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj package Razorpay
dotnet add FarmerMarketplace.Application/FarmerMarketplace.Application.csproj package Twilio

# EF Core CLI tools (needed for migrations later)
dotnet tool install --global dotnet-ef 2>/dev/null || echo "dotnet-ef already installed globally, skipping"

cd ..

echo ""
echo "============================================================"
echo "Done! New .NET backend created at: backend/"
echo "Old Python backend backed up at: $BACKUP_NAME/"
echo "(delete that backup folder manually once you've confirmed"
echo " you don't need anything from it)"
echo "============================================================"
echo ""
echo "Next steps:"
echo "  cd backend"
echo "  dotnet build          # confirm everything compiles"
echo "  dotnet run --project FarmerMarketplace.Api"
echo "============================================================"
