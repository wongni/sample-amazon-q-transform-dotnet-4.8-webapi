# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ProductsWebAPI/ProductsWebAPI.csproj", "ProductsWebAPI/"]
RUN dotnet restore "ProductsWebAPI/ProductsWebAPI.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/ProductsWebAPI"
RUN dotnet publish "ProductsWebAPI.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published output
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/api/products || exit 1

# Set entrypoint
ENTRYPOINT ["dotnet", "ProductsWebAPI.dll"]
