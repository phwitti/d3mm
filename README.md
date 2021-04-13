# d3mm
mod.io Mod-Manager for Desperados III (Level Files). Every time something is called 'mod' here it refers to the modified level-files that Desperados III supports. Installing Level files always had this process of searching online, downloading and unpacking to the correct directory, which has been super tedious and one reason I could never bring myself to trying them out -- though this process brought myself to writing this mod-manager...
![The d3mm-Application features a classic list-view with check-boxes for installing level-files](https://github.com/phwitti/d3mm/blob/main/images/v1.0.0.png?raw=true)

## Features
List / Sort / Install / Uninstall Mods from mod.io.

## Installation
Builds for windows can be downloaded on the releases page (built with Visual Studio 2017) -- winforms is also quite well supported in mono on linux. You can also build from source with d3mm.vs2017.csproj (VS2017) or d3mm.csproj using .net5 (though Object-List-View is not supported on .net5 so you might have various bugs and problems using that).

## Configuration
Next to the executable lies a d3mm.config.json file where the 'Desperados III' save-folder is set, the .d3mm-Folder containing the mod-managers data is placed underneath that. In the .d3mm-Folder there is the main-configuration-file (config.json), containing all other (more dynamic) properties; Most relevant are likely UserDirectory and Executable -- both are attempted to be auto-configured for steam on windows, though might be wrong or not set at all for Epic-Games-Store or gog or other operating systems.

## Modification
The code is hopefully quite readable and open for modifications. A short breakdown:
- ApplicationProperties.cs: Loading and saving the config.json; containing all configurable properties.
- D3ModDatabase.cs: The database that acts on the .d3mm folder, downloading preview-images and mod-files as well as extracting or deleting them on install / uninstall. Contains the collection of D3ModInfo-objects.
- D3ModInfo.cs: For every mod there is an info-object created containing all data for this mod.
- D3ModManager.cs: The class of the main-window; mainly initialization and ui-events.
- D3ModManager.Designer.cs: Nothing in there except the pre-generated stuff.
- Programm.cs: Initializing application properties (settings) and opening the main-window.

## Changelog
### [1.0.0] - 2021-04-13
#### Added
- List View containing all mods from mod.io for Desperados III
- Web-Browser viewing the web-page for the selected mod.
- Install / Uninstall of mod-files.
- Start Desperados III from inside the application.

## Contribute / Features that would be nice
#### [1.1.0]
- respect fields:
  - status
  - visible
  - maturity
  - filehash
  - virus_status
  - virus_positive
- add field to D3ModInfo: online (bool), to track and remove mods, that are no longer online, after uninstall
- more fancy (and condensed) view (title at vertical top, description underneith)
#### [1.2.0]
- Buttons for Tags
- Auto-Updating mods (option)
- Super Special Handling for vader1's Ingame-Modding-Tool/-Editor
