
<html>
    <p align="center">
    <img src="wiki_resources/patanext_logo_banner.png" alt="PataNext Logo!" width="320" height="155" />
    </p>
    <h2 align="center">
    PataNext
    </h2>
</html>

___
### Welcome!
PataNext (also knew as Patapon 4 The Last Battle or P4TLB/TLB) is a community based project based on the [PATAPON](https://en.wikipedia.org/wiki/Patapon) series originally made by Rolito and Pyramid.

___
### Framework:
This game use the framework 'revghost' [(link)](https://github.com/guerro323/revecs) and the ECS library 'revecs' [(link)](https://github.com/guerro323/revecs)

___
### Developping:
**Requirements:**
- .NET 6 SDK
- git

**Project Steps:**
- First time:
  1. Clone the repository
  2. Execute `dotnet cake prebuild.cake` in the terminal at the main folder *(aka where the README is)*
  3. You can build or either run the project.
- On each dependencies (eg: revghost update) change:
  1. Remove the appropriate packages in your local nuget folder.
     1. You can find the local packages folder by typing `dotnet nuget locals global-packages -l` in the terminal.
     2. Delete the appropriate folder (eg: if revghost need an update, delete the revghost folder)
  2. Execute 'dotnet cake prebuild.cake' in the terminal at the main folder.
  3. You can build or either run the project with the changes.

**Godot Client:**
1. In the main folder run `godot Godot/project/project.godot`
2. This should launch Godot with the respective project.
3. Click on the start icon in Godot to start the project.
4. (FOR NOW: When started, you'll be greeted by the beautiful `icon.png` bouncing around)

<html>
    <p align="center">
    <img src="wiki_resources/logo.png" alt="Super Logo!" width="64" height="79" />
    </p>
</html>
