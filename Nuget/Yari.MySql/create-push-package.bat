@echo off
set /p Version= Enter package version (1.0.X): 
echo Generating package verison %Version%
"C:\Program Files\Nuget\nuget" pack "N:\Documents\PrimeVoiX\yari\master\Nuget\Yari.MySql\Yari.MySql.nuspec" -OutputDirectory "N:\Documents\PrimeVoiX\yari\master\Nuget\Yari.MySql" -Version %Version%
dotnet nuget push "N:\Documents\PrimeVoiX\yari\master\Nuget\Yari.MySql\Yari.MySql.%Version%.nupkg" -k oy2bswk7hgrgpyv2f2vafui3iyfhwq4ejdr6gtvdfa5k64 -s https://api.nuget.org/v3/index.json
pause