package net.azirale.geosharer.mod;

// Imports
import java.lang.reflect.Field;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.List;
import net.minecraft.client.Minecraft;
import net.minecraft.client.multiplayer.ServerData;
import cpw.mods.fml.client.FMLClientHandler;
import net.minecraft.world.World;
import net.minecraft.world.chunk.Chunk;

// Class
public class GeoSharerCore {
    	private List<GeoSharerChunk> updateChunks;
    	private boolean isActive;
    	private Minecraft mc;
    	private String outputFolderPath; 
    	private String serverName;
    	
    	// Constructor
    	public GeoSharerCore()
    	{
    		this.isActive = false;
    		this.mc = Minecraft.getMinecraft();
    		this.updateChunks = new ArrayList<GeoSharerChunk>();
    		this.outputFolderPath = "mods/GeoSharer";
    	}
    	
    	// Start tracking chunk data
    	public void activate(World world)
    	{
    		// Check if the mod is active already
    		if (isActive)
    		{
    			System.out.println("GeoSharer: Tried to re-activate while mod was already active");
    			return;
    		}
    		// try and get the server data - in 1.7.2.10.12.0.1024 this is still obfuscated
   			ServerData srvdata = mc.func_147104_D();
   			if (srvdata == null)
   			{
   				System.out.println("GeoSharer: No ServerData - single player world?");
   				return;
   			}
   			else this.serverName = srvdata.serverName.replaceAll("[^\\w]", "");
   			this.isActive = true;
   			System.out.println("GeoSharer: mod is active");
    	}
    	
    	
    	
    	// Stop tracking chunk data (and save)
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
    				if (newChunk.isChunkLoaded) this.addChunk(newChunk);
    			}
    		}
    		// output all stored chunks and deactivate
    		this.writeOut();
    		this.isActive = false;
    	}
    	
    	// save current data to file
    	private void writeOut()
    	{
    		String timeText = new SimpleDateFormat("yyyyMMdd_HHmmss").format(Calendar.getInstance().getTime());
    		String fileName =  "mods/GeoSharer/" + serverName +"/" + serverName +"_" + timeText  + ".geosharer";
    		List<GeoSharerChunk> writeChunks = this.updateChunks;
    		System.out.println("Geosharer: Writing " + writeChunks.size()  + " chunks to '" + fileName + "'");
    		this.updateChunks = new ArrayList<GeoSharerChunk>();
    		GeoWriter.writeToFile(fileName, writeChunks);
    	}
    	
    	// add a new chunk to stored data
    	public void addChunk(Chunk chunk)
    	{
    		if (!isActive) return; // Don't bother, the mod isn't active
    		if (chunk == null) return; // can't save a null object
    		if (chunk.isEmpty()) return; // no point saving an empty chunk
    		if (chunk.worldObj.provider.dimensionId != 0) return; // only do overworld
    		GeoSharerChunk newChunk = GeoSharerChunk.CreateFromChunk(chunk);
    		updateChunks.remove(newChunk);
    		updateChunks.add(newChunk);
    		// if (updateChunks.size() > 10000) trimStoredChunks();
    	}
    	
    	public void printStatus()
    	{
    		if (mc == null) return;
    		if (mc.thePlayer == null) return;
    		if (isActive) mc.thePlayer.sendChatMessage("GeoSharer is active, holding " + updateChunks.size() + " chunks");
    		else mc.thePlayer.sendChatMessage("GeoSharer is inactive");
    	}
}