# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy only the project file(s) and restore dependencies
COPY "TrainTray food order booking system/*.csproj" "./TrainTray food order booking system/"
WORKDIR "/app/TrainTray food order booking system"
RUN dotnet restore

# Copy the rest of the application and build it
COPY . ./
RUN dotnet publish -c Release -o out

# Use the runtime image to run the built application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the built files from the build stage
COPY --from=build-env /app/TrainTray food order booking system/out .

# Set the entry point for the application
CMD ["dotnet", "TrainTray food order booking system.dll"]
