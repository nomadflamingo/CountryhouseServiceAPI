FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY ["CountryhouseService.API/CountryhouseService.API.csproj", "CountryhouseService.API/"]
RUN dotnet restore "CountryhouseService.API/CountryhouseService.API.csproj"
COPY . .
WORKDIR "/src/CountryhouseService.API"
RUN dotnet build "CountryhouseService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CountryhouseService.API.dll"]