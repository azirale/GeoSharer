package net.azirale.geosharer.mod;

// Imports
import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import net.minecraft.client.Minecraft;
import net.minecraft.src.ModLoader;
import net.minecraft.world.World;
import net.minecraft.world.chunk.Chunk;

// Class
public class GeoSharerCore {
        private GeoFileWriter writer;
    	private List<GeoSharerChunk> updateChunks;
    	private boolean isActive;
    	private Minecraft mc;
    	
    	public GeoSharerCore()
    	{
    		this.isActive = false;
    		this.mc = ModLoader.getMinecraftInstance();
    		this.updateChunks = new ArrayList<GeoSharerChunk>();
    	}
    	
    	public void activate(World world)
    	{
    		if (isActive) // Already active, don't do anything
    		{
    			System.out.println("GeoSharer: Tried to re-activate while mod was already active");
    			return;
    		}
    		this.writer = GeoFileWriter.createNew();
    		if (this.writer == null)
    		{
    			System.err.println("GeoSharer: Activation blocked by failure to create file writer");
    			return;
    		}
    		this.isActive = true;
    		System.out.println("GeoSharer: mod is active");
    	}
    	
    	public void deactivate(World world)
    	{
    		if (!isActive) // Already inactive
    		{
    			System.out.println("GeoSharer: Tried to deactivate while mod was already inactive");
    			return;
    		}
    		// Scan around the player for loaded chunks and add them to the save list
    		int playerX = (int)mc.thePlayer.posX/16;
    		int playerZ = (int)mc.thePlayer.posZ/16;
    		for (int x=-10;x<=10;++x)
    		{
    			for (int z=-10;z<=10;++z)
    			{
    				Chunk newChunk = world.getChunkFromChunkCoords(playerX+x, playerZ+z);
    				if (!newChunk.isChunkLoaded) System.out.println("GeoSharer: End-of-world save tried to get chunk beyond sight range");
    				else this.addChunk(newChunk);
    			}
    		}
    		// output all stored chunks and deactivate
   			for (GeoSharerChunk chunk : updateChunks) saveChunk(chunk);
   			this.writer.close();
   			this.writer = null;
   			this.updateChunks.clear();
   			this.isActive = false;
    	}
    	
    	public void addChunk(Chunk chunk)
    	{
    		if (!isActive) return; // Don't bother, the mod isn't active
    		if (chunk == null) return; // can't save a null object
    		if (chunk.isEmpty()) return; // no point saving an empty chunk
    		if (chunk.worldObj.provider.dimensionId != 0) return; // we only support overworld atm, -1=nether 1=end
    		int maxY = 0;
    		for (int i=0;i<256;++i)
    		{
    			maxY = chunk.heightMap[i] > maxY?chunk.heightMap[i]:maxY;
    		}
    		GeoSharerChunk newChunk = GeoSharerChunk.CreateFromChunk(chunk);
    		updateChunks.remove(newChunk);
    		updateChunks.add(newChunk);
    		if (updateChunks.size() > 10000) trimStoredChunks();
    	}
    	
    	public void printStatus(){
    		if (mc == null) return;
    		if (mc.thePlayer == null) return;
    		if (isActive) mc.thePlayer.addChatMessage("GeoSharer is active, holding " + updateChunks.size() + " chunks");
    		else mc.thePlayer.addChatMessage("GeoSharer is inactive");
    	}

    	private void trimStoredChunks(){
    		mc.thePlayer.addChatMessage("GeoSharer is holding over 10,000 chunks, dumping most distant chunks to disk");
   			GeoChunkComparator com = new GeoChunkComparator();
   			Collections.sort(updateChunks, com);
   			int lastChunk = updateChunks.size();
   			int x=0;
   			for (int i=lastChunk-1;i>=500;--i){
   				saveChunk(updateChunks.get(i));
   				updateChunks.remove(i);
   				++x;
   			}
   			mc.thePlayer.addChatMessage("GeoSharer saved " + x + " chunks to disk.");
    	}
    	
    	private void saveChunk(GeoSharerChunk chunk){
    		if (!isActive) return;
    		if (!writer.writeChunk(chunk))
    		{
    			this.mc.thePlayer.addChatMessage("GeoSharer failed to write to output file - Deactivating and abandoning file saves");
    			this.writer.close();
    			this.writer = null;
    			this.updateChunks.clear();
    			this.isActive = false;
    		}
    	}
}