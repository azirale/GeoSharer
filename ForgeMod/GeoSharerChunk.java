package net.azirale.civcraft;

import java.io.ByteArrayOutputStream;
import java.nio.ByteBuffer;
import java.util.Comparator;
import java.util.zip.GZIPOutputStream;
import org.bouncycastle.util.encoders.Base64;

import net.minecraft.src.ModLoader;
import net.minecraft.world.chunk.Chunk;


class GeoSharerChunk
{
	public int x;
	public int z;
	public byte bytes[];
	
	private static int get_ArrayLength()
	{
		// DATETIME X Y BIOMES BLOCK&DMG
		return 8 + 4 + 4 + 256 + 2*16*16*256;
	}
	
	public boolean equals(Object other){
		if (other instanceof GeoSharerChunk) return Equals((GeoSharerChunk)other);
		return false;
	}
	private boolean Equals(GeoSharerChunk other) { return this.x==other.x && this.z==other.z; }
	private boolean Equals(Chunk other) { return this.x == other.xPosition && this.z==other.zPosition; }
	
	public static GeoSharerChunk CreateFromChunk(Chunk chunk) {
		GeoSharerChunk value = new GeoSharerChunk();
		// Set this object's ID
		value.x = chunk.xPosition;
		value.z = chunk.zPosition;
		// Prepare a byte array to insert raw data
		int bytesLength = get_ArrayLength();
		byte[] rawBytes = new byte[bytesLength];
		// insert date
		byte[] dateBytes = ByteBuffer.allocate(8).putLong(System.currentTimeMillis()).array();
		for (int i=0;i<8;++i) rawBytes[i]=dateBytes[i];
		// insert x
		byte[] xBytes = ByteBuffer.allocate(4).putInt(value.x).array();
		for (int i=0;i<4;++i) rawBytes[i+8]=xBytes[i];
		// insert z
		byte[] yBytes = ByteBuffer.allocate(4).putInt(value.z).array();
		for (int i=0;i<4;++i) rawBytes[i+12]=yBytes[i];
		// insert biome data
		byte[] biomes = chunk.getBiomeArray();
		for (int i=0;i<256;++i)
		{
			rawBytes[i+16]=biomes[i];
		}
		// insert chunk data
		for (int y=0;y<256;++y){
		for (int x=0;x<16;++x){
		for (int z=0;z<16;++z){
			int i=(y*16*16+x*16+z)*2+16+256;
			if (y<30)
			{
				// possibly add exceptions for areas that have been dug out below this
				rawBytes[i]= y==0 ? (byte)7 : (byte)1;
			}
			else
			{
				rawBytes[i]= (byte)(chunk.getBlockID(x, y, z));
				rawBytes[i+1] = (byte)(chunk.getBlockMetadata(x, y, z));
			}
		}}}
		// compress (raw size is ~128kB  per chunk)
		ByteArrayOutputStream byteStream = new ByteArrayOutputStream(rawBytes.length);
		try {
			GZIPOutputStream gzipper = new GZIPOutputStream(byteStream);
			gzipper.write(rawBytes);
			gzipper.close();
			// Convert the stream to a byte array, then save it as a string, ready for output to a file
			value.bytes = Base64.encode(byteStream.toByteArray());
		}
		catch (Exception ex) {
			value.bytes = null;
		}
		return value;
	}
	
	private GeoSharerChunk()
	{
		
	}
}

class GeoChunkComparator implements Comparator<GeoSharerChunk>{
	private int playerX;
	private int playerZ;
	private boolean havePlayerCoords;
	
	public GeoChunkComparator(){
		if (ModLoader.getMinecraftInstance().thePlayer == null) havePlayerCoords = false;
		else {
			havePlayerCoords = true;
			playerX = ModLoader.getMinecraftInstance().thePlayer.chunkCoordX;
			playerZ = ModLoader.getMinecraftInstance().thePlayer.chunkCoordZ;
		}
	}
	
	@Override
	public int compare(GeoSharerChunk a, GeoSharerChunk b) {
		if (!havePlayerCoords) return 0;
		int adist= Math.max(Math.abs(a.x-playerX),Math.abs(a.z-playerZ));
		int bdist= Math.max(Math.abs(b.x-playerX),Math.abs(b.z-playerZ));
		if (adist>bdist) return -1;
		if (adist<bdist) return +1;
		return 0;
	}
}
