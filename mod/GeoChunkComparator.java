package net.azirale.geosharer.mod;

import java.util.Comparator;
import net.minecraft.client.Minecraft;
import net.minecraft.client.entity.EntityClientPlayerMP;

class GeoChunkComparator implements Comparator<GeoSharerChunk>{
	private int playerX;
	private int playerZ;
	private boolean havePlayerCoords;
	
	public GeoChunkComparator(){
		EntityClientPlayerMP player = Minecraft.getMinecraft().thePlayer;
		if (player == null) havePlayerCoords = false;
		else
		{
			havePlayerCoords = true;
			playerX = player.chunkCoordX;
			playerZ = player.chunkCoordZ;
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