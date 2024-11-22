local function printTable(Table, Ident)
    -- collect the keys
    local keys = {}
    for key in pairs(Table) do
        keys[#keys+1] = key
    end

    -- sort the keys
    table.sort(keys)

    -- output the table based on sorted keys
    for i=1,#keys do
        local key = keys[i]
        if type(Table[key]) == 'table' then
            print(Ident..'"'..key..'"= {')
            printTable(Table[key], Ident..'    ')
            print(Ident..'}')
        else
            print(Ident..'"'..key..'"='..Table[key])
        end
    end
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

local function FindKeePassCommandExe()
    -- Begin: get current working directory
    local handle = io.popen('"cd"', "r")
    local cwd = handle:read("*all")
    handle:close()

    while (string.len(cwd) >= 1) do
        local lastCharacter = string.sub(cwd, string.len(cwd), string.len(cwd))
        if (lastCharacter ~= "\r") and (lastCharacter ~= "\n") then
            break
        end

        cwd = string.sub(cwd, 1, string.len(cwd) - 1)
    end
    -- End: get current working directory

    return cwd.."\\..\\bin\\release\\KeePassCommand.exe"
end

local function Example(KeePassEntry, KeePassCommandExe)
    local EntryName = "Sample Entry"

    local Options = {}
    Options.KeePassCommandExe = KeePassCommandExe
    Options.FieldNames = { "extra field 1", "extra password 1" }
    Options.AttachmentNames = { "example_attachment.txt" }

    -- retrieve KeePass entry
    local entry = KeePassEntry.get(EntryName, Options)
    if string.len(entry.title) == 0 then
        print('Communication failed:')
        print('- Is KeePass not started, locked or is the database not opened ?')
        print('- Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?')
        print('- Is the entry not allowed to be queried (e.g. not permitted when using the filesystem) ?')

        return 2
    end

    print('Sample Entry + extra field 1 + extra password 1 + example_attachment.txt:')
    printTable(entry, '')

    return 0
end

local function ExampleListGroup(KeePassEntry, KeePassCommandExe)
    local EntryName = "All Entries"

    local Options = {}
    Options.KeePassCommandExe = KeePassCommandExe

    -- listgroup
    local titles = KeePassEntry.ListGroup(EntryName, Options)

    print('Example for ListGroup:')
    printTable(titles, '')

    -- foreach
    local titleNo
    for titleNo = 1, #titles do
        local title = titles[titleNo]

        local entry = KeePassEntry.get(title, Options)
        print('Retrieved: '..entry['title'])
    end
end

-- require KeePassEntry.lua containing class KeePassEntry
package.path = './../src/ProgrammingLanguagesConnectors/KeePassEntry.lua;' .. package.path
local KeePassEntry = require("KeePassEntry")

-- Locate KeePassCommand.exe, default is 1st commandline argument
local KeePassCommandExe = arg[1]
if KeePassCommandExe == nil then
    KeePassCommandExe = FindKeePassCommandExe()
end
if not file_exists(KeePassCommandExe) then
    print("KeePassCommand.exe not found: "..KeePassCommandExe)
    return 1
end

-- Begin Example
local exitcode = Example(KeePassEntry, KeePassCommandExe)
if (exitcode == 0) then
    print('')
    print('')
    ExampleListGroup(KeePassEntry, KeePassCommandExe)
end
-- End Example

return exitcode
