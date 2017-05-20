FROM microsoft/aspnetcore:1.0

WORKDIR /app

# From the context 'publish' subdir to the working dir on the image (relative paths refer to the workdir)
COPY publish .

# We can opt-in for Event Store through an environment variable, a command line argument, or through the appsettings.json file
# ENV "EventStoreOptions:ServerUri" "tcp://localhost:1113"

# The 'aspnetcore' image instructs ASP.NET Core to use port 80 by setting: ENV ASPNETCORE_URLS http://+:80
ENTRYPOINT ["dotnet", "WebApp.dll"]