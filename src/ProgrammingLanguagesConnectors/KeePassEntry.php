<?php
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
