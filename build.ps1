$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$source = Join-Path $PSScriptRoot 'Program.cs'
$output = Join-Path $PSScriptRoot 'CosmeticSavePatcher.exe'
& $compiler /nologo /optimize+ /target:exe /platform:anycpu /r:System.Core.dll "/out:$output" $source
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Write-Output "Built $output"
