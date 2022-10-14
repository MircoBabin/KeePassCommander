<?php

if ($argc < 2) throw new \Exception('Provide full path to KeePassCommand.exe as first commandline parameter');

$KeePassCommand_exe = $argv[1];
$KeePassEntry_php = str_replace('/', '\\', __DIR__ . '/..\src\ProgrammingLanguagesConnectors\KeePassEntry.php');

// require KeePassEntry.php containing class KeePassEntry
require_once($KeePassEntry_php);

$titles = \KeePassCommander\KeePassEntry::ListGroup('All Entries', [
    'KeePassCommandExe' => $KeePassCommand_exe,
]);
    
echo json_encode($titles, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT);
