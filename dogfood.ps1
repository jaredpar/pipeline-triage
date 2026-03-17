$pipelineRoot = $PSScriptRoot
$marketplacePath = Join-Path $pipelineRoot "artifacts\marketplace"
$dogfoodPath = Join-Path $pipelineRoot "artifacts\dogfood"
$packagePath = Join-Path $pipelineRoot "artifacts\package"

if (Test-Path $packagePath) { Remove-Item $packagePath -Recurse -Force }
if (Test-Path $marketplacePath) { Remove-Item $marketplacePath -Recurse -Force }
New-Item -ItemType Directory -Path $dogfoodPath -Force | Out-Null

dotnet pack (Join-Path $pipelineRoot "Pipeline.slnx") -c Debug --nologo
New-Item -ItemType Directory -Path $marketplacePath -Force | Out-Null
Copy-Item -Path (Join-Path $pipelineRoot "marketplace.json") -Destination $marketplacePath -Force
Copy-Item -Path (Join-Path $pipelineRoot "plugins") -Destination (Join-Path $marketplacePath "plugins") -Recurse -Force

# Override the marketplace name for dogfooding
$marketplaceJsonPath = Join-Path $marketplacePath "marketplace.json"
$marketplaceContent = Get-Content $marketplaceJsonPath -Raw
$marketplaceContent = $marketplaceContent.Replace('"basic-plugins"', '"basic-plugins-dogfood"')
Set-Content $marketplaceJsonPath $marketplaceContent

# Override the nuget.org source with the local package output for dogfooding
Get-ChildItem -Path $marketplacePath -Filter "plugin.json" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content.Replace("https://api.nuget.org/v3/index.json", $packagePath)
    Set-Content $_.FullName $content
}

Push-Location $dogfoodPath
try {
    & copilot plugin marketplace add $marketplacePath
    & copilot plugin install basic-triage@basic-plugins-dogfood
    & copilot @args
}
finally {
    & copilot plugin marketplace remove basic-plugins-dogfood
    Pop-Location
}
