using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ApplyFoliage : MonoBehaviour
{
    /// <summary>
    /// The only required item which is the model that will be spawned
    /// </summary>
	public GameObject m_foliagePrefab = null;

    /// <summary>
    /// A mask texture overrides random placement
    /// </summary>
	public Texture2D m_maskTexture = null;

    /// <summary>
    /// A place to put the random items until they are combined
    /// </summary>
    [HideInInspector]
    public GameObject m_foliageRoot = null;
    
    /// <summary>
    /// The generated foliage will be combined into
    /// a single mesh
    /// </summary>
    [HideInInspector]
    public Mesh m_mesh = null;

    /// <summary>
    /// The final combined mesh
    /// </summary>
    [HideInInspector]
    public GameObject m_combinedMesh = null;

    #region Detect Item Changes

    /// <summary>
    /// Track item changes
    /// </summary>
    public class ChangeItemBool
    {
        public bool m_value = false;
        public bool m_oldValue = true;
        public ChangeItemBool(bool val, bool old)
        {
            m_value = val;
            m_oldValue = old;
        }
        public bool HasChanged()
        {
            return m_value != m_oldValue;
        }
        public void Change()
        {
            m_oldValue = m_value;
        }
    }
    public class ChangeItemInt
    {
        public int m_value = 1;
        public int m_oldValue = 0;
        public ChangeItemInt(int val, int old)
        {
            m_value = val;
            m_oldValue = old;
        }
        public bool HasChanged()
        {
            return m_value != m_oldValue;
        }
        public void Change()
        {
            m_oldValue = m_value;
        }
    }
    public class ChangeItemFloat
    {
        public float m_value = 1f;
        public float m_oldValue = 0f;
        public ChangeItemFloat(float val, float old)
        {
            m_value = val;
            m_oldValue = old;
        }
        public bool HasChanged()
        {
            return m_value != m_oldValue;
        }
        public void Change()
        {
            m_oldValue = m_value;
        }
    }
    public class ChangeItemVector2
    {
        public Vector2 m_value = Vector2.one;
        public Vector2 m_oldValue = Vector2.zero;
        public ChangeItemVector2(Vector2 val, Vector2 old)
        {
            m_value = val;
            m_oldValue = old;
        }
        public bool HasChanged()
        {
            return m_value != m_oldValue;
        }
        public void Change()
        {
            m_oldValue = m_value;
        }
    }
    public class ChangeItemVector3
    {
        public Vector3 m_value = Vector3.one;
        public Vector3 m_oldValue = Vector3.zero;
        public ChangeItemVector3(Vector3 val, Vector3 old)
        {
            m_value = val;
            m_oldValue = old;
        }
        public bool HasChanged()
        {
            return m_value != m_oldValue;
        }
        public void Change()
        {
            m_oldValue = m_value;
        }
    }

    #endregion

    /// <summary>
	/// Detect spacing changes
	/// </summary>
    public ChangeItemVector3 m_position = new ChangeItemVector3(Vector3.zero, Vector3.zero);
    public ChangeItemVector2 m_spacing = new ChangeItemVector2(new Vector2(0.5f, 0.5f), Vector2.zero);
    public ChangeItemVector3 m_rotationDeltaMin = new ChangeItemVector3(Vector3.zero, Vector3.one);
    public ChangeItemVector3 m_rotationDeltaMax = new ChangeItemVector3(new Vector3(30, 360, 30), Vector3.one);
    public ChangeItemVector3 m_scaleDeltaMin = new ChangeItemVector3(new Vector3(35f, 35f, 35f), Vector3.zero);
    public ChangeItemVector3 m_scaleDeltaMax = new ChangeItemVector3(new Vector3(50f, 50f, 50f), Vector3.zero);
    public ChangeItemInt m_density = new ChangeItemInt(100, 0);
    public ChangeItemBool m_createMask = new ChangeItemBool(false, true);
    public ChangeItemFloat m_brushOpacity = new ChangeItemFloat(100f, 0f);
    public ChangeItemFloat m_brushSize = new ChangeItemFloat(16f, 0f);

    /// <summary>
    /// Paint modes
    /// </summary>
    public enum PaintModes
    {
        DRAW,
        ERASE,
        MAX
    }

    /// <summary>
    /// The default paint mode
    /// </summary>
    [HideInInspector]
    public PaintModes m_paintMode = PaintModes.DRAW;

    /// <summary>
    /// The color of the brush
    /// </summary>
    public Color m_brushColor = Color.white;

    /// <summary>
    /// Record all the points used by the item
    /// </summary>
    [HideInInspector]
    public List<Vector2> m_points = new List<Vector2>();
    public void AddPoint(Vector2 p)
    {
        m_points.Add(p);
    }
    public void ClearPoints()
    {
        m_points.Clear();
    }

    /// <summary>
    ///Check if a point is too close 
    /// </summary>
    /// <param name="p">
    /// A <see cref="Vector2"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.Boolean"/>
    /// </returns>
    public bool IsTooClose(Vector2 p)
    {
        foreach (Vector2 old in m_points)
        {
            if ((Mathf.Abs(p.x - old.x) < m_spacing.m_value.x) &&
                (Mathf.Abs(p.y - old.y) < m_spacing.m_value.y))
            {
                return true;
            }
        }
        return false;
    }
	
	/// <summary>
	/// A list of spawned children
	/// </summary>
    [HideInInspector]
	public List<GameObject> m_children = new List<GameObject>();

    [HideInInspector]
    public GameObject m_brushOverlay = null;

    void OnGUI()
    {
        if (null != Event.current &&
            Event.current.type == EventType.MouseMove)
        {
            Vector3 mousePosition = Event.current.mousePosition;
            GUILayout.Label(string.Format("Mouse ({0}, {1})", mousePosition.x, mousePosition.y));
            Debug.Log(string.Format("Mouse ({0}, {1})", mousePosition.x, mousePosition.y));
        }
    }
}