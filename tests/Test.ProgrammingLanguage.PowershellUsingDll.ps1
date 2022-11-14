param(
    [Parameter(Mandatory=$true)][string]$KeePassCommand_exe=''
)
    
$KeePassEntryUsingDll_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath '..\src\ProgrammingLanguagesConnectors\KeePassEntryUsingDll.ps1'

# include KeePassEntryUsingDll.ps1 containing function KeePassEntry
. $KeePassEntryUsingDll_ps1

# [System.Diagnostics.Debugger]::Launch()

$options = [pscustomobject]@{
    KeePassCommandDll = $KeePassCommand_exe.Substring(0, $KeePassCommand_exe.Length-4) + 'Dll.dll'
    FieldNames = @('extra field 1', 'extra password 1')
    AttachmentNames = @('example_attachment.txt')
}

$object = KeePassEntry -title "Sample Entry" -options $options
    
$object | ConvertTo-Json -Compress | Out-String | % Trim | Write-Host -NoNewline
