using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/OutlineEx", 15)]
public class OutlineEx : Shadow
{
    static float scale = Mathf.Sin(45 * Mathf.PI / 180);
    protected OutlineEx()
    { }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        var verts = ListPool<UIVertex>.Get();
        vh.GetUIVertexStream(verts);

        var neededCpacity = verts.Count * 5;
        if (verts.Capacity < neededCpacity)
            verts.Capacity = neededCpacity;

        var start = 0;
        var end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x * scale, effectDistance.y * scale);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x * scale, -effectDistance.y * scale);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x * scale, effectDistance.y * scale);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x * scale, -effectDistance.y * scale);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, 0);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, 0);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, effectDistance.y);

        start = end;
        end = verts.Count;
        ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, 0, -effectDistance.y);

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
        ListPool<UIVertex>.Release(verts);
    }
}
