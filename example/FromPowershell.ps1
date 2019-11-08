function ExitWithCode 
{ 
    param 
    ( 
        $exitcode 
    )

    $host.SetShouldExit($exitcode) 
    exit $exitcode
}

function Example
{
    $object = KeePassEntry -title "Sample Entry"
    if ($object -eq $null) {
        Write-Host "KeePass is not started"
        Write-Host "Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?"
        return 2
    }
    
    $object | Out-String | Write-Host
    return 0
}

# find KeePassEntry.ps1
$KeePassEntry_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath "KeePassEntry.ps1"
if (-Not (Test-Path $KeePassEntry_ps1)) {
    $KeePassEntry_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath "..\bin\release\KeePassEntry.ps1"
    if (-Not (Test-Path $KeePassEntry_ps1)) {
        Write-Host "KeePassEntry.ps1 not found"
        ExitWithCode -exitcode 1
    }
} 

# include KeePassEntry.ps1 containing function KeePassEntry
. $KeePassEntry_ps1

# BEGIN example
$exitcode = Example
# END example

# remove included function KeePassEntry
Remove-Item function:\KeePassEntry

#exit
ExitWithCode -exitcode $exitcode


