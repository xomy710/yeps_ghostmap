using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FoliagePanel : EditorWindow
{
    #region Menu Interaction

    static FoliagePanel m_instance = null;
    public static FoliagePanel GetPanel()
    {
        if (null == m_instance)
        {
            m_instance = EditorWindow.GetWindow<FoliagePanel>(false, "Foliage Panel", false);
        }
        return m_instance;
    }

    public static void RefreshPanel()
    {
        FoliagePanel panel = GetPanel();
        panel.Repaint();
        SceneView.RepaintAll();
    }

    [MenuItem("Window/Open Ecosystem Panel")]
    public static void OpenEditor()
    {
        GetPanel();
    }

    const string FOLIAGE_ITEM_EXTENSION = ".FBX";

    const string FOLIAGE_ITEM_PATH = "Assets/Ecosystem Generator/FoliageItems/Prefabs";

    static List<string> m_foliageItems = new List<string>();

    static List<string> GetFoliageItems()
    {
        if (m_foliageItems.Count == 0)
        {
            ReloadFoliageItems();
        }
        return m_foliageItems;
    }

    static List<string> ReloadFoliageItems()
    {
        m_foliageItems.Clear();
        try
        {
            DirectoryInfo di = new DirectoryInfo(FOLIAGE_ITEM_PATH);
            if (!di.Exists)
            {
                Debug.LogError(string.Format("Foliage item folder does not exist={0}", FOLIAGE_ITEM_PATH));
                return m_foliageItems;
            }

            FileInfo[] files = di.GetFiles();
            if (null == files)
            {
                Debug.LogError(string.Format("Cannot find foliage items={0}", FOLIAGE_ITEM_PATH));
                return m_foliageItems;
            }

            foreach (FileInfo fi in files)
            {
                if (null == fi ||
                    string.IsNullOrEmpty(fi.Name) ||
                    string.IsNullOrEmpty(fi.Extension) ||
                    !fi.Extension.ToUpper().Equals(FOLIAGE_ITEM_EXTENSION))
                {
                    continue;
                }
                m_foliageItems.Add(fi.Name);
            }

            return m_foliageItems;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("Failed to find foliage items exception={0}", ex));
            return m_foliageItems;
        }
    }

    [MenuItem("GameObject/Ecosystem/Camera Top Down")]
    public static void AimCamera()
    {
        if (null == Camera.main)
        {
            return;
        }
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);
        Camera.main.transform.position = new Vector3(0, 10, 0);
    }

    [MenuItem("GameObject/Ecosystem/Add Foliage Layer")]
    public static void AddFoliageLayer()
    {
        GameObject go = Selection.activeGameObject;
        if (null == go)
        {
            return;
        }

        List<string> foliageItems = GetFoliageItems();
        if (foliageItems.Count > 0)
        {
            ApplyFoliage item;
            if (AddFoliageLayer(go, foliageItems[0], out item))
            {
                m_selectedItem = item;
                Regenerate(item);
                RefreshPanel();
            }
        }
    }

    [MenuItem("GameObject/Ecosystem/Add Sample Layers (6)")]
    public static void AddSampleLayers()
    {
        GameObject go = Selection.activeGameObject;
        if (null == go)
        {
            return;
        }

        List<string> foliageItems = GetFoliageItems();
        foreach (string name in foliageItems)
        {
            ApplyFoliage item;
            if (AddFoliageLayer(go, name, out item))
            {
                m_selectedItem = item;
                Regenerate(item);
            }
        }
        RefreshPanel();
    }

    [MenuItem("GameObject/Ecosystem/Add Foliage Layer", validate = true)]
    [MenuItem("GameObject/Ecosystem/Add Sample Layers (6)", validate = true)]
    public static bool ValidateAddFoliageLayer()
    {
        GameObject go = Selection.activeGameObject;
        if (null == go)
        {
            return false;
        }

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (null == mr)
        {
            return false;
        }

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (null == mf)
        {
            return false;
        }
        
        return true;
    }
    
    #endregion

    /// <summary>
    /// The selected foliage item in the panel
    /// </summary>
    static ApplyFoliage m_selectedItem = null;

    #region Helper methods

    /// <summary>
    /// Add the named foliage layer
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    static bool AddFoliageLayer(GameObject go, string name, out ApplyFoliage item)
    {
        string assetPath = string.Format("Assets/Ecosystem Generator/FoliageItems/Prefabs/{0}", name);
        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
        if (null == prefab)
        {
            item = null;
            return false;
        }

        item = go.AddComponent<ApplyFoliage>();
        if (null == item)
        {
            return false;
        }

        item.m_foliagePrefab = prefab;

        //mark as changed
        m_foliageChanged = true;

        //assign the layer
        item.gameObject.layer = FOLIAGE_LAYER;

        return true;
    }

    /// <summary>
    /// Assign with the named foliage
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    static bool AssignFoliage(ApplyFoliage item, string name)
    {
        string assetPath = string.Format("Assets/Ecosystem Generator/FoliageItems/Prefabs/{0}", name);
        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
        if (null == prefab)
        {
            item = null;
            return false;
        }

        item.m_foliagePrefab = prefab;

        return true;
    }

    #endregion

    /// <summary>
    /// Scroll vector
    /// </summary>
    static Vector2 m_scroll = Vector2.zero;

    bool m_showPickFoliage = false;

    public void OnGUI()
    {
        wantsMouseMove = true;
        if (null != Event.current &&
            Event.current.type == EventType.MouseMove)
        {
            m_mousePosition = Event.current.mousePosition;
            GUILayout.Label(string.Format("Mouse ({0}, {1})", m_mousePosition.x, m_mousePosition.y));
            //Debug.Log(string.Format("Mouse ({0}, {1})", m_mousePosition.x, m_mousePosition.y));
			Repaint();
			return;
        }

        GameObject go = Selection.activeGameObject;
        if (null == go)
        {
            GUILayout.Label("No foliage selected");
            return;
        }

        ApplyFoliage[] layers = go.GetComponents<ApplyFoliage>();
        if (null == layers)
        {
            GUILayout.Label("No foliage selected");
            return;
        }

        m_scroll = GUILayout.BeginScrollView(m_scroll);

        GUILayout.BeginHorizontal();

        GUILayout.Label("Show");
        GUILayout.Label("Action");

        GUILayout.EndHorizontal();

        if (m_selectedItem == null)
        {
            if (GUILayout.Button("[Select None]"))
            {
                m_selectedItem = null;
            }
        }
        else
        {
            if (GUILayout.Button("Select None"))
            {
                m_selectedItem = null;
            }
        }

        bool found = false;
        for (int layer = 0; layer < layers.Length; ++layer)
        {
            ApplyFoliage foliage = layers[layer];
            if (null == foliage)
            {
                continue;
            }
            GUILayout.BeginHorizontal();
            foliage.enabled = GUILayout.Toggle(foliage.enabled, string.Empty, GUILayout.Width(20));
            if (null != foliage.m_combinedMesh)
            {
                foliage.m_combinedMesh.SetActiveRecursively(foliage.enabled);
            }
            if (m_selectedItem == foliage)
            {
                found = true;
                if (GUILayout.Button(string.Format("[Select Layer{0}]", layer + 1)))
                {
                    m_selectedItem = foliage;
                }
            }
            else
            {
                if (GUILayout.Button(string.Format("Select Layer{0}", layer + 1)))
                {
                    m_selectedItem = foliage;
                }
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        if (found)
        {
            GUILayout.Label("Selected Item");
            if (m_showPickFoliage)
            {
                if (GUILayout.Button("[Pick Foliage]"))
                {
                    m_showPickFoliage = !m_showPickFoliage;
                }
            }
            else
            {
                if (GUILayout.Button("Pick Foliage"))
                {
                    m_showPickFoliage = !m_showPickFoliage;
                }
            }

            if (m_showPickFoliage)
            {
                if (GUILayout.Button("Reload Items"))
                {
                    ReloadFoliageItems();
                }
                List<string> foliageItems = GetFoliageItems();
                foreach (string name in foliageItems)
                {
                    if (GUILayout.Button(name))
                    {
                        AssignFoliage(m_selectedItem, name);
                        m_foliageChanged = true;
                        Regenerate(m_selectedItem);
                        RefreshPanel();
                    }
                }
                EditorGUILayout.Separator();
            }

            Show(m_selectedItem);
        }

        GUILayout.EndScrollView();
    }

    public static void Show(ApplyFoliage item)
    {
        try
        {
            if (null == item)
            {
                //Debug.LogError("Selection changed from ApplyFoliage");
                return;
            }

            //assign the layer
            item.gameObject.layer = FOLIAGE_LAYER;

            if (null != item.m_maskTexture &&
                GUILayout.Button("Remove Mask"))
            {
                m_foliageChanged = true;
                RemoveMaskTexture(item);
            }

            if (null == item.m_maskTexture &&
                GUILayout.Button("Create Mask"))
            {
                m_foliageChanged = true;
                CreateMaskTexture(item);
            }

            //bool hasSelectedPaintItem = false;
            if (null != item.m_maskTexture)
            {
                if (GUILayout.Button(string.Format("{0}Paint this mask{1}",
                    (m_focusFoliage == item) ? "[" : string.Empty,
                    (m_focusFoliage == item) ? "]" : string.Empty)))
                {
                    m_foliageChanged = true;
                    if (m_focusFoliage == item)
                    {
                        m_focusFoliage = null;
                    }
                    else
                    {
                        m_focusFoliage = item;
                        //hasSelectedPaintItem = true;

                        // update the brush overlay while the item is selected
                        item.StartCoroutine(GetPanel().UpdateTheEditor());
                    }
                }
            }

            if (m_focusFoliage == item)
            {
                InitializeBrushOverlay(item);

                bool brushChanged = false;
                EditorGUILayout.BeginHorizontal();
                if (ShowChangeItem("Brush Size:", item.m_brushSize))
                {
                    brushChanged = true;
                }
                item.m_brushSize.m_value = GUILayout.HorizontalSlider(item.m_brushSize.m_value, 1, 256);
                if (item.m_brushSize.HasChanged())
                {
                    item.m_brushSize.Change();
                    brushChanged = true;
                }
                EditorGUILayout.EndHorizontal();

                if (brushChanged &&
                    null != item.m_brushOverlay)
                {
                    float bs = item.m_brushSize.m_value / 128.0f;
                    item.m_brushOverlay.transform.localScale = new Vector3(bs, bs, bs);
                }

                EditorGUILayout.BeginHorizontal();
                if (ShowChangeItem("Brush Opacity:", item.m_brushOpacity))
                {
                    m_foliageChanged = true;
                }
                item.m_brushOpacity.m_value = GUILayout.HorizontalSlider(item.m_brushOpacity.m_value, 1, 100);
                if (item.m_brushOpacity.HasChanged())
                {
                    item.m_brushOpacity.Change();
                    m_foliageChanged = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                DeinitializeBrushOverlay(item);
            }

            item.m_position.m_value = item.gameObject.transform.localPosition;
            if (ShowChangeItem("Pos x:", "Pos y:", "Pos z:", item.m_position))
            {
                item.gameObject.transform.localPosition = item.m_position.m_value;
                m_foliageChanged = true;
            }

            if (ShowChangeItem("Space x:", "Space z:", item.m_spacing))
            {
                m_foliageChanged = true;
            }

            if (ShowChangeItem("Min Rot x:", "Min Rot y:", "Min Rot z:", item.m_rotationDeltaMin))
            {
                m_foliageChanged = true;
            }

            if (ShowChangeItem("Max Rot x:", "Max Rot y:", "Max Rot z:", item.m_rotationDeltaMax))
            {
                m_foliageChanged = true;
            }

            if (ShowChangeItem("Min Scale x:", "Min Scale y:", "Min Scale z:", item.m_scaleDeltaMin))
            {
                m_foliageChanged = true;
            }

            if (ShowChangeItem("Max Scale x:", "Max Scale y:", "Max Scale z:", item.m_scaleDeltaMax))
            {
                m_foliageChanged = true;
            }

            if (ShowChangeItem("Density:", item.m_density))
            {
                m_foliageChanged = true;
            }

            if (m_foliageChanged)
            {
                Regenerate(item);

                // clear the flag
                m_foliageChanged = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("Inspector exception={0}", ex));
        }
    }

    #region Show various item types

    static bool ShowChangeItem(string fieldA, ApplyFoliage.ChangeItemBool item)
    {
        item.m_value = EditorGUILayout.Toggle(fieldA, item.m_value);
        if (item.HasChanged())
        {
            item.Change();
            return true;
        }
        return false;
    }

    static bool ShowChangeItem(string fieldA, ApplyFoliage.ChangeItemInt item)
    {
        item.m_value = EditorGUILayout.IntField(fieldA, item.m_value);
        if (item.HasChanged())
        {
            item.Change();
            return true;
        }
        return false;
    }

    static bool ShowChangeItem(string fieldA, ApplyFoliage.ChangeItemFloat item)
    {
        item.m_value = EditorGUILayout.FloatField(fieldA, item.m_value);
        if (item.HasChanged())
        {
            item.Change();
            return true;
        }
        return false;
    }

    static bool ShowChangeItem(string fieldA, string fieldB, ApplyFoliage.ChangeItemVector2 item)
    {
        item.m_value.x = EditorGUILayout.FloatField(fieldA, item.m_value.x);
        item.m_value.y = EditorGUILayout.FloatField(fieldB, item.m_value.y);
        if (item.HasChanged())
        {
            item.Change();
            return true;
        }
        return false;
    }

    static bool ShowChangeItem(string fieldA, string fieldB, string fieldC, ApplyFoliage.ChangeItemVector3 item)
    {
        item.m_value.x = EditorGUILayout.FloatField(fieldA, item.m_value.x);
        item.m_value.y = EditorGUILayout.FloatField(fieldB, item.m_value.y);
        item.m_value.z = EditorGUILayout.FloatField(fieldC, item.m_value.z);
        if (item.HasChanged())
        {
            item.Change();
            return true;
        }
        return false;
    }

    #endregion

    #region Create Foliage Logic

    /// <summary>
    /// Detect changes that regenerate the mesh
    /// </summary>
    public static bool m_foliageChanged = false;

    public const string COMBINED_MESH = "CombinedMesh";
    public const string FOLIAGE_ROOT = "FoliageRoot";
    public const int MAX_DENSITY = 200;
    public const int MAX_ITERATIONS = 200;
    public const int FOLIAGE_LAYER = 8;
    public const int FOLIAGE_LAYER_MASK = 1 << 8;

    public static void Regenerate(ApplyFoliage item)
    {
        // destroy the old objects
        if (m_foliageChanged)
        {
            foreach (GameObject child in item.m_children)
            {
                UnityEngine.Object.DestroyImmediate(child, true);
            }
            item.m_children.Clear();
            item.ClearPoints();

            if (null != item.m_combinedMesh)
            {
                Object.DestroyImmediate(item.m_combinedMesh.GetComponent<MeshRenderer>(), true);
                Object.DestroyImmediate(item.m_combinedMesh.GetComponent<MeshFilter>(), true);
                Object.DestroyImmediate(item.m_combinedMesh, true);
                item.m_combinedMesh = null;
            }

            if (null != item.m_foliageRoot)
            {
                //kill it
                Object.DestroyImmediate(item.m_foliageRoot, true);
                item.m_foliageRoot = null;
            }
        }

        //raycast against each mesh renderer that's found on the attached gameObject
        MeshRenderer[] mrs = item.gameObject.GetComponents<MeshRenderer>();
        if (null != mrs)
        {
            foreach (MeshRenderer mr in mrs)
            {
                if (null == mr ||
                    null == mr.gameObject ||
                    mr.gameObject.name.ToUpper().StartsWith(COMBINED_MESH))
                {
                    continue;
                }

                // attach a mesh collider if it's missing
                MeshCollider mc = mr.gameObject.GetComponent<MeshCollider>();
                if (null == mc)
                {
                    mc = mr.gameObject.AddComponent<MeshCollider>();
                }

                // assign the mask so we can see what's being painted
                //if (hasSelectedPaintItem &&
                //    null != mr.sharedMaterial &&
                //    null != item.m_maskTexture)
                //{
                //mr.sharedMaterial = new Material(Shader.Find("Diffuse"));
                //mr.sharedMaterial.mainTexture = item.m_maskTexture;
                //}

                // raycast center
                Vector3 center = mr.bounds.center;

                // raycast width
                Vector3 extends = mr.bounds.extents;

                if (item.m_spacing.m_value.x > 0f &&
                    item.m_spacing.m_value.y > 0f)
                {
                    // skip if the raycast area is too small
                    if (extends.x <= 0 ||
                        extends.z <= 0)
                    {
                        continue;
                    }

                    if (m_foliageChanged)
                    {
                        GenerateFoliage(item, center, extends);
                    }
                }
            }
        }
    }

    public static void GenerateFoliage(ApplyFoliage foliage, Vector3 center, Vector3 extends)
    {
        if (null == foliage)
        {
            return;
        }

        GameObject rootGO = new GameObject(FOLIAGE_ROOT);
        foliage.m_foliageRoot = rootGO;
        rootGO.transform.localPosition = Vector3.zero;
        rootGO.transform.localRotation = Quaternion.identity;
        rootGO.transform.localScale = Vector3.one;
        rootGO.transform.parent = foliage.transform;

        if (null != foliage.m_maskTexture)
        {
            int width = foliage.m_maskTexture.width;
            int height = foliage.m_maskTexture.height;
            for (int y = 0; y < height; y += 16)
            {
                //put into -1 to 1 space
                float ny = 2f * y / (float)height - 1f;
                // put into -extends.z to extends.z space
                float mz = ny * extends.z;
                for (int x = 0; x < width; x += 16)
                {
                    Color pixel = foliage.m_maskTexture.GetPixel(x, y);
                    if (pixel == TRANSPARENT_COLOR)
                    {
                        continue;
                    }
                    // put into -1 to 1 space
                    float nx = 2f * x / (float)width - 1f;
                    // put into -extends.x to extends.x space
                    float mx = nx * extends.x;
                    Vector3 randPos = new Vector3(mx, 0, mz);
                    Vector3 rayPos = center + randPos;
                    CanStandAtRayMask(foliage, x, y, rayPos, rootGO);
                }
            }
        }
        else
        {
            for (int index = 0;
                index < MAX_ITERATIONS &&
                foliage.m_children.Count < foliage.m_density.m_value &&
                foliage.m_children.Count < MAX_DENSITY;
                ++index)
            {
                Vector3 randPos = new Vector3(Random.Range(-extends.x, extends.x), 0, Random.Range(-extends.z, extends.z));
                Vector3 rayPos = center + randPos;
                CanStandAtRayRandom(foliage, center, extends, rayPos, rootGO);
            }
        }

        CombineIntoMesh(foliage);
        CleanUp(foliage);
    }

    public static void CanStandAtRayMask(ApplyFoliage foliage, int x, int y, Vector3 rayPos, GameObject rootGO)
    {
        if (null == foliage.m_foliagePrefab)
        {
            return;
        }

        if (null == foliage.m_maskTexture)
        {
            return;
        }

        // Raycast to get the terrain height
        RaycastHit hit;
        // move the ray up and cast downward
        if (Physics.Raycast(new Vector3(rayPos.x, 1000, rayPos.z), -Vector3.up, out hit, Mathf.Infinity, FOLIAGE_LAYER_MASK))
        {
            Color hitColor = foliage.m_maskTexture.GetPixel(x, y);
            if (hitColor == TRANSPARENT_COLOR)
            {
                return;
            }

            GameObject foliagePrefab = foliage.m_foliagePrefab;

            Vector2 p;
            p.x = hit.point.x;
            p.y = hit.point.z;
            if (foliage.IsTooClose(p))
            {
                return;
            }
            else
            {
                foliage.AddPoint(p);
            }

            Quaternion randRot = Quaternion.Euler(
                Random.Range(foliage.m_rotationDeltaMin.m_value.x, foliage.m_rotationDeltaMax.m_value.x),
                Random.Range(foliage.m_rotationDeltaMin.m_value.y, foliage.m_rotationDeltaMax.m_value.y),
                Random.Range(foliage.m_rotationDeltaMin.m_value.z, foliage.m_rotationDeltaMax.m_value.z));
            Vector3 randScale = new Vector3(
                Random.Range(foliage.m_scaleDeltaMin.m_value.x, foliage.m_scaleDeltaMax.m_value.x),
                Random.Range(foliage.m_scaleDeltaMin.m_value.y, foliage.m_scaleDeltaMax.m_value.y),
                Random.Range(foliage.m_scaleDeltaMin.m_value.z, foliage.m_scaleDeltaMax.m_value.z));

            GameObject go = (GameObject)Object.Instantiate(foliagePrefab, Vector3.zero, Quaternion.identity);
            go.transform.localPosition = hit.point;
            go.transform.localRotation = randRot;
            go.transform.localScale = randScale;
            go.transform.parent = rootGO.transform;

            foliage.m_children.Add(go);
        }
    }

    public static void CanStandAtRayRandom(ApplyFoliage foliage, Vector3 center, Vector3 extends, Vector3 rayPos, GameObject rootGO)
    {
        if (null == foliage.m_foliagePrefab)
        {
            return;
        }

        // Raycast to get the terrain height
        RaycastHit hit;
        // move the ray up and cast downward
        if (Physics.Raycast(new Vector3(rayPos.x, 1000, rayPos.z), -Vector3.up, out hit, Mathf.Infinity, FOLIAGE_LAYER_MASK))
        {
            GameObject foliagePrefab = foliage.m_foliagePrefab;

            Vector2 p;
            p.x = hit.point.x;
            p.y = hit.point.z;
            if (foliage.IsTooClose(p))
            {
                return;
            }
            else
            {
                foliage.AddPoint(p);
            }

            Quaternion randRot = Quaternion.Euler(
                Random.Range(foliage.m_rotationDeltaMin.m_value.x, foliage.m_rotationDeltaMax.m_value.x),
                Random.Range(foliage.m_rotationDeltaMin.m_value.y, foliage.m_rotationDeltaMax.m_value.y),
                Random.Range(foliage.m_rotationDeltaMin.m_value.z, foliage.m_rotationDeltaMax.m_value.z));
            Vector3 randScale = new Vector3(
                Random.Range(foliage.m_scaleDeltaMin.m_value.x, foliage.m_scaleDeltaMax.m_value.x),
                Random.Range(foliage.m_scaleDeltaMin.m_value.y, foliage.m_scaleDeltaMax.m_value.y),
                Random.Range(foliage.m_scaleDeltaMin.m_value.z, foliage.m_scaleDeltaMax.m_value.z));

            GameObject go = (GameObject)Object.Instantiate(foliagePrefab, Vector3.zero, Quaternion.identity);
            go.transform.localPosition = hit.point;
            go.transform.localRotation = randRot;
            go.transform.localScale = randScale;
            go.transform.parent = rootGO.transform;

            foliage.m_children.Add(go);
        }
    }

    /// <summary>
    /// Combine all the children into a single mesh
    /// </summary>
    /// <param name="foliage"></param>
    static void CombineIntoMesh(ApplyFoliage foliage)
    {
        if (null == foliage ||
            null == foliage.m_children ||
            foliage.m_children.Count == 0)
        {
            return;
        }

        // count the mesh filters
        Material sharedMaterial = null;
        int countMfs = 0;
        foreach (GameObject go in foliage.m_children)
        {
            if (null == go)
            {
                continue;
            }
            MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>(true);
            if (null != mrs)
            {
                foreach (MeshRenderer mr in mrs)
                {
                    if (null == mr)
                    {
                        continue;
                    }
                    try
                    {
                        if (null == sharedMaterial &&
                            null != mr.sharedMaterial)
                        {
                            sharedMaterial = mr.sharedMaterial;
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>(true);
            if (null == mfs)
            {
                continue;
            }
            foreach (MeshFilter mf in mfs)
            {
                if (null == mf)
                {
                    continue;
                }
                Mesh mesh = mf.sharedMesh;
                if (null == mesh)
                {
                    continue;
                }
                if (null == mesh.vertices ||
                    null == mesh.triangles ||
                    null == mesh.uv ||
                    null == mesh.normals ||
                    null == mesh.tangents ||
                    null == mesh.colors)
                {
                    continue;
                }
                ++countMfs;
            }
        }
        if (countMfs == 0)
        {
            return;
        }

        // create combine instances
        CombineInstance[] combine = new CombineInstance[countMfs];
        int index = 0;
        foreach (GameObject go in foliage.m_children)
        {
            if (null == go)
            {
                continue;
            }
            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>(true);
            if (null == mfs)
            {
                continue;
            }
            foreach (MeshFilter mf in mfs)
            {
                if (null == mf)
                {
                    continue;
                }
                Mesh mesh = mf.sharedMesh;
                if (null == mesh)
                {
                    continue;
                }
                if (null == mesh.vertices ||
                    null == mesh.triangles ||
                    null == mesh.uv ||
                    null == mesh.normals ||
                    null == mesh.tangents ||
                    null == mesh.colors)
                {
                    continue;
                }
                combine[index].mesh = mf.sharedMesh;
                combine[index].transform = mf.transform.localToWorldMatrix;
                mf.gameObject.active = false;
                ++index;
            }
        }

        // create the new mesh
        foliage.m_mesh = new Mesh();
        foliage.m_mesh.CombineMeshes(combine);

        // show the combined mesh
        foliage.m_combinedMesh = new GameObject();
        foliage.m_combinedMesh.name = string.Format("{0}_{1}", COMBINED_MESH, foliage.m_children[0].name);
        foliage.m_combinedMesh.transform.parent = foliage.transform;

        MeshRenderer childMr = foliage.m_combinedMesh.AddComponent<MeshRenderer>();
        if (null != sharedMaterial)
        {
            childMr.GetComponent<Renderer>().sharedMaterial = sharedMaterial;
        }
        MeshFilter childMf = foliage.m_combinedMesh.AddComponent<MeshFilter>();
        childMf.sharedMesh = foliage.m_mesh;
    }

    /// <summary>
    /// Clean up the temp items used to create the combined mesh
    /// </summary>
    /// <param name="foliage"></param>
    static void CleanUp(ApplyFoliage foliage)
    {
        //destroy the children
        foreach (GameObject child in foliage.m_children)
        {
            UnityEngine.Object.DestroyImmediate(child, true);
        }
        foliage.m_children.Clear();
        foliage.ClearPoints();

        if (null != foliage.m_foliageRoot)
        {
            //kill it
            Object.DestroyImmediate(foliage.m_foliageRoot, true);
            foliage.m_foliageRoot = null;
        }
    }

    #endregion

    #region Painting Loop

    /// <summary>
    /// Update the brush overlay in a coroutine
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateTheEditor()
    {
        // get the mouse position
        if (null != Event.current)
        {
            m_mousePosition = Event.current.mousePosition;
        }

        // dropped the item
        if (null == m_focusFoliage)
        {
            yield break;
        }

        // wait and yield
        yield return null;

        if (null != m_focusFoliage)
        {
            //Debug.Log("UpdateTheEditor");
            MainUpdate(m_focusFoliage);
        }
        
        // dropped the item
        if (null != m_focusFoliage)
        {
            // reinvoke
            m_focusFoliage.StartCoroutine(UpdateTheEditor());
        }
    }
    
    #endregion

    #region Painting Logic

    static Color TRANSPARENT_COLOR = new Color(0, 0, 0, 0);
    const int MASK_DIMENSIONS = 1024;

    /// <summary>
    /// This is the foliage selected for painting
    /// </summary>
    public static ApplyFoliage m_focusFoliage = null;

    /// <summary>
    /// Create the texture to draw on
    /// </summary>
    /// <param name="foliage"></param>
    public static void CreateMaskTexture(ApplyFoliage foliage)
    {
        if (null == foliage.m_maskTexture)
        {
            foliage.m_maskTexture = new Texture2D(MASK_DIMENSIONS, MASK_DIMENSIONS, TextureFormat.ARGB32, true);
            Color[] pixels = new Color[MASK_DIMENSIONS * MASK_DIMENSIONS];
            for (int index = 0; index < pixels.Length; ++index)
            {
                pixels[index] = TRANSPARENT_COLOR;
            }
            foliage.m_maskTexture.SetPixels(pixels);
            foliage.m_maskTexture.Apply();
        }
    }

    /// <summary>
    /// Remove the texture to draw on
    /// </summary>
    /// <param name="foliage"></param>
    public static void RemoveMaskTexture(ApplyFoliage foliage)
    {
        if (null != foliage.m_maskTexture)
        {
            Object.DestroyImmediate(foliage.m_maskTexture, true);
            foliage.m_maskTexture = null;
        }
    }

    const string BRUSH_NAME = "BrushOverlay";

    const string BRUSH_TEXTURE = "Assets/Ecosystem Generator/Editor/Textures/brush.png";

    public static void InitializeBrushOverlay(ApplyFoliage foliage)
    {
        if (null != foliage.m_brushOverlay)
        {
            return;
        }
        foliage.m_brushOverlay = GameObject.Find(BRUSH_NAME);
        if (null == foliage.m_brushOverlay)
        {
            foliage.m_brushOverlay = GameObject.CreatePrimitive(PrimitiveType.Plane);
            foliage.m_brushOverlay.name = BRUSH_NAME;
        }
        foliage.m_brushOverlay.transform.localPosition = Vector3.zero;
        foliage.m_brushOverlay.transform.localRotation = Quaternion.identity;
        foliage.m_brushOverlay.transform.localScale = Vector3.one;

        // load the brush texture
        UnityEngine.Object objText = AssetDatabase.LoadAssetAtPath(BRUSH_TEXTURE, typeof(Texture2D));
        if (null == objText ||
            !(objText is Texture2D))
        {
            Debug.LogError("Failed to find brush texture");
            return;
        }

        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.mainTexture = (Texture2D)objText;
        foliage.m_brushOverlay.GetComponent<Renderer>().sharedMaterial = material;
    }

    public static void DeinitializeBrushOverlay(ApplyFoliage foliage)
    {
        if (null != foliage.m_brushOverlay)
        {
            if (null != foliage.m_brushOverlay.GetComponent<Renderer>() &&
                null != foliage.m_brushOverlay.GetComponent<Renderer>().sharedMaterial)
            {
                Object.DestroyImmediate(foliage.m_brushOverlay.GetComponent<Renderer>().sharedMaterial, true);
            }

            Object.DestroyImmediate(foliage.m_brushOverlay, true);
            foliage.m_brushOverlay = null;
        }
    }

    /// <summary>
    /// Update the brush overlay
    /// </summary>
    /// <param name="foliage"></param>
    public void UpdateBrushOverlay(ApplyFoliage foliage)
    {
        if (null == foliage ||
            null == foliage.m_brushOverlay ||
            null == Camera.main ||
            null == Camera.main.transform)
        {
            return;
        }

        GameObject brushOverlay = foliage.m_brushOverlay;
        Vector3 mousePos = m_mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        // move the ray up and cast downward
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, FOLIAGE_LAYER_MASK))
        {
            brushOverlay.transform.position = hit.point;
            brushOverlay.transform.LookAt(Camera.main.transform);

            //rotate plane x + 90
            Vector3 euler = brushOverlay.transform.eulerAngles;
            euler.x += 90;
            brushOverlay.transform.eulerAngles = euler;
        }
    }

    /// <summary>
    /// Capture the mouse position in the update event
    /// </summary>
    Vector3 m_mousePosition = Vector3.zero;

    public void MainUpdate(ApplyFoliage foliage)
    {
        if (null == foliage ||
            m_focusFoliage != foliage ||
            null == foliage.m_maskTexture ||
            null == foliage.m_brushOverlay)
        {
            return;
        }

        if (null == foliage.m_brushOverlay.GetComponent<Renderer>())
        {
            Debug.LogError("Brush overlay renderer is missing");
            return;
        }

        if (null == foliage.m_brushOverlay.GetComponent<Renderer>().sharedMaterial)
        {
            Debug.LogError("Brush overlay material is missing");
            return;
        }

        if (null == foliage.m_brushOverlay.GetComponent<Renderer>().sharedMaterial.mainTexture)
        {
            Debug.LogError("Brush overlay texture is missing");
            return;
        }

        Texture2D brushTexture = (Texture2D)foliage.m_brushOverlay.GetComponent<Renderer>().sharedMaterial.mainTexture;

        // declaration of the canvas texture
        Texture2D canvasTexture;

        // update the brush overlay position
        UpdateBrushOverlay(foliage);

        // apply changes every N miliseconds rather than each frame
        if (m_detectedChange &&
            m_applyTimer < System.DateTime.Now)
        {
            // get the canvas texture
            canvasTexture = foliage.m_maskTexture;

            // error checking
            if (null == canvasTexture)
            {
                return;
            }

            // wait until mouse is done
            if (!Input.GetMouseButton(0) &&
                !Input.GetMouseButtonDown(0) &&
                !Input.GetMouseButton(1) &&
                !Input.GetMouseButtonDown(1))
            {
                // apply texture changes
                canvasTexture.Apply();

                // clear the flag
                m_detectedChange = false;

                // regenerate the foliage
                m_foliageChanged = true;
            }

            // delay next texture update
            m_applyTimer = System.DateTime.Now + System.TimeSpan.FromMilliseconds(100);
        }

        // undo any changes on right click
        if (Input.GetMouseButton(1) ||
            Input.GetMouseButtonDown(1))
        {
            // get a reference to the canvas texture
            canvasTexture = foliage.m_maskTexture;

            // error checking
            if (null == canvasTexture ||
                null == m_lastPixels)
            {
                return;
            }

            // revert changes to last pixel snapshot
            canvasTexture.SetPixels(m_lastPixels);

            // flag for texture update
            m_detectedChange = true;
            return;
        }

        // get the button state of left click
        bool buttonDown =
            Input.GetMouseButton(0) ||
            Input.GetMouseButtonDown(0);

        // exit if no button is pressed
        if (!buttonDown)
        {
            m_lastButtonDown = buttonDown;
            return;
        }

        // The first button down event will
        // copy the texture
        // and paint
        // a.k.a snapshot
        bool firstButtonDown = false;

        // if this is the first button press event,
        // copy the texture
        if (!m_lastButtonDown &&
            buttonDown)
        {
            firstButtonDown = true;
        }

        // record the button state
        m_lastButtonDown = buttonDown;

        // error checking
        if (null == brushTexture)
        {
            return;
        }

        // get a reference to the canvas texture
        canvasTexture = foliage.m_maskTexture;

        // error checking
        if (null == canvasTexture)
        {
            return;
        }

        // copy the texture to track opacity changes
        if (firstButtonDown)
        {
            m_lastPixels = canvasTexture.GetPixels();
        }

        // use the mouse position to paint on the canvas
        Vector3 mousePos = m_mousePosition;

        // normalize opacity, put into 0 to 1 range
        float opacityRatio = foliage.m_brushOpacity.m_value * 0.01f;

        // use half the brush size, used to calculate the target window
        float halfBrushSize = foliage.m_brushSize.m_value * 0.5f;

        // calculate the target window of where to stamp the brush
        float targetMinX = (mousePos.x - halfBrushSize);

        // power of 2 texture performance is better, scale down to canvas size
        float targetMinY = (mousePos.y - halfBrushSize) / (float)Screen.height * canvasTexture.height;

        float targetMaxX = (mousePos.x + halfBrushSize);

        // power of 2 texture performance is better, scale down to canvas size
        float targetMaxY = (mousePos.y + halfBrushSize) / (float)Screen.height * canvasTexture.height;

        //keep the target window in bounds of the canvas - bounds check
        targetMinX = Mathf.Min(canvasTexture.width - 1, Mathf.Max(0f, targetMinX));
        targetMinY = Mathf.Min(canvasTexture.height - 1, Mathf.Max(0f, targetMinY));
        targetMaxX = Mathf.Min(canvasTexture.width - 1, Mathf.Max(0f, targetMaxX));
        targetMaxY = Mathf.Min(canvasTexture.height - 1, Mathf.Max(0f, targetMaxY));

        //calculate dimensions of the target window,
        // width and height of the target window
        float targetWidth = targetMaxX - targetMinX;
        float targetHeight = targetMaxY - targetMinY;
        if (targetWidth == 0f ||
            targetHeight == 0f)
        {
            return;
        }

        // iterate over the target pixels - target window
        for (float y = targetMinY; y < canvasTexture.height && y < targetMaxY; ++y)
        {
            for (float x = targetMinX; x < canvasTexture.width && x < targetMaxX; ++x)
            {
                //interpolate we want the entire source to stretch to the target window
                float sourceX = (x - targetMinX) / targetWidth * brushTexture.width;
                float sourceY = (y - targetMinY) / targetHeight * brushTexture.height;

                // calculate the 1D color index
                int colorIndex = ((int)x) + ((int)y) * canvasTexture.width;

                //get the original color
                Color originalColor = m_lastPixels[colorIndex];

                //get the source color
                Color sourceColor = brushTexture.GetPixel((int)sourceX, (int)sourceY);
                float sourceIntensity = sourceColor.a;

                //get the target color
                Color targetColor = canvasTexture.GetPixel((int)x, (int)y);

                if (foliage.m_paintMode == ApplyFoliage.PaintModes.DRAW)
                {
                    //alter the destination color, try to paint towards the brush color
                    targetColor.r = PickBrush(originalColor.r, targetColor.r, sourceIntensity, opacityRatio, foliage.m_brushColor.r);
                    targetColor.g = PickBrush(originalColor.g, targetColor.g, sourceIntensity, opacityRatio, foliage.m_brushColor.g);
                    targetColor.b = PickBrush(originalColor.b, targetColor.b, sourceIntensity, opacityRatio, foliage.m_brushColor.b);
                    targetColor.a = PaintBrush(originalColor.a, targetColor.a, sourceIntensity, opacityRatio);
                }
                else
                {
                    // just erase when in erase mode
                    targetColor.r = EraseBrush(originalColor.r, targetColor.r, sourceIntensity, opacityRatio);
                    targetColor.g = EraseBrush(originalColor.g, targetColor.g, sourceIntensity, opacityRatio);
                    targetColor.b = EraseBrush(originalColor.b, targetColor.b, sourceIntensity, opacityRatio);
                    targetColor.a = EraseBrush(originalColor.a, targetColor.a, sourceIntensity, opacityRatio);
                }

                //set the target color
                canvasTexture.SetPixel((int)x, (int)y, targetColor);
            }
        }

        // a change was made, wait for the texture delay to apply
        m_detectedChange = true;
    }

    /// <summary>
    /// Keep track of button changes
    /// </summary>
    bool m_lastButtonDown = false;

    /// <summary>
    /// For undo operations,
    /// and for opacity changes,
    /// keep a copy of the last pixels operated on
    /// </summary>
    Color[] m_lastPixels = null;

    /// <summary>
    /// Apply texture updates on a delay rather than
    /// every frame for performance reasons
    /// </summary>
    System.DateTime m_applyTimer = System.DateTime.MinValue;

    /// <summary>
    /// We only need to apply texture changes when a change was detected
    /// </summary>
    bool m_detectedChange = false;

    /// <summary>
    /// Paint opacity changes while keeping the color in range of the original value
    /// and inside the valid bounds
    /// </summary>
    /// <param name="startLimit"></param>
    /// <param name="targetValue"></param>
    /// <param name="sourceIntensity"></param>
    /// <param name="opacityRatio"></param>
    /// <returns></returns>
    float PaintBrush(float startLimit, float targetValue, float sourceIntensity, float opacityRatio)
    {
        return Mathf.Min(startLimit + opacityRatio, Mathf.Min(1f, targetValue + sourceIntensity * opacityRatio));
    }

    /// <summary>
    /// Erase opacity changs while keeping the color in range of the original value
    /// and inside the valid bounds
    /// </summary>
    /// <param name="startLimit"></param>
    /// <param name="targetValue"></param>
    /// <param name="sourceIntensity"></param>
    /// <param name="opacityRatio"></param>
    /// <returns></returns>
    float EraseBrush(float startLimit, float targetValue, float sourceIntensity, float opacityRatio)
    {
        return Mathf.Max(startLimit - opacityRatio, Mathf.Max(0f, targetValue - sourceIntensity * opacityRatio));
    }

    /// <summary>
    /// Decide whether to paint or erase,
    /// while also aiming toward the destination value
    /// </summary>
    /// <param name="startLimit"></param>
    /// <param name="targetValue"></param>
    /// <param name="sourceIntensity"></param>
    /// <param name="opacityRatio"></param>
    /// <param name="destinationValue"></param>
    /// <returns></returns>
    float PickBrush(float startLimit, float targetValue, float sourceIntensity, float opacityRatio, float destinationValue)
    {
        if (startLimit == destinationValue)
        {
            return destinationValue;
        }
        else if (startLimit < destinationValue)
        {
            return Mathf.Min(destinationValue, PaintBrush(startLimit, targetValue, sourceIntensity, opacityRatio));
        }
        else // if (destinationValue < startLimit)
        {
            return Mathf.Max(destinationValue, EraseBrush(startLimit, targetValue, sourceIntensity, opacityRatio));
        }
    }

    #endregion
}