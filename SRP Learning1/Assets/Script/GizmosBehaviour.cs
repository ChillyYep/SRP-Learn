using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosBehaviour : MonoBehaviour
{
    Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };
    private void OnDrawGizmos()
    {
        var cullingSpheres = CY.Rendering.Shadows.cascadeSpheres;
        int count = 0;
        foreach (var item in cullingSpheres)
        {
            Gizmos.color = colors[(count++) % colors.Length];
            Gizmos.DrawWireSphere(new Vector3(item.x, item.y, item.z), item.w);
        }
    }
}
