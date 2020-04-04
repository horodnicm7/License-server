import glob
import os
import shutil

src_file = "plugins_list.txt"
dest = "Server\\Plugins\\"
dll_format = "{0}\\{0}\\obj\\Debug\\netstandard2.0\\{0}.dll"

files = glob.glob("{0}*".format(dest))
for f in files:
    os.remove(f)

with open(src_file) as file:
    for line in file:
        line = line.strip()
        try:
            shutil.copy(dll_format.format(line), dest)
        except Exception as e:
            print(str(e))
            
# os.startfile("Server\\DarkRift.Server.Console.exe")