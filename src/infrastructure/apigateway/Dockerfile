#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["core/apigateway/SchoolMngr.Services.ApiGateway/SchoolMngr.Services.ApiGateway.csproj", "core/apigateway/SchoolMngr.Services.ApiGateway/"]
RUN dotnet restore "core/apigateway/SchoolMngr.Services.ApiGateway/SchoolMngr.Services.ApiGateway.csproj"
COPY . .
WORKDIR "/src/core/apigateway/SchoolMngr.Services.ApiGateway"
RUN dotnet build "SchoolMngr.Services.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolMngr.Services.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolMngr.Services.ApiGateway.dll"]