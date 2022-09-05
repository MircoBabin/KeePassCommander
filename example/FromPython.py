import inspect, os.path
import sys
import imp

# get path of this file FromPython.py
module_filename = inspect.getframeinfo(inspect.currentframe()).filename
module_path     = os.path.dirname(os.path.abspath(module_filename))

# find KeePassEntry.py
KeePassEntry_py = os.path.join(module_path, "KeePassEntry.py")
if not os.path.exists(KeePassEntry_py):
  KeePassEntry_py = os.path.join(module_path, "..\\bin\\release\\KeePassEntry.py")
  if not os.path.exists(KeePassEntry_py):
    sys.exit(1);

# load KeePassEntry.py containing function KeePassEntry
KeePassEntryModule = imp.load_source('KeePassEntryModule', KeePassEntry_py)

# BEGIN example
options = dict()
options['FieldNames'] = ['extra field 1', 'extra password 1']
options['AttachmentNames'] = ['example_attachment.txt']

entry = KeePassEntryModule.KeePassEntry('Sample Entry', options)
if (len(entry['title']) == 0):
  print "KeePass is not started"
  print "Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?"
  sys.exit(2)
  
print "title      : " + entry['title']
print "username   : " + entry['username']
print "password   : " + entry['password']
print "url        : " + entry['url']
print "urlscheme  : " + entry['urlscheme']
print "urlhost    : " + entry['urlhost']
print "urlport    : " + entry['urlport']
print "urlpath    : " + entry['urlpath']
print "notes      : " + entry['notes']

print "fields     : "
print entry['fields']

print "attachments: "
print entry['attachments']

# END example

#exit
sys.exit(0)


