package net.azirale.geosharer.mod;

import java.io.BufferedOutputStream;
import java.io.ByteArrayOutputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.util.List;
import java.util.zip.GZIPOutputStream;

public class GeoWriter
{
	public static void writeToFile(String filePath, List<GeoSharerChunk> saveChunks)
	{
		int version = 4;
		int numChunks = saveChunks.size();
		int x[] = new int[numChunks];
		int z[] = new int[numChunks];
		long timestamp[] = new long[numChunks];
		int chunkStarts[] = new int[numChunks];
		
		System.out.println("Geosharer: GeoWriter writing " + numChunks + " chunks");
		
		try
		{
			File outFile = new File(filePath);
			outFile.getParentFile().mkdirs();
			outFile.createNewFile();
			FileOutputStream outStream = new FileOutputStream(outFile);
			GZIPOutputStream zipStream = new GZIPOutputStream(outStream);
			DataOutputStream datStream = new DataOutputStream(zipStream);

			
			// write the metadata first
			int thisStart=4 + 4 + (numChunks * (4+4+8+4)); // VERSION + NUMCHUNKS + [numchunks]*X+Z+TIME+START
			for (int i=0;i<numChunks;++i)
			{
				GeoSharerChunk chunk = saveChunks.get(i);
				x[i] = chunk.x;
				z[i] = chunk.z;
				timestamp[i] = chunk.timestamp;
				chunkStarts[i]=thisStart;
				thisStart+=chunk.bytes.length;
			}
			datStream.writeInt(version);
			datStream.writeInt(numChunks);
			for (int i=0;i<numChunks;++i) { datStream.writeInt(x[i]); }
			for (int i=0;i<numChunks;++i) { datStream.writeInt(z[i]); }
			for (int i=0;i<numChunks;++i) { datStream.writeLong(timestamp[i]); }
			for (int i=0;i<numChunks;++i) { datStream.writeInt(chunkStarts[i]); }
			// now write the chunk data
			for (int i=0;i<numChunks;++i) { datStream.write(saveChunks.get(i).bytes); }
			datStream.flush();
			zipStream.flush();
			outStream.flush();
			datStream.close();
			zipStream.close();
			outStream.close();
		}
		catch (Exception ex)
		{
			System.err.println("Geosharer: Could not write output file '" + filePath + "'");
			System.err.println(ex.getMessage());
		}
		
	}
}
