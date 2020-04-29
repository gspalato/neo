FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY /Axion.Core/Axion.Core.csproj .

RUN dotnet nuget add source https://www.myget.org/F/discord-net/api/v3/index.json -n MyGet

RUN dotnet restore Axion.Core.csproj

COPY . .
RUN dotnet publish Axion.Core -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out .
COPY appsettings.json .

ENV TERM xterm

ENTRYPOINT ["dotnet", "Axion.Core.dll"]