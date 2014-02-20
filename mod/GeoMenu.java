/* Disabled until I figure out more about 172

package net.azirale.geosharer.mod;

import net.minecraft.client.gui.GuiButton;
import net.minecraft.client.gui.GuiScreen;
import net.minecraft.src.ModLoader;
import net.minecraft.src.mod_Geosharer;

public class GeoMenu extends GuiScreen
{
	public static final int GUI_ID = 0;
	
	public GeoMenu ()
	{
		System.out.println("GEOMENU CONSTRUCTOR");
	}
	
	@Override
	public void initGui()
	{
		System.out.println("GEOMENU INITGUI");
		buttonList.clear();
		// Create a new button
		AddButton(1,width/2-50,height/2-40,100,20,"New Button");
	}
	
	// just to make it explicit how this works
	private void AddButton(int buttonID, int left, int top, int width, int height, String name)
	{
		buttonList.add(new GuiButton(buttonID, left, top, width, height, name));
	}
    
    @Override
    public void drawScreen(int par1, int par2, float par3)
    {
        this.drawDefaultBackground();
        super.drawScreen(par1, par2, par3);
    }
}
/**/