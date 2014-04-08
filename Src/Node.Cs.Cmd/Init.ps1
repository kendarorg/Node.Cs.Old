param($installPath, $toolsPath, $package, $project)

# Get the open solution.
$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

# Get the solution dir
$solutionFile = Get-ChildItem $solution.filename
$solutionDir = $solutionFile.DirectoryName

Write-Host "Solution Dir: $solutionDir"

# Setup the .nodeCs dir name
$nodeCsDir = Join-Path -path $solutionDir -ChildPath .nodeCs

Write-Host "nodeCs Dir: $nodeCsDir"

# Create the .nodeCs dir
New-Item -ItemType Directory -Force -Path $nodeCsDir

$packageString = $package.ToString()
$packageDir = $packageString -replace " ","." 
Write-Host "Package: $package"
Write-Host "Package Dir: $packageDir"
Write-Host "Type: $packageType"
# Copy items in .nodeCs dir
#Copy-Item c:\scripts\test.txt c:\test



# Create the parent solution folder.
#$parentProject = $solution.AddSolutionFolder("Parent")

# Create a child solution folder.
#$parentSolutionFolder = Get-Interface $parentProject.Object ([EnvDTE80.SolutionFolder])
#$childProject = $parentSolutionFolder.AddSolutionFolder("Child")

# Add a file to the child solution folder.
#$childSolutionFolder = Get-Interface $childProject.Object ([EnvDTE80.SolutionFolder])
#$fileName = "D:\projects\MyProject\MyProject.csproj"
#$projectFile = $childSolutionFolder.AddFromFile($fileName)