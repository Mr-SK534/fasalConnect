FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["backend/FarmerMarketplace.Api/FarmerMarketplace.Api.csproj", "backend/FarmerMarketplace.Api/"]
RUN dotnet restore "backend/FarmerMarketplace.Api/FarmerMarketplace.Api.csproj"

# Copy source code and publish
COPY . .
WORKDIR "/src/backend/FarmerMarketplace.Api"
RUN dotnet publish "FarmerMarketplace.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FarmerMarketplace.Api.dll"]
