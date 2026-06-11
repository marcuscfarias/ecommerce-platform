FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY [".editorconfig", "./"]
COPY ["Ecommerce.Admin.Web/Ecommerce.Admin.Web.csproj", "Ecommerce.Admin.Web/"]
RUN dotnet restore "Ecommerce.Admin.Web/Ecommerce.Admin.Web.csproj"

COPY . .
WORKDIR "/src/Ecommerce.Admin.Web"
RUN dotnet publish "./Ecommerce.Admin.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM nginx:alpine AS final
COPY Ecommerce.Admin.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 443
