package net.azirale.geosharer;

import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.zip.GZIPOutputStream;

import net.minecraft.src.ModLoader;

public class GeoFileWriter
{
	// Fields
	private FileOutputStream outStream;
	private GZIPOutputStream zipStream;
	private BufferedOutputStream bufStream;
	
	// Constructor - Private, use factory
	private GeoFileWriter(FileOutputStream outStream, GZIPOutputStream zipStream, BufferedOutputStream bufStream)
	{
		this.outStream = outStream;
		this.zipStream = zipStream;
		this.bufStream = bufStream;
	}
	
	public boolean writeChunk(GeoSharerChunk chunk)
	{
		try {
			bufStream.write(chunk.bytes);
			bufStream.write('\n');
			//outStream.write(chunk.bytes);
			//outStream.write('\n');
		}
		catch (Exception ex) {
			System.err.println("GeoSharer: Failed to write to output file");
			return false;
		}
		return true;
	}
	
	public void close()
	{
		try
		{
			bufStream.flush();
			bufStream.close();
			zipStream.finish();
			zipStream.flush();
			zipStream.close();
			outStream.flush();
			outStream.close();
		}
		catch (Exception e)
		{
			System.err.println("GeoSharer: Filewriter failed to flush and close");
		}
	}
	
	
	// Factory
	public static GeoFileWriter createNew()
	{
		File outFile = null;
		FileOutputStream outStream = null;
		GZIPOutputStream zipStream = null;
		BufferedOutputStream bufStream = null;
		String serverName = ModLoader.getMinecraftInstance().getServerData().serverName.replaceAll("[^\\w]", "");
		String timeText = new SimpleDateFormat("yyyyMMdd_HHmmss").format(Calendar.getInstance().getTime());
		String folderPath = "mods/GeoSharer";
		String fileName =  "mods/GeoSharer/" + serverName +"_" + timeText  + ".geosharer";
		try {
			outFile = new File(fileName);
			System.out.println("GeoSharer: Attempting to output to file '" + outFile.getAbsolutePath() + "'");
			outFile.getParentFile().mkdirs();
			outFile.createNewFile();
			outStream = new FileOutputStream(outFile);
			zipStream = new GZIPOutputStream(outStream);
			bufStream = new BufferedOutputStream(zipStream);
		}
		catch (Exception ex)
		{
			if (outFile == null) System.err.println("GeoSharer: Failed to create outFile object");
			else if (!outFile.exists()) System.err.println("GeoSharer: Failed to create output file");
			else if (outStream == null) System.err.println("GeoSharer: Failed to create buffered writer");
			return null; // couldn't create a valid GeoFileWriter
		}
		GeoFileWriter value = new GeoFileWriter(outStream, zipStream, bufStream);
		return value;
	}
}
