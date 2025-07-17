# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all files
COPY . .

# Restore and publish
WORKDIR /app/AudioInterviewer.API
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose the ports your app uses
EXPOSE 5035

# Set the environment 
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="http://+:5035"

# Health check to verify the container is running correctly
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:5035/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "AudioInterviewer.API.dll"]
