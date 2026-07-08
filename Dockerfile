FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/CarRent.Model/ src/CarRent.Model/
COPY src/CarRent.DAL/ src/CarRent.DAL/
COPY src/CarRent.Web/ src/CarRent.Web/
RUN dotnet restore src/CarRent.Web/CarRent.Web.csproj
RUN dotnet publish src/CarRent.Web/CarRent.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DatabaseProvider=Sqlite
RUN mkdir -p /app/Data /app/logs /app/wwwroot/uploads
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CarRent.Web.dll"]
