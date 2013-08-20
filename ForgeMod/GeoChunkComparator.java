package net.azirale.geosharer;

import java.util.Comparator;

import net.minecraft.src.ModLoader;

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