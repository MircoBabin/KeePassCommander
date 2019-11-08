<?php

function Example() {
    $entry = new \KeePassCommander\KeePassEntry('Sample Entry');
    if (empty($entry->title)) {
        echo 'KeePass is not started'."\r\n";
        echo 'Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?'. "\r\n";
        
        return 2;
    }
    
    print_r($entry);
    
    return 0;
}

// find KeePassEntry.php
$KeePassEntry_php = str_replace('/', '\\', __DIR__ . '/KeePassEntry.php');
if (!file_exists($KeePassEntry_php)) {
    $KeePassEntry_php = str_replace('/', '\\', __DIR__ . '/../bin/release/KeePassEntry.php');
    if (!file_exists($KeePassEntry_php)) {
        echo 'KeePassEntry.php not found'."\r\n";
        exit(1);
    }
}

// require KeePassEntry.php containing class KeePassEntry
require_once($KeePassEntry_php);

//BEGIN example
$exitcode = Example();
//END example

exit($exitcode);
