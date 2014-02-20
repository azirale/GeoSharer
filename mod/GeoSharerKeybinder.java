/* Disabled until I figure out 172

package net.azirale.geosharer.mod;

import java.util.EnumSet;

import org.lwjgl.input.Keyboard;

import cpw.mods.fml.client.FMLClientHandler;
import cpw.mods.fml.client.registry.KeyBindingRegistry;
import cpw.mods.fml.client.registry.KeyBindingRegistry.KeyHandler;
import cpw.mods.fml.common.TickType;
import net.minecraft.client.settings.KeyBinding;
import net.minecraft.src.mod_Geosharer;

public class GeoSharerKeybinder extends KeyHandler
{
	private GeoSharerCore core;
	
	public static GeoSharerKeybinder CreateNew(GeoSharerCore core)
	{
		// array of key bindings (and whether each repeats when held down)
    	KeyBinding[] key = {new KeyBinding("GeoSharer Menu", Keyboard.KEY_GRAVE)};
    	boolean[] repeat = {false};
    	// register new keybinding class
    	GeoSharerKeybinder createme = new GeoSharerKeybinder(key,repeat);
    	createme.core = core;
        KeyBindingRegistry.registerKeyBinding(createme);
        return createme;
	}

	public GeoSharerKeybinder(KeyBinding[] keyBindings, boolean[] repeatings) {
		super(keyBindings, repeatings);
	}

	@Override
	public String getLabel() {
		return "GeoSharer Keybindings";
	}

	@Override
	public void keyDown(EnumSet<TickType> types, KeyBinding kb, boolean tickEnd, boolean isRepeat)
	{
		// not used
	}

	@Override
	public void keyUp(EnumSet<TickType> types, KeyBinding kb, boolean tickEnd) {
		// Grave is open menu
		if (kb.keyCode == Keyboard.KEY_GRAVE && tickEnd && FMLClientHandler.instance().getClient().currentScreen == null)
		{
			System.out.println("-------------------");
			mod_Geosharer.instance.gui.PopMenu();
		}
	}

	@Override
	public EnumSet<TickType> ticks() {
		return EnumSet.of(TickType.CLIENT);
	}

}
/**/