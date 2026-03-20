# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BasketStats.sln .
COPY src/BasketStats.Domain/BasketStats.Domain.csproj src/BasketStats.Domain/
COPY src/BasketStats.Application/BasketStats.Application.csproj src/BasketStats.Application/
COPY src/BasketStats.Infrastructure/BasketStats.Infrastructure.csproj src/BasketStats.Infrastructure/
COPY src/BasketStats.API/BasketStats.API.csproj src/BasketStats.API/
RUN dotnet restore src/BasketStats.API/BasketStats.API.csproj

COPY src/ src/
RUN dotnet publish src/BasketStats.API/BasketStats.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "BasketStats.API.dll"]
