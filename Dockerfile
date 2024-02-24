# Use the official ASP.NET Core SDK 8.0 image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy the project files to the container
COPY . .

# Restore NuGet packages
RUN dotnet restore

# Build the application
RUN dotnet build -c Release -o /app/build

# Run the unit tests
# RUN dotnet test

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose the port
EXPOSE 5243

# Start the ASP.NET Core application
CMD ["dotnet", "Kyoto.dll"]
