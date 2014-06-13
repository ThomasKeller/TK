properties {
  $base_dir = resolve-path .\..
  $source_dir = "$base_dir\src"
  
  $framework = "4.5"
  $build_artifacts_dir = "$base_dir\build_artifacts"
  $build_artifacts_debug_dir = "$build_artifacts_dir\Debug"
  $build_artifacts_release_dir = "$build_artifacts_dir\Release"
  $deploy_dir = "$base_dir\deploy"

  $tools_dir = "$base_dir\..\tools"
  $nuget = "$tools_dir\nuget.exe"
  $config = "Debug"
  $test_dir = "$build_artifacts_dir\$config\Tests"
}

task default -depends PackageSolution

task -name Init -description "Init environment" -action { 
  cls
}

task -name Clean -description "clean the solution" -depends Init -action { 
  Write-Host "Executing Clean Task..." -ForegroundColor Green
  $objFolders = Get-ChildItem -path $source_dir -Filter obj -Recurse
  foreach($objFolder in $objFolders) {
    if (!$objFolder.FullName) { continue }
    Write-Host "delete: " $objFolder.FullName -ForegroundColor Green
    Remove-Item $objFolder.FullName -recurse -force
  }  
  $binFolders = Get-ChildItem -path $source_dir -Filter bin -Recurse
  foreach($binFolder in $binFolders) {
    if (!$binFolder.FullName) { continue }
    Write-Host "delete: " $binFolder.FullName -ForegroundColor Green
    Remove-Item $binFolder.FullName -recurse -force
  }  
  
  Remove-Item $deploy_dir -Recurse -ErrorAction SilentlyContinue
  Remove-Item $build_artifacts_dir -Recurse -ErrorAction SilentlyContinue
  Write-Host
}

task -name NuGetRestore -description "NuGet Restore" -action { 
  Write-Host "Executing NuGet Restore Task..." -ForegroundColor Green
  $objPackageConfigs = Get-ChildItem -path $source_dir -Filter packages.config -Recurse
  foreach($objPackageConfig in $objPackageConfigs) {
    Write-Host "restore: " $objPackageConfig.FullName -ForegroundColor Green
	& $nuget restore $objPackageConfig.FullName -PackagesDirectory "$base_dir\packages"
  }  
}

task -name NuGetUpdate -description "NuGet Restore" -action { 
  Write-Host "Executing NuGet Restore Task..." -ForegroundColor Green
  $objPackageConfigs = Get-ChildItem -path $source_dir -Filter packages.config -Recurse
  foreach($objPackageConfig in $objPackageConfigs) {
    Write-Host "update: " $objPackageConfig.FullName -ForegroundColor Green
	& $nuget update $objPackageConfig.FullName -PackagesDirectory "$base_dir\packages"
  }  
}

task -name NuGetPack -description "NuGet Pack" -action { 
  Write-Host "Executing NuGet Pack Task..." -ForegroundColor Green
  & .\nugetPackProject.ps1
}

task -name Compile -description "compile the solution" -depends Clean,NuGetRestore { 
  Write-Host "Executing Compile Task..." -ForegroundColor Green
  mkdir $build_artifacts_dir -ErrorAction SilentlyContinue  | out-null
  
  $solutions = Get-ChildItem -Path $base_dir -Filter *.sln -Recurse
  
  ConfigureBuildEnvironment $framework
  
  Write-Host "Compile Debug" -ForegroundColor DarkGreen
  foreach($solution in $solutions) {
    Write-Host "Compile (Debug): " $solution.FullName -ForegroundColor Green
    Exec { msbuild $solution.FullName /t:Build /p:Configuration=Debug /v:quiet /p:OutDir=$build_artifacts_debug_dir } 
  }
  
  Write-Host "Compile Release" -ForegroundColor DarkGreen
  foreach($solution in $solutions) {
    Write-Host "Compile (Release): " $solution.FullName -ForegroundColor Green
    Exec { msbuild $solution.FullName /t:Build /p:Configuration=Release /v:quiet /p:OutDir=$build_artifacts_release_dir } 
  }
  Write-Host "done..." -ForegroundColor Green
  Write-Host
} 

task -name Test -description "test the solution" -depends Compile -action { 
  Write-Host 'Executing Test Task' -ForegroundColor Green
  $tests = Get-ChildItem -Path $build_artifacts_debug_dir -Filter *Test.dll -Recurse
  foreach($test in $tests) {
    Write-Host $test
	& $tools_dir\nunit\nunit-console-x86.exe $test.FullName /nologo /nodots /framework=net-4.5 /xml=$build_artifacts_dir\FullSystemTestResult.xml
  }
}

task -name CopyFiles -description "copy files (external script)" -depends Test -action { 
  Write-Host 'Executing Copy Task' -ForegroundColor Green
  . .\copy.ps1
}

task -name PackageSolution -description "package files" -depends CopyFiles -action { 
  Write-Host 'Package Solution Task' -ForegroundColor Green

}

task ? -Description "Helper to display task info" {
	Write-Documentation
}