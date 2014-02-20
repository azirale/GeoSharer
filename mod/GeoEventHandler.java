package net.azirale.geosharer.mod;

import cpw.mods.fml.common.eventhandler.SubscribeEvent;
import net.minecraftforge.common.MinecraftForge;
import net.minecraftforge.event.world.ChunkEvent;
import net.minecraftforge.event.world.WorldEvent;

public class GeoEventHandler {
	
	private GeoSharerCore geoCore;
	
	public static GeoEventHandler CreateNew(GeoSharerCore core)
	{
		GeoEventHandler handler = new GeoEventHandler(core);
    	MinecraftForge.EVENT_BUS.register(handler);
    	return handler;
	}
	
	public GeoEventHandler(GeoSharerCore core)
	{
		this.geoCore = core;
	}
	
	@SubscribeEvent
	public void onChunkChange(ChunkEvent.Unload chunksave)
	{
		if (chunksave == null) return;
		if (chunksave.getChunk() == null) return;
		geoCore.addChunk(chunksave.getChunk());
	}
	
	@SubscribeEvent
	public void onWorldLoad(WorldEvent.Load loading)
	{
		geoCore.activate(loading.world);
	}
	
	@SubscribeEvent
	public void onWorldUnload(WorldEvent.Unload unloading)
	{
		geoCore.deactivate(unloading.world);
	}
}
