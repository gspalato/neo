FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore

# Build and publish a release
RUN cd Neo.Core && dotnet publish -c Release -o ../out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Neo.Core.dll"]