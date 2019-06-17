# This file is put in the same directory as KeePassCommand.exe.
# When KeePassCommand.exe is located somewhere else, adjust line 22 $KeePassCommandExe = Join-Path ...
#   
# run: powershell -NoProfile -ExecutionPolicy Bypass -file KeePassEntry.ps1

function KeePassEntry
{
    param (
        [Parameter(Mandatory=$true)][string]$title = ""
     )
     
    $entry_title = '';
    $entry_username = '';
    $entry_password = '';
    $entry_url = '';
    $entry_urlscheme = '';
    $entry_urlhost = '';
    $entry_urlport = '';
    $entry_urlpath = '';
    $entry_notes = '';
 
    $KeePassCommandExe = Join-Path -Path $PSScriptRoot  -ChildPath "KeePassCommand.exe"
    $lines = & $KeePassCommandExe "get" $title
    
    $state = 0;
    Foreach ($line in $lines)
    {
        if ($line.length -ge 2)
        {
            switch($state)
            {
                0 {
                    if ($line.Substring(0, 2) -eq "B`t") {
                        $state = 1;
                    }
                }
                
                Default {
                    if ($line.Substring(0, 2) -eq "I`t") {
                        $line = $line.Substring(2).Replace("`r","").Replace("`n","");

                        switch($state)
                        {
                            1 {$entry_title = $line;}
                            2 {$entry_username = $line;}
                            3 {$entry_password = $line;}
                            4 {$entry_url=$line;}
                            5 {$entry_urlscheme=$line;}
                            6 {$entry_urlhost=$line;}
                            7 {$entry_urlport=$line;}
                            8 {$entry_urlpath=$line;}
                            9 {if (-not $line -eq "") { $entry_notes = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($line)); } }
                        }
                        
                        $state++;
                    } elseif ($line.Substring(0, 2) -eq "E`t") {
                        $state = 0;
                        if (-not $entry_title -eq "") { break };
                    }
                }
            }
        }
    }

    if ($entry_title -eq "") { return $null; };
    
    return [pscustomobject]@{
        title = $entry_title
        username = $entry_username
        password = $entry_password
        url = $entry_url
        urlscheme = $entry_urlscheme
        urlhost = $entry_urlhost
        urlport = $entry_urlport
        urlpath = $entry_urlpath
        notes = $entry_notes
    }
}

#BEGIN example

$object = KeePassEntry -title "Sample Entry"
Write-Output $object

#END example

