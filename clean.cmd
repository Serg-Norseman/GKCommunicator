rmdir .\.vs /s /q

rmdir .\GKNetCore\bin /s /q
rmdir .\GKNetCore\obj /s /q

rmdir .\GKCommunicatorApp\bin /s /q
rmdir .\GKCommunicatorApp\obj /s /q

rmdir .\GKCommunicatorPlugin\bin /s /q
rmdir .\GKCommunicatorPlugin\obj /s /q

rmdir .\GKNetCore.Tests\bin /s /q
rmdir .\GKNetCore.Tests\obj /s /q
rmdir .\GKNetCore.Tests\OpenCover /s /q

del msbuild.log

rmdir .\cov-int /s /q
del .\coverity.zip

rmdir .\.sonarqube /s /q
del .\*.xml
