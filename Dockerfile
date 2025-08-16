# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file first to restore dependencies
COPY ["BackEnd-FLOWER_SHOP.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "BackEnd-FLOWER_SHOP.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet build "BackEnd-FLOWER_SHOP.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BackEnd-FLOWER_SHOP.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output
COPY --from=publish /app/publish .

# Render uses PORT environment variable
EXPOSE $PORT

# Configure ASP.NET Core to use the PORT environment variable
ENV ASPNETCORE_URLS=http://*:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "BackEnd-FLOWER_SHOP.dll"]