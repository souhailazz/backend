# ----- Step 1: Build the .NET Backend -----
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
    WORKDIR /app
    
    # Copy all files to the container
    COPY . .
    
    # Specify the exact project file for restore and publish
    RUN dotnet restore AppartementReservationAPI.csproj
    
    # Publish the project to the /app/published directory
    RUN dotnet publish AppartementReservationAPI.csproj -c Release -o /app/published
    
    # ----- Step 2: Serve the Backend -----
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS backend-runtime
    WORKDIR /app
    
    # Copy the published files from the build stage to the runtime stage
    COPY --from=backend-build /app/published .
    
    # Expose the API port that Railway will use
    EXPOSE 8080
    
    # Run the Backend API
    ENTRYPOINT ["dotnet", "AppartementReservationAPI.dll"]
    