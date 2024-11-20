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

function KeePassEntry {
    param (
        [Parameter(Mandatory = $true)][string]$title,
        [Parameter(Mandatory = $false)]$options
    )
     
    New-Variable -Name 'entry_title'       -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_username'    -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_password'    -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_url'         -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_urlscheme'   -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_urlhost'     -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_urlport'     -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_urlpath'     -Value ''     -Scope Local -Option AllScope
    New-Variable -Name 'entry_notes'       -Value ''     -Scope Local -Option AllScope
                                                            
    New-Variable -Name 'entry_fields'      -Value @()    -Scope Local -Option AllScope
                                                            
    New-Variable -Name 'entry_attachments' -Value @()    -Scope Local -Option AllScope
    
    function OSExecute_ReturnStdoutLines {
        param (
            [Parameter(Mandatory = $true)][string]$cmd,
            [Parameter(Mandatory = $true)][array]$parmsList
        )
        
        $arguments = ''
        Foreach ($parm in $parmsList) {
            $arguments = $arguments + ' "' + $parm + '"'
        }

        $procInfo = New-Object System.Diagnostics.ProcessStartInfo -Property @{
            FileName               = $cmd
            Arguments              = $arguments
            RedirectStandardOutput = $true
            UseShellExecute        = $false
        }
        
        $proc = New-Object System.Diagnostics.Process
        $proc.StartInfo = $procInfo
        try {
            $proc.Start() | Out-Null
        }
        catch {
            throw "Error starting: " + $cmd + " " + $arguments
        }
        $proc.WaitForExit()        

        $stream = $proc.StandardOutput.BaseStream;
        $output = [byte[]]::new(0)
        $buffer = [byte[]]::new(4096)
        while ($true) {
            $bytesRead = $stream.Read($buffer, 0, $buffer.length)
            if ($bytesRead -eq 0) {
                break
            }
            
            $tmp = [byte[]]::new($output.Length + $bytesRead)
            [System.Array]::Copy($output, 0, $tmp, 0             , $output.Length)
            [System.Array]::Copy($buffer, 0, $tmp, $output.Length, $bytesRead    )
            $output = $tmp
        }
                
        $output = [System.Text.Encoding]::UTF8.GetString($output)
        if ($output.Length -eq 0) {
            return @();
        }
        
        if ($output.EndsWith("`r`n")) {
            $output = $output.Substring(0, $output.Length - 2)
        }
        
        return $output.Split("`r`n")
    }
        
    
    function Get {
        param (
            [Parameter(Mandatory = $true)][string]$KeePassCommandExe,
            [Parameter(Mandatory = $true)][string]$title
        )
        
        $outputLines = OSExecute_ReturnStdoutLines -cmd $KeePassCommandExe -parmsList @('get', '-stdout-utf8nobom', $title)

        $titleFound = $false
        $state = 0
        Foreach ($line in $outputLines) {
            if ($line.length -ge 2) {
                switch ($state) {
                    0 {
                        if ($line.Substring(0, 2) -eq "B`t") {
                            $state = 1
                        }
                    }
                    
                    Default {
                        if ($line.Substring(0, 2) -eq "I`t") {
                            $value = $line.Substring(2)

                            switch ($state) {
                                1 {
                                    if ($value -eq $title) {
                                        $entry_title = $value
                                        $titleFound = $true
                                        $state++
                                    }
                                    else {
                                        $state = 0
                                    }
                                }
                                2 {
                                    $entry_username = $value
                                    $state++
                                }
                                3 {
                                    $entry_password = $value
                                    $state++
                                }
                                4 {
                                    $entry_url = $value
                                    $state++
                                }
                                5 {
                                    $entry_urlscheme = $value
                                    $state++
                                }
                                6 {
                                    $entry_urlhost = $value
                                    $state++
                                }
                                7 {
                                    $entry_urlport = $value
                                    $state++
                                }
                                8 {
                                    $entry_urlpath = $value
                                    $state++
                                }
                                9 {
                                    if (-not $value -eq "") { 
                                        $entry_notes = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($value)) 
                                    } 
                                    $state++
                                }
                            }
                        }
                        elseif ($line.Substring(0, 2) -eq "E`t") {
                            $state = 0
                            if ($titleFound) { break }
                        }
                    }
                }
            }
        }
    }
    
    function GetField {
        param (
            [Parameter(Mandatory = $true)][string]$KeePassCommandExe,
            [Parameter(Mandatory = $true)]$fieldNames
        )
        
        $outputLines = OSExecute_ReturnStdoutLines -cmd $KeePassCommandExe -parmsList (@('getfield', '-stdout-utf8nobom', $title) + $fieldNames)
        
        $titleFound = $false
        $state = 0
        Foreach ($line in $outputLines) {
            if ($line.length -ge 2) {
                switch ($state) {
                    0 {
                        if ($line.Substring(0, 2) -eq "B`t") {
                            $state = 1
                        }
                    }
                    
                    Default {
                        if ($line.Substring(0, 2) -eq "I`t") {
                            $name = ''
                            $value = $line.Substring(2)
                            $p = $value.IndexOf("`t")
                            if ($p -ge 0) {
                                $name = $value.Substring(0, $p)
                                $value = $value.Substring($p + 1)
                            }
                            else {
                                $name = ''
                                $value = ''
                            }

                            switch ($state) {
                                1 {
                                    if ($name -eq 'title' -And $value -eq $title) {
                                        $entry_title = $value
                                        $titleFound = $true
                                        $state++
                                    }
                                    else {
                                        $state = 0
                                    }
                                }
                                2 {
                                    if ($name -ne '') {
                                        if (-not $value -eq "") { 
                                            $value = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($value)) 
                                        } 
                                        
                                        $entry_fields += [pscustomobject]@{
                                            name  = $name
                                            value = $value
                                        }
                                    }
                                }
                            }
                        }
                        elseif ($line.Substring(0, 2) -eq "E`t") {
                            $state = 0
                            if ($titleFound) { break }
                        }
                    }
                }
            }
        }
    }

    function GetAttachment {
        param (
            [Parameter(Mandatory = $true)][string]$KeePassCommandExe,
            [Parameter(Mandatory = $true)]$attachmentNames
        )
        
        $outputLines = OSExecute_ReturnStdoutLines -cmd $KeePassCommandExe -parmsList (@('getattachment', '-stdout-utf8nobom', $title) + $attachmentNames)
        
        $titleFound = $false
        $state = 0
        Foreach ($line in $outputLines) {
            if ($line.length -ge 2) {
                switch ($state) {
                    0 {
                        if ($line.Substring(0, 2) -eq "B`t") {
                            $state = 1
                        }
                    }
                    
                    Default {
                        if ($line.Substring(0, 2) -eq "I`t") {
                            $name = ''
                            $value = $line.Substring(2)
                            $p = $value.IndexOf("`t")
                            if ($p -ge 0) {
                                $name = $value.Substring(0, $p)
                                $value = $value.Substring($p + 1)
                            }
                            else {
                                $name = ''
                                $value = ''
                            }

                            switch ($state) {
                                1 {
                                    if ($name -eq 'title' -And $value -eq $title) {
                                        $entry_title = $value
                                        $titleFound = $true
                                        $state++
                                    }
                                    else {
                                        $state = 0
                                    }
                                }
                                2 {
                                    if ($name -ne '') {
                                        if (-not $value -eq "") { 
                                            $value = [System.Convert]::FromBase64String($value) 
                                        } 
                                        
                                        $entry_attachments += [pscustomobject]@{
                                            name  = $name
                                            value = $value
                                        }
                                    }
                                }
                            }
                        }
                        elseif ($line.Substring(0, 2) -eq "E`t") {
                            $state = 0
                            if ($titleFound) { break }
                        }
                    }
                }
            }
        }
    }
    
    $KeePassCommandExe = $null
    $fieldNames = $null
    $attachmentNames = $null
    if ($options -is [PSCustomObject]) {
        if ($options.psobject.properties.match('KeePassCommandExe') ) {
            if ($options.KeePassCommandExe -is [string]) {
                $KeePassCommandExe = $options.KeePassCommandExe
            }
        }
        
        if ($options.psobject.properties.match('FieldNames') ) {
            if ($options.FieldNames -is [array]) {
                $fieldNames = $options.FieldNames
            }
        }
        
        if ($options.psobject.properties.match('AttachmentNames') ) {
            if ($options.AttachmentNames -is [array]) {
                $attachmentNames = $options.AttachmentNames
            }
        }
    }
    
    if ($KeePassCommandExe -eq $null) {
        $KeePassCommandExe = Join-Path -Path $PSScriptRoot  -ChildPath "KeePassCommand.exe"
    }
    if (-Not (Test-Path $KeePassCommandExe)) {
        Throw "KeePassCommand.exe not found: " + $KeePassCommandExe
    }
    
    Get -KeePassCommandExe $KeePassCommandExe -title $title
    if ($entry_title -ne $title) { return $null }
    
    if ($fieldNames -is [array]) {
        GetField -KeePassCommandExe $KeePassCommandExe -fieldNames $fieldNames
    }
    
    if ($attachmentNames -is [array]) {
        GetAttachment -KeePassCommandExe $KeePassCommandExe -attachmentNames $attachmentNames
    }
    
    return [pscustomobject]@{
        title       = $entry_title
        username    = $entry_username
        password    = $entry_password
        url         = $entry_url
        urlscheme   = $entry_urlscheme
        urlhost     = $entry_urlhost
        urlport     = $entry_urlport
        urlpath     = $entry_urlpath
        notes       = $entry_notes
        
        fields      = $entry_fields
        
        attachments = $entry_attachments
    }
}
