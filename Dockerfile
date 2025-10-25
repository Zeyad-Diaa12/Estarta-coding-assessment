# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["EmployeeStatus/EmployeeStatus.csproj", "EmployeeStatus/"]
RUN dotnet restore "EmployeeStatus/EmployeeStatus.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/EmployeeStatus"
RUN dotnet build "EmployeeStatus.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EmployeeStatus.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeStatus.dll"]
