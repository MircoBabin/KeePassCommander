--[[
KeePass Commander
https://github.com/MircoBabin/KeePassCommander - MIT license

Copyright (c) 2018 Mirco Babin

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
--]]

local KeePassEntry = {}

do
    -- Private Scope because of "do"
    local function RunKeePassCommand(KeePassCommandExe, command, EntryName, parameters)
        local cmd = '"'..KeePassCommandExe..'" "-stdout-utf8nobom" "'..command..'" "'..EntryName..'"'

        for parameterNo = 1, #parameters do
            cmd = cmd.." \""..parameters[parameterNo].."\""
        end

        local handle = io.popen('"'..cmd..'"', "r") -- On Windows, you must enclose your command line (program + arguments) in additional outer-level quotes.
        local result = handle:read("*a")
        handle:close()

        return result;
    end

    local function file_exists(name)
        if name == nil then
            return false
        end

        local fhandle=io.open(name,"r")
        if fhandle == nil then
            return false
        end

        io.close(fhandle)

        return true
    end

    local function from_base64(data)
        -- https://devforum.roblox.com/t/base64-encoding-and-decoding-in-lua/1719860
        local b = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'
        data = string.gsub(data, '[^'..b..'=]', '')
        return (data:gsub('.', function(x)
            if (x == '=') then return '' end
            local r,f='',(b:find(x)-1)
            for i=6,1,-1 do r=r..(f%2^i-f%2^(i-1)>0 and '1' or '0') end
            return r;
        end):gsub('%d%d%d?%d?%d?%d?%d?%d?', function(x)
            if (#x ~= 8) then return '' end
            local c=0
            for i=1,8 do c=c+(x:sub(i,i)=='1' and 2^(8-i) or 0) end
            return string.char(c)
        end))
    end

    local function RunKeePassCommand_Get(KeePassCommandExe, EntryName)
        local result = {}
        result.title = ""
        result.username = ""
        result.password = ""
        result.url = ""
        result.urlscheme = ""
        result.urlhost = ""
        result.urlport = ""
        result.urlpath = ""
        result.notes = ""
        result.fields = {}
        result.attachments = {}

        local output = RunKeePassCommand(KeePassCommandExe, "get", EntryName, {})

        local lineBegin = 1
        local lineEnd = 0
        local line
        local state = 0;
        local titleFound = false
        while true do
            lineBegin = lineEnd + 1
            if lineBegin > string.len(output) then break end

            lineEnd = string.find(output, "\n", lineBegin)
            if lineEnd == nil then
                lineEnd = string.len(output) + 1
            end

            line = string.sub(output, lineBegin, lineEnd-1)
            if string.len(line) >= 2 then
                local twoChars = string.sub(line, 1, 2)
                if state == 0 then
                    if twoChars == "B\t" then
                        state = 1
                    end
                else
                    local value = string.sub(line, 3)

                    if twoChars == "I\t" then
                        if state == 1 then
                            if value == EntryName then
                                result.title = value
                                titleFound = true
                                state = state + 1
                            else
                                state = 0
                            end
                        elseif state == 2 then
                            result.username = value
                            state = state + 1
                        elseif state == 3 then
                            result.password = value
                            state = state + 1
                        elseif state == 4 then
                            result.url = value
                            state = state + 1
                        elseif state == 5 then
                            result.urlscheme = value
                            state = state + 1
                        elseif state == 6 then
                            result.urlhost = value
                            state = state + 1
                        elseif state == 7 then
                            result.urlport = value
                            state = state + 1
                        elseif state == 8 then
                            result.urlpath = value
                            state = state + 1
                        elseif state == 9 then
                            if string.len(value) > 0 then
                                result.notes = from_base64(value);
                            end
                            state = state + 1
                        end
                    elseif twoChars == "E\t" then
                        state = 0
                        if titleFound then
                            break
                        end
                    end
                end
            end
        end

        return result
    end

    local function RunKeePassCommand_GetField(KeePassCommandExe, EntryName, fieldnames)
        local result = {}

        local output = RunKeePassCommand(KeePassCommandExe, "getfield", EntryName, fieldnames)

        local lineBegin = 1
        local lineEnd = 0
        local line
        local state = 0;
        local titleFound = false
        local pos
        local fieldname
        local fieldvalue
        while true do
            lineBegin = lineEnd + 1
            if lineBegin > string.len(output) then break end

            lineEnd = string.find(output, "\n", lineBegin)
            if lineEnd == nil then
                lineEnd = string.len(output) + 1
            end

            line = string.sub(output, lineBegin, lineEnd-1)
            if string.len(line) >= 2 then
                local twoChars = string.sub(line, 1, 2)
                if state == 0 then
                    if twoChars == "B\t" then
                        state = 1
                    end
                else
                    local value = string.sub(line, 3)

                    if twoChars == "I\t" then
                        pos = string.find(value, "\t");
                        if pos ~= nil then
                            fieldname = string.sub(value, 1, pos-1)
                            fieldvalue = string.sub(value, pos+1)
                        else
                            fieldname = ""
                            fieldvalue = ""
                        end

                        if state == 1 then
                            if (fieldname == "title" and fieldvalue == EntryName) then
                                titleFound = true;
                                state = state + 1
                            else
                                state = 0
                            end
                        elseif state == 2 then
                            if string.len(fieldname) > 0 then
                                result[fieldname] = from_base64(fieldvalue)
                            end
                        end
                    elseif twoChars == "E\t" then
                        state = 0
                        if titleFound then
                            break
                        end
                    end
                end
            end
        end

        return result
    end

    local function RunKeePassCommand_GetAttachment(KeePassCommandExe, EntryName, attachmentnames)
        local result = {}

        local output = RunKeePassCommand(KeePassCommandExe, "getattachment", EntryName, attachmentnames)

        local lineBegin = 1
        local lineEnd = 0
        local line
        local state = 0;
        local titleFound = false
        local pos
        local fieldname
        local fieldvalue
        while true do
            lineBegin = lineEnd + 1
            if lineBegin > string.len(output) then break end

            lineEnd = string.find(output, "\n", lineBegin)
            if lineEnd == nil then
                lineEnd = string.len(output) + 1
            end

            line = string.sub(output, lineBegin, lineEnd-1)
            if string.len(line) >= 2 then
                local twoChars = string.sub(line, 1, 2)
                if state == 0 then
                    if twoChars == "B\t" then
                        state = 1
                    end
                else
                    local value = string.sub(line, 3)

                    if twoChars == "I\t" then
                        pos = string.find(value, "\t");
                        if pos ~= nil then
                            fieldname = string.sub(value, 1, pos-1)
                            fieldvalue = string.sub(value, pos+1)
                        else
                            fieldname = ""
                            fieldvalue = ""
                        end

                        if state == 1 then
                            if (fieldname == "title" and fieldvalue == EntryName) then
                                titleFound = true;
                                state = state + 1
                            else
                                state = 0
                            end
                        elseif state == 2 then
                            if string.len(fieldname) > 0 then
                                result[fieldname] = from_base64(fieldvalue)
                            end
                        end
                    elseif twoChars == "E\t" then
                        state = 0
                        if titleFound then
                            break
                        end
                    end
                end
            end
        end

        return result
    end

    function KeePassEntry.get(EntryName, Options)
        local KeePassCommandExe = nil -- Lua has no concept in which directory this module (KeePassEntry.lua) is located.
        local fieldnames = {}
        local attachmentnames = {}

        local value = Options.KeePassCommandExe
        if value ~= nil then
            KeePassCommandExe = value
        end

        value = Options.FieldNames
        if value ~= nil then
            fieldnames = value
        end

        value = Options.AttachmentNames
        if value ~= nil then
            attachmentnames = value
        end

        if not file_exists(KeePassCommandExe) then
            error("KeePassCommand.exe not found: "..KeePassCommandExe)
        end

        local result = RunKeePassCommand_Get(KeePassCommandExe, EntryName)
        result.fields = RunKeePassCommand_GetField(KeePassCommandExe, EntryName, fieldnames)
        result.attachments = RunKeePassCommand_GetAttachment(KeePassCommandExe, EntryName, attachmentnames)

        return result
    end
end

return KeePassEntry