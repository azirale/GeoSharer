package net.azirale.civcraft;

import net.minecraft.world.chunk.Chunk;
import net.minecraftforge.event.ForgeSubscribe;
import net.minecraftforge.event.world.ChunkDataEvent;
import net.minecraftforge.event.world.ChunkEvent;
import net.minecraftforge.event.world.WorldEvent;

public class GeoEventHandler {
	
	@ForgeSubscribe
	public void onChunkChange(ChunkEvent chunksave){
		if (chunksave == null) return;
		if (chunksave.getChunk() == null) return;
		GeoSharer.instance.AddChunk(chunksave.getChunk());
	}
	
	@ForgeSubscribe
	public void onWorldLoad(WorldEvent.Load loading){
		GeoSharer.instance.WorldActive(loading.world);
	}
	
	@ForgeSubscribe
	public void onWorldUnload(WorldEvent.Unload unloading){
		GeoSharer.instance.ShutDown();
	}
}
