#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/IdentityAndUserManagement/UserManagement.API/UserManagement.API.csproj", "Services/IdentityAndUserManagement/UserManagement.API/"]
COPY ["Services/IdentityAndUserManagement/Identity.DAL/Identity.DAL.csproj", "Services/IdentityAndUserManagement/Identity.DAL/"]
RUN dotnet restore "Services/IdentityAndUserManagement/UserManagement.API/UserManagement.API.csproj"
COPY . .
WORKDIR "/src/Services/IdentityAndUserManagement/UserManagement.API"
RUN dotnet build "UserManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagement.API.dll"]