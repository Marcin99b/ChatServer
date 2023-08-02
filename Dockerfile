FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ChatServer.WebApi/ChatServer.WebApi.csproj", "ChatServer.WebApi/"]
RUN dotnet restore "ChatServer.WebApi/ChatServer.WebApi.csproj"
COPY . .
WORKDIR "/src/ChatServer.WebApi"
RUN dotnet build "ChatServer.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatServer.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
USER ContainerUser
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ConsoleApp1.dll"]