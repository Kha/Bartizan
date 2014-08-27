# Bartizan

A mod framework for [TowerFall Ascension](http://www.towerfall-game.com/) (copyright 2013 Matt Thorson, obviously).

* [Included Mods](#included-mods)
  * [Game Modes](#game-modes)
  * [Variants](#variants)
  * [Dev Mods](#dev-mods)
* [Installation](#installation)
* [Hacking](#hacking)

# Included Mods

## Game Modes

### Respawn
<p align="center">
  <img src="img/respawn.png?raw=true"/>
</p>


![](img/respawn.gif?raw=true)
![](img/respawn2.gif?raw=true)

Best played with Gunn Style activated, obviously.  
Not shown on the replay gifs: our awesome in-game kill count HUDs!

### Crawl

<p align="center">
 <img src="img/crawl.png?raw=true"/>
</p>

![](img/crawl.gif?raw=true)
![](img/crawl2.gif?raw=true)

*Variants: No Balancing, No Treasure, Start with Toy Arrows*

Inspired by a certain other indie game - kill living players to regain your humanity!  
Unlike in other game modes, you score points for killing enemy ghosts.  
This may be our most ambitious mod yet, and therefore not quite yet balanced. Toy arrows are a good way to nerf living players if you feel the ghosts are too weak.

## Variants

### No Head Bounce ![](Mod/Content/Atlas/menuAtlas/variants/noHeadBounce.png?raw=true)

![](img/noHeadBounce.gif?raw=true)

### No Ledge Grab ![](Mod/Content/Atlas/menuAtlas/variants/noLedgeGrab.png?raw=true)

Koala hunters no more.

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

## Dev Mods

These mods are intended for development or are simply unfinished and only available if you compile Bartizan from source. If enough people want to see one of these included in the official releases, we may flesh them out and include them.

### Keyboard Config for Second Player

Walk/aim with WASD, jump J, shoot K, dash Right Shift

### Slow Time Orb on Back Button

For those perfect-looking quest runs. Only available for Xbox game pads.

### End Round on Center (Steam) Button

Useful for immediately saving a scene to a gif. Only available for Xbox game pads.


Installation
============

* Extract the zip from our [releases](https://github.com/Kha/Bartizan/releases/) according to you platform, then start `Wizard.exe`. This will patch TowerFall.exe and the graphics resources, and save the original files in a new folder `Original`.
* On new TowerFall releases, you'll need to delete the `Original` folder and re-run the Wizard (and possibly need a new release of Bartizan if the update has broken any mods).
* To uninstall, simply reset your TowerFall installation by selecting `Properties -> Local Files -> Verify Integrity of Game Cache` in the Steam context menu for TowerFall.

Hacking
=======

While most of Bartizan was developed using MonoDevelop on Linux, using Visual Studio or any other IDE on Windows should work just as well. We haven't put any time into getting it to work on OS X, and at a cursory glance it looks like while the Patcher works, the generated TowerFall.exe crashes the runtime so bad not even the stacktrace can be displayed, so... OS X support isn't likely to happen.
If you're an OS X / mono wizard and want to take a look at how to fix this, we'd love the help.

* Copy Steam/SteamApps/common/TowerFall to bin/Original (or at least TowerFall.exe and Content/Atlas, to save some copying time)
* Build Bartizan.sln. The AfterBuild targets should do all the dirty work:
  * Using [Mono.Cecil](https://github.com/jbevain/cecil), the base image `BaseTowerFall.exe` is derived from `Original/TowerFall.exe` by marking members of TowerFall classes as public and virtual (where applicable) so that we can use/override them in `Mod.dll` (and `DevMod.dll`, which is ignored by the Wizard and contains the development-only mods).
  * Any members of classes marked as `[Patch]` in `Mod.dll` will be merged back into their respective base class to form the resulting `TowerFall.exe`.
* Copy (or just symlink) the new `TowerFall.exe`, `*Mod.dll` and `Content/` back to the TowerFall Steam directory.

As an aside, due to the rather unusual way we're patching the game, you won't be able to use all of the fancy C# language features in your mods. If you're planning on using obscure features, expect obscure error messages.
