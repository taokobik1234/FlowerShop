﻿# Flower-Shop
Clone then run following command:
cd BackEnd-FLOWER_SHOP # cd into BackEnd-FLOWER_SHOP folder
dotnet restore # Restore dependencies
dotnet run # Start the development server (Open the link and start coding)
dotnet build # Build for production

Incase there is still error, try running:
dotnet add package Microsoft.AspNetCore.Authentication --version 8.0.0
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0

Project structure:
/

├── Controllers/ # Holds API controller files (e.g., for categories or authentication). 
├── DTOs/ # Contains Data Transfer Objects used to shape data sent between client and server. Look here to update or create new data models.s
├── Data/ # Manages database access and context. This is where database connections and operations are handled—edit if you need to adjust the database setup.
├── Entities/ # Stores the entity models representing database tables. Update these files to reflect changes in the database structure.
├── Enums/ # ncludes enumeration types for specific data values (e.g., status codes). Add or modify enums here as needed for the project logic.
├── Migrations/ # Database migrations
├── Properties/ # Project properties
├── Services/ # Contains service layer files for business logic (e.g., category API logic). Expand or modify services here for new features.
├── .gitignore # Git ignore file
├── BackEnd-FLOWER_SHOP.csproj # Project file
├── BackEnd-FLOWER_SHOP.http # HTTP request file
├── Program.cs # Application entry point
└── appsettings.Development.json # Configuration file for development settings (e.g., database connection strings). Edit this for local environment tweaks.
