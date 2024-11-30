# Find out compiling source folder from a binary
Your program may be built in a different machine and may have different source code structure. 
To debug the program which is built in remote machine with different source folder,
you need to remap the compiling source folder to your current local source folder.
To find out the original compiling source folder, you can do like this:
```bash
# Use readelf
readelf --debug-dump=info <program_name> | grep DW_AT_comp_dir

# Use llvm-dwarfdump
llvm-dwarfdump <program_name> | grep DW_AT_comp_dir
```bash
After you finding out the original compiling source folder, you can map it to your local folder:
```
target settings set target.source-map <original_source> <current_source>

# for example
target settings set target.source-map /home/user/repos/myapp /home/john/source/myapp
```
