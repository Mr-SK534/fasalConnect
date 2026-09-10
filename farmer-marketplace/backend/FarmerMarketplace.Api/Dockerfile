FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["FarmerMarketplace.Api.csproj", "./"]
RUN dotnet restore "FarmerMarketplace.Api.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "FarmerMarketplace.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Render passes PORT environment variable dynamically
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FarmerMarketplace.Api.dll"]
