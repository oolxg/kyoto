FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Smug.csproj", "./"]
RUN dotnet restore "Smug.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Smug.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Smug.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Smug.dll"]
