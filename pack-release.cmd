@echo off
nuget pack src\Ancestor\Ancestor.Core\Ancestor.Core.csproj -OutputDirectory build\nupkgs\
nuget pack src\Ancestor\Ancestor.DataAccess\Ancestor.DataAccess.csproj -OutputDirectory build\nupkgs\ 