del .\bin\*.* /s /q
for /d %%p in (.\bin\*) do rd "%%p" /s /q

rmdir .\.vs /s /q

del .\TestResult.xml

rmdir .\GKNetCore\bin /s /q
rmdir .\GKNetCore\obj /s /q

rmdir .\GKNetUI\bin /s /q
rmdir .\GKNetUI\obj /s /q

rmdir .\GKNetUI.EtoForms\bin /s /q
rmdir .\GKNetUI.EtoForms\obj /s /q

rmdir .\GKCommunicatorApp\bin /s /q
rmdir .\GKCommunicatorApp\obj /s /q

rmdir .\GKCommunicatorPlugin\bin /s /q
rmdir .\GKCommunicatorPlugin\obj /s /q

rmdir .\GKNetCore.Tests\bin /s /q
rmdir .\GKNetCore.Tests\obj /s /q
rmdir .\GKNetCore.Tests\OpenCover /s /q

rmdir .\GKNetLocationsPlugin\bin /s /q
rmdir .\GKNetLocationsPlugin\obj /s /q
