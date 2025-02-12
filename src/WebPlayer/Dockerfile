FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/WebPlayer/WebPlayer.csproj", "src/WebPlayer/"]
COPY ["src/PlayerCore/PlayerCore.csproj", "src/PlayerCore/"]
COPY ["src/LegacyASL/LegacyASL.csproj", "src/LegacyASL/"]
COPY ["src/IASL/IASL.csproj", "src/IASL/"]
COPY ["src/Utility/Utility.csproj", "src/Utility/"]
COPY ["src/WorldModel/WorldModel.csproj", "src/WorldModel/"]
RUN dotnet restore "src/WebPlayer/WebPlayer.csproj"
COPY . .
WORKDIR "/src/src/WebPlayer"
RUN dotnet build "WebPlayer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebPlayer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebPlayer.dll"]
