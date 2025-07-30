# ----- Build Stage -----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj & restore as distinct layers (for cache)
COPY *.csproj .
RUN dotnet restore ./AudioInterviewer.API.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/out

# ----- Runtime Stage -----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install curl for health checks
RUN apt-get update && apt-get install -y curl

WORKDIR /app
COPY --from=build /app/out .

# Expose your API port
EXPOSE 5035

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5035

# Healthcheck for container
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:5035/health || exit 1

ENTRYPOINT ["dotnet", "AudioInterviewer.API.dll"]
