using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ApplyFoliage))]
public class ApplyFoliageInspector : Editor
{
    public override void OnInspectorGUI()
    {
        FoliagePanel.RefreshPanel();

        base.OnInspectorGUI(); //show normal inspector
    }

    public void OnDisable()
    {
        try
        {
            ApplyFoliage item = (ApplyFoliage)target;
            if (null == item)
            {
                //Debug.LogError("Selection changed from ApplyFoliage");
                FoliagePanel.RefreshPanel();
                return;
            }

            if (FoliagePanel.m_focusFoliage == item)
            {
                FoliagePanel.m_focusFoliage = null;
                FoliagePanel.DeinitializeBrushOverlay(item);
            }

            FoliagePanel.RefreshPanel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("Inspector exception={0}", ex));
        }
    }
}