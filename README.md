
<html>
    <p align="center">
    <img src="https://pre00.deviantart.net/4960/th/pre/i/2017/334/1/6/_patapon_4_tlb__p4_logo_variant_2_by_guerro323-dbvceq0.png" alt="Super Logo!" width="64" height="64" />
    </p>
    <h2 align="center">
    Patapon 4 - The Last Battle
    </h2>
</html>

___
### Packages:
This game (also Stormium) use a package system, for a better code maintenance.

-   [Core package](GameClient/Packages/pack.p4.core)
-   [Default package](GameClient/Packages/pack.p4.default)
-   [Shared package](GameClient/Packages/pack.guerro.shared)
___
### Modding:
-   Create characters:
    -   Implement a custom character for a mission
    -   Implement a custom playable character

___

### Game folders (exemple with my two games):
```
/Projects/
├── Common/
│   ├── Packages/
    │   ├── package.guerro.shared/
    │   ├── package.stormium.core/
    │   ├── package.patapon.core/

├── STORMIUM/
│   ├── <ProjectStormiumUnity>/
    │   ├── StormiumGameClient/
        │   ├── Packages/
            │   ├── manifest.json
│   ├── (related files and folders...)

├── Patapon/
│   ├── <ProjectPatapon4>/
    │   ├── Patapon4GameClient/
        │   ├── Packages/
            │   ├── manifest.json
│   ├── (related files and folders...)
```

`manifest.json`
```json
{
    "dependencies":
    {
        "package.guerro.shared": "file:../../../../Common/Packages/package.guerro.shared",
        "package.<game>.core": "file:../../../../Common/Packages/package.<game>.core",
        "other.packages": "..."
    }
}
```

### Game structure:
```
/<Game>

-- Private access
├── Internal/
│   ├── Packages/
    │   ├── <package.Game.core>/
    │   ├── package.guerro.shared/ -- Should this package be put into the game core instead?
│   ├── Assets/
    │   ├── <GameAssets>
    │   ├── StreamingAssets/
        │   ├── Mods/
            │   ├── <Mod1>/
                │   ├── Config/
                │   ├── Inputs/
                │   ├── ...

-- Public access
├── WorkSpace/
│   ├── Mods/
    │   ├── <Mod1>/
        -- Will override the configs from streamingassets folder
        │   ├── Config/
        │   ├── Inputs
        │   ├── ...
```
