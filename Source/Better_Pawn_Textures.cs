using System.Collections.Generic;
using Verse;
using Harmony;
using System.Reflection;
using System;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

namespace Better_Pawn_Textures
{
    public class Better_Pawn_Textures : Mod
    {
        public Better_Pawn_Textures(ModContentPack content) : base(content)
        {
            var harmony = HarmonyInstance.Create("com.fyarn.better_pawn_textures");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public class PawnGraphicSetPatch
    {
        public static bool Prefix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.RaceProps.Animal) {
                if (__instance.pawn.kindDef.GetModExtension<BPTModExtension>() is BPTModExtension extension) {
                    Pawn pawn = __instance.pawn;
                    PawnKindLifeStage curKindLifeStage = __instance.pawn.ageTracker.CurKindLifeStage;

                    Color color = extension.colors.RandomElement();
                    Graphic baseGraphic = (Graphic) ((pawn.gender == Gender.Female) ?
                        curKindLifeStage.femaleGraphicData?.Graphic ?? curKindLifeStage.bodyGraphicData.Graphic :
                        curKindLifeStage.bodyGraphicData.Graphic);

                    //may spawn packs all default colors
                    if (pawn.RaceProps.packAnimal) {
                        __instance.packGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.nakedGraphic.path + "Pack", ShaderDatabase.Cutout, __instance.nakedGraphic.drawSize, color);
                    }

                    if (curKindLifeStage.dessicatedBodyGraphicData is GraphicData dessicated) {
                        __instance.dessicatedGraphic = dessicated.GraphicColoredFor(__instance.pawn);
                    }
                    
                    __instance.nakedGraphic = baseGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, color, Color.white);
                    
                    return false;
                }
            }
            return true;
        }
    }

    public class Graphic : Graphic_Multi
    {
        private string RandomPath()
        {
            string pathBase = PathBase();

            if (path != pathBase) {
                Debug.Log("Trying to get a random path when a path is already specified.");
                return path;
            }

            // find all assets with custom number tags
            int count = 0;

            UnityEngine.Object o = ContentFinder<Texture2D>.Get(pathBase + (count + 1) + "_back", false);
            
            while (o != null) {
                count++;
                o = ContentFinder<Texture2D>.Get(pathBase + (count + 1) + "_back", false);
            }

            Debug.Log("Found " + count + " custom textures.");
            // if any assets have been found choose one randomly, otherwise 
            if (count > 0) {
                count = UnityEngine.Random.Range(1, count + 1);
                pathBase += count;
            }

            Debug.Log("Chose texture " + count);

            return pathBase;
        }
        
        private string PathBase()
        {
            string req = path;
            try {
                while (true) {
                    int x = int.Parse(req.Substring(path.Length - 1));
                    // fail on already having a tex number
                    Debug.Log("Trimming " + x);
                    req = req.Substring(0, req.Length - 1);
                }
            } catch (FormatException) {
                // not a numbered texpath, go ahead with execution
            };

            return req;
        }

        public override Verse.Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            Debug.Log("Path hopefully does not have a number: " + path);
            path = PathBase();
            Debug.Log("Path should not have a number: " + path);
            path = RandomPath();
            return GraphicDatabase.Get<Graphic_Multi>(path, newShader, drawSize, newColor, Color.white, data);
        }
    }

    public class BPTModExtension : DefModExtension
    {
        public readonly List<Color> colors;
    }

    internal class Debug
    {
        [Conditional("DEBUG")]
        internal static void Log(string s)
        {
            Verse.Log.Message(s);
        }
    }
}