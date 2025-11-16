FROM node:lts AS build-ui
WORKDIR /ui

COPY acs-emulator-ui/package*.json ./
RUN npm install

COPY acs-emulator-ui/ ./
RUN npm run build --omit=dev

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-net
WORKDIR /app

COPY AcsEmulatorAPI/*.csproj ./AcsEmulatorAPI/
COPY NuGet.config ./

RUN dotnet restore AcsEmulatorAPI --configfile ./NuGet.config

COPY AcsEmulatorAPI ./AcsEmulatorAPI/

WORKDIR /app/AcsEmulatorAPI
RUN dotnet build
RUN dotnet publish -c Release -o out

COPY --from=build-ui /ui/build ./out/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-net /app/AcsEmulatorAPI/out .

VOLUME ["/data"]

ENV ASPNETCORE_HTTPS_PORTS=443

RUN mkdir -p /https /data \
    && addgroup --system appgroup \
    && adduser --system --ingroup appgroup appuser \
    && chown -R appuser:appgroup /app /https /data

EXPOSE 443

USER appuser

CMD ["dotnet", "AcsEmulatorAPI.dll"]
