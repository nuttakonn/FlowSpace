# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire monorepo to handle project-to-project references
COPY . .

# Restore dependencies using the solution file
# Context is repository root, so path is apps/api/FlowSpace.slnx
RUN dotnet restore "apps/api/FlowSpace.slnx"

# Build and publish the API project
# Path is apps/api/src/FlowSpace.Api
WORKDIR "/src/apps/api/src/FlowSpace.Api"
RUN dotnet publish "FlowSpace.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official Playwright .NET image as the runtime base 
FROM mcr.microsoft.com/playwright/dotnet:v1.44.0-jammy AS runtime
WORKDIR /app

# Expose the default ASP.NET Core port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "FlowSpace.Api.dll"]
