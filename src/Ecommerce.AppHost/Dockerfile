FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Ecommerce.AppHost/Ecommerce.AppHost.csproj", "Ecommerce.AppHost/"]
COPY ["Ecommerce.Catalog/Ecommerce.Catalog.Api/Ecommerce.Catalog.Api.csproj", "Ecommerce.Catalog/Ecommerce.Catalog.Api/"]
COPY ["Ecommerce.Catalog/Ecommerce.Catalog.Application/Ecommerce.Catalog.Application.csproj", "Ecommerce.Catalog/Ecommerce.Catalog.Application/"]
COPY ["Ecommerce.Catalog/Ecommerce.Catalog.Domain/Ecommerce.Catalog.Domain.csproj", "Ecommerce.Catalog/Ecommerce.Catalog.Domain/"]
COPY ["Ecommerce.Catalog/Ecommerce.Catalog.Infrastructure/Ecommerce.Catalog.Infrastructure.csproj", "Ecommerce.Catalog/Ecommerce.Catalog.Infrastructure/"]
COPY ["Ecommerce.Kernel/Ecommerce.Kernel.API/Ecommerce.Kernel.API.csproj", "Ecommerce.Kernel/Ecommerce.Kernel.API/"]
COPY ["Ecommerce.Kernel/Ecommerce.Kernel.Application/Ecommerce.Kernel.Application.csproj", "Ecommerce.Kernel/Ecommerce.Kernel.Application/"]
COPY ["Ecommerce.Kernel/Ecommerce.Kernel.Domain/Ecommerce.Kernel.Domain.csproj", "Ecommerce.Kernel/Ecommerce.Kernel.Domain/"]
COPY ["Ecommerce.Kernel/Ecommerce.Kernel.Infrastructure/Ecommerce.Kernel.Infrastructure.csproj", "Ecommerce.Kernel/Ecommerce.Kernel.Infrastructure/"]
RUN dotnet restore "Ecommerce.AppHost/Ecommerce.AppHost.csproj"

COPY . .
WORKDIR "/src/Ecommerce.AppHost"
RUN dotnet publish "./Ecommerce.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.AppHost.dll"]
