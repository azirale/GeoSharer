GeoSharer
=========
A Minecraft ForgeModLoader mod and a .NET tool for sharing minecraft multiplayer map data. Tailored for use with Civcraft (http://www.civcraft.vg/).


Description
===========
Use Minecraft Forge and add the mod to your external mods. The mod will automatically download overworld data from any multiplayer server you play on, and store it in a stripped down and compressed ".geosharer" format in your .minecraft/mods/GeoSharer/ directory.

These ".geosharer" files can be shared with map-making collaborators to ensure that everyone has the most up to date world data. Each chunk is given a timestamp, which is used to synchronise data from multiple ".geosharer" payloads.

To take the ".geosharer" files and turn them into a world you need to use the GeoSharerCore library. If you want an existing package, use the WPF application. If you would like to roll your own UI you can use the core library to wrap your application around.


Compiling
=========
The mod is compiled using Eclipse and Forge MCP for the relevant version of Minecraft.

The merge utility is compiled in VS2010 targeting the .NET v4 framework.


Source Directory Structure
==========================
The mod source files are in mod/ and are all Java
The core library is in core/ and is C#.NET
A basic WPF based gui is in wpf/ and is C#.NET


Using the Core Library
======================
There are two major objects to use: WorldBuilder and GeoReader.

The GeoReader is an IEnumerable for the chunk data that is in the ".geosharer" files. You can use the "Attach()" method to add files to the GeoReader one at a time, and it will serve up chunks from all of them.

The WorldBuilder is used to create/merge ".geosharer" chunk data into a new or existing world. Pass it the output directory path and a GeoReader object with ".geosharer" files attached, and it will merge that data into the world.