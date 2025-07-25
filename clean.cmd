del .\bin\*.* /s /q
for /d %%p in (.\bin\*) do rd "%%p" /s /q

rmdir .\.vs /s /q

del .\TestResult.xml

rmdir .\BSLib.TeamsNet\bin /s /q
rmdir .\BSLib.TeamsNet\obj /s /q

rmdir .\BSLib.TeamsNet.UI\bin /s /q
rmdir .\BSLib.TeamsNet.UI\obj /s /q

rmdir .\BSLib.TeamsNet.App\bin /s /q
rmdir .\BSLib.TeamsNet.App\obj /s /q

rmdir .\BSLib.TeamsNet.Tests\bin /s /q
rmdir .\BSLib.TeamsNet.Tests\obj /s /q
rmdir .\BSLib.TeamsNet.Tests\OpenCover /s /q
