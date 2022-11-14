param(
    [Parameter(Mandatory=$true)][string]$KeePassCommand_exe=''
)
    
$KeePassEntry_ps1 = Join-Path -Path $PSScriptRoot  -ChildPath '..\src\ProgrammingLanguagesConnectors\KeePassEntry.ps1'

# include KeePassEntry.ps1 containing function KeePassEntry
. $KeePassEntry_ps1

$options = [pscustomobject]@{
    KeePassCommandExe = $KeePassCommand_exe
    FieldNames = @('extra field 1', 'extra password 1')
    AttachmentNames = @('example_attachment.txt')
}

$object = KeePassEntry -title "Sample Entry" -options $options
    
$object | ConvertTo-Json -Compress | Out-String | % Trim | Write-Host -NoNewline
