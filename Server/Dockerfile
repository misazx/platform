FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RoguelikeGame.Server/RoguelikeGame.Server.csproj", "RoguelikeGame.Server/"]
COPY ["RoguelikeGame.Shared/RoguelikeGame.Shared.csproj", "RoguelikeGame.Shared/"]
RUN dotnet restore "RoguelikeGame.Server/RoguelikeGame.Server.csproj"
COPY . .
RUN dotnet publish "RoguelikeGame.Server/RoguelikeGame.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5002
EXPOSE 5003

ENV ASPNETCORE_URLS=http://0.0.0.0
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RoguelikeGame.Server.dll"]
