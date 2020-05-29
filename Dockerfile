FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /app/build
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as final
COPY --from=build /app/build .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "exec", "gamemaster.dll"]