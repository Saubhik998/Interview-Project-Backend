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

# Set the environment (optional)
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="http://+:5035"

# Start the application
ENTRYPOINT ["dotnet", "AudioInterviewer.API.dll"]
