Kerbal Space Program [as posted on Curse](http://www.curse.com/ksp-mods/kerbal/224287-kronal-vessel-viewer-kvv-exploded-ship-view) and featured in [Scott Manley's Video](https://www.youtube.com/watch?v=Y9csr64ghh4)

Check [this forks release page](https://github.com/bigorangemachine/ksp-kronalutils/releases) for a download alternative

Developers who wish to contribute should [branch dev-master](https://github.com/bigorangemachine/ksp-kronalutils/tree/dev-master).

`git clone git@github.com:bigorangemachine/ksp-kronalutils.git`

`cd ksp-kronalutils/`

`git checkout dev-master`

==========================================

### Bigorangemachine's Fork

#### v0.0.4 - Pitch Perfect
* Added 'Auto-Preview' checkbox (for slower computers)
* [HOT FIX] Fixed Bug where parts would not 'Offset' (Formerly Explode View) unless Procedural Fairings was installed
* Background colour sliders (white is no longer the only background colour render option) located under 'Blue Print'
* Blue Print Shader is now disabled by default
  * 'Blue Print shader' was causing the issue with the white rendering lines and off colouring in the bottom left corner
  * Background colour controls are now available under 'Blue Print' which will eventually become 'Background' or 'Canvas'
* UI Adjustments
  * Shadow Control Dial (experimental)
  * Bigger buttons
  * Moved Orthographic Button
  * Changed 'Exploded' references to 'Offset'
  * Image quality can now be controlled with a dial
* [Git deckblad](https://github.com/deckblad) ([KSP forums mrBlaQ](http://forum.kerbalspaceprogram.com/members/102679-mrBlaQ)) resolved:
  * Shadow Rendering Control
  * Adjusted Camera Positioning
  * Improved Camera Controls
  * Part Option for Clamps
  * Procedural fairings bug fixes
    * Existing bug still exists where you must select minimum 4 fairings to hide 'Front Half'
  * Edge Detect shader adjustment
* To Install:
  * Replace all Existing <KSP ROOT>GameData/KronalUtils/ files (.DLL & KronalUtils/edn shader changed but be sure replace everything)
  * No Dependancies
* To Build/Compile:
  * Normal KSP Modding (Build with required KSP DLLs)
  * Download and Build with [KAS.dll](https://github.com/KospY/KAS)
  * Download and Build with [ProceduralFairings.dll](https://github.com/e-dog/ProceduralFairings)

#### v0.0.3 - mrBlaQ
* GUI Window Click trap implmented.  (Thanks [Git M4V](https://github.com/m4v/RCSBuildAid/blob/master/Plugin/GUI/MainWindow.cs#L296) for directing me here)
* [Git deckblad](https://github.com/deckblad) ([KSP forums mrBlaQ](http://forum.kerbalspaceprogram.com/members/102679-mrBlaQ)) resolved:
  * Fixed white lines issue by restricting image size to 4096px (max any dimension)
  * Made all renders Jump Up to 4096px.  This creates higher quality renders with smaller craft.
* Nils Daumann [\(Git Slin\)](https://github.com/Slin/) was kind enough to change the license on the fxaa shader. 
* To Install:
  * Replace all Existing <KSP ROOT>GameData/KronalUtils/ files (.DLL & KronalUtils/fxaa shader changed but be sure replace everything)
  * No Dependancies
* To Build/Compile:
  * Normal KSP Modding (Build with required KSP DLLs)
  * Download and Build with [KAS dll](https://github.com/KospY/KAS)

#### v0.0.2 - Dat-U-Eye

* Change Config Defaults
* Changed button layouts and preview
* [Git deckblad](https://github.com/deckblad) ([KSP forums mrBlaQ](http://forum.kerbalspaceprogram.com/members/102679-mrBlaQ)) added support for [KAS Parts](https://github.com/KospY/KAS)
* To Install:
  * Replace all Existing <KSP ROOT>GameData/KronalUtils/ files (.DLL is only changed file but be sure replace everything)
  * No Dependancies
* To Build/Compile:
  * Normal KSP Modding (Build with required KSP DLLs)
  * Download and Build with [KAS dll](https://github.com/KospY/KAS)

#### v0.0.1 - El Padlina

* Fixed glitch where Save button wouldn't undisable.  Now disables when you click 'Revert' after click 'Explode'
* Commits from [Pull Request 4e2601f](https://github.com/WojtekWZ/ksp-kronalutils/commit/4e2601f071dcb2d573b49d096c2a7c3e0fdf05ae) from [Git WojtekWZ](https://github.com/WojtekWZ) aka [Reddit /u/el_padlina](http://www.reddit.com/user/el_padlina)
  * Added GUI Button
  * New Dials for better control over shaders
* To Install:
  * Everything is new
  * Replace all Existing <KSP ROOT>GameData/KronalUtils/ files
  * No Dependancies


#### v0.0.0 - Revival

* Made 'Stable' with Stock KSP v0.24.2
* Writes to screenshot folder (Windows/OSX confirmed)
* Includes name of Vessel in filename
* To Install: 
  * Everything is new
  * Replace all Existing <KSP ROOT>GameData/KronalUtils/ files
  * No Dependancies


====================


#### Kronal Utils for KSP
------------------

As I haven't been able to continue developing my KSP related stuff,
I'm now releasing it into the public domain using the Unlicense --
no strings attached nor nazti viral licences, do whatever you want with it! :-)

Be warned that this is a dump of my personal utilities, and it's not that
much documented and it's a mixed bag of different things.

The two zip files contain the blender models I created for my parts and the compiled version my stuff (which includes the shaders, DAEs and textures).

In `src\` you find:

#### Axes in the editor
------------------
This is named `KRSEditorAxis` and does what the name says, shows 3 cartesian axes in the vessel editor centered around the center of mass of your ship.

The nice thing about it is that it hides some of the axes depending on your position.

[Here's a YouTube video of it.](https://www.youtube.com/watch?v=fvQ4SPKGc0M)

 
#### ~~Hinges and stepper motors~~
-------------------------

~~`KRSHinge` is a part module that can turn anything into a stepper motor I did on my own for kicks and giggles. The difference of this in regards to competing implementations is that~~

~~1. it is not bound to draconian licensing and~~

~~2. that it can actually snap to angles, and make the joint actively hold a position -- i.e. the joint compensates when you place a heavier object and does not move down.~~

Removed.  This fork will not be supporting these parts

#### ~~Binding keys and analog gamepad controls in the VAB~~
---------------------------------------------------

~~Both `KRSControl` and `KRSInputAttribute` enable using a graphical interface to bind keys and gamepad controls (including analog ones) to my part modules.~~

~~For now this only works with `KRSHinge` but I don't see why it couldn't be made work with other things.~~

~~When I developed this I wanted to make it less intrusive, i.e. not needing to modify your part module to make it compatible with this, but I haven't put much thought on this since so it's the way it is.~~

~~**NOTE**: There is some bug in this that borks the placement of struts, so be warned.~~

Removed.

#### Shader material properties parser
---------------------------------

In `MaterialProperties` you'll find the class `ShaderMaterial` than can be used to read from a shader material the properties you can tweak in it (the same you'd see if you were to open it in Unity3D).

I use this in `KRSVesselShotUI` to allow the user to edit shader properties from the VAB.

#### Vessel screenshot utility
-------------------------

This is contained in `KRSVesselShot`, `KRSVesselShotUI` and `VesselViewConfig`. The intent of this is to be a tool for taking a screenshot that covers all your spacecraft, so that you can show it to other people and so on.

It proves a way to make exploded views, hiding some parts, and use orthographic projection.

`VesselViewConfig` can also useful for other projects that need to hide stuff in the VAB.

Here's how it looks configuring it to use no-color and orthographic projection:

![Screenshot](http://i.imgur.com/aWJVCsz.png)

and here's how it looks with coloring and perspective projection:

![Screenshot2](http://i.imgur.com/ByToBdP.jpg)

That's it!

-- Kronal


#### v0.0.0a

* Made 'Stable' with Stock KSP v0.23.0