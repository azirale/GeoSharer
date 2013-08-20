package net.azirale.geosharer;

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
	
	private static int get_ArrayLength(int maxY)
	{
		// VERSION, DATETIME, X, Y, BIOMES, HEIGHT, BLOCK, DATA
		return 1 + 8 + 4 + 4 + 256 + 1 + (maxY+1)*16*16 + (maxY+1)*16*8;
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
		byte[] xBytes = ByteBuffer.allocate(4).putInt(value.x).array();
		for (int i=0;i<4;++i) rawBytes[i+offset]=xBytes[i];
		offset += 4;
		// insert z
		byte[] yBytes = ByteBuffer.allocate(4).putInt(value.z).array();
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
			if (y<30)
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
					//rawBytes[d] = (byte)(meta * 16); // bitshift left
				}
				else
				{
					//rawBytes[d] = (byte)(meta + rawBytes[d]); // add the two together
				}
			}
		}}}
		ByteArrayOutputStream byteStream = new ByteArrayOutputStream(rawBytes.length);
		try {
			GZIPOutputStream gzipper = new GZIPOutputStream(byteStream);
			gzipper.write(rawBytes);
			gzipper.finish();
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
