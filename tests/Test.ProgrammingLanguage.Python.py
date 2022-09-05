import inspect, os.path
import sys
import imp
import json

if (len(sys.argv) < 2):
  raise Exception("Provide full path to KeePassCommand.exe as first commandline parameter")
  
KeePassCommandExe = sys.argv[1];

# get path of this file Test.ProgrammingLanguage.Python.py
module_filename = inspect.getframeinfo(inspect.currentframe()).filename
module_path     = os.path.dirname(os.path.abspath(module_filename))
KeePassEntry_py = os.path.join(module_path, "..\\src\ProgrammingLanguagesConnectors\\KeePassEntry.py")

# load KeePassEntry.py containing function KeePassEntry
KeePassEntryModule = imp.load_source('KeePassEntryModule', KeePassEntry_py)

# BEGIN example
options = dict()
options['KeePassCommandExe'] = KeePassCommandExe
options['FieldNames'] = ['extra field 1', 'extra password 1']
options['AttachmentNames'] = ['example_attachment.txt']

entry = KeePassEntryModule.KeePassEntry('Sample Entry', options)

print json.dumps(entry, sort_keys=True);
