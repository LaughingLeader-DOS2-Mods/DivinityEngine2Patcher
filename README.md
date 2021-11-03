# The Divinity Engine 2 Patcher

This is both a patch injector, and a collection of patches/fixes (LeaderTweaks.Patch.dll).

## Leader Tweaks Fixes/Changes

### Add Resource Wizard (Content Browser)

* All resources will now display all supported file types by default (so models will default to showing .gr2 and .lsm types, instead of just .lsm).
* Fixed the starting directory being incorrect (models will now start the window in Assets/Models, effects Assets/Effects/Effects_Bank/, etc).
* Newly added resources will be named Name_GUID.lsf. This is to make them easier to find when looking to manually edit or find files.

### Animation Preview Window Tweaks

* Fixed a null exception, preventing you from exiting the window, if the window's animation resource was null.
* Fixed the window assuming animation changes have taken place when the animation resource is null (i.e. it asking you to save changes that don't exist).
* Made the window stop always being on top of everything when it loses focus.
* If no preview visual is set, the window will now load the default human male proxymesh visual, allowing you to always see the timeline / animation duration.

### Message Tweaks

* Messages now have varying colors, instead of all being red, yellow, or white.
* Osiris debug text displays as [Osiris] now, instead of "Osiris triggered an assert".
* Various warning spam messages no longer show up (like "filename x does not exist, can't load mod!").

### Panel Tweaks

The "Eyes of a Child" game window is now renamed to simply "Game", taking up significantly less tab space.

### Project Tweaks

* The Mods/ModName_UUID/meta.lsx data will now be reloaded when opening the Project Settings and Publish windows. This fixes an issue where the editor would ignore any changes you may have made to it externally (like version changes).

### Resources

* A resource's filename will be preserved when editing/saving changes. Previously the editor would always save it as GUID.lsf, regardless of what it was already named.
* Physics resources will no longer crash the editor when previewing them without a level loaded (they simply won't display a preview).
* Local resources to project will no longer incorrectly be marked as "Inherited", which made them undeletable.
* Fixed a null reference exception when deleting visual/animation resources.

### Root Templates Panel

This panel will now load after a project is loaded, allowing you to make changes without needing to load a level.



