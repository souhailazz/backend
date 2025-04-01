# ----- Étape 1: Build du Backend .NET -----
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
    WORKDIR /app
    COPY AppartementReservationAPI/ AppartementReservationAPI/
    WORKDIR /app/AppartementReservationAPI
    RUN dotnet restore
    RUN dotnet publish -c Release -o /app/published
    
    # ----- Étape 2: Build du Frontend React/Vite -----
    FROM node:20 AS frontend-build
    WORKDIR /frontend
    COPY package.json package-lock.json ./
    RUN npm install
    COPY . .
    RUN npm run build
    
    # ----- Étape 3: Exécution du Backend -----
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS backend-runtime
    WORKDIR /app
    COPY --from=backend-build /app/published .
    COPY --from=frontend-build /frontend/dist ./wwwroot
    ENTRYPOINT ["dotnet", "AppartementReservationAPI.dll"]
    