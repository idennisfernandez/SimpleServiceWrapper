# SimpleServiceWrapper
A simple .net framework based windows service wrapper for dotnet core. 
Once install, it monitors the started application every 1 minute and restarts it if stopped.

Configuration.

Sample config for Dotnet Core application.

<appSettings>
    <add key="instancecount" value="1" />  
  No. of instances of the application the service will spawn.
    <add key="executablepath" value="dotnet.exe" />
    <add key="applicationpath" value="C:\sample\test.dll" />
  Path to you application assembly.
  </appSettings>

