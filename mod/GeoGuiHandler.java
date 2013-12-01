package net.azirale.geosharer.mod;

import net.minecraft.client.Minecraft;
import net.minecraft.entity.player.EntityPlayer;
import net.minecraft.src.ModLoader;
import net.minecraft.src.mod_Geosharer;
import net.minecraft.world.World;
import cpw.mods.fml.common.network.IGuiHandler;
import cpw.mods.fml.common.network.NetworkRegistry;

public class GeoGuiHandler implements IGuiHandler
{
	private GeoSharerCore core;
	
	public static GeoGuiHandler CreateNew(mod_Geosharer mod, GeoSharerCore core)
	{
		GeoGuiHandler createme = new GeoGuiHandler(core);
		NetworkRegistry.instance().registerGuiHandler(mod, createme);		
		return createme;
	}
	
	private GeoGuiHandler(GeoSharerCore core)
	{
		this.core = core;
	}

	@Override
	public Object getServerGuiElement(int ID, EntityPlayer player, World world, int x, int y, int z)
	{
		// none
		return null;
	}

	public void PopMenu()
	{
		Minecraft mc = ModLoader.getMinecraftInstance();
		mc.thePlayer.openGui(mod_Geosharer.instance, GeoMenu.GUI_ID, null, 0, 0, 0);
	}
	
	@Override
	public Object getClientGuiElement(int ID, EntityPlayer player, World world, int x, int y, int z) {
		if (ID == GeoMenu.GUI_ID){ return new GeoMenu(); }
		return null;
	}

}
