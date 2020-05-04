ARG VERSION=3.1-alpine3.10

# Build

FROM mcr.microsoft.com/dotnet/core/sdk:$VERSION AS build

WORKDIR /app

COPY /Axion.Core/Axion.Core.csproj .

RUN dotnet nuget add source https://www.myget.org/F/discord-net/api/v3/index.json -n MyGet
RUN dotnet restore Axion.Core.csproj

COPY . .
RUN dotnet publish Axion.Core -c Release -o out

# Runtime

FROM mcr.microsoft.com/dotnet/core/aspnet:$VERSION

RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app
  
RUN apk add --no-cache bash

USER app
WORKDIR /app

ENV TERM xterm

COPY --from=build /app/out .
COPY appsettings.json .


ENTRYPOINT ["dotnet", "Axion.Core.dll"]