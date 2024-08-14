# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy the project file and restore dependencies
#COPY NewFace.csproj .
COPY *.csproj ./
RUN dotnet restore

# Copy all the source files and build the application
#COPY . .
#RUN dotnet publish -c Release -o /app --no-restore
COPY . ./
RUN dotnet publish -c Release -o /app

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "NewFace.dll"]