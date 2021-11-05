**NOTE: This file is not yet finished and is currently in rework (since the game is being remade into my framework)** 

For contributing new features to the game, please check the project roadmap to see which feature you can develop.

# Guidelines

Please check https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/    
It's possible that the project iself don't follow some of the guideline, if  that the case, report an issue.
(It may also be possible that there are some exceptions, that I'll need to add later here)

- For Unity files (MonoBehaviour based or serialized struct/classes), use a pascal-case naming.
- Static fields (and auto properties) shouldn't be used too much (If you wish to store properties, use configuration components in respective worlds)
- Do not create any MonoBehaviour files, except when used for Presentation/Backend gameobjects. 

# Running/Building this project

- Required Unity version: **2020.1.0a17**
- Require an internet connection for downloading the project packages
- Require Visual Studio C++ Tools to be installed (for 'Burst' package to work.)

## From this repository files:
*If you are already experiented with Unity projects, please continue to the next point.*  
*To be sure to be up to date on P4TLB packages, always update the manifest file from this repository when there is an update.*

This repository offer you an empty project to run *Patapon 4 The Last Battle*.  
1. Download or clone the repository, and then open the project [p4tlb-empty/](p4tlb-empty) in Unity.
2. ???
3. Have fun!

## From scratch:
1. Create an empty Unity project in the version.
2. Download or copy [p4tlb-empty/Packages/manifest.json](p4tlb-empty/Packages/manifest.json) and put it in the project package folders. 
3. Wait for Unity to download the required packages
4. ???
5. Have fun!

# Pull Requests
Pull requests on this repository are only accepted if there are typos.  
Send the pull requests on the correct repositories (see the Issues section.)

# Issues
Report issues into their respective repositories:
- Patapon 4 Core: https://github.com/guerro323/package.patapon.core
- GameBase: https://github.com/StormiumTeam/package.stormiumteam.gamebase
- Networking: https://github.com/StormiumTeam/package.stormiumteam.networking
- Shared: https://github.com/StormiumTeam/package.stormiumteam.shared

If you don't know on which repositories to report the issue, send me a message on Discord (project or DM), or create an issue on this repository, I'll move it later.

### todo...