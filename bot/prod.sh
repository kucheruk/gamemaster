#!/bin/sh
ASPNETCORE_ENVIRONMENT=production
dotnet clean
dotnet build
docker-compose --context botprod up -d
# docker-compose --context botstaging down
# docker-compose --context botstaging up -d
