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


# This file is put in the same directory as KeePassCommandDll.dll.

function KeePassEntry
{
    param (
        [Parameter(Mandatory=$true)][string]$title = ""
     )
     
    $KeePassCommandDll = Join-Path -Path $PSScriptRoot  -ChildPath "KeePassCommandDll.dll"
    if (-Not (Test-Path $KeePassCommandDll)) {
        Throw "KeePassCommandDll.dll not found: " + $KeePassCommandDll
    }
    
    [Reflection.Assembly]::LoadFile("$KeePassCommandDll") | Out-Null
    $entries = [KeePassCommandDll.Api]::get($title)
    
    if ($entries.Count -eq 0) {
        return $null;
    }
    $entry = $entries[0]
    
    return [pscustomobject]@{
        title = $entry.Title
        username = $entry.Username
        password = $entry.Password
        url = $entry.Url
        urlscheme = $entry.UrlScheme
        urlhost = $entry.UrlHost
        urlport = $entry.UrlPort
        urlpath = $entry.UrlPath
        notes = $entry.Notes
    }
}
