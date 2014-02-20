package net.azirale.geosharer.mod;

import java.io.ByteArrayOutputStream;
import java.io.DataOutput;
import java.io.DataOutputStream;
import java.io.IOException;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.zip.GZIPOutputStream;

import net.minecraft.client.Minecraft;
import net.minecraft.client.multiplayer.ServerData;
import net.minecraft.nbt.NBTTagCompound;
import net.minecraft.nbt.NBTTagList;
import net.minecraft.world.chunk.Chunk;
import net.minecraft.world.chunk.storage.ExtendedBlockStorage;




class GeoSharerChunk
{
	public int x;
	public int z;
	public long timestamp;
	public byte bytes[];
	
	
	// Private constructor to force factory method
	private GeoSharerChunk() { }
		
	// Equality methods	
	public boolean equals(Object other)
	{
		if (other instanceof GeoSharerChunk) return Equals((GeoSharerChunk)other);
		return false;
	}
	private boolean Equals(GeoSharerChunk other) { return this.x==other.x && this.z==other.z; }
	private boolean Equals(Chunk other) { return this.x == other.xPosition && this.z==other.zPosition; }
		

	public static GeoSharerChunk CreateFromChunk(Chunk chunk) {
		GeoSharerChunk value = new GeoSharerChunk();
		if (chunk == null)
		{
			System.err.println("GeoSharer: Was passed a null chunk for GeoSharerChunk.CreateFromChunk()");
			value.bytes = null;
			return value;
		}
		// Set this object's ID
		value.x = chunk.xPosition;
		value.z = chunk.zPosition;
		value.timestamp = System.currentTimeMillis();
		NBTTagCompound root = getChunkNBT(chunk, value.timestamp);
		value.bytes = bytesFromNBT(root);
		return value;
	}
	
	private static NBTTagCompound getChunkNBT(Chunk chunk, long timestamp)
    {
		NBTTagCompound root = new NBTTagCompound();
        NBTTagCompound level = new NBTTagCompound();
        root.setTag("Level", level);
        level.setInteger("xPos", chunk.xPosition);
        level.setInteger("zPos", chunk.zPosition);
        //level.setInteger("GeoDimension", chunk.worldObj.provider.dimensionId);
        NBTTagList sections = new NBTTagList();

        ExtendedBlockStorage[] allBlocks = chunk.getBlockStorageArray();
        int i = allBlocks.length;
        NBTTagCompound thisSection;
        for (int j = 0; j < i; ++j)
        {
            ExtendedBlockStorage blocks = allBlocks[j];

            if (blocks != null)
            {
            	thisSection = new NBTTagCompound();
            	thisSection.setByte("Y", (byte)(blocks.getYLocation() >> 4 & 255));
            	thisSection.setByteArray("Blocks", blocks.getBlockLSBArray());
                if (blocks.getBlockMSBArray() != null)
                {
                	thisSection.setByteArray("Add", blocks.getBlockMSBArray().data);
                }
                thisSection.setByteArray("Data", blocks.getMetadataArray().data);
                sections.appendTag(thisSection);
            }
        }
        level.setTag("Sections", sections);
        level.setByteArray("Biomes", chunk.getBiomeArray());
        return root;
    }
	
	// reflection to access the write method of NBTTagCompound
	private static Method NBTTagCompoundWrite = null;
	private static void AcquireNBTTagCompoundWrite()
	{
		for (Method method : NBTTagCompound.class.getDeclaredMethods())
		{
			if (!method.getReturnType().equals(void.class)) continue;
			Class[] x = method.getParameterTypes();
			if (x.length==1 && x[0].equals(DataOutput.class))
			{
				method.setAccessible(true);
				NBTTagCompoundWrite=method;
				break;
			}
		}
		if (NBTTagCompoundWrite==null) System.err.println("GeoSharer: Did not get NBTTagCompound write Method");
	}
	
	private static byte[] bytesFromNBT(NBTTagCompound root)
	{
		try
		{
			ByteArrayOutputStream byteStream = new ByteArrayOutputStream();
			DataOutputStream dataOut = new DataOutputStream(new GZIPOutputStream(byteStream));
			
			// root.write(dataOut);
			// method is no longer public, so reflection to the rescue!
			if (NBTTagCompoundWrite==null) AcquireNBTTagCompoundWrite();
			try { NBTTagCompoundWrite.invoke(root, dataOut); }
			catch (Exception e) { System.err.println("GeoSharer: Reflecting access to NBTTagCompound.write failed miserably"); }
			
			dataOut.close();
			byte[] value = byteStream.toByteArray();
			return value;
		} catch (IOException e) {
			System.err.println("GeoSharer: Hit an exception when trying to encode new GeoChunkNBT byte array");
			return null;
		}
	}
}
