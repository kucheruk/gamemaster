#!/bin/sh
export ASPNETCORE_ENVIRONMENT=production
docker-compose --context botprod build
docker-compose --context botprod up -d
# docker-compose --context botstaging down
# docker-compose --context botstaging up -d
