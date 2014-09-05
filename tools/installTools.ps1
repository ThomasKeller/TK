function CreateJunction {
  param($nugetName, $toolName, $relativePath)
  Write-Host "Install: $nugetName"
  $checkName = Get-ChildItem -Filter $toolName
  if (!$checkName)
  {
    & .\nuget.exe install $nugetName
    $fullToolName = Get-ChildItem -Filter "$nugetName*"
	if ($fullToolName)
	{
      Write-Host "Create Junction: $toolName $fullToolName$relativePath"
      & .\junction.exe $toolName "$fullToolName$relativePath"
      write-host "done..."
	}
  }
  else
  {
    Write-Host "$toolName is already installed"
  }
}

cls
# Add here the new NuGet Pacakages
# Parameter: 
# 1. NuGet package name (https://www.nuget.org/packages)
# 2. create symbolic links with the given name independent from the version number
# 3. relative path (e.g. NUnit.Runners.2.6.3\tools is the complete path)
CreateJunction NUnit.Runners NUnit \tools

#CreateJunction WiX.Toolset WixToolset \tools