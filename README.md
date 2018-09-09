# Better Pawn Textures

## Usage
Use this `.dll` to add custom texture/color pools to animals. See included files for example usage, copy the Assemblies folder to your mod to make it work!

### Add Custom Textures
##### Usage
```xml
<Patch>
    <Operation Class="PatchOperationAdd">
        <xpath>/Defs/PawnKindDef[defName="<!-- Animal name here -->"]/lifeStages/li/bodyGraphicData</xpath>
        <value>
            <graphicClass>Better_Pawn_Textures.Graphic</graphicClass>
        </value>
    </Operation>
</Patch>
```

IMPORTANT: Add `AnimalName_south`, `AnimalName_east`, and `AnimalName_north` versions to `Textures/Pawn/Animal/AnimalName/`

##### Example
```xml 
<Patch>
    <Operation Class="PatchOperationAdd">
        <xpath>/Defs/PawnKindDef[defName="Cat"]/lifeStages/li/bodyGraphicData</xpath>
        <value>
            <graphicClass>Better_Pawn_Textures.Graphic</graphicClass>
        </value>
    </Operation>
</Patch>
```

#### Add Custom Colors
##### Usage
```xml 
<Patch>
    <Operation Class="PatchOperationAddModExtension">
        <xpath>/Defs/PawnKindDef[defName="<!-- Animal name here -->"]</xpath>
        <value>
            <li Class="Better_Pawn_Textures.BPTModExtension">
                <colors>
                    <li><!-- Color1 --></li>
                    <li><!-- Color2 --></li>
                    <!-- ... -->
                    <li><!-- ColorN --></li>
                </colors>
                <shaderType><!-- Shader type from ShaderDatabase --></shaderType>
            </li>
        </value>
    </Operation>
</Patch>
```

##### Example
```xml 
<Patch>
    <Operation Class="PatchOperationAddModExtension">
        <xpath>/Defs/PawnKindDef[defName="Cat"]</xpath>
        <value>
            <li Class="Better_Pawn_Textures.BPTModExtension">
                <colors>
                    <li>(66,134,244)</li>
                    <li>(182,66,244)</li>
                    <li>(244,238,66)</li>
                </colors>
                <shaderType>CutoutSkin</shaderType>
            </li>
        </value>
    </Operation>
</Patch>
```

##### Combined Example
```xml 
<?xml version="1.0" encoding="utf-8" ?>
<Patch>
    <Operation Class="PatchOperationAdd">
        <xpath>/Defs/PawnKindDef[defName="Cat"]/lifeStages/li/bodyGraphicData</xpath>
        <value>
            <graphicClass>Better_Pawn_Textures.Graphic</graphicClass>
        </value>
    </Operation>
    
    <Operation Class="PatchOperationAddModExtension">
        <xpath>/Defs/PawnKindDef[defName="Cat"]</xpath>
        <value>
            <li Class="Better_Pawn_Textures.BPTModExtension">
                <colors>
                    <li>(66,134,244)</li>
                    <li>(182,66,244)</li>
                    <li>(244,238,66)</li>
                </colors>
                <shaderType>CutoutSkin</shaderType>
            </li>
        </value>
    </Operation>
</Patch>
```
