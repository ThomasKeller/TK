function CreateJunction {
  param($nugetName, $toolName, $relativePath)
  Write-Host "Install: $nugetName"
  & .\nuget.exe install $nugetName
  $fullToolName = Get-ChildItem -Filter "$toolName*"
  Write-Host "Create Junction: $toolName $fullToolName$relativePath"
  & .\junction.exe $toolName "$fullToolName$relativePath"
  write-host "done..."
}

cls
CreateJunction NUnit.Runners NUnit \tools