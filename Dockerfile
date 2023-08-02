FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore ChatServer.WebApi/ChatServer.WebApi.csproj
RUN ChatServer.WebApi/ChatServer.WebApi.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "ChatServer.WebApi.dll"]