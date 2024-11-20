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

    public $fields;

    public $attachments;

    private static function OSExecute_ReturnStdoutLines($cmd)
    {
        $proc = @popen($cmd, 'rb');
        if (false === $proc) {
            throw new \Exception('popen failed: '.$KeePassCommandExe);
        }

        $output = '';
        while (!(@feof($proc))) {
            $output .= @fread($proc, 4096);
        }

        @pclose($proc);

        if ('' === $output) {
            return [];
        }

        if ("\r\n" === substr($output, -2, 2)) {
            $output = substr($output, 0, -2);
        }

        return explode("\r\n", $output);
    }

    public static function ListGroup($entryname, $options = null)
    {
        $KeePassCommandExe = null;
        if (is_array($options)) {
            if (isset($options['KeePassCommandExe']) && is_string($options['KeePassCommandExe'])) {
                $KeePassCommandExe = $options['KeePassCommandExe'];
            }
        }

        if (null === $KeePassCommandExe) {
            $KeePassCommandExe = __DIR__.'/KeePassCommand.exe';
        }
        $KeePassCommandExe = str_replace('/', '\\', $KeePassCommandExe);
        if (!file_exists($KeePassCommandExe)) {
            throw new \Exception('KeePassCommand.exe not found: '.$KeePassCommandExe);
        }
        $KeePassCommandExe = '"'.$KeePassCommandExe.'"';

        $KeePassCommandExe .= ' listgroup -stdout-utf8nobom '.escapeshellarg($entryname);
        $outputLines = self::OSExecute_ReturnStdoutLines($KeePassCommandExe);

        $titles = [];
        foreach ($outputLines as $title) {
            if ('' !== trim($title)) {
                $titles[] = $title;
            }
        }

        return $titles;
    }

    /*
    options = (null | array with each key optional) [
        'KeePassCommand.exe' => (string) full path and filename of KeePassCommand.exe e.g. 'c:\KeePass\KeePassCommand.exe'
        'FieldNames' => (array) array of fieldnames to retrieve. e.g. ['field1', 'field2', ...]
        'AttachmentNames' = (array) array of attachmentnames to retrieve. e.g. ['file.txt', 'key-private.pem', 'key-public.pem', ...]
    ]
    */
    public function __construct($entryname, $options = null)
    {
        $this->title = '';
        $this->username = '';
        $this->password = '';
        $this->url = '';
        $this->urlscheme = '';
        $this->urlhost = '';
        $this->urlport = '';
        $this->urlpath = '';
        $this->notes = '';

        $this->fields = [];

        $this->attachments = [];

        $KeePassCommandExe = null;
        $fieldNames = null;
        $attachmentNames = null;
        if (is_array($options)) {
            if (isset($options['KeePassCommandExe']) && is_string($options['KeePassCommandExe'])) {
                $KeePassCommandExe = $options['KeePassCommandExe'];
            }

            if (isset($options['FieldNames']) && is_array($options['FieldNames'])) {
                $fieldNames = $options['FieldNames'];
            }

            if (isset($options['AttachmentNames']) && is_array($options['AttachmentNames'])) {
                $attachmentNames = $options['AttachmentNames'];
            }
        }

        if (null === $KeePassCommandExe) {
            $KeePassCommandExe = __DIR__.'/KeePassCommand.exe';
        }
        $KeePassCommandExe = str_replace('/', '\\', $KeePassCommandExe);
        if (!file_exists($KeePassCommandExe)) {
            throw new \Exception('KeePassCommand.exe not found: '.$KeePassCommandExe);
        }
        $KeePassCommandExe = '"'.$KeePassCommandExe.'"';

        $this->Get($KeePassCommandExe, $entryname);
        if ($this->title === $entryname) {
            if (is_array($fieldNames)) {
                $this->GetField($KeePassCommandExe, $fieldNames);
            }

            if (is_array($attachmentNames)) {
                $this->GetAttachment($KeePassCommandExe, $attachmentNames);
            }
        }
    }

    private function Get($KeePassCommandExe, $entryname)
    {
        $KeePassCommandExe .= ' get -stdout-utf8nobom '.escapeshellarg($entryname);
        $outputLines = self::OSExecute_ReturnStdoutLines($KeePassCommandExe);

        $titleFound = false;
        $state = 0;
        foreach ($outputLines as $line) {
            switch ($state) {
                case 0:
                    if ("B\t" === substr($line, 0, 2)) {
                        $state = 1;
                    }
                    break;
                default:
                    if ("I\t" === substr($line, 0, 2)) {
                        $value = substr($line, 2);

                        switch ($state) {
                            case 1:
                                if ($value === $entryname) {
                                    $this->title = $value;
                                    $titleFound = true;
                                    ++$state;
                                } else {
                                    $state = 0;
                                }
                                break;
                            case 2:
                                $this->username = $value;
                                ++$state;
                                break;
                            case 3:
                                $this->password = $value;
                                ++$state;
                                break;
                            case 4:
                                $this->url = $value;
                                ++$state;
                                break;
                            case 5:
                                $this->urlscheme = $value;
                                ++$state;
                                break;
                            case 6:
                                $this->urlhost = $value;
                                ++$state;
                                break;
                            case 7:
                                $this->urlport = $value;
                                ++$state;
                                break;
                            case 8:
                                $this->urlpath = $value;
                                ++$state;
                                break;
                            case 9:
                                if ('' !== $value) {
                                    $this->notes = base64_decode($value);
                                    if (false === $this->notes) {
                                        $this->notes = '';
                                    }
                                }
                                ++$state;
                                break;
                        }
                    } elseif ("E\t" === substr($line, 0, 2)) {
                        $state = 0;
                        if ($titleFound) {
                            break;
                        }
                    }
                    break;
            }
        }
    }

    private function GetField($KeePassCommandExe, $fieldNames)
    {
        $KeePassCommandExe .= ' getfield -stdout-utf8nobom '.escapeshellarg($this->title);
        foreach ($fieldNames as $name) {
            $KeePassCommandExe .= ' '.escapeshellarg($name);
        }
        $outputLines = self::OSExecute_ReturnStdoutLines($KeePassCommandExe);

        $titleFound = false;
        $state = 0;
        foreach ($outputLines as $line) {
            switch ($state) {
                case 0:
                    if ("B\t" === substr($line, 0, 2)) {
                        $state = 1;
                    }
                    break;
                default:
                    if ("I\t" === substr($line, 0, 2)) {
                        $name = '';
                        $value = substr($line, 2);
                        $p = strpos($value, "\t");
                        if (false !== $p) {
                            $name = substr($value, 0, $p);
                            $value = substr($value, $p + 1);
                        } else {
                            $name = '';
                            $value = '';
                        }

                        switch ($state) {
                            case 1:
                                if ('title' === $name && $value === $this->title) {
                                    $titleFound = true;
                                    ++$state;
                                } else {
                                    $state = 0;
                                }
                                break;
                            case 2:
                                if ('' !== $name) {
                                    $this->fields[$name] = base64_decode($value);
                                }
                                break;
                        }
                    } elseif ("E\t" === substr($line, 0, 2)) {
                        $state = 0;
                        if ($titleFound) {
                            break;
                        }
                    }
                    break;
            }
        }
    }

    private function GetAttachment($KeePassCommandExe, $attachmentNames)
    {
        $KeePassCommandExe .= ' getattachment -stdout-utf8nobom '.escapeshellarg($this->title);
        foreach ($attachmentNames as $name) {
            $KeePassCommandExe .= ' '.escapeshellarg($name);
        }
        $outputLines = self::OSExecute_ReturnStdoutLines($KeePassCommandExe);

        $titleFound = false;
        $state = 0;
        foreach ($outputLines as $line) {
            switch ($state) {
                case 0:
                    if ("B\t" === substr($line, 0, 2)) {
                        $state = 1;
                    }
                    break;
                default:
                    if ("I\t" === substr($line, 0, 2)) {
                        $value = substr($line, 2);
                        $p = strpos($value, "\t");
                        if (false !== $p) {
                            $name = substr($value, 0, $p);
                            $value = substr($value, $p + 1);
                        } else {
                            $name = '';
                            $value = '';
                        }

                        switch ($state) {
                            case 1:
                                if ('title' === $name && $value === $this->title) {
                                    $titleFound = true;
                                    ++$state;
                                } else {
                                    $state = 0;
                                }
                                break;
                            case 2:
                                if ('' !== $name) {
                                    $this->attachments[$name] = base64_decode($value);
                                }
                                break;
                        }
                    } elseif ("E\t" === substr($line, 0, 2)) {
                        $state = 0;
                        if ($titleFound) {
                            break;
                        }
                    }
                    break;
            }
        }
    }
}
