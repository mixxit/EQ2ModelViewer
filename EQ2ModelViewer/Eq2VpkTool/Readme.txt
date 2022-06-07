Eq2VpkTool v1.2.3 Readme
------------------------

Eq2VpkTool is a simple tool that lets you view and extract the contents of Everquest II VPK files.
This first version provides only basic functionality, but future versions may provide more advanced features.

- Installation

Just unzip all files to the desired folder.

This tool is written in C# 2.0, so you need the .NET Framework 2.0. You can download the framework from:

http://www.microsoft.com/downloads/details.aspx?familyid=0856EACB-4362-4B0D-8EDD-AAB15C5E04F5&displaylang=en (for x86)
http://www.microsoft.com/downloads/details.aspx?familyid=B44A0000-ACF8-4FA1-AFFB-40E78D788B00&displaylang=en (for x64)

There are no other prerequisites except for a working Everquest II installation.

- Usage

You can't open VPK files directly in this version. Instead you must open a VPL file (usually AssetsLib.vpl).

Note: If you don't see an AssetsLib.vpl file on your main EQ2 directory please run EQ2 once so the file is created.

VPL files don't contain any file data, they only reference a set of VPK files. Once you point the tool to a 
valid VPL file it will start processing the referenced VPK files and listing their contents. You can start 
exploring the contents immediately. The list of contents will be updated in real-time.

Processing the whole Everquest II asset library takes anywhere between five seconds and a few minutes, depending 
on the speed of the machine. Extracting all of it takes much longer.

At any time, you can view any of the files by double-clicking on them. They will be extracted to your temporary
folder and deleted when you close the application.

You can extract a directory by right-clicking on it in the directory tree and selecting "Extract" or "Extract with
path information" from the context menu. You can also use the main menu for this. Alternatively, you can select multiple
files and folders to extract from the file list view.

If you choose the "Extract with path information" option, the directory structure will be preserved when the files
are extracted. If you just want to extract the selected files/directories to the destination folder with no extra
folders created, use the "Extract" option.

- Decrypting maps

Starting from version 1.2 it is possible to decrypt some of them maps in the 'nrvobm' directory. When you right-click
on any file in this directory you'll see a new option in the context menu: "Decrypt and extract". This option is only
enabled for the maps for which the tool knows the decryption key.

All the maps in the 'nrvobm' folder are encrypted using the same algorithm, but the key needed to decrypt them is sent 
from the server when the character enters the zone, and varies from map to map. The known decryption keys are stored
in the configuration file.

If you don't find an 'nrvobm' folder in your VPK files it probably means it was removed on a patch.

- Notes

* Don't try opening the "MediaSupLib.vpl" file, as it is not supported. This file is not a valid VPL file.

- License

This program is free software and is distributed under the GNU General Public License. 

Read License.txt for details.

- Contact

Please send any feedback to blaz@blazlabs.com.

Website: http://eq2.blazlabs.com
