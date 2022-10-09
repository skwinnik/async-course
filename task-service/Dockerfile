FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY ./task-service ./task-service
COPY ./common ./common
COPY ./auth-common ./auth-common
# Restore as distinct layers
RUN dotnet restore ./task-service/task-service.csproj
# Build and publish a release
RUN dotnet publish -c Release -o out ./task-service/task-service.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app/task-service
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "task-service.dll"]