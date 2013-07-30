GeoSharer
=========

A Forge mod and a .NET tool for sharing minecraft map data


Description
===========
The ForgeMod .java files will need to be compiled using the Forge developer tools incorporating MCP.
The current target version of Minecraft is 1.5.2

The .cs files are compiled in Visual Studio Express 2012, for a console application.
The target .NET framework version is 4.5



How it works
============
(assuming you have everything compiled already)

Install the forge mod as per normal. While in a game the mod will automatically save compressed
versions of the raw block data plus a timestamp for all chunks that are loaded. Once the game exists,
the data is saved to a Base64 encoded file with a timestamped filename.

These files can be opened and read by the merge utility. The merge utility uses Substrate to create
proper chunk data and region files for minecraft. Running the create world or merge changes procedures
should result in a world that is up-to-date.

The reason for this is the timestamp attached to each chunk that says exactly when it was last seen.
This allows multiple people to run the mod and share their output files without overwriting each other's
changes. This can be useful if, for example, you want to make a collaborative effort to map a SMP world.
