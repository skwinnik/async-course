FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./TaskService ./TaskService
COPY ./Common ./Common
COPY ./Common.Auth ./Common.Auth
# Restore as distinct layers
RUN dotnet restore ./TaskService/TaskService.csproj
# Build and publish a release
RUN dotnet publish -c Release -o out ./TaskService/TaskService.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app/TaskService
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "TaskService.dll"]