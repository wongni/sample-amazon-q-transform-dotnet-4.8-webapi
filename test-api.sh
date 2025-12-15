#!/bin/bash

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}=== Products API Test ===${NC}\n"

SQL_PASSWORD="YourStrong@Passw0rd"
SQL_CONTAINER="products-sqlserver"
API_CONTAINER="products-api"
API_PORT=8080

cleanup() {
    docker stop $API_CONTAINER $SQL_CONTAINER 2>/dev/null || true
    docker rm $API_CONTAINER $SQL_CONTAINER 2>/dev/null || true
}

cleanup

# Start SQL Server
echo -e "${GREEN}1. Starting SQL Server...${NC}"
docker run -d --name $SQL_CONTAINER \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=$SQL_PASSWORD" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest > /dev/null

sleep 25

# Create database and table
echo -e "${GREEN}2. Creating database and table...${NC}"
docker exec $SQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SQL_PASSWORD" -C \
  -Q "CREATE DATABASE ProductsDB;"

docker exec $SQL_CONTAINER /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SQL_PASSWORD" -C -d ProductsDB \
  -Q "CREATE TABLE Products (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Category NVARCHAR(50) NOT NULL, Price DECIMAL(18,2) NOT NULL);"

# Build and run API
echo -e "${GREEN}3. Building and starting API...${NC}"
docker build -t products-api:test . > /dev/null 2>&1

CONNECTION_STRING="Server=$SQL_CONTAINER;Database=ProductsDB;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=True;"

docker run -d --name $API_CONTAINER \
  --link $SQL_CONTAINER:sqlserver \
  -p $API_PORT:80 \
  -e "ConnectionStrings__ProductsContext=$CONNECTION_STRING" \
  products-api:test > /dev/null

sleep 15

BASE_URL="http://localhost:$API_PORT/api/products"

echo -e "${GREEN}4. Running endpoint tests...${NC}\n"

# Test 1: GET empty
echo -e "${YELLOW}Test 1: GET /api/products (empty)${NC}"
curl -s $BASE_URL
echo -e "\n"

# Test 2: POST create
echo -e "${YELLOW}Test 2: POST /api/products${NC}"
RESPONSE=$(curl -s -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","category":"Electronics","price":999.99}')
echo "$RESPONSE"
PRODUCT_ID=$(echo "$RESPONSE" | grep -o '"id":[0-9]*' | grep -o '[0-9]*')
echo -e "\nCreated ID: $PRODUCT_ID\n"

# Test 3: GET all
echo -e "${YELLOW}Test 3: GET /api/products${NC}"
curl -s $BASE_URL
echo -e "\n"

# Test 4: GET by ID
echo -e "${YELLOW}Test 4: GET /api/products/$PRODUCT_ID${NC}"
curl -s "$BASE_URL/$PRODUCT_ID"
echo -e "\n"

# Test 5: PUT update
echo -e "${YELLOW}Test 5: PUT /api/products/$PRODUCT_ID${NC}"
curl -s -X PUT "$BASE_URL/$PRODUCT_ID" \
  -H "Content-Type: application/json" \
  -d "{\"id\":$PRODUCT_ID,\"name\":\"Gaming Laptop\",\"category\":\"Electronics\",\"price\":1299.99}"
echo -e "\n"

# Test 6: POST another
echo -e "${YELLOW}Test 6: POST another product${NC}"
curl -s -X POST $BASE_URL \
  -H "Content-Type: application/json" \
  -d '{"name":"Mouse","category":"Accessories","price":29.99}'
echo -e "\n"

# Test 7: GET all
echo -e "${YELLOW}Test 7: GET /api/products (2 items)${NC}"
curl -s $BASE_URL
echo -e "\n"

# Test 8: DELETE
echo -e "${YELLOW}Test 8: DELETE /api/products/$PRODUCT_ID${NC}"
curl -s -X DELETE "$BASE_URL/$PRODUCT_ID" -w "\nHTTP Status: %{http_code}\n"
echo -e "\n"

# Test 9: GET final
echo -e "${YELLOW}Test 9: GET /api/products (after delete)${NC}"
curl -s $BASE_URL
echo -e "\n"

echo -e "${GREEN}=== All tests completed ===${NC}"
echo -e "${YELLOW}Cleanup: docker stop $API_CONTAINER $SQL_CONTAINER && docker rm $API_CONTAINER $SQL_CONTAINER${NC}"
