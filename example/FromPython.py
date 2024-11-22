import inspect, os.path
import sys

# begin: python 3 deprecated imp.load_source

if sys.version_info >= (3, 0):
    from importlib.machinery import SourceFileLoader
else:
    import imp
# end: python 3 deprecated imp.load_source

# get path of this file FromPython.py

module_filename = inspect.getframeinfo(inspect.currentframe()).filename
module_path = os.path.dirname(os.path.abspath(module_filename))

# find KeePassEntry.py

KeePassEntry_py = os.path.join(module_path, "KeePassEntry.py")
if not os.path.exists(KeePassEntry_py):
    KeePassEntry_py = os.path.join(module_path, "..\\bin\\release\\KeePassEntry.py")
    if not os.path.exists(KeePassEntry_py):
        sys.exit(1)
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
options["FieldNames"] = ["extra field 1", "extra password 1"]
options["AttachmentNames"] = ["example_attachment.txt"]

entry = KeePassEntryModule.KeePassEntry("Sample Entry", options)
if len(entry["title"]) == 0:
    print("Communication failed:")
    print("- Is KeePass not started, locked or is the database not opened ?")
    print(
        "- Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?"
    )
    print(
        "- Is the entry not allowed to be queried (e.g. not permitted when using the filesystem) ?"
    )
    sys.exit(2)
print("title      : " + entry["title"])
print("username   : " + entry["username"])
print("password   : " + entry["password"])
print("url        : " + entry["url"])
print("urlscheme  : " + entry["urlscheme"])
print("urlhost    : " + entry["urlhost"])
print("urlport    : " + entry["urlport"])
print("urlpath    : " + entry["urlpath"])
print("notes      : " + entry["notes"])

print("fields     : ")
print(entry["fields"])

print("attachments: ")
print(entry["attachments"])

# END example

# exit

sys.exit(0)
