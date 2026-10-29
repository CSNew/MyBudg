# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# คัดลอกเฉพาะ csproj ก่อนเพื่อให้ layer ของ restore ถูก cache
COPY src/MyBudg.Entity/*.csproj src/MyBudg.Entity/
COPY src/MyBudg.Common/*.csproj src/MyBudg.Common/
COPY src/MyBudg.Data/*.csproj src/MyBudg.Data/
COPY src/MyBudg.Data.Postgres/*.csproj src/MyBudg.Data.Postgres/
COPY src/MyBudg.Business/*.csproj src/MyBudg.Business/
COPY src/MyBudg.WebApp/*.csproj src/MyBudg.WebApp/
RUN dotnet restore src/MyBudg.WebApp/MyBudg.WebApp.csproj
COPY src/ src/
RUN dotnet publish src/MyBudg.WebApp/MyBudg.WebApp.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# image ทางการมีผู้ใช้ "app" ที่ไม่ใช่ root อยู่แล้ว
USER app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=0
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyBudg.WebApp.dll"]
