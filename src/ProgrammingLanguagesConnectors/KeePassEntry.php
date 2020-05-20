<?php
/*
KeePass Commander
https://github.com/MircoBabin/KeePassCommander - MIT license 

Copyright (c) 2018 Mirco Babin

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/


/* This file is put in the same directory as KeePassCommand.exe.
*/

namespace KeePassCommander;

class KeePassEntry
{
    public $title;
    public $username;
    public $password;
    public $url;
    public $urlscheme;
    public $urlhost;
    public $urlport;
    public $urlpath;
    public $notes;
    
    public function __construct($entryname) {
        $this->title = '';
        $this->username = '';
        $this->password = '';
        $this->url = '';
        $this->urlscheme = '';
        $this->urlhost = '';
        $this->urlport = '';
        $this->urlpath = '';
        $this->notes = '';
        
        $cmd = str_replace('/', '\\', __DIR__ . '/KeePassCommand.exe');
        if (!file_exists($cmd)) {
            throw new \Exception('KeePassCommand.exe not found: ' . $cmd);
        }
        $cmd .= ' get "' . $entryname . '"';
        $output = shell_exec($cmd);
        
        $lines = explode("\n", $output);
        $state = 0;
        foreach($lines as $line) {
            switch($state) {
                case 0:
                    if (substr($line, 0, 2) === "B\t") {
                        $state = 1;
                    }
                    break;
                default:
                    if (substr($line, 0, 2) === "I\t") {
                        $value = substr($line,2);
                        
                        switch($state) {
                            case 1: 
                                if ($value === $entryname) {
                                    $this->title = $value;
                                    $state++;
                                } else {
                                    $state = 0;
                                }
                                break;
                            case 2:
                                $this->username = $value;
                                $state++;
                                break;
                            case 3:
                                $this->password = $value;
                                $state++;
                                break;
                            case 4:
                                $this->url = $value;
                                $state++;
                                break;
                            case 5:
                                $this->urlscheme = $value;
                                $state++;
                                break;
                            case 6:
                                $this->urlhost = $value;
                                $state++;
                                break;
                            case 7:
                                $this->urlport = $value;
                                $state++;
                                break;
                            case 8:
                                $this->urlpath = $value;
                                $state++;
                                break;
                            case 9:
                                if ($value !== '') {
                                    $this->notes = base64_decode($value);
                                    if ($this->notes === false) $this->notes = '';
                                }
                                $state++;
                                break;
                        }
                    } else if (substr($line, 0, 2) === "E\t") {
                        $state = 0;
                        if ($this->title !== '') break;
                    }
                    break;
            }
        }
    }
}
