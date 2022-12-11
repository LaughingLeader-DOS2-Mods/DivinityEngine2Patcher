# The Divinity Engine 2 Patcher

This is both a patch injector (DxgiNext.dll), and a collection of patches/fixes (LeaderTweaks.Patch.dll).

## Requirements

Requires [Norbyte's Script Extender](https://github.com/Norbyte/ositools/releases/latest).

## Installation

[Check the releases section for an installation guide.](https://github.com/LaughingLeader-DOS2-Mods/DivinityEngine2Patcher/releases/latest)

## Fixes/Changes

### Disable Save Loading  

The editor attempts to instantiate modules stored in all of the active profile's saves. This can increase loading times, and previously the workaround was to move  all of your saves out of your profile folder. This functionality is now disabled by default, but it can be re-enabled in `Patches/settings.toml`.

### Add Resource Wizard (Content Browser)

* All resources will now display all supported file types by default (so models will default to showing .gr2 and .lsm types, instead of just .lsm).
* Fixed the starting directory being incorrect (models will now start the window in Assets/Models, effects Assets/Effects/Effects_Bank/, etc).
* Newly added resources will be named Name_GUID.lsf. This is to make them easier to find when looking to manually edit or find files.

### Animation Preview Window Tweaks

* Fixed a null exception, preventing you from exiting the window, if the window's animation resource was null.
* Fixed the window assuming animation changes have taken place when the animation resource is null (i.e. it asking you to save changes that don't exist).
* Made the window stop always being on top of everything when it loses focus.
* If no preview visual is set, the window will now load the default human male proxymesh visual, allowing you to always see the timeline / animation duration.
* Textkeys can now always be added to animations.

### Message Tweaks

* Messages now have varying colors, instead of all being red, yellow, or white.
* Osiris debug text displays as [Osiris] now, instead of "Osiris triggered an assert".
* Various warning spam messages no longer show up (like "filename x does not exist, can't load mod!").
* Additional messages can be ignored by editing the settings file (Patches/settings.toml - launch the editor with the patcher enabled to generate this).

### Panel Tweaks

* The "Eyes of a Child" game window is now renamed to simply "Game", taking up significantly less tab space. This name can be changed in the settings.
* Incorporated Norbyte's fixes, including:
  * Fixed the Wall Construction Wizard.
  * Fixed the "Create Prefab" option in the context menu.
  * Fixed the "Export Selection to Root Template" option in the context menu.
  * Added a "Tile Set" button to the Tile Set Editor dialog.

### Project Tweaks

* The Mods/ModName_UUID/meta.lsx data will now be reloaded when opening the Project Settings and Publish windows. This fixes an issue where the editor would ignore any changes you may have made to it externally (like version changes).

### Resources

* A resource's filename will be preserved when editing/saving changes. Previously the editor would always save it as GUID.lsf, regardless of what it was already named.
* Physics resources will no longer crash the editor when previewing them without a level loaded (they simply won't display a preview).
* Local resources to project will no longer incorrectly be marked as "Inherited", which made them undeletable.
* Fixed a null reference exception when deleting visual/animation resources.

### Root Templates Panel

This panel will now load after a project is loaded, allowing you to make changes without needing to load a level.

### Clipboard Actions  

All the various "Copy GUID to clipboard", "Copy <Name> GUID to clipboard" etc actions have been tweaked for consistency.

When copying a single entry, the editor would add an extra line break (this may have been dependent on which UI was doing the copying). This has been fixed.

Additionally `<Name> GUID` is now copied as `Name_GUID`, and `Type Name_GUID` is now copied as `Type_Name_GUID`.

## Settings

`Patches/settings.toml` will be generated when the patcher runs, if it doesn't already exist. In this file you can disable specific patchers, and tweak some additional options, such as adding more message log text patterns to ignore.
