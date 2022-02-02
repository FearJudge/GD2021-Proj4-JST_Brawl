using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeNode : MonoBehaviour
{
    public delegate void NodeUpgradeEv(int id, ArgVal[] args);
    public static event NodeUpgradeEv Upgrade;
    public static event NodeUpgradeEv Degrade;

    public struct NodePair
    {
        public NodePair(int start, int end)
        {
            a = start;
            b = end;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(NodePair)) { NodePair np = (NodePair)obj; return ((np.a == a && np.b == b) || (np.a == b && np.b == a)); }
            return base.Equals(obj);
        }

        public int a;
        public int b;
    }

    [System.Serializable]
    public struct ArgVal
    {
        public string argument;
        public int value;
    }

    private static Color BASECOL = new Color(0f, 0f, 0f);
    private static Color AVCOL = new Color(0.4f, 0,4f, 1f);
    public int id;
    public string nodeName;
    [TextArea] public string nodeInformation;

    public UpgradeNode[] children;
    List<UpgradeNode> parents = new List<UpgradeNode>();

    public int costToAllocate = 1;
    public int requiredParentCounts = 1;
    public ArgVal[] parameters = new ArgVal[] { };

    int parentsCalled = 0;
    bool available = false;
    bool allocated = false;
    Color borderc;

    [SerializeField] GameObject border;
    ImageData images;

    struct ImageData
    {
        public SpriteRenderer icon;
        public SpriteRenderer border;
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (var node in children)
        {
            if (node == null) { throw new MissingReferenceException(string.Format("UNASSIGNED NODE! @ node id = {0}", id)); }
            node.parents.Add(this);
            TreeController.DrawLine(this, node);
        }
        images.icon = GetComponent<SpriteRenderer>();
        images.border = border.GetComponent<SpriteRenderer>();
        
        Initialize();
    }

    public void ResetMe()
    {
        parentsCalled = 0;
        if (allocated) { TreeController.UpgradePoints += costToAllocate; Degrade?.Invoke(id, parameters); }
        allocated = false;
        foreach (var parent in parents)
        {
            TreeController.ResetValueBools(new NodePair(id, parent.id));
        }
    }

    public void Initialize()
    {
        borderc = BASECOL;
        images.border.color = borderc;
        TreeController.AddAsNode(this);
        if (requiredParentCounts <= 0) { TreeController.AddAsAvailable(this); }
    }

    public void Call()
    {
        parentsCalled++;
    }

    public void SetLinesToThis()
    {
        foreach (var node in parents)
        {
            if (node.CheckRequiredNodes()) { TreeController.AvailableLine(new NodePair(id, node.id), true); }
        }
    }

    public void SetAvailable()
    {
        if (allocated) { return; }
        borderc = AVCOL;
        if (images.border == null) { return; }
        images.border.color = borderc;
    }

    private void OnMouseEnter()
    {
        if (allocated) { images.border.color = new Color(0.8f, 0.8f, 0.6f); } else { images.border.color = new Color(0.7f, 0.7f, 0.7f); }

        foreach (var parent in parents)
        {
            if (!parent.allocated) {TreeController.HighLightLine(new NodePair(parent.id, id), true); }
        }
        TreeController.SetPopUp(nodeName, nodeInformation);
        if (allocated) { return; }
        foreach (var child in children)
        {
            TreeController.HighLightLine(new NodePair(child.id, id), true, false);
        }
    }

    private void OnMouseExit()
    {
        images.border.color = borderc;
        foreach (var parent in parents)
        {
            if (!parent.allocated) { TreeController.HighLightLine(new NodePair(parent.id, id), false); }
        }
        TreeController.HidePopUp();
        foreach (var child in children)
        {
            TreeController.HighLightLine(new NodePair(child.id, id), false, false);
        }
    }

    private void OnMouseUpAsButton()
    {
        if (!IsThisAllowed(true)) { return; }
        Upgrade?.Invoke(id, parameters);
        Allocate();
    }

    public void Allocate()
    {
        allocated = true;
        foreach (var node in children)
        {
            node.Call();
            if (node.IsThisAllowed()) { TreeController.AddAsAvailable(node); }
            if (node.allocated) { TreeController.ActivateLine(new NodePair(node.id, id)); }
        }
        foreach (var parent in parents)
        {
            if (parent.allocated) { TreeController.ActivateLine(new NodePair(parent.id, id)); }
        }
        TreeController.UpgradePoints -= costToAllocate;
        TreeController.AddToState(id);
        borderc = new Color(1f, 1f, 0.4f);
        images.border.color = borderc;
    }

    public bool IsThisAllowed(bool message = false)
    {
        if (!CheckRequiredNodes()) { if (message) { TreeController.DrawErrorMessage("Required parent(s) not allocated!"); } return false; }
        if (allocated) { if (message) { TreeController.DrawErrorMessage("Node Already Allocated!"); } return false; }
        if (TreeController.UpgradePoints < costToAllocate) { if (message) { TreeController.DrawErrorMessage("Not enough points to Allocate!"); } return false; }
        return true;
    }

    public bool CheckRequiredNodes()
    {
        if (parentsCalled < requiredParentCounts) { return false; }
        return true;
    }
}
