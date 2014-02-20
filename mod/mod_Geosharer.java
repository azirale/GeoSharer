//package net.minecraft.src;
package net.azirale.geosharer.mod;

import cpw.mods.fml.common.Mod;
import cpw.mods.fml.common.Mod.EventHandler;
import cpw.mods.fml.common.Mod.Instance;
import cpw.mods.fml.common.event.FMLInitializationEvent;
import cpw.mods.fml.common.event.FMLPostInitializationEvent;
import cpw.mods.fml.common.event.FMLPreInitializationEvent;



@Mod(modid="GeoSharer", name="GeoSharer", version="1.7.2.0")
public class mod_Geosharer
{
        @Instance("GeoSharer")
        public static mod_Geosharer instance; // singleton enabler
        public GeoSharerCore core; // worker object
        public GeoEventHandler events; // event handler
        //public GeoSharerKeybinder keys; // keybinding handler
        //public GeoGuiHandler gui; // gui handler
        
        public GeoSharerCore getCore()
        {
        	return this.core;
        }
        
        @EventHandler
        public void preInit(FMLPreInitializationEvent event) { }
        
        @EventHandler
        public void load(FMLInitializationEvent event) {
        	// Open up the core of the mod - the class that handles the work
        	this.core = new GeoSharerCore();
        	this.events = GeoEventHandler.CreateNew(this.core);
        	//this.keys = GeoSharerKeybinder.CreateNew(this.core);
        	//this.gui = GeoGuiHandler.CreateNew(this, this.core);
        }
        
        @EventHandler
        public void postInit(FMLPostInitializationEvent event) { }
}
