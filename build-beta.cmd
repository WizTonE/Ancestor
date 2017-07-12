@echo off
nuget pack src\Ancestor\Ancestor.Core\Ancestor.Core.csproj -Suffix Beta -OutputDirectory build\nupkgs\ -Build
nuget pack src\Ancestor\Ancestor.DataAccess\Ancestor.DataAccess.csproj -Suffix Beta -OutputDirectory build\nupkgs\ -Build
pause
