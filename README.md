GeoSharer
=========
A Minecraft ForgeModLoader mod and a .NET/Mono tool for sharing minecraft multiplayer map data. Tailored for use with Civcraft (http://www.civcraft.vg/) --Dead link: Civcraft was shut down some time ago--


Description
===========
UI programs for merging *.geosharer data into world saves. Also contains some world management functions, such as relighting chunks and obfuscating underground areas.


Compiling
=========
The merge utility is compiled in VS2010/VS2012 targeting the .NET v4 framework. It is compatible with Mono.

You will also require GeoSharerLib and Substrate, either as source files in your solution or as referenced libraries.


Source Directory Structure
==========================
winform/ is for a basic WinForms UI that is compatible with Mono. It supports drag+drop operations with files and folders.

console/ is for a console UI that can be used to accomplish similar functions to the WinForms UI.
