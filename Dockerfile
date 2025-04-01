# ----- Step 1: Build the .NET Backend -----
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
    WORKDIR /app
    COPY . . 
    RUN dotnet restore
    RUN dotnet publish -c Release -o /app/published
    
    # ----- Step 2: Serve the Backend -----
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS backend-runtime
    WORKDIR /app
    COPY --from=backend-build /app/published .
    
    # Expose the API port
    EXPOSE 5000
    
    # Run the Backend API
    ENTRYPOINT ["dotnet", "AppartementReservationAPI.dll"]
    