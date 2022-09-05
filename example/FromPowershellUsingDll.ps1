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
    $options = [pscustomobject]@{
        FieldNames = @('extra field 1', 'extra password 1')
        AttachmentNames = @('example_attachment.txt')
    }
    
    $object = KeePassEntry -title "Sample Entry" -options $options
    if ($object -eq $null) {
        Write-Host "KeePass is not started"
        Write-Host "Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?"
        return 2
    }
    
    $object | Out-String | Write-Host
    return 0
}

# find KeePassEntryUsingDll.ps1
$KeePassEntryUsingDll_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath "KeePassEntryUsingDll.ps1"
if (-Not (Test-Path $KeePassEntryUsingDll_ps1)) {
    $KeePassEntryUsingDll_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath "..\bin\release\KeePassEntryUsingDll.ps1"
    if (-Not (Test-Path $KeePassEntryUsingDll_ps1)) {
        Write-Host "KeePassEntryUsingDll.ps1 not found"
        ExitWithCode -exitcode 1
    }
} 

# include KeePassEntryUsingDll.ps1 containing function KeePassEntry
. $KeePassEntryUsingDll_ps1

# BEGIN example
$exitcode = Example
# END example

# remove included function KeePassEntry
Remove-Item function:\KeePassEntry

#exit
ExitWithCode -exitcode $exitcode


