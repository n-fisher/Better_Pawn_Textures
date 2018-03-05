using System.Collections.Generic;
using Verse;
using Harmony;
using System.Reflection;
using System;
using UnityEngine;
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
                    __instance.ClearCache();
                    Pawn pawn = __instance.pawn;
                    PawnKindLifeStage curKindLifeStage = __instance.pawn.ageTracker.CurKindLifeStage;

                    Debug.Log(curKindLifeStage.ToStringSafe());

                    Graphic baseGraphic = new Graphic(((pawn.gender == Gender.Female) ?
                        curKindLifeStage.femaleGraphicData?.Graphic ?? curKindLifeStage.bodyGraphicData.Graphic :
                        curKindLifeStage.bodyGraphicData.Graphic));

                    Debug.Log(baseGraphic.ToStringSafe());
                    if (baseGraphic == null) {
                        return true;
                    }

                    int availableTextureCount = Graphic.ModTextureCount(baseGraphic.path);
                    string safeTextureIndex;
                    if (availableTextureCount > 0) {
                        int textureIndex = (new System.Random(pawn.thingIDNumber * 2)).Next() % availableTextureCount;
                        safeTextureIndex = (1 + textureIndex).ToString();
                    } else {
                        safeTextureIndex = "";
                    }
                    int colorIndex = (new System.Random(pawn.thingIDNumber)).Next() % extension.colors.Count;
                    Color color = extension.colors[colorIndex];

                    //may spawn packs all default colors
                    if (pawn.RaceProps.packAnimal) {
                        __instance.packGraphic = GraphicDatabase.Get<Graphic_Multi>(Graphic.PathBase(baseGraphic.path) + "Pack", ShaderDatabase.Cutout, baseGraphic.drawSize, color);
                    }
                    Debug.Log(__instance.packGraphic.ToStringSafe());

                    if (curKindLifeStage.dessicatedBodyGraphicData is GraphicData dessicated) {
                        __instance.dessicatedGraphic = dessicated.GraphicColoredFor(__instance.pawn);
                    }
                    Debug.Log(__instance.dessicatedGraphic.ToStringSafe());

                    __instance.nakedGraphic = baseGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, color, safeTextureIndex);
                    Debug.Log(__instance.nakedGraphic.ToStringSafe());

                    return false;
                }
            }
            return true;
        }
    }

    public class Graphic : Graphic_Multi
    {
        public Graphic(Verse.Graphic graphic)
        {
            path = graphic.path;
            drawSize = graphic.drawSize;
            data = graphic.data;
            color = graphic.color;
            colorTwo = graphic.colorTwo;

        }

        public Graphic() : base()
        {
        }

        public static int ModTextureCount(string path)
        {
            // find all assets with custom number tags
            int count = 0;

            UnityEngine.Object o = ContentFinder<Texture2D>.Get(path + (count + 1) + "_back", false);

            while (o != null) {
                count++;
                o = ContentFinder<Texture2D>.Get(path + (count + 1) + "_back", false);
            }

            return count;
        }

        private string RandomPath(int thingid)
        {
            string pathBase = PathBase(path);

            if (path != pathBase) {
                Debug.Log("Trying to get a random path when a path is already specified.");
                return path;
            }

            int textureIndex = ModTextureCount(path);

            Debug.Log("Found " + textureIndex + " custom textures.");

            textureIndex = (thingid % textureIndex) + 1;

            Debug.Log("Chose texture " + textureIndex);

            pathBase += textureIndex;

            return pathBase;
        }

        internal static string PathBase(string req)
        {
            try {
                while (true) {
                    int x = int.Parse(req.Substring(req.Length - 1));
                    // fail on already having a tex number
                    Debug.Log("Trimming " + x);
                    req = req.Substring(0, req.Length - 1);
                }
            } catch (FormatException) {
                // not a numbered texpath, go ahead with execution
            };

            return req;
        }

        public Verse.Graphic GetColoredVersion(Shader newShader, Color newColor, string textureIndex)
        {
            Debug.Log("Path hopefully does not have a number: " + path);
            path = PathBase(path);
            Debug.Log("Path should not have a number: " + path);
            path += textureIndex;
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