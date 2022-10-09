FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./auth-service ./auth-service
COPY ./common ./common
COPY ./auth-common ./auth-common
# Restore as distinct layers
RUN dotnet restore ./auth-service/auth-service.csproj
# Build and publish a release
RUN dotnet publish -c Release -o out ./auth-service/auth-service.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app/auth-service
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "auth-service.dll"]