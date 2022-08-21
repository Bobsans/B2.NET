param(
    [Parameter(Mandatory)][string]$path,
    [Parameter(Mandatory)][bool]$increment
)

$oldpath = Get-Location
Set-Location $path

if ($increment) {
    $file = $( Get-ChildItem *.csproj )[0]
    $regex = '(?<=Version>(?:\d+\.)+)(\d+)(?=<)'
    $found = (Get-Content $file) | Select-String -Pattern $regex
    $incremented = [int]$found.matches[0].value + 1
    (Get-Content $file) -replace $regex, $incremented | Set-Content $file -Encoding UTF8
}

dotnet pack --configuration Release
foreach ($file in $( Get-ChildItem .\bin\Release\*.nupkg )) {
    dotnet nuget push $file --skip-duplicate
}

Set-Location $oldpath
