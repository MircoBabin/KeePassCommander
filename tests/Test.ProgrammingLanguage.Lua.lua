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

-- require KeePassEntry.lua containing class KeePassEntry
package.path = './../src/ProgrammingLanguagesConnectors/KeePassEntry.lua;' .. package.path
local KeePassEntry = require("KeePassEntry")

-- set the Options
local EntryName = "Sample Entry"
local Options = {}
Options.KeePassCommandExe = arg[1]
Options.FieldNames = { "extra field 1", "extra password 1" }
Options.AttachmentNames = { "example_attachment.txt" }

-- retrieve KeePass entry
local entry = KeePassEntry.get(EntryName, Options)
if string.len(entry.title) == 0 then
    print('KeePass is not started')
    print('Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?')

    return 2
end

-- ordered output
printTable(entry, '')

return 0
