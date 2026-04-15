FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DepartementService/DepartementService.csproj", "DepartementService/"]
RUN dotnet restore "DepartementService/DepartementService.csproj"

COPY . .
WORKDIR "/src/DepartementService"

RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DepartementService.dll"]