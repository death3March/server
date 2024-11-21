FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /HackU_2024_server

COPY . .
RUN dotnet restore ./HackU_2024_server
RUN dotnet publish -c Release -o out ./HackU_2024_server

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /HackU_2024_server
COPY --from=build-env /HackU_2024_server/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "HackU_2024_server.dll"]