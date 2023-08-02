FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

COPY . ./
RUN ChatServer.WebApi/ChatServer.WebApi.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ChatServer.WebApi.dll"]