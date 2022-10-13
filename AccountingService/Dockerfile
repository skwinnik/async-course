FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./AccountingService ./AccountingService
COPY ./Common.Events ./Common.Events
COPY ./Common.Events.Schemas ./Common.Events.Schemas
COPY ./Common.Auth ./Common.Auth
# Restore as distinct layers
RUN dotnet restore ./AccountingService/AccountingService.csproj
# Build and publish a release
RUN dotnet publish -c Release -o out ./AccountingService/AccountingService.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app/AccountingService
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AccountingService.dll"]