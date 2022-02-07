#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Killboard.API/Killboard.API.csproj", "Killboard.API/"]
COPY ["Killboard.Domain/Killboard.Domain.csproj", "Killboard.Domain/"]
COPY ["Killboard.Data/Killboard.Data.csproj", "Killboard.Data/"]
RUN dotnet restore "Killboard.API/Killboard.API.csproj"
COPY . .
WORKDIR "/src/Killboard.API"
RUN dotnet build "Killboard.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Killboard.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Killboard.API.dll"]