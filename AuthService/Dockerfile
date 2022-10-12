FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./AuthService ./AuthService
COPY ./Common ./Common
COPY ./Common.Auth ./Common.Auth
# Restore as distinct layers
RUN dotnet restore ./AuthService/AuthService.csproj
# Build and publish a release
RUN dotnet publish -c Release -o out ./AuthService/AuthService.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app/AuthService
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AuthService.dll"]