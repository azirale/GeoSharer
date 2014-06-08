GeoSharer
=========
A Minecraft ForgeModLoader mod and a .NET/Mono tool for sharing minecraft multiplayer map data. Tailored for use with Civcraft (http://www.civcraft.vg/).


Description
===========
Use Minecraft Forge and add the mod zip to your mods directory. The mod will automatically download overworld data from any multiplayer server you play on, and store it in a stripped down and compressed *.geosharer format in the mods/GeoSharer/[servername]/ directory.

These *.geosharer files can be shared with map-making collaborators to ensure that everyone has the most up to date world data. Each chunk is given a timestamp, which is used to synchronise data from multiple *.geosharer payloads.

To take the *.geosharer files and turn them into a world you need to use the GeoSharerCore library. If you want an existing package, use the winforms application. If you would like to roll your own UI you can use the core library to wrap your application around. Be wary of using the core library however as method signatures and overall design are likely to change.


Compiling
=========
The mod is compiled using Eclipse and Forge MCP for the relevant version of Minecraft. Version naming is kept in step with Minecraft.

The merge utility is compiled in VS2010/VS2012 targeting the .NET v4 framework. It is compatible with Mono.


Source Directory Structure
==========================
The mod source files are in mod/ and are all Java

The core library is in core/ and is C#.NET and compatible with Mono

A basic GUI is in winform/ and is C#.NET and compatible with Mono

wpf/ contains a now outdated WPF gui in C#.NET


Using the Core Library
======================
There are two major objects to use: GeoMultifile and GeoWorldWriter.

The GeoMultifile has *.geosharer files attached to it using the AttachFile() method. When all of the files you want are attached you can use GetLatestChunkData() to pull in a List of just the most recent chunk data in the attached files.

The GeoWorldWriter uses UpdateWorld() to merge the chunk data from the GeoMultifile into a new or existing world save. Pressing and Messaging events are available to receive information from the GeoWorldWriter as it progresses.
