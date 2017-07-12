@echo off
nuget pack src\Ancestor\Ancestor.Core\Ancestor.Core.csproj -OutputDirectory build\nupkgs\  -Properties Configuration=Release
nuget pack src\Ancestor\Ancestor.DataAccess\Ancestor.DataAccess.csproj -OutputDirectory build\nupkgs\   -Properties Configuration=Release
pause