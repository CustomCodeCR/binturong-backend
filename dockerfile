# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && \
  apt-get install -y tzdata apt-utils libgdiplus libc6-dev && \
  ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
  rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app/wwwroot && chmod 755 /app/wwwroot
WORKDIR /app
EXPOSE 8080
ENV TZ=America/Costa_Rica
ENV ASPNETCORE_URLS=http://+:8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/binturong.Api/binturong.Api.csproj", "src/binturong.Api/"]
COPY ["src/binturong.Application/binturong.Application.csproj", "src/binturong.Application/"]
COPY ["src/binturong.Domain/binturong.Domain.csproj", "src/binturong.Domain/"]
COPY ["src/binturong.Infrastructure/binturong.Infrastructure.csproj", "src/binturong.Infrastructure/"]
COPY ["src/binturong.SharedKernel/binturong.SharedKernel.csproj", "src/binturong.SharedKernel/"]

RUN dotnet restore "src/binturong.Api/binturong.Api.csproj"

COPY . .

WORKDIR "/src/src/binturong.Api"
RUN dotnet build "binturong.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/binturong.Api

RUN dotnet publish "binturong.Api.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 755 /app

ENTRYPOINT ["dotnet", "binturong.Api.dll"]
