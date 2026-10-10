FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG VERSION=1.0.0
WORKDIR /source

COPY .editorconfig Directory.Build.props Directory.Packages.props ./
COPY src/Server src/Server
RUN dotnet restore src/Server/Server.Host --arch $TARGETARCH
RUN dotnet publish src/Server/Server.Host --arch $TARGETARCH --no-restore --output /app -p:Version=$VERSION

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Server.Host.dll"]
