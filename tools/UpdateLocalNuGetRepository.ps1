function UpdateLocalNuGetPath {
    param($localNugetPath)

    Write-Host "Set local NuGet path"
    if (!$localNugetPath)
    {
        $localNugetPath = Get-Item -path .. 
        $localNuGetPath = $localNugetPath.FullName + '\localNuGet'
    }
    $nuGetConfigFiles = Get-ChildItem -path '..' -Filter "NuGet.config" -Recurse
    
    foreach($nuGetConfigFile in $nuGetConfigFiles) {
        Write-Host "found: " $nuGetConfigFile.FullName -ForegroundColor Green

        [xml]$myXML = Get-Content $nuGetConfigFile.FullName
        foreach($element in $myXML.configuration.packageSources.add) {
            if ($element.Key -eq 'localNuGet') {
                  $element.Value = $localNugetPath
                Write-Host "modify: " $nuGetConfigFile.FullName -ForegroundColor Green
                $myXML.Save($nuGetConfigFile.FullName);
            } 
        }
    }
}

UpdateLocalNuGetPath($args[0])

