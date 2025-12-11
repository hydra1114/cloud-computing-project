FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["InventoryApi.csproj", "./"]
RUN dotnet restore "InventoryApi.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "InventoryApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InventoryApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryApi.dll"]
