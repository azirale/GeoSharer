package net.minecraft.src;

import cpw.mods.fml.common.Mod;
import cpw.mods.fml.common.Mod.Init;
import cpw.mods.fml.common.Mod.Instance;
import cpw.mods.fml.common.Mod.PostInit;
import cpw.mods.fml.common.Mod.PreInit;
import cpw.mods.fml.common.event.FMLInitializationEvent;
import cpw.mods.fml.common.event.FMLPostInitializationEvent;
import cpw.mods.fml.common.event.FMLPreInitializationEvent;
import cpw.mods.fml.common.network.NetworkMod;
import net.azirale.geosharer.GeoEventHandler;
import net.azirale.geosharer.GeoSharerCore;
import net.minecraftforge.common.MinecraftForge;

@Mod(modid="GeoSharer", name="GeoSharer", version="1.5.2.0")
@NetworkMod(clientSideRequired=true, serverSideRequired=false)
public class mod_Geosharer
{
        @Instance("GeoSharer")
        public static mod_Geosharer instance; // singleton enabler
        private GeoSharerCore core; // worker object
        
        public GeoSharerCore getCore()
        {
        	return this.core;
        }
        
        @PreInit
        public void preInit(FMLPreInitializationEvent event) { }
        
        @Init
        public void load(FMLInitializationEvent event) {
        	this.core = new GeoSharerCore();
        	MinecraftForge.EVENT_BUS.register(new GeoEventHandler(this.core));
        }
        
        @PostInit
        public void postInit(FMLPostInitializationEvent event) { }
}
