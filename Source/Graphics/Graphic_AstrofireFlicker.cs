
using RimWorld;
using UnityEngine;
using Verse;
namespace VanillaGravshipExpanded
{
    public class Graphic_AstrofireFlicker : Graphic_Collection
    {
        private const int BaseTicksPerFrameChange = 15;

        private const float MaxOffset = 0.05f;

        public override Material MatSingle => subGraphics[Rand.Range(0, subGraphics.Length)].MatSingle;

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thingDef == null)
            {
                Vector3 vector = loc;
                Log.ErrorOnce("Fire DrawWorker with null thingDef: " + vector.ToString(), 3427324);
                return;
            }
            if (subGraphics == null)
            {
                Log.ErrorOnce("Graphic_Flicker has no subgraphics " + thingDef?.ToString(), 358773632);
                return;
            }
            int num = Find.TickManager.TicksGame;
            if (thing != null)
            {
                num += Mathf.Abs(thing.thingIDNumber ^ 0x80FD52);
            }
            int num2 = num / 15;
            int num3 = Mathf.Abs(num2 ^ ((thing?.thingIDNumber ?? 0) * 391)) % subGraphics.Length;
            float num4 = 1f;
           
            Astrofire fire = thing as Astrofire;
          
            if (fire != null)
            {
                num4 = fire.fireSize;
            }
       
            if (num3 < 0 || num3 >= subGraphics.Length)
            {
                Log.ErrorOnce("Fire drawing out of range: " + num3.ToString(), 7453435);
                num3 = 0;
            }
            Graphic graphic = subGraphics[num3];
           
            Vector3 a = GenRadial.RadialPattern[num2 % GenRadial.RadialPattern.Length].ToVector3() / GenRadial.MaxRadialPatternRadius;
            a *= 0.05f;
            Vector3 pos = loc + a * num4;
            if (thing?.Graphic?.data != null)
            {
                pos += thing.Graphic.data.DrawOffsetForRot(rot);
            }
          
            Vector3 s = new Vector3(num4, 1f, num4);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, graphic.MatSingle, 0);
        }

        public override string ToString()
        {
            return "Flicker(subGraphic[0]=" + subGraphics[0]?.ToString() + ", count=" + subGraphics.Length.ToString() + ")";
        }
    }
}