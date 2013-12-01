package net.azirale.geosharer.mod;

import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.Comparator;
import java.util.zip.GZIPOutputStream;
import org.bouncycastle.util.encoders.Base64;

import net.minecraft.nbt.NBTTagCompound;
import net.minecraft.src.ModLoader;
import net.minecraft.world.chunk.Chunk;


class GeoSharerChunk
{
	private static final int ChunkWidth = 16; // straightforward
	private static final int ChunkArea = 256; // 16*16
	private static final byte BedrockID = 7;
	private static final byte StoneID = 1;
	
	public int x;
	public int z;
	public byte bytes[];
	

	
	public boolean equals(Object other){
		if (other instanceof GeoSharerChunk) return Equals((GeoSharerChunk)other);
		return false;
	}
	private boolean Equals(GeoSharerChunk other) { return this.x==other.x && this.z==other.z; }
	private boolean Equals(Chunk other) { return this.x == other.xPosition && this.z==other.zPosition; }
	
	public static GeoSharerChunk CreateFromChunk(Chunk chunk) {
		GeoSharerChunk value = new GeoSharerChunk();
		if (chunk == null)
		{
			System.err.println("GeoSharer was passed a null chunk for GeoSharerChunk.CreateFromChunk()");
			value.bytes = null;
			return value;
		}
		// Set this object's ID
		value.x = chunk.xPosition;
		value.z = chunk.zPosition;
		value.bytes = getNBTBytes(chunk);
		return value;
	}

	// DEPRECATED
	private byte[] getRawBytes(Chunk chunk)
	{
		// Prepare a byte array to insert raw data
		int maxY = 0;
		for (int i=0;i<256;++i)
		{
			maxY = chunk.heightMap[i] > maxY?chunk.heightMap[i]:maxY;
		}
		int bytesLength = get_ArrayLength(maxY);
		int offset = 0;
		byte[] rawBytes = new byte[bytesLength];
		// insert version
		rawBytes[offset] = (byte)2; // version
		offset++;
		// insert date
		byte[] dateBytes = ByteBuffer.allocate(8).putLong(System.currentTimeMillis()).array();
		for (int i=0;i<8;++i) rawBytes[i+offset]=dateBytes[i];
		offset += 8;
		// insert x
		byte[] xBytes = ByteBuffer.allocate(4).putInt(chunk.xPosition).array();
		for (int i=0;i<4;++i) rawBytes[i+offset]=xBytes[i];
		offset += 4;
		// insert z
		byte[] yBytes = ByteBuffer.allocate(4).putInt(chunk.zPosition).array();
		for (int i=0;i<4;++i) rawBytes[i+offset]=yBytes[i];
		offset += 4;
		// insert biome data
		byte[] biomes = chunk.getBiomeArray();
		for (int i=0;i<256;++i)
		{
			rawBytes[i+offset]=biomes[i];
		}
		offset+=256;
		// insert max height
		rawBytes[offset] = (byte) maxY;
		offset++;
		// insert block ids
		for (int y=0;y<=maxY;++y){
		for (int x=0;x<16;++x){
		for (int z=0;z<16;++z){
			int i=(y*16*16+x*16+z)+offset;
			int d=offset + (maxY+1)*16*16 + y*16*8 + x*8 + z/2;
			if (y<10)
			{
				// possibly add exceptions for areas that have been dug out below this
				rawBytes[i]= y==0 ? (byte)7 : (byte)1;
				//rawBytes[d]= (byte)0;
			}
			else
			{
				rawBytes[i]= (byte)(chunk.getBlockID(x, y, z));
				int meta = chunk.getBlockMetadata(x,y,z);
				if (z%2==0)
				{
					rawBytes[d] = (byte)(meta << 4); // bitshift left 4 spaces
				}
				else
				{
					rawBytes[d] = (byte)(rawBytes[d] | meta); // combine the two
				}
			}
		}}}
		ByteArrayOutputStream byteStream = new ByteArrayOutputStream();
		byte[] value = null;
		try {
			GZIPOutputStream gzipper = new GZIPOutputStream(byteStream);
			gzipper.write(rawBytes);
			gzipper.finish();
			gzipper.close();
			// Convert the stream to a byte array, then save it as a string, ready for output to a file
			value = Base64.encode(byteStream.toByteArray());
		}
		catch (Exception ex) {
			System.err.println("GeoSharer hit an exception when trying to encode new GeoChunk byte array");
			value = null;
		}
		return value;
	}
	
	private static int get_ArrayLength(int maxY)
	{
		// VERSION, DATETIME, X, Y, BIOMES, HEIGHT, BLOCK, DATA
		return 1 + 8 + 4 + 4 + 256 + 1 + (maxY+1)*16*16 + (maxY+1)*16*8;
	}

	private static byte[] getNBTBytes(Chunk chunk)
	{
		NBTTagCompound georoot;
		georoot = new NBTTagCompound();
		georoot.setName("GeoSharerChunk");
		// Version
		georoot.setInteger("Version", 3);
		// Timestamp
		georoot.setLong("GeoTimestamp", System.currentTimeMillis());
		// Chunk X and Z
		georoot.setInteger("X", chunk.xPosition);
		georoot.setInteger("Z", chunk.zPosition);
		// Biomes
		georoot.setByteArray("Biomes", chunk.getBiomeArray());
		// MaxY level (do not bother storing air blocks)
		int maxY = 0;
		for (int i=0;i<ChunkArea;++i)
		{
			maxY = chunk.heightMap[i] > maxY?chunk.heightMap[i]:maxY;
		}
		georoot.setInteger("MaxY", maxY);
		// Block ID and data values - integers in case of mods
		byte[] blockIDs = new byte[(maxY+1)*ChunkArea];
		byte[] blockData = new byte[(maxY+1)*ChunkArea/2]; // 4-bit not 8-bit
		for (int y=0;y<=maxY;++y) {
		for (int x=0;x<16;++x) {
		for (int z=0;z<16;++z) {
			int i=(y*ChunkArea+x*ChunkWidth+z);
			if (y<10)
			{
				// possibly add exceptions for areas that have been dug out below this
				// Default to bedrock for y=0, stone for y=1 through 9 
				blockIDs[i]= y==0 ? BedrockID : StoneID;
			}
			else
			{
				blockIDs[i]= (byte)(chunk.getBlockID(x, y, z));
				byte meta = (byte)(chunk.getBlockMetadata(x, y, z));
				if (z%2==0)
				{
					blockData[i/2] = (byte)(meta << 4);
				}
				else
				{
					blockData[i/2] = (byte)(blockData[i/2] | meta);
				}
			}
		}}} // end of x:y:z loops
		georoot.setByteArray("BlockIDs", blockIDs);
		georoot.setByteArray("BlockData", blockData);
		
		try
		{
			ByteArrayOutputStream byteStream = new ByteArrayOutputStream();
			DataOutputStream dataOut = new DataOutputStream(new GZIPOutputStream(byteStream));
			georoot.writeNamedTag(georoot, dataOut);
			dataOut.close();
			byte[] value = Base64.encode(byteStream.toByteArray());
			return value;
		} catch (IOException e) {
			System.err.println("GeoSharer hit an exception when trying to encode new GeoChunkNBT byte array");
			return null;
		}
		
	}
	
	// Private constructor to force factory method
	private GeoSharerChunk() { }
}
