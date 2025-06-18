FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef && \
    dotnet tool install --global dotnet-aspnet-codegenerator
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["AdminDashboard/AdminDashboard.csproj", "AdminDashboard/"]
RUN dotnet restore "AdminDashboard/AdminDashboard.csproj"

COPY . .
WORKDIR "/src/AdminDashboard"
RUN dotnet build "AdminDashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdminDashboard.csproj" -c Release -o /app/publish \
    --no-restore \
    --no-build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl openssl && \
    rm -rf /var/lib/apt/lists/*

ENV Serilog__WriteTo__0__Name=Console
ENV Serilog__WriteTo__0__Args__outputTemplate="[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s \
  CMD curl -f http://localhost:80/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "AdminDashboard.dll"]