package net.azirale.civcraft;
// java stuff
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.List;
import net.minecraft.client.Minecraft;
import net.minecraft.src.ModLoader;
import net.minecraft.world.World;
// forge
import net.minecraft.world.chunk.Chunk;
import net.minecraftforge.common.MinecraftForge;
import net.minecraftforge.event.ForgeSubscribe;
import cpw.mods.fml.common.Mod;
import cpw.mods.fml.common.Mod.Init;
import cpw.mods.fml.common.Mod.Instance;
import cpw.mods.fml.common.Mod.PostInit;
import cpw.mods.fml.common.Mod.PreInit;
	//import cpw.mods.fml.common.SidedProxy;
import cpw.mods.fml.common.event.FMLInitializationEvent;
import cpw.mods.fml.common.event.FMLPostInitializationEvent;
import cpw.mods.fml.common.event.FMLPreInitializationEvent;
import cpw.mods.fml.common.network.NetworkMod;
// the mod
@Mod(modid="GeoSharer", name="GeoSharer", version="1.5.2.0")
@NetworkMod(clientSideRequired=true, serverSideRequired=false)
public class GeoSharer {

        // The instance of your mod that Forge uses.
        @Instance("GeoSharer")
        public static GeoSharer instance;
        
        private File outFile;
    	private FileOutputStream writer;
    	private List<GeoSharerChunk> updateChunks;
    	private boolean isActive;
    	private Minecraft mc;
    	
    	public GeoSharer()
    	{
    		isActive=false;
    		mc = ModLoader.getMinecraftInstance();
    		updateChunks = new ArrayList<GeoSharerChunk>();
    	}
    	
    	public void activate(World world)
    	{
    		if (isActive) return; // Already active
    		if (!world.isRemote) { isActive = false; return; } // Does not activate on local worlds
    		// Open up a file stream to write to
    		String serverName = mc.getServerData().serverName.replaceAll("[^\\w]", "");
    		String timeText = new SimpleDateFormat("yyyyMMdd_HHmmss").format(Calendar.getInstance().getTime());
    		String folderPath = "mods/GeoSharer";
    		String fileName =  "mods/GeoSharer/" + serverName +"_" + timeText  + ".geosharer";
    		try {
    			outFile = new File(fileName);
    			System.out.println("GeoSharer: Attempting to output to file '" + outFile.getAbsolutePath() + "'");
    			outFile.getParentFile().mkdirs();
    			outFile.createNewFile();
    			writer = new FileOutputStream(outFile);
    		}
    		catch (Exception ex)
    		{
    			if (outFile == null) System.out.println("GeoSharer: Failed to create outFile object");
    			if (!outFile.exists()) System.out.println("GeoSharer: Failed to create output file");
    			if (writer == null) System.out.println("GeoSharer: Failed to create buffered writer");
    			System.out.println("GeoSharer: mod is inactive");
    			outFile = null;
    			writer = null;
    			isActive = false;
    		}
    		isActive = true;
    	}
    	
    	public void deactivate(World world)
    	{
    		int playerX = (int)mc.thePlayer.posX/16;
    		int playerZ = (int)mc.thePlayer.posZ/16;
    		for (int x=-10;x<=10;++x)
    		{
    			for (int z=-10;z<=10;++z)
    			{
    				Chunk newChunk = world.getChunkFromChunkCoords(playerX+x, playerZ+z);
    				if (!newChunk.isChunkLoaded) System.out.println("GeoSharer: End-of-world save tried to get chunk beyond sight range");
    				else this.AddChunk(newChunk);
    			}
    		}
    		
    		try {
    			for (GeoSharerChunk chunk : updateChunks) if (isActive) SaveChunk(chunk);
    			if (writer != null)
    			{
    				writer.flush();
    				writer.close();
    			}
    		} catch (Exception e) {
				System.err.println("GeoSharer was unable to save your output file.");
			} finally {
	    		updateChunks.clear();
    			outFile = null;
    			writer = null;
        		this.isActive = false;
    		}
    	}
    	
    	public void AddChunk(Chunk chunk)
    	{
    		if (!isActive) return; // Don't bother, the mod isn't active
    		if (chunk == null) return;
    		GeoSharerChunk newChunk = GeoSharerChunk.CreateFromChunk(chunk);
    		//if (updateChunks.remove(newChunk)) mc.thePlayer.addChatMessage("Removed chunk at x=" + newChunk.x + " z=" + newChunk.z);
    		updateChunks.remove(newChunk);
    		updateChunks.add(newChunk);
    		if (updateChunks.size() > 1000) TrimStoredChunks();
    		//mc.thePlayer.addChatMessage("Added chunk at x=" + newChunk.x + " z=" + newChunk.z);
    		//mc.thePlayer.addChatMessage("There are " + updateChunks.size() + " chunks stored");
    		//mc.thePlayer.addChatMessage("Latest chunk is ~ " + newChunk.bytes.length + " bytes");
    		//mc.thePlayer.addChatMessage(new String(newChunk.bytes));
    		//System.out.println(outFile.getAbsolutePath());
    	}
    	
    	
    	
    	
    	private void TrimStoredChunks(){
   			GeoChunkComparator com = new GeoChunkComparator();
   			Collections.sort(updateChunks, com);
   			int lastChunk = updateChunks.size();
   			for (int i=lastChunk-1;i>=500;--i){
   				SaveChunk(updateChunks.get(i));
   				updateChunks.remove(i);
   			}
   			
    	}
    	
    	
    	
    	
    	private void SaveChunk(GeoSharerChunk chunk){
    		if (!isActive) return;
    		try {
    			writer.write(chunk.bytes);
    			writer.write('\n');
    			mc.thePlayer.addChatMessage("Saved Chunk x=" + chunk.x + " z=" + chunk.z);
    			System.out.println("Saved Chunk x=" + chunk.x + " z=" + chunk.z);
    		}
    		catch (Exception ex) {
    			mc.thePlayer.addChatMessage("GeoSharer failed to write to output file. Mod shutting down.");
    			System.out.println("GeoSharer failed to write to output file. Mod shutting down.");
    			this.isActive = false;
    			this.updateChunks.clear();
    			this.outFile = null;
    			this.writer = null;
    		}
    	}
    	
    	
    	
    	public void PrintStatus(){
    		if (mc == null) return;
    		if (mc.thePlayer == null) return;
    		if (isActive) mc.thePlayer.addChatMessage("GeoSharer is active, holding " + updateChunks.size() + " chunks");
    		else mc.thePlayer.addChatMessage("GeoSharer is inactive");
    	}
    	
    	
    	
    	
    	
    	
    	
        // Says where the client and server 'proxy' code is loaded.
        //@SidedProxy(clientSide="tutorial.generic.client.ClientProxy", serverSide="tutorial.generic.CommonProxy")
        //public static CommonProxy proxy;
        
        @PreInit
        public void preInit(FMLPreInitializationEvent event) { }
        
        @Init
        public void load(FMLInitializationEvent event) { 
        	MinecraftForge.EVENT_BUS.register(new GeoEventHandler());
        }
        
        @PostInit
        public void postInit(FMLPostInitializationEvent event) { }
}