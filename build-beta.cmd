@echo off
SET /P VER=Enter the version [1]: 
IF /I "%VER%" EQU "" set "ver=1"
nuget pack src\Ancestor\Ancestor.Core\Ancestor.Core.csproj -Suffix Beta.%VER% -OutputDirectory build\nupkgs\ -Build
nuget pack src\Ancestor\Ancestor.DataAccess\Ancestor.DataAccess.csproj -Suffix Beta.%VER% -OutputDirectory build\nupkgs\ -Build
pause
