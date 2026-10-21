# Розгортання на Windows. Параметри: -Server, -User, -Pass, -MongoUri
param([string]$Server="localhost",[string]$User="sa",[Parameter(Mandatory)][string]$Pass,[string]$MongoUri="mongodb://localhost:27017")
$ErrorActionPreference="Stop"; Set-Location $PSScriptRoot
foreach($f in "p1/schema.sql","p1/procedures.sql","p1/seed.sql","p1/security.sql","p2/schema.sql","p2/seed.sql"){
  sqlcmd -S $Server -U $User -P $Pass -C -b -I -f 65001 -i $f; if($LASTEXITCODE){throw "Помилка у $f"} }
foreach($f in "p3/collections.js","p3/seed.js"){ mongosh $MongoUri --quiet --file $f; if($LASTEXITCODE){throw "Помилка у $f"} }
Write-Host "Готово."
