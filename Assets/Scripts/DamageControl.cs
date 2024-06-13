using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageControl : MonoBehaviour
{
    public List<Renderer> mesh;
    public List<Color> damageColor;
    int currentColor;



    public void ActiveDamage()
    {
        CancelInvoke(nameof(StopDamage));
        InvokeRepeating(nameof(SetColorDamage), 0, 0.2f);
        Invoke(nameof(StopDamage), 2);
    }

    public void SetColorDamage()
    {
        currentColor++;
        if (currentColor > damageColor.Count)
        {
            currentColor = 0;
        }

        for (int i = 0; i < mesh.Count; i++)
        {
            mesh[i].material.color = damageColor[currentColor];
        }


    }
    public void StopDamage()
    {
        CancelInvoke(nameof(SetColorDamage));
        for (int i = 0; i < mesh.Count; i++)
        {
            mesh[i].material.color = damageColor[0];
        }
    }
}
