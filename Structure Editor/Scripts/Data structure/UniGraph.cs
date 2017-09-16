using UnityEngine;
using System.Collections.Generic;
using System;

public class UniGraph <TNode, TLink> : ISerializationCallbackReceiver
{
    [SerializeField] protected TNode[] m_nodeValuesSerialized;
    [SerializeField] protected TLink[] m_linkValuesSerialized;
    [SerializeField] protected int[] m_fromSerialized;
    [SerializeField] protected int[] m_toSerialized;
    [SerializeField] protected LinkType[] m_linksTypesSerizalized;

    protected List<UniNode> m_nodes;

    public class UniNode 
    {
        protected List<UniLink> m_links = new List<UniLink>();
        protected TNode m_value;

        public UniNode(TNode nodeValue)
        {
            m_value = nodeValue;
        }

        public TNode value
        {
            get { return m_value; }
        }

        public TNode[] GetLinkedNodesValues(LinkType linkType)
        {
            UniNode[] linkedNodes = GetLinkedNodes(linkType);
            if (linkedNodes == null) return null;

            TNode[] linkedNodesValues = new TNode[linkedNodes.Length];
            for (int i = 0; i < linkedNodes.Length; i++)
            {
                linkedNodesValues[i] = linkedNodes[i].value;
            }

            return linkedNodesValues;
        }

        public TLink[] GetLinksValues(LinkType linkType)
        {
            UniLink[] links = GetLinks(linkType);
            if (links == null) return null;

            TLink[] linkedNodesValues = new TLink[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                linkedNodesValues[i] = links[i].value;
            }

            return linkedNodesValues;
        }

        #region Nodes
        public UniNode[] GetLinkedNodes(bool removeDoubles = true)
        {
            List<UniNode> linkedNodes = new List<UniNode>();
            for(int i = 0; i < m_links.Count; i++)
            {
                UniNode other = m_links[i].other;
                if(removeDoubles)
                {
                    if(!linkedNodes.Contains(other))
                    {
                        linkedNodes.Add(other);
                    }
                }
                else
                {
                    linkedNodes.Add(other);
                }
            }
            return linkedNodes.ToArray();
        }

        public UniNode[] GetLinkedNodes(out TLink[] linkValues)
        {
            List<UniNode> linkedNodes = new List<UniNode>();
            List<TLink> linksValuesList = new List<TLink>();

            for (int i = 0; i < m_links.Count; i++)
            {
                linkedNodes.Add(m_links[i].other);
                linksValuesList.Add(m_links[i].value);
            }

            linkValues = linksValuesList.ToArray();
            return linkedNodes.ToArray();
        }

        public UniNode[] GetLinkedNodes(LinkType linkType, bool removeDoubles = true)
        {
            List<UniNode> linkedNodes = new List<UniNode>();

            for (int i = 0; i < m_links.Count; i++)
            {
                if (m_links[i].type == linkType)
                {
                    if(removeDoubles)
                    {
                        if (!linkedNodes.Contains(m_links[i].other))
                        {
                            linkedNodes.Add(m_links[i].other);
                        }
                    }
                    else
                    {
                        linkedNodes.Add(m_links[i].other);
                    }
                }
            }

            return linkedNodes.ToArray();
        }

        public UniNode[] GetLinkedNodes(LinkType linkType, out TLink[] linkValues)
        {
            List<UniNode> linkedNodes = new List<UniNode>();
            List<TLink> linksValuesList = new List<TLink>();

            for (int i = 0; i < m_links.Count; i++)
            {
                if (m_links[i].type == linkType)
                {
                    linkedNodes.Add(m_links[i].other);
                    linksValuesList.Add(m_links[i].value);
                }
            }

            linkValues = linksValuesList.ToArray();
            return linkedNodes.ToArray();
        }

        public UniNode[] GetLinkedNodesRecursive(LinkType linkType)
        {
            List<UniNode> linkedNodes = new List<UniNode>(GetLinkedNodes(linkType));
            List<UniNode> previouslyDiscoveredNodes = new List<UniNode>(linkedNodes);

            while (previouslyDiscoveredNodes.Count > 0)
            {
                List<UniNode> newNodes = new List<UniNode>();
                for (int i = 0; i < previouslyDiscoveredNodes.Count; i++)
                {
                    newNodes.AddRange(previouslyDiscoveredNodes[i].GetLinkedNodes(linkType));
                }

                previouslyDiscoveredNodes.Clear();
                for (int i = 0; i < newNodes.Count; i++)
                {
                    if (!linkedNodes.Contains(newNodes[i]))
                    {
                        linkedNodes.Add(newNodes[i]);
                        previouslyDiscoveredNodes.Add(newNodes[i]);
                    }
                }
            }

            return linkedNodes.ToArray();
        }
        #endregion
        #region Links
        public void AddLink(UniLink link)
        {
            if(m_links == null) m_links = new List<UniLink>();
            if (!m_links.Contains(link)) m_links.Add(link);
        }

        public UniLink[] GetLinks()
        {
            if (m_links == null) return null;
            return m_links.ToArray();
        }

        public UniLink[] GetLinks(LinkType linkType)
        {
            if (m_links == null) return null;
            List<UniLink> linksWithOther = new List<UniLink>();
            foreach (UniLink link in m_links)
            {
                if (link.type == linkType)
                {
                    linksWithOther.Add(link);
                }
            }
            return linksWithOther.ToArray();
        }

        public UniLink[] GetLinks(UniNode other, LinkType linkType)
        {
            if (m_links == null) return null;
            List<UniLink> linksWithOther = new List<UniLink>();
            foreach(UniLink link in m_links)
            {
                if ((link.other == other) && (link.type == linkType))
                {
                    linksWithOther.Add(link);
                }
            }
            return linksWithOther.ToArray();
        }

        public UniLink GetLink(UniNode other, LinkType linkType)
        {
            if (m_links == null) return null;
            foreach (UniLink link in m_links)
            {
                if ((link.other == other) && (link.type == linkType))
                {
                    return link;
                }
            }
            return null;
        }

        public void RemoveLinks(UniNode node)
        {
            List<UniLink> linksToRemove = new List<UniLink>();
            foreach(UniLink link in m_links)
            {
                if(link.other == node)
                {
                    linksToRemove.Add(link);
                }
            }

            foreach (UniLink link in linksToRemove) m_links.Remove(link);
        }

        public bool HasLink(UniNode other)
        {
            foreach(UniLink link in m_links)
            {
                if(link.other == other)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasLink(UniNode other, LinkType linkType)
        {
            foreach (UniLink link in m_links)
            {
                if ((link.other == other) && (link.type == linkType))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }

    public class UniLink
    {
        protected UniNode m_other;
        protected TLink m_value;
        protected LinkType m_type;

        public UniLink(UniNode other, TLink value, LinkType type)
        {
            m_other = other;
            m_value = value;
            m_type = type;
        }

        public UniNode other
        {
            get { return m_other; }
        }

        public TLink value
        {
            get { return m_value; }
        }

        public LinkType type
        {
            get { return m_type; }
        }

        public bool MatchesValue(TLink value)
        {
            if (m_value == null)
            {
                return (value == null);
            }

            return m_value.Equals(value);
        }
    }

    public enum LinkType
    {
        OneWay,
        Reverse,
        Bitypenal
    }

    static public LinkType ReverseDirection(LinkType type)
    {
        switch(type)
        {
            case LinkType.OneWay: return LinkType.Reverse;
            case LinkType.Reverse: return LinkType.OneWay;
            default: return LinkType.Bitypenal;
        }
    }

    #region Constructors
    /// <summary>
    /// Instantiate a new UniGraph.
    /// Lists (of nodes and links) use the default capacity defined by System.Collections.Generic.List.
    /// </summary>
    public UniGraph()
    {
        m_nodes = new List<UniNode>();
    }

    /// <summary>
    /// Instantiate a new UniGraph.
    /// Nodes list uses the given capacity.
    /// </summary>
    /// <param name="nodesCapacity">Capacity of the Nodes list.</param>
    public UniGraph(int nodesCapacity)
    {
        m_nodes = new List<UniNode>(nodesCapacity);
    }
    #endregion

    #region (De)Serialisation
    public void OnBeforeSerialize()
    {
        #region Nodes serialization
        if(m_nodes == null)
        {
            m_nodes = new List<UniNode>();
        }

        m_nodeValuesSerialized = GetNodesValues();
        #endregion
        #region Links serialization
        List<UniNode> fromNodes;
        List<UniLink> links = GetLinks(out fromNodes);

        m_linkValuesSerialized = new TLink[links.Count];
        m_fromSerialized = new int[links.Count];
        m_toSerialized = new int[links.Count];
        m_linksTypesSerizalized = new LinkType[links.Count];

        for (int i = 0; i < links.Count; i++)
        {
            int fromIndex = GetNodeIndex(fromNodes[i]);
            int toIndex = GetNodeIndex(links[i].other);

            if((fromIndex < 0) || (toIndex < 0))
            {
                continue;
            }

            m_fromSerialized[i] = fromIndex;
            m_toSerialized[i] = toIndex;
            m_linkValuesSerialized[i] = links[i].value;
            m_linksTypesSerizalized[i] = links[i].type;
        }
        #endregion
    }

    public void OnAfterDeserialize()
    {
        #region Nodes deserialization
        if(m_nodeValuesSerialized == null)
        {
            m_nodes = new List<UniNode>();
        }
        else
        {
            m_nodes = new List<UniNode>(m_nodeValuesSerialized.Length);
            for(int i = 0; i < m_nodeValuesSerialized.Length; i++)
            {
                m_nodes.Add(new UniNode(m_nodeValuesSerialized[i]));
            }
        }
        
        #endregion
        #region Links deserialization
        if (m_linkValuesSerialized != null)
        {
            for (int i = 0; i < m_linkValuesSerialized.Length; i++)
            {
                int fromIndex = m_fromSerialized[i];
                int toIndex = m_toSerialized[i];
                m_nodes[fromIndex].AddLink(new UniLink(m_nodes[toIndex], m_linkValuesSerialized[i], m_linksTypesSerizalized[i]));
            }
        }
        #endregion
    }
    #endregion

    #region Getters
    public TNode[] GetNodesValues()
    {
        TNode[] nodeValues = new TNode[m_nodes.Count];
        for (int i = 0; i < m_nodes.Count; i++)
        {
            nodeValues[i] = m_nodes[i].value;
        }
        return nodeValues;
    }

    public TNode[] GetLinkedNodesValues(TNode nodeValue, LinkType linkType)
    {
        UniNode node = GetNode(nodeValue);
        if(node == null) return null;

        return node.GetLinkedNodesValues(linkType);
    }

    public TLink[] GetLinksValues(TNode nodeValue, LinkType linkType)
    {
        UniNode node = GetNode(nodeValue);
        if (node == null) return null;

        return node.GetLinksValues(linkType);
    }

    public TLink GetLinkValueBetween(TNode fromValue, TNode toValue, LinkType linkType)
    {
        UniNode from = GetNode(fromValue);
        if (from == null) return default(TLink);
        UniNode to = GetNode(toValue);
        if (to == null) return default(TLink);

        UniLink link = from.GetLink(to, linkType);
        if (link == null) return default(TLink);
        return link.value;
    }

    public bool Contains(TNode nodeValue)
    {
        if (m_nodes == null) return false;
        foreach (UniNode node in m_nodes) if (node.value.Equals(nodeValue)) return true;
        return false;
    }
    #region Protected (helpers)
    #region Nodes
    protected int GetNodeIndex(UniNode node)
    {
        if(m_nodes == null)
        {
            return -1;
        }

        for(int i = 0; i < m_nodes.Count; i++)
        {
            if(m_nodes[i] == node)
            {
                return i;
            }
        }

        return -1;
    }

    protected int GetNodeIndex(TNode nodeValue)
    {
        if (m_nodes == null)
        {
            return -1;
        }

        for (int i = 0; i < m_nodes.Count; i++)
        {
            if (m_nodes[i].value.Equals(nodeValue))
            {
                return i;
            }
        }

        return -1;
    }

    protected UniNode GetNode(TNode nodeValue)
    {
        if (m_nodes == null) return null;

        for (int i = 0; i < m_nodes.Count; i++)
        {
            if (m_nodes[i].value.Equals(nodeValue))
            {
                return m_nodes[i];
            }
        }

        return null;
    }

    #endregion
    #region Links
    protected UniLink GetLink(UniNode from, UniNode to, LinkType linkType)
    {
        if ((m_nodes == null) || (from == null) || (to == null))  return null;

        UniLink[] links = from.GetLinks(to, linkType);
        if ((links == null) || (links.Length == 0)) return null;

        return links[0];
    }

    protected UniLink GetLink(UniNode from, UniNode to, LinkType linkType, TLink linkValue)
    {
        if ((m_nodes == null) || (from == null) || (to == null))  return null;

        UniLink[] links = from.GetLinks(to, linkType);
        if ((links == null) || (links.Length == 0)) return null;

        for (int i = 0; i < links.Length; i++)
        {
            if(links[i].MatchesValue(linkValue))
            {
                return links[i];
            }
        }

        return null;
    }

    protected bool HasLink(UniNode from, UniNode to, LinkType linkType, TLink linkValue)
    {
        return GetLink(from, to, linkType, linkValue) != null;
    }

    protected List<UniLink> GetLinks(out List<UniNode> fromNodes)
    {
        List<UniLink> links = new List<UniLink>();
        fromNodes = new List<UniNode>();
        foreach (UniNode node in m_nodes)
        {
            UniLink[] nodeLinks = node.GetLinks();
            links.AddRange(nodeLinks);
            for(int i = 0; i < nodeLinks.Length; i++)
            {
                fromNodes.Add(node);
            }
        }
        return links;
    }
    #endregion
    #endregion
    #endregion

    #region Mutation
    /// <summary>
    /// Adds a node with the given value to the graph.
    /// Note : No links are created during this process.
    /// </summary>
    /// <param name="nodeValue"></param>
    public void AddNode(TNode nodeValue)
    {
        if(m_nodes == null)
        {
            m_nodes = new List<UniNode>();
        }
        m_nodes.Add(new UniNode(nodeValue));
    }

    /// <summary>
    /// Creates and adds a link between nodes having given values.
    /// If multiple nodes have the same value, it is only added to the first one.
    /// </summary>
    /// <param name="fromValue">Value of the origin node.</param>
    /// <param name="toValue">Value of the destination node.</param>
    /// <param name="linkValue">Value attached to the new link.</param>
    /// <param name="linkType">Type of link.</param>
    public void AddLink(TNode fromValue, TNode toValue, TLink linkValue, LinkType linkType)
    {
        UniNode from = GetNode(fromValue);
        UniNode to = GetNode(toValue);

        if ((from == null) || (to == null))
        {
            return;
        }

        AddLink(from, to, linkValue, linkType);
    }

    /// <summary>
    /// Creates and adds a link between nodes having given values.
    /// If multiple nodes have the same value, it is only added to the first one.
    /// </summary>
    /// <param name="fromValue">Value of the origin node.</param>
    /// <param name="toValue">Value of the destination node.</param>
    /// <param name="linkValue">Value attached to the new link.</param>
    /// <param name="linkType">Type of link.</param>
    /// <param name="reverseWayValue">A second link is created, whose type is linkType reversed and value is this parameter.</param>
    public void AddLink(TNode fromValue, TNode toValue, TLink linkValue, TLink reverseWayValue, LinkType linkType)
    {
        UniNode from = GetNode(fromValue);
        UniNode to = GetNode(toValue);

        if ((from == null) || (to == null))
        {
            return;
        }

        AddLink(from, to, linkValue, reverseWayValue, linkType);
    }

    public void RemoveOne(TNode nodeValue)
    {
        RemoveNode(GetNode(nodeValue));
    }

    public void RemoveAll(TNode nodeValue)
    {
        List<UniNode> nodesToRemove = new List<UniNode>();
        foreach (UniNode node in m_nodes)
        {
            if (nodeValue.Equals(node.value))
            {
                nodesToRemove.Add(node);
            }
        }

        for(int i = 0; i < nodesToRemove.Count; i++)
        {
            RemoveNode(nodesToRemove[i]);
        }
    }

    public void Clear()
    {
        m_nodes.Clear();
    }

    #region Protected (helpers)
    protected UniNode AddNodeR(TNode nodeValue)
    {
        if (m_nodes == null)
        {
            m_nodes = new List<UniNode>();
        }
        UniNode node = new UniNode(nodeValue);
        m_nodes.Add(node);
        return node;
    }

    protected void AddLink(UniNode from, UniNode to, TLink linkValue, LinkType linkType)
    {
        UniLink linkFromOrigin = new UniLink(to, linkValue, linkType);
        from.AddLink(linkFromOrigin);

        UniLink linkFromDest = new UniLink(from, linkValue, ReverseDirection(linkType));
        to.AddLink(linkFromDest);
    }

    protected void AddLink(UniNode from, UniNode to, TLink linkValue, TLink reverseWayValue, LinkType linkType)
    {
        UniLink linkFromOrigin = new UniLink(to, linkValue, linkType);
        from.AddLink(linkFromOrigin);

        UniLink linkFromDest = new UniLink(from, reverseWayValue, ReverseDirection(linkType));
        to.AddLink(linkFromDest);
    }

    protected void RemoveNode(UniNode node)
    {
        for(int i = 0; i < m_nodes.Count; i++)
        {
            m_nodes[i].RemoveLinks(node);
        }

        m_nodes.Remove(node);
    }
    #endregion
    #endregion
}