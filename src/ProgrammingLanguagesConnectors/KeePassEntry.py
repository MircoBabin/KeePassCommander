# KeePass Commander
# https://github.com/MircoBabin/KeePassCommander - MIT license 
# 
# Copyright (c) 2018 Mirco Babin
# 
# Permission is hereby granted, free of charge, to any person
# obtaining a copy of this software and associated documentation
# files (the "Software"), to deal in the Software without
# restriction, including without limitation the rights to use,
# copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the
# Software is furnished to do so, subject to the following
# conditions:
# 
# The above copyright notice and this permission notice shall be
# included in all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
# HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
# WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.


# This file is put in the same directory as KeePassCommand.exe.

import inspect, os.path
import subprocess
import base64

def KeePassEntry(title, options = None):
  entry = dict()
  entry['title'] = ''
  entry['username'] = ''
  entry['password'] = ''
  entry['url'] = ''
  entry['urlscheme'] = ''
  entry['urlhost'] = ''
  entry['urlport'] = ''
  entry['urlpath'] = ''
  entry['notes'] = ''
  
  entry['fields'] = []

  entry['attachments'] = []
  
  def Get(KeePassCommandExe, title):
    lines = subprocess.check_output([KeePassCommandExe, 'get', title],universal_newlines=True)
  
    titleFound = False
    state = 0
    for line in lines.splitlines(False):
      if (len(line) >= 2):
        if (state == 0):
          if (line[0:2] == "B\t"):
            state = 1
        else:
          if (line[0:2] == "I\t"):
            value = line[2:]
            
            if (state == 1):
              if (value == title):
                entry['title'] = value
                titleFound = True
                state += 1
              else:
                state = 0
            elif (state == 2):
              entry['username'] = value
              state += 1
            elif (state == 3):
              entry['password'] = value
              state += 1
            elif (state == 4):
              entry['url'] = value
              state += 1
            elif (state == 5):
              entry['urlscheme'] = value
              state += 1
            elif (state == 6):
              entry['urlhost'] = value
              state += 1
            elif (state == 7):
              entry['urlport'] = value
              state += 1
            elif (state == 8):
              entry['urlpath'] = value
              state += 1
            elif (state == 9):
              if (len(value) > 0):
                value_decoded_bytes = base64.b64decode(value.encode('ascii'));
                entry['notes'] = value_decoded_bytes.decode('utf-8')
              state += 1
          elif (line[0:2] == "E\t"):
            state = 0
            if (titleFound):
              break
  
  def GetField(KeePassCommandExe, fieldNames):
    lines = subprocess.check_output([KeePassCommandExe, 'getfield', title] + fieldNames,universal_newlines=True)
  
    titleFound = False
    state = 0
    for line in lines.splitlines(False):
      if (len(line) >= 2):
        if (state == 0):
          if (line[0:2] == "B\t"):
            state = 1
        else:
          if (line[0:2] == "I\t"):
            name = ''
            value = line[2:]
            p = value.find("\t")
            if p >= 0:
              name = value[:p]
              value = value[p+1:]
            else:
              name = ''
              value = ''
            
            if (state == 1):
              if (name == 'title' and value == title):
                titleFound = True
                state += 1
              else:
                state = 0
            elif (state == 2):
              value_decoded_bytes = base64.b64decode(value.encode('ascii'));
            
              field = dict();
              field['name'] = name
              field['value'] = value_decoded_bytes.decode('utf-8')
              entry['fields'] += [field];
              
          elif (line[0:2] == "E\t"):
            state = 0
            if (titleFound):
              break
  
  def GetAttachment(KeePassCommandExe, attachmentNames):
    lines = subprocess.check_output([KeePassCommandExe, 'getattachment', title] + attachmentNames,universal_newlines=True)
  
    titleFound = False
    state = 0
    for line in lines.splitlines(False):
      if (len(line) >= 2):
        if (state == 0):
          if (line[0:2] == "B\t"):
            state = 1
        else:
          if (line[0:2] == "I\t"):
            name = ''
            value = line[2:]
            p = value.find("\t")
            if p >= 0:
              name = value[:p]
              value = value[p+1:]
            else:
              name = ''
              value = ''
            
            if (state == 1):
              if (name == 'title' and value == title):
                titleFound = True
                state += 1
              else:
                state = 0
            elif (state == 2):
              attachment = dict();
              attachment['name'] = name
              attachment['value'] = base64.b64decode(value.encode('ascii'));
              entry['attachments'] += [attachment];
              
          elif (line[0:2] == "E\t"):
            state = 0
            if (titleFound):
              break
              
  KeePassCommandExe = None;
  fieldNames = None;
  attachmentNames = None;
  if (isinstance(options, dict)):
    if ("KeePassCommandExe" in options):
      KeePassCommandExe = options["KeePassCommandExe"]
    if ("FieldNames" in options):
      fieldNames = options["FieldNames"]
    if ("AttachmentNames" in options):
      attachmentNames = options["AttachmentNames"]
       
  if (KeePassCommandExe is None):
    # get path of this file KeePassEntry.py
    module_filename = inspect.getframeinfo(inspect.currentframe()).filename
    module_path     = os.path.dirname(os.path.abspath(module_filename))
    KeePassCommandExe = os.path.join(module_path, "KeePassCommand.exe")
  if not os.path.exists(KeePassCommandExe):
    raise Exception("KeePassCommand.exe not found: " + KeePassCommandExe)
  
  Get(KeePassCommandExe, title)
  if (entry['title'] == title):
    if not fieldNames is None:
      GetField(KeePassCommandExe, fieldNames)

    if not attachmentNames is None:
      GetAttachment(KeePassCommandExe, attachmentNames)
      
  return entry
