<?php

function Example()
{
    $entry = new \KeePassCommander\KeePassEntry('Sample Entry', [
        'FieldNames' => ['extra field 1', 'extra password 1'],
        'AttachmentNames' => ['example_attachment.txt'],
    ]);
    if (empty($entry->title)) {
        echo 'Communication failed:'."\r\n";
        echo '- Is KeePass not started, locked or is the database not opened ?'."\r\n";
        echo '- Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?'."\r\n";
        echo '- Is the entry not allowed to be queried (e.g. not permitted when using the filesystem) ?'."\r\n";

        return 2;
    }

    echo 'Sample Entry + extra field 1 + extra password 1 + example_attachment.txt:'."\r\n";
    print_r($entry);

    return 0;
}

function ExampleUnicode()
{
    $entry = new \KeePassCommander\KeePassEntry('Unicode Entry');

    echo 'Unicode Entry:'."\r\n";
    print_r($entry);

    return 0;
}

function ExampleListgroup()
{
    $titles = \KeePassCommander\KeePassEntry::ListGroup('All Entries');

    echo 'Example for ListGroup:'."\r\n";
    print_r($titles);

    foreach ($titles as $title) {
        $entry = new \KeePassCommander\KeePassEntry($title);
        echo 'Retrieved: '.$entry->title."\r\n";
    }
}

// find KeePassEntry.php
$KeePassEntry_php = str_replace('/', '\\', __DIR__.'/KeePassEntry.php');
if (!file_exists($KeePassEntry_php)) {
    $KeePassEntry_php = str_replace('/', '\\', __DIR__.'/../bin/release/KeePassEntry.php');
    if (!file_exists($KeePassEntry_php)) {
        echo 'KeePassEntry.php not found'."\r\n";
        exit(1);
    }
}

// require KeePassEntry.php containing class KeePassEntry
require_once $KeePassEntry_php;

// BEGIN example
$exitcode = Example();
if (0 === $exitcode) {
    echo "\r\n";
    echo "\r\n";
    ExampleUnicode();
    echo "\r\n";
    echo "\r\n";
    ExampleListgroup();
}
// END example

exit($exitcode);
