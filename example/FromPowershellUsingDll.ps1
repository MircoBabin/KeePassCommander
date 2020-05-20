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
$KeePassEntryUsingDll_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath "KeePassEntryUsingDll.ps1"
if (-Not (Test-Path $KeePassEntryUsingDll_ps1)) {
    $KeePassEntryUsingDll_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath "..\bin\release\KeePassEntryUsingDll.ps1"
    if (-Not (Test-Path $KeePassEntryUsingDll_ps1)) {
        Write-Host "KeePassEntryUsingDll.ps1 not found"
        ExitWithCode -exitcode 1
    }
} 

# include KeePassEntry.ps1 containing function KeePassEntry
. $KeePassEntryUsingDll_ps1

# BEGIN example
$exitcode = Example
# END example

# remove included function KeePassEntry
Remove-Item function:\KeePassEntry

#exit
ExitWithCode -exitcode $exitcode


