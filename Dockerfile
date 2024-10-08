FROM mcr.microsoft.com/dotnet/sdk:9.0 as build-env
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY /src/BrowserlessBot/*.csproj ./
RUN dotnet nuget add source https://repository.decryptology.net/repository/Nuget/ -n decryptology.net
RUN dotnet restore 

# Copy everything else and build
COPY /src/BrowserlessBot ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build-env /src/out .
ENTRYPOINT dotnet BrowserlessBot.dll $TELEGRAM_BOT_TOKEN $BROWSERLESS_ENDPOINT $BROWSERLESS_TOKEN $ADMIN_CHATID