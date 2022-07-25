FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder

COPY . .
WORKDIR BackendSite/BackendSite
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=builder /app .
RUN apt-get update -y && apt-get install -y libgdiplus
EXPOSE 80

ENTRYPOINT ["dotnet", "BackendSite.dll"]
