# DbPatcher

Simple but probably overly complicated program that replaces bytecode in Frostbite ShaderDb's

## Usage
DbPatcher [Path to original bytecode] [Path to replacement bytecode] [Path to ShaderDb resource]

Output is fixed to PatchedShaderDb.res in the same directory as the original db.
This is a simple batch script to mass patch a db.
```
setlocal EnableDelayedExpansion

set DB="shaderdb.res"
for %%f in ("cso/*") do (
  DbPatcher.exe "original/%%f" "cso/%%f" !DB!
  set DB="PatchedShaderDb.res"
)
```
And this is another script to batch compile hlsl pixel shaders using fxc
```
for %%f in ("hlsl/*") do fxc_x86.exe -T ps_5_0 /E main "hlsl/%%f" /Fo "cso/%%~nf.cso"
```
Frostbite(at least the version in SWBF2015 which all this was tested with) uses version `6.3.9600.16384` of the compiler, so i'd reccomend tracking that down if you wanna compile a shader properly.


## Credits
NativeReader and NativeWriter classes from [FrostyToolSuite](https://github.com/CadeEvs/FrostyToolsuite) 

[@Brawltendo](https://github.com/Brawltendo) for assistance with ShaderDb stuff as well as creating the [ShaderDataPlugin](https://github.com/Brawltendo/FrostyShaderDataPlugin) which makes this more easily possible with it's abilty to export the bytecode

[@NatalieWhatever](https://github.com/NatalieWhatever) for helping me write this, i couldn't have done it without her <3
