TowerClimb-Descension
=====================

A mod framework for [TowerFall Ascension](http://www.towerfall-game.com/) (copyright 2013 Matt Thorson, obviously).

Hacking
=======

* Copy Steam/SteamApps/common/TowerFall to bin/Original (or at least TowerFall.exe and Content/Atlas, to save some copying time)
* Compile Patcher and run `./Patcher.exe makeBaseImage` in bin/  
  This will create a BaseTowerFall.exe where all classes are unsealed, all fields public and all methods virtual public, so you can meaningfully derive from any TowerFall class.
* Now you can compile Mod, which depends on BaseTowerFall.exe
* Run `./Patcher.exe patch` to inline all classes from Mod.dll marked as [Patch] into their respective base class from Original/TowerFall.exe.
* Copy (or just symlink) the resulting TowerFall.exe and Content/ back to the TowerFall Steam directory.
