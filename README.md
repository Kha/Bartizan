TowerClimb-Descension
=====================

A mod framework for [TowerFall Ascension](http://www.towerfall-game.com/) (copyright 2013 Matt Thorson, obviously).

Hacking
=======

* Copy your TowerFall.exe to bin/OrigTowerFall.exe
* Compile Patcher and run `./Patcher.exe makeBaseImage` in bin/  
  This will create a BaseTowerFall.exe where all classes are unsealed, all fields public and all methods virtual public, so you can meaningfully derive from any TowerFall class.
* Now you can compile Mod, which depends on BaseTowerFall.exe
* Run `./Patcher.exe patchBaseImage` to replace all object creations in BaseTowerFall.exe with the respective class in Mod.dll, if any
* Copy (or just symlink) the resulting TowerFall.exe and Mod.dll back to the TowerFall Steam directory.

Whenever you have added or removed derived classes to/from Mod, re-run patchBaseImage.
