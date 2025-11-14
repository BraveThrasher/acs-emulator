FROM node:lts AS build-ui
WORKDIR /ui

COPY acs-emulator-ui/package*.json ./
RUN npm install

COPY acs-emulator-ui/ ./
RUN npm run build --omit=dev

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-net
WORKDIR /app

COPY AcsEmulatorAPI/*.csproj ./AcsEmulatorAPI/
COPY AcsEmulatorAPI.Tests/*.csproj ./AcsEmulatorAPI.Tests/
COPY NuGet.config ./

RUN dotnet restore AcsEmulatorAPI --configfile ./NuGet.config
RUN dotnet restore AcsEmulatorAPI.Tests --configfile ./NuGet.config

COPY AcsEmulatorAPI ./AcsEmulatorAPI/
COPY AcsEmulatorAPI.Tests ./AcsEmulatorAPI.Tests/

WORKDIR /app/AcsEmulatorAPI.Tests
RUN dotnet test

WORKDIR /app/AcsEmulatorAPI
RUN dotnet build
RUN dotnet publish -c Release -o out

COPY --from=build-ui /ui/build ./out/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-net /app/AcsEmulatorAPI/out .

ENV ASPNETCORE_URLS=https://0.0.0.0;http://0.0.0.0
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=443
EXPOSE 8080
EXPOSE 443

CMD ["dotnet", "--additional-deps", "AcsEmulatorAPI.deps.json", "AcsEmulatorAPI.dll"]
