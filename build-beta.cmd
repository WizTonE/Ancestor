@ECHO OFF
SET /P VER=Enter the version [1]: 
IF /I "%VER%" EQU "" SET "ver=1"
nuget pack src\Ancestor\Ancestor.Core\Ancestor.Core.csproj -OutputDirectory build\nupkgs\ -Build -Properties Configuration=Release;Beta=%VER%
pause