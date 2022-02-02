using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    public struct LineData
    {
        public LineData(LineRenderer lr)
        {
            renderer = lr;
            available = false;
            activated = false;
        }

        public LineRenderer renderer;
        public bool activated;
        public bool available;
    }

    public static TreeController instance;
    public Camera treeCam;
    public RectTransform popUp;
    public TMPro.TextMeshProUGUI nodeName;
    public TMPro.TextMeshProUGUI nodeInfo;
    public Material lrmat;
    public Gradient colorBaseLine;
    public Gradient colorAvailableLine;
    public Gradient colorActiveLine;
    public Gradient colorHiLineParent;
    public Gradient colorHiLineChild;
    public TMPro.TextMeshProUGUI pointcounter;
    public GameObject errorbox;
    public TMPro.TextMeshProUGUI errortext;
    public Dictionary<UpgradeNode.NodePair, LineData> renderers = new Dictionary<UpgradeNode.NodePair, LineData>();
    public List<UpgradeNode> available = new List<UpgradeNode>();
    private List<UpgradeNode> allNodes = new List<UpgradeNode>();
    public GameObject lineContainer;
    public Vector2 boxLimitForTree = new Vector2(300f, 300f);
    public Vector2 zoomLimitsForTree = new Vector2(5f, 30f);

    static Vector3 startMousePos;
    static bool drag = false;
    [SerializeField] private int startUpgradePoints = 0;
    static int upgradePoints = 0;
    public static int UpgradePoints
    {
        get
        {
            return upgradePoints;
        }
        set
        {
            upgradePoints = value;
            instance.pointcounter.text = string.Format("Points: {0}", upgradePoints);
        }
    }

    public List<int> treeState = new List<int>();

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
        UpgradePoints = startUpgradePoints;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camPos = treeCam.transform.position;
        Vector3 mousePosition = treeCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(camPos.z - transform.position.z)));
        popUp.position = mousePosition;
        if (popUp.localPosition.x > camPos.x + 180f) { popUp.pivot = new Vector2(1.2f, popUp.pivot.y); }
        else if (popUp.localPosition.x < camPos.x - 180f) { popUp.pivot = new Vector2(-0.2f, popUp.pivot.y); }
        if (popUp.localPosition.y > camPos.y + 100f) { popUp.pivot = new Vector2(popUp.pivot.x, 1.2f); }
        else if (popUp.localPosition.y < camPos.y - 100f) { popUp.pivot = new Vector2(popUp.pivot.x, -0.2f); }

        if (Input.mouseScrollDelta.y < 0f)
        {
            treeCam.orthographicSize = Mathf.Clamp(treeCam.orthographicSize + 1f, zoomLimitsForTree.x, zoomLimitsForTree.y);
        }
        else if (Input.mouseScrollDelta.y > 0f)
        {
            treeCam.orthographicSize = Mathf.Clamp(treeCam.orthographicSize - 1f, zoomLimitsForTree.x, zoomLimitsForTree.y);
        }

        if (!drag) { return; }
        treeCam.transform.position += (startMousePos - mousePosition);
        camPos = treeCam.transform.position;
        treeCam.transform.position = new Vector3(Mathf.Clamp(camPos.x, -boxLimitForTree.x, boxLimitForTree.x), Mathf.Clamp(camPos.y, -boxLimitForTree.y, boxLimitForTree.y), camPos.z);
    }

    public void ResetSkillTree()
    {
        available.Clear();
        for (int a = 0; a < allNodes.Count; a++)
        {
            allNodes[a].ResetMe();
            allNodes[a].Initialize();
        }
        treeState.Clear();
    }

    public void OnMouseDown()
    {
        startMousePos = treeCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(treeCam.transform.position.z - transform.position.z)));
        drag = true;
    }

    public void OnMouseUp()
    {
        drag = false;
    }

    public static void ResetValueBools(UpgradeNode.NodePair key)
    {
        LineData ld = instance.renderers[key];
        ld.activated = false;
        ld.available = false;
        ld.renderer.colorGradient = instance.colorBaseLine;
        instance.renderers[key] = ld;
    }

    public static void AddAsNode(UpgradeNode node)
    {
        if (instance.allNodes.Contains(node)) { return; }
        instance.allNodes.Add(node);
        if (instance.treeState.Contains(node.id)) { node.Allocate(); }
    }

    public static UpgradeNode GetNodeByID(int findid)
    {
        UpgradeNode un = instance.allNodes.Find(p => p.id == findid);
        return un;
    }

    public static void AddAsAvailable(UpgradeNode node)
    {
        if (instance.available.Contains(node)) { return; }
        instance.available.Add(node);
        node.SetLinesToThis();
        node.SetAvailable();
    }

    public static void SetPopUp(string name, string info)
    {
        instance.popUp.gameObject.SetActive(true);
        instance.nodeName.text = name;
        instance.nodeInfo.text = info;
    }

    public static void DrawErrorMessage(string msg)
    {
        instance.errorbox.SetActive(true);
        instance.errortext.text = msg;
        instance.Invoke("HideErrorMessage", 3f);
    }

    public void HideErrorMessage()
    {
        errorbox.SetActive(false);
    }

    public static void AddToState(int id)
    {
        if (!instance.treeState.Contains(id)) { instance.treeState.Add(id); }
    }

    public static void DrawLine(UpgradeNode a, UpgradeNode b)
    {
        UpgradeNode.NodePair nodep = new UpgradeNode.NodePair(a.id, b.id);
        if (instance.renderers.ContainsKey(nodep)) { Debug.Log(string.Format("Line{0}-{1}", a.id, b.id)); return; }
        GameObject line = new GameObject(string.Format("Line{0}-{1}", a.id, b.id));
        line.transform.parent = instance.lineContainer.transform;
        LineRenderer nlr = line.AddComponent<LineRenderer>();
        instance.renderers.Add(nodep, new LineData(nlr));
        nlr.SetPosition(0, a.transform.position);
        nlr.SetPosition(1, b.transform.position);
        nlr.material = instance.lrmat;
        nlr.sortingOrder = -10;
        nlr.colorGradient = instance.colorBaseLine;
    }

    public static void HighLightLine(UpgradeNode.NodePair line, bool mode, bool parent = true)
    {
        if (mode)
        {
            if (parent) { instance.renderers[line].renderer.colorGradient = instance.colorHiLineParent; }
            else { instance.renderers[line].renderer.colorGradient = instance.colorHiLineChild; }
            
        }
        else
        {
            if (instance.renderers[line].activated) { instance.renderers[line].renderer.colorGradient = instance.colorActiveLine; }
            else if (instance.renderers[line].available) { instance.renderers[line].renderer.colorGradient = instance.colorAvailableLine; }
            else { instance.renderers[line].renderer.colorGradient = instance.colorBaseLine; }
            
        }
    }

    public static void ActivateLine(UpgradeNode.NodePair line, bool active = true)
    {
        LineData dat = instance.renderers[line];
        dat.activated = active;
        instance.renderers[line] = dat;
        if (!active) { return; }
        instance.renderers[line].renderer.colorGradient = instance.colorActiveLine;
    }

    public static void AvailableLine(UpgradeNode.NodePair line, bool available)
    {
        LineData dat = instance.renderers[line];
        dat.available = available;
        instance.renderers[line] = dat;
        HighLightLine(line, false);
    }

    public static void HidePopUp()
    {
        instance.popUp.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
