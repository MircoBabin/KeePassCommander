<?php

if ($argc < 2) throw new \Exception('Provide full path to KeePassCommand.exe as first commandline parameter');

$KeePassCommand_exe = $argv[1];
$KeePassEntry_php = str_replace('/', '\\', __DIR__ . '/..\src\ProgrammingLanguagesConnectors\KeePassEntry.php');

// require KeePassEntry.php containing class KeePassEntry
require_once($KeePassEntry_php);

$entry = new \KeePassCommander\KeePassEntry('Sample Entry', [
    'KeePassCommandExe' => $KeePassCommand_exe,
    'FieldNames' => [ 'extra field 1', 'extra password 1' ],
    'AttachmentNames' => [ 'example_attachment.txt' ],
]);
    
echo json_encode($entry, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT);
