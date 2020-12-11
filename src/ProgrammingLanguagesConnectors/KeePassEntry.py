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

def KeePassEntry(title):
  entry_title = '';
  entry_username = '';
  entry_password = '';
  entry_url = '';
  entry_urlscheme = '';
  entry_urlhost = '';
  entry_urlport = '';
  entry_urlpath = '';
  entry_notes = '';

  # get path of this file KeePassEntry.py
  module_filename = inspect.getframeinfo(inspect.currentframe()).filename
  module_path     = os.path.dirname(os.path.abspath(module_filename))
  
  KeePassCommandExe = os.path.join(module_path, "KeePassCommand.exe")
  
  if not os.path.exists(KeePassCommandExe):
    raise Exception("KeePassCommand.exe not found: " + KeePassCommandExe)
  
  lines = subprocess.check_output([KeePassCommandExe, 'get', title],universal_newlines=True)
  
  state = 0
  for line in lines.splitlines(False):
    if (len(line) >= 2):
      if (state == 0):
        if (line[0:2] == "B\t"):
          state = 1
      else:
        if (line[0:2] == "I\t"):
          line = line[2:]
          
          if (state == 1):
            entry_title = line
          elif (state == 2):
            entry_username = line
          elif (state == 3):
            entry_password = line
          elif (state == 4):
            entry_url = line
          elif (state == 5):
            entry_urlscheme = line
          elif (state == 6):
            entry_urlhost = line
          elif (state == 7):
            entry_urlport = line
          elif (state == 8):
            entry_urlpath = line
          elif (state == 9):
            if (len(line) > 0):
              line_decoded_bytes = base64.b64decode(line.encode('ascii'));
              entry_notes = line_decoded_bytes.decode('utf-8')
              
          state += 1
        elif (line[0:2] == "E\t"):
          state = 0
          if (len(entry_title) > 0):
            break
            
  result = dict()
  result['title'] = entry_title
  result['username'] = entry_username
  result['password'] = entry_password
  result['url'] = entry_url
  result['urlscheme'] = entry_urlscheme
  result['urlhost'] = entry_urlhost
  result['urlport'] = entry_urlport
  result['urlpath'] = entry_urlpath
  result['notes'] = entry_notes
  return result
