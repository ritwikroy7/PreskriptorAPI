
FROM microsoft/aspnetcore:1.1.1
LABEL Name="preskriptor-api" Version="1.0"
COPY out /app
WORKDIR /app
EXPOSE 5000/tcp
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "PreskriptorAPI.dll"]
