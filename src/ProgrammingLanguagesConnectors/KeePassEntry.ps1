# KeePass Commander
# https://github.com/MircoBabin/KeePassCommander - MIT license 
# 
# Copyright (c) 2018 Mirco Babin
# 
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation
# files (the "Software"), to deal in the Software without
# restriction, including without limitation the rights to use,
# copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following
# conditions:
# 
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
# HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.


# This file is put in the same directory as KeePassCommand.exe.

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
    if (-Not (Test-Path $KeePassCommandExe)) {
        Throw "KeePassCommand.exe not found: " + $KeePassCommandExe
    }
    
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
