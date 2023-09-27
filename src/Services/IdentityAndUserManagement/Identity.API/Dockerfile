#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/IdentityAndUserManagement/Identity.API/Identity.API.csproj", "Services/IdentityAndUserManagement/Identity.API/"]
COPY ["Services/IdentityAndUserManagement/Identity.DAL/Identity.DAL.csproj", "Services/IdentityAndUserManagement/Identity.DAL/"]
RUN dotnet restore "Services/IdentityAndUserManagement/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/Services/IdentityAndUserManagement/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]