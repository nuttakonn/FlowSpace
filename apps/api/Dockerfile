# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire monorepo to handle project-to-project references
COPY . .

# Restore dependencies using the solution file
RUN dotnet restore "apps/api/FlowSpace.slnx"

# Build and publish the API project as a SELF-CONTAINED application for linux-x64
# This bundles the .NET 10 runtime with the application, as the Playwright base image only has .NET 8
WORKDIR "/src/apps/api/src/FlowSpace.Api"
RUN dotnet publish "FlowSpace.Api.csproj" -c Release -o /app/publish \
    -r linux-x64 \
    --self-contained true \
    /p:UseAppHost=true

# Use the official Playwright .NET image as the runtime base 
# This image contains the browser binaries and native dependencies for PDF/PNG exports
FROM mcr.microsoft.com/playwright/dotnet:v1.44.0-jammy AS runtime
WORKDIR /app

# Expose the default ASP.NET Core port
# Render will automatically detect this or use the PORT env var
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Copy the published SELF-CONTAINED output from the build stage
COPY --from=build /app/publish .

# For self-contained apps, we execute the generated binary directly
# We use a shell form to ensure environment variables like PORT are processed if we wanted to 
# but .NET 8+ handles ASPNETCORE_HTTP_PORTS well.
ENTRYPOINT ["./FlowSpace.Api"]
