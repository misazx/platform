FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RoguelikeGame.Server.csproj", "./"]
RUN dotnet restore "RoguelikeGame.Server.csproj"
COPY . .
RUN dotnet publish "RoguelikeGame.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "RoguelikeGame.Server.dll"]
