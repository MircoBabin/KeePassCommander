import inspect, os.path
import sys

# begin: python 3 deprecated imp.load_source

if sys.version_info >= (3, 0):
    from importlib.machinery import SourceFileLoader
else:
    import imp
# end: python 3 deprecated imp.load_source


import binascii
import json

if len(sys.argv) < 2:
    raise Exception(
        "Provide full path to KeePassCommand.exe as first commandline parameter"
    )
KeePassCommandExe = sys.argv[1]

# get path of this file Test.ProgrammingLanguage.Python.py

module_filename = inspect.getframeinfo(inspect.currentframe()).filename
module_path = os.path.dirname(os.path.abspath(module_filename))
KeePassEntry_py = os.path.join(
    module_path, "..\\src\ProgrammingLanguagesConnectors\\KeePassEntry.py"
)

# load KeePassEntry.py containing function KeePassEntry
# begin: python 3 deprecated imp.load_source

KeePassEntryModule = None
if sys.version_info >= (3, 0):
    KeePassEntryModule = SourceFileLoader(
        "KeePassEntryModule", KeePassEntry_py
    ).load_module()
else:
    KeePassEntryModule = imp.load_source("KeePassEntryModule", KeePassEntry_py)
# end: python 3 deprecated imp.load_source

# BEGIN example

options = dict()
options["KeePassCommandExe"] = KeePassCommandExe
options["FieldNames"] = ["extra field 1", "extra password 1"]
options["AttachmentNames"] = ["example_attachment.txt"]

entry = KeePassEntryModule.KeePassEntry("Sample Entry", options)


class JsonBytesEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, bytes):
            return binascii.hexlify(obj, " ").decode("utf-8")
        return json.JSONEncoder.default(self, obj)


print(json.dumps(entry, sort_keys=True, separators=(",", ":"), cls=JsonBytesEncoder))
