# Stage 1: Build the application
# Use the .NET SDK image for building your application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file first to restore dependencies.
# This assumes the Dockerfile is in the same directory as the .csproj.
COPY ["BackEnd-FLOWER_SHOP.csproj", "./"]

# Restore NuGet packages for the project
RUN dotnet restore "BackEnd-FLOWER_SHOP.csproj"

# Copy the rest of the application code from the current build context (D:\dex\FlowerShop)
# into the container's /src directory.
COPY . .

# Build the application
# We are already in /src, which contains BackEnd-FLOWER_SHOP.csproj
RUN dotnet build "BackEnd-FLOWER_SHOP.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# The WORKDIR is already /src from the previous stage.
RUN dotnet publish "BackEnd-FLOWER_SHOP.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from the 'publish' stage
COPY --from=publish /app/publish .

# Expose port 80 (standard for ASP.NET Core web apps in containers)
EXPOSE 80

# Define the entry point for the container
# This is the command that runs when the container starts
ENTRYPOINT ["dotnet", "BackEnd-FLOWER_SHOP.dll"]