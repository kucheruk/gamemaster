FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as final
RUN apt-get update && apt-get install -y \
    iputils-ping \
 && rm -rf /var/lib/apt/lists/* 
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
RUN env
ENTRYPOINT ["dotnet", "gamemaster.dll"]
