FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything
COPY . .
# Restore as distinct layers
RUN cd Oculus.Common && dotnet restore
RUN cd Oculus.Kernel && dotnet restore
# Build and publish a release
RUN cd Oculus.Kernel && dotnet publish -c Release -o ../out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Oculus.Kernel.dll"]