using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
namespace GraphLibrary
{
    public static class ExtensionMethods
    {
        ///<summary>hasID filters out any object that doesn't have the correct Guid</summary>
        ///<param name="graphobjects">the input to be filtered.</param>
        ///<param name="id">The guid you are searching for.</param>
        ///<returns>An IEnumerable of GraphObjects</returns>        
        public static IEnumerable<T> hasID<T> (this IEnumerable<T> graphobjects, Guid id) where T : GraphObject
        {
            return graphobjects.Where(graphobject => graphobject.ID == id).ToList();
        }
        /// <summary>
        /// hasLabel filters out any node that doesn't have the correct label
        /// </summary>
        /// <param name="graphnodes">input to be filtered</param>
        /// <param name="labels">the labels to filter on</param>
        /// <returns></returns>
        public static IEnumerable<GraphNode> hasLabel (this List<GraphNode> graphnodes, params string[] labels)
        {
            foreach (GraphNode node in graphnodes)
            {
                if (node.Labels.Intersect(labels).Count() > 0)
                {
                    yield return node;
                }
            }
            yield break;
        }
        /// <summary>
        /// filters out any Objects that don't contain a specified property
        /// </summary>
        /// <param name="graphobjects"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static IEnumerable<GraphObject> hasKey (this IEnumerable<GraphObject> graphobjects,params string[] keys) 
        {
            foreach (GraphObject graphobject in graphobjects)
            {
                if (graphobject.Properties.Keys.Intersect(keys).Count() > 0)
                {
                    yield return graphobject;
                }
            }
            yield break;
        }
        public static IEnumerable<string> values<T> (this IEnumerable<T> graphobjects, params string[] keys) where T : GraphObject
        {
            foreach (GraphObject graphobject in graphobjects) 
            {
                foreach (string key in keys)
                {
                    string value = null;
                    if (graphobject.Properties.TryGetValue(key,out value))
                    {
                        yield return value;
                    }
                }
            }
            yield break;
        }
        /// <summary>
        /// Method returns the nodes related to the current set of notes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<GraphObject> outV (this IEnumerable<GraphObject> nodes)
        {
            foreach (GraphNode node in nodes)
            {
                foreach (GraphRelation relation in node.Relationships)
                {
                    //GraphNode result = relation.RightNode;
                    yield return relation.RightNode;
                } 
            }
            yield break;
        }
        public static IEnumerable<GraphRelation> outE (this IEnumerable<GraphObject> nodes) 
        {
            foreach (GraphNode node in nodes)
            {
                foreach (GraphRelation relation in node.Relationships)
                {
                    //GraphNode result = relation.RightNode;
                    yield return relation;
                } 
            }
            yield break;
        }
    }
    /// <summary>
    /// The GraphObject contains some common properties for nodes and edges to inhereit.
    /// </summary>
    public class GraphObject 
    {
        public Guid ID;
        public Dictionary<string,string> Properties = new Dictionary<string,string>();
        public GraphObject () :this(Guid.NewGuid(),new Dictionary<string,string>()) { }
        public GraphObject (Guid guid) : this (guid,new Dictionary<string,string>()) { }
        public GraphObject (Dictionary<string,string> properties) : this(Guid.NewGuid(),properties) { }
        public GraphObject (Guid guid, Dictionary<string,string> properties)
        {
            ID = guid;
            Properties = properties;
        }
    }
    /// <summary>
    /// GraphNodes are the vertices of the graph. These are for containing the records of the database.
    /// </summary>
    public class GraphNode : GraphObject , IEquatable<GraphNode>
    {
        public List<GraphRelation> Relationships = new List<GraphRelation>();
        //public List<string> Labels;
        public List<string> Labels = new List<string>();
        public GraphNode (params string[] label) : this (Guid.NewGuid(),new Dictionary<string,string>(),label) { }
        public GraphNode (Guid guid, params string[] label) : this (guid, new Dictionary<string,string>(), label) { }
        public GraphNode (Dictionary<string,string> properties,params string[] label) : this(Guid.NewGuid(),properties,label) { }
        public GraphNode (Guid guid, Dictionary<string,string> properties, params string[] label) : base (guid,properties)
        {
            foreach (string value in label)
            {
                this.AddLabel(value);
            }
        }
        public void AddLabel(string label)
        {
            Labels.Add(label);
        }
        public bool Equals(GraphNode OtherNode)
        {
            return (this.ID == OtherNode.ID);
        }
    }
    /// <summary>
    /// This class holds the nodes and edges of the database.
    /// </summary>
    public class GraphDatabase 
    {
        private List<GraphNode> NodeTable = new List<GraphNode>();
        private List<GraphRelation> RelationTable = new List<GraphRelation>();
        /// <summary>
        /// The AddNode method adds a new node to the database.
        /// </summary>
        /// <param name="node">This should be a <see cref="GraphNode"/> object.</param>
        public void AddNode (GraphNode node) 
        {
            NodeTable.Add(node);
        }
        /// <summary>
        /// This method adds nodes and relations from an XML file.
        /// </summary>
        /// <param name="filename">This should be an xml file created using the Serialize method.</param>
        public void LoadXML(string filename){
            //init a new document object
            XmlDocument XmlData = new XmlDocument();
            //load the xml file
            XmlData.Load(filename);
            //get our nodes and relations
            XmlNodeList Nodes = XmlData.SelectNodes("/Graph/Node");
            XmlNodeList Relations = XmlData.SelectNodes("/Graph/Relation");
            //process nodes
            foreach (XmlNode node in Nodes)
            {
                //get the node Guid
                string Id = node.SelectSingleNode("@Id").Value;
                Guid guid = new Guid(Id);
                //init the node object
                GraphNode NewNode = new GraphNode(guid);
                //add labels to the node object
                foreach (XmlNode LabelNode in node.SelectNodes("Label"))
                {
                    NewNode.Labels.Add(LabelNode.InnerText);
                }
                //add properties to the node object
                foreach (XmlNode property in node.SelectNodes("Property"))
                {
                    string PropKey = property.Attributes.GetNamedItem("Name").Value;
                    string PropValue = property.InnerText;
                    NewNode.Properties.Add(PropKey,PropValue);
                }
                //Add the node to the database object
                this.AddNode(NewNode);
            }
            //process relations
            foreach (XmlNode relation in Relations)
            {
                Guid Id = new Guid(relation.Attributes.GetNamedItem("Id").Value);
                string type = relation.Attributes.GetNamedItem("Type").Value;
                Guid LeftNodeGuid = new Guid(relation.Attributes.GetNamedItem("LeftNode").Value);
                Guid RightNodeGuid = new Guid(relation.Attributes.GetNamedItem("RightNode").Value);
                GraphRelation NewRelation = new GraphRelation(Id,type,this.V().hasID(LeftNodeGuid).Single(),this.V().hasID(RightNodeGuid).Single());
                foreach (XmlNode property in relation.SelectNodes("Property"))
                {
                    string PropKey = property.Attributes.GetNamedItem("Name").Value;
                    string PropValue = property.InnerText;
                    NewRelation.Properties.Add(PropKey,PropValue);
                }
                this.AddRelationship(NewRelation);
            }
        }
        public List<GraphNode> V () 
        {
            return NodeTable;
        }
        public void AddRelationship (GraphRelation relation)
        {
            RelationTable.Add(relation);
            relation.LeftNode.Relationships.Add(relation);
        }
        public void Serialize (string filename)
        {
            // setup our settings for the xml writer. I want it to 
            // indent lines for easy reading
            XmlWriterSettings WriterOpts = new XmlWriterSettings();
            WriterOpts.Indent = true;
            WriterOpts.IndentChars = "\t";
            // create the xml writer object
            XmlWriter Writer = XmlWriter.Create(filename, WriterOpts);
            Writer.WriteStartDocument();
            // our root node is called Graph. It should have a version
            // number in case I change the format later
            Writer.WriteStartElement("Graph");
            Writer.WriteAttributeString("Version", "1.0.0");
            // Add list of nodes to xml
            foreach (GraphNode node in this.NodeTable)
            {
                Writer.WriteStartElement("Node");
                Writer.WriteAttributeString("Id", node.ID.ToString());
                foreach (string label in node.Labels)
                {
                    Writer.WriteElementString("Label", label);
                }
                foreach (string key in node.Properties.Keys)
                {
                    Writer.WriteStartElement("Property");
                    Writer.WriteAttributeString("Name", key);
                    Writer.WriteString(node.Properties[key]);
                    Writer.WriteEndElement();
                }
                Writer.WriteEndElement();
            }
            // Add list of relations/edges to xml
            foreach (GraphRelation relation in this.RelationTable)
            {
                Writer.WriteStartElement("Relation");
                Writer.WriteAttributeString("Id", relation.ID.ToString());
                Writer.WriteAttributeString("Type", relation.RelationshipType);
                Writer.WriteAttributeString("LeftNode", relation.LeftNode.ID.ToString());
                Writer.WriteAttributeString("RightNode", relation.RightNode.ID.ToString());
                foreach (string key in relation.Properties.Keys)
                {
                    Writer.WriteElementString("Property", relation.Properties[key]);
                    Writer.WriteAttributeString("Name", key);
                }
                Writer.WriteEndElement();
            }
            //Finish the document
            Writer.WriteEndElement();
            Writer.WriteEndDocument();
            Writer.Flush();
            Writer.Close();
        }

    }
    public class GraphRelation : GraphObject, IEquatable<GraphRelation>
    {
        public GraphNode LeftNode;
        public GraphNode RightNode;
        public string RelationshipType;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relationshiptype"></param>
        /// <param name="leftnode"></param>
        /// <param name="rightnode"></param>
        public GraphRelation (string relationshiptype, GraphNode leftnode, GraphNode rightnode) : this(Guid.NewGuid(),relationshiptype,new Dictionary<string,string>(),leftnode,rightnode) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="relationshiptype"></param>
        /// <param name="leftnode"></param>
        /// <param name="rightnode"></param>
        public GraphRelation (Guid id, string relationshiptype, GraphNode leftnode, GraphNode rightnode) : this(id, relationshiptype,new Dictionary<string,string>(),leftnode,rightnode) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relationshiptype"></param>
        /// <param name="properties"></param>
        /// <param name="leftnode"></param>
        /// <param name="rightnode"></param>
        public GraphRelation (string relationshiptype, Dictionary<string, string> properties, GraphNode leftnode, GraphNode rightnode) : this(Guid.NewGuid(),relationshiptype,properties,leftnode,rightnode) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="relationshiptype"></param>
        /// <param name="properties"></param>
        /// <param name="leftnode"></param>
        /// <param name="rightnode"></param>
        public GraphRelation (Guid id, string relationshiptype, Dictionary<string, string> properties, GraphNode leftnode, GraphNode rightnode) : base(id,properties)
        {
            LeftNode = leftnode;
            RelationshipType = relationshiptype;
            RightNode = rightnode;
        }
        public bool Equals(GraphRelation OtherRelation)
        {
            return (this.ID == OtherRelation.ID);
        }
    }
}