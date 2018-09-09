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
                bool isCustom = false;
                Pawn pawn = __instance.pawn;
                PawnKindLifeStage curKindLifeStage = __instance.pawn.ageTracker.CurKindLifeStage;

                Graphic baseGraphic = new Graphic(((pawn.gender == Gender.Female) ?
                    curKindLifeStage.femaleGraphicData?.Graphic ?? curKindLifeStage.bodyGraphicData.Graphic :
                    curKindLifeStage.bodyGraphicData.Graphic));
                
                if (baseGraphic is null) {
                    Debug.Log("Basegraphic for " + pawn.ThingID + " null");
                    return true;
                }
                Color color = baseGraphic.color;
                /*if (baseGraphic.Shader.name != ShaderType.Cutout.ToString()) {
                    Log.Error("boutta override " + pawn.Label + "'s " + baseGraphic.Shader.name + " with cutoutskin :(");
                }*/
                Shader shader = ShaderDatabase.Cutout;
                //has custom colors
                Debug.Log("Checking custom color " + pawn.Label);

                if (__instance.pawn.kindDef.GetModExtension<BPTModExtension>() is BPTModExtension extension) {
                    isCustom = true;
                    int colorIndex = (new System.Random(pawn.thingIDNumber)).Next() % extension.colors.Count;
                    color = extension.colors[colorIndex];
                    FieldInfo fieldInfo = typeof(ShaderDatabase).GetField(extension.shaderType, BindingFlags.Static | BindingFlags.Public);
                    shader = (Shader)fieldInfo.GetValue(null);
                    Debug.Log("chose special color " + color + " with shader " + shader);
                    Debug.Log(typeof(ShaderDatabase).GetFields(BindingFlags.Static | BindingFlags.Public).ToStringSafeEnumerable());
                }
                else {
                    Debug.Log(__instance.pawn.kindDef.GetModExtension<BPTModExtension>().ToStringSafe());
                }

                string safeTextureIndex = "";
                //has custom texture
                Debug.Log("Checking custom texture " + pawn.Label);
                if (__instance.pawn.ageTracker.CurKindLifeStage.bodyGraphicData.graphicClass == typeof(Graphic)) {
                    isCustom = true;
                    int availableTextureCount = Graphic.ModTextureCount(baseGraphic.path);
                    if (availableTextureCount > 0) {
                        int textureIndex = (new System.Random(pawn.thingIDNumber * 2)).Next() % availableTextureCount;
                        safeTextureIndex = (1 + textureIndex).ToString();
                    }
                    Debug.Log("chose texture " + safeTextureIndex);
                }


                if (isCustom) {
                    __instance.ClearCache();

                    if (curKindLifeStage.dessicatedBodyGraphicData is GraphicData dessicated) {
                        __instance.dessicatedGraphic = dessicated.GraphicColoredFor(__instance.pawn);
                    }
                    
                    //may spawn packs all default colors
                    if (pawn.RaceProps.packAnimal) {
                        Debug.Log("pack animal with shader:" + shader.ToStringSafe() + color.ToStringSafe());
                        __instance.packGraphic = GraphicDatabase.Get<Graphic_Multi>(Graphic.PathBase(baseGraphic.path) + "Pack", shader, baseGraphic.drawSize, color);
                    }
                    __instance.nakedGraphic = (new Graphic(baseGraphic)).GetColoredVersion(shader, color, safeTextureIndex);
                    Debug.Log("Resolved " + __instance.nakedGraphic);
                    return false;
                }

                return true;
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

            UnityEngine.Object o = ContentFinder<Texture2D>.Get(path + (count + 1) + "_north", false);

            while (o != null) {
                count++;
                o = ContentFinder<Texture2D>.Get(path + (count + 1) + "_north", false);
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
        public readonly string shaderType;
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