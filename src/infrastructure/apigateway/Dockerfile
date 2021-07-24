#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["infrastructure/apigateway/SchoolMngr.Infrastructure.ApiGateway.csproj", "infrastructure/apigateway/"]
COPY ["infrastructure/shared/SchoolMngr.Infrastructure.Shared.csproj", "infrastructure/shared/"]
COPY ["../../Codeit.NetStdLibrary.Base/Codeit.NetStdLibrary.Base/Codeit.NetStdLibrary.Base.csproj", "../../Codeit.NetStdLibrary.Base/Codeit.NetStdLibrary.Base/"]
COPY ["../../Codeit.NetStdLibrary.Base/Codeit.NetStdLibrary.Base.Abstractions/Codeit.NetStdLibrary.Base.Abstractions.csproj", "../../Codeit.NetStdLibrary.Base/Codeit.NetStdLibrary.Base.Abstractions/"]
RUN dotnet restore "infrastructure/apigateway/SchoolMngr.Infrastructure.ApiGateway.csproj"
COPY . .
WORKDIR "/src/infrastructure/apigateway"
RUN dotnet build "SchoolMngr.Infrastructure.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolMngr.Infrastructure.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolMngr.Infrastructure.ApiGateway.dll"]