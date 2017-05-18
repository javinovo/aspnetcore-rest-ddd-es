FROM microsoft/aspnetcore:1.0

WORKDIR /app

# From the context 'publish' subdir to the working dir on the image (relative paths refer to the workdir)
COPY publish .

# The 'aspnetcore' image instructs ASP.NET Core to use port 80 by setting: ENV ASPNETCORE_URLS http://+:80
ENTRYPOINT ["dotnet", "WebApp.dll"]