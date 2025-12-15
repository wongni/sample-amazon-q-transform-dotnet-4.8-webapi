# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (layer caching)
COPY ["ProductsWebAPI/ProductsWebAPI.csproj", "ProductsWebAPI/"]
RUN dotnet restore "ProductsWebAPI/ProductsWebAPI.csproj"

# Copy source code and build
COPY ProductsWebAPI/ ProductsWebAPI/
WORKDIR /src/ProductsWebAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
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

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/api/products || exit 1

# Container metadata labels
LABEL maintainer="products-api"
LABEL application="products-api"
LABEL version="1.0"

# Entry point
ENTRYPOINT ["dotnet", "ProductsWebAPI.dll"]
