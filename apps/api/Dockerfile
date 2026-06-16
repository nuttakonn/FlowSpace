# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire solution
COPY . .

# Restore dependencies
WORKDIR /src/src/FlowSpace.Api
RUN dotnet restore

# Build and publish the API project
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official Playwright .NET image as the runtime base 
# This includes all necessary browser dependencies (Chromium, WebKit, Firefox) required by the ExportWorker
FROM mcr.microsoft.com/playwright/dotnet:v1.44.0-jammy AS runtime
WORKDIR /app

# Expose the default ASP.NET Core port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "FlowSpace.Api.dll"]
