git.exe pull --progress -v --no-rebase "origin"
C:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe GKConnectivity.sln /t:Clean /p:Configuration=Debug
C:\windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe GKConnectivity.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /flp:Summary;Verbosity=minimal
rem "C:\Program Files (x86)\NUnit 2.6.4\bin\nunit-console-x86.exe" GKCommTests\bin\Debug\GKCommTests.dll
.\GKCommunicatorApp\bin\Debug\GKCommunicatorApp.exe
