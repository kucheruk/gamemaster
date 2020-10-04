#!/bin/sh
ASPNETCORE_ENVIRONMENT=staging
dotnet clean
dotnet build
docker-compose --context botstaging up -d
# docker-compose --context botstaging down
# docker-compose --context botstaging up -d
