call .\clean.cmd

git.exe pull --progress -v --no-rebase "origin"
C:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe GKCommunicator.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
.\GKCommunicatorApp\bin\Debug\GKCommunicatorApp.exe
