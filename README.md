# Bartizan

A mod framework for [TowerFall Ascension](http://www.towerfall-game.com/) (copyright 2013 Matt Thorson, obviously).

# Included Mods

## New Game Modes

### Respawn
<p align="center">
  <img src="img/respawn.png?raw=true"/>
</p>


![](img/respawn.gif?raw=true)
![](img/respawn2.gif?raw=true)

Not shown on the replay gifs: our awesome in-game kill count HUDs!

### Crawl

<p align="center">
 <img src="img/crawl.png?raw=true"/>
</p>

![](img/crawl.gif?raw=true)
![](img/crawl2.gif?raw=true)

*Variants: No Balancing, No Treasure, Start with Toy Arrows*

Inspired by a certain other indie game - kill human players to regain your humanity!  
Unlike in other game modes, you score points for killing enemy ghosts.  
This may be our most ambitious mod yet, and therefore not quite yet balanced.

## Variants

### No Head Bounce ![](Mod/Content/Atlas/menuAtlas/variants/noHeadBounce.png?raw=true)

![](img/noHeadBounce.gif?raw=true)

### No Ledge Grab ![](Mod/Content/Atlas/menuAtlas/variants/noLedgeGrab.png?raw=true)

### Awfully Slow Arrows ![](Mod/Content/Atlas/menuAtlas/variants/awfullySlowArrows.png?raw=true)

![](img/awfullySlowArrows2.gif?raw=true)
![](img/awfullySlowArrows.gif?raw=true)

### Awfully Fast Arrows ![](Mod/Content/Atlas/menuAtlas/variants/awfullyFastArrows.png?raw=true)

![](img/awfullyFastArrows.gif?raw=true)

### Infinite Arrows ![](Mod/Content/Atlas/menuAtlas/variants/infiniteArrows.png?raw=true)

![](img/infiniteArrows.gif?raw=true)
![](img/infiniteArrows2.gif?raw=true)

### No Dodge Cooldowns ![](Mod/Content/Atlas/menuAtlas/variants/noDodgeCooldowns.png?raw=true)

![](img/noDodgeCooldown.gif?raw=true)

### Gotta Go Fast! ![](Mod/Content/Atlas/menuAtlas/variants/gottaGoFast.png?raw=true)

![](img/gottagofast.gif?raw=true)

Installation
============

* Extract [Bartizan.zip](), then start `Wizard.exe`. This will patch TowerFall.exe and the graphics resources, and save the original files in a new folder `Original`.
* To uninstall, simply reset your TowerFall installation by selecting `Properties -> Local Files -> Verify Integrity of Game Cache` in the Steam context menu for TowerFall.

Hacking
=======

While most of Bartizan was developed using MonoDevelop on Linux, using Visual Studio or any other IDE on Windows should work just as well.

* Copy Steam/SteamApps/common/TowerFall to bin/Original (or at least TowerFall.exe and Content/Atlas, to save some copying time)
* Build Bartizan.sln. The AfterBuild targets should do all the dirty work:
  * Using [Mono.Cecil](https://github.com/jbevain/cecil), the base image `BaseTowerFall.exe` is derived from `Original/TowerFall.exe` by marking members of TowerFall classes as public and virtual (where applicable) so that we can use/override them in `Mod.dll` (and `DevMod.dll`, which is ignored by the Wizard and contains the development-only mods).
  * Any members of classes marked as `[Patch]` in `Mod.dll` will be merged back into their respective base class to form the resulting `TowerFall.exe`.
* Copy (or just symlink) the new `TowerFall.exe`, `*Mod.dll` and `Content/` back to the TowerFall Steam directory.
