using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Reflection;
namespace GraphLibrary
{
    public static class ExtensionMethods
    {
        ///<summary>hasID filters out any object that doesn't have the correct Guid</summary>
        ///<param name="graphobjects">the input <see cref="IEnumerable<GraphObject>"/> to be filtered.</param>
        ///<param name="id">The <see cref="Guid"/> you are searching for.</param>
        ///<returns>A list containing the object with the given Guid.</returns>        
        public static IEnumerable<GraphObject> hasID (this IEnumerable<GraphObject> graphobjects, Guid id)
        {
            return graphobjects.Where(graphobject => graphobject.ID == id).ToList();
        }
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
        //this has() overload removes any objects that don't have a 
        //property of the right name.
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
        public static IEnumerable<object> values (this IEnumerable<GraphObject> graphobjects, params string[] keys) 
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
/*        public static IEnumerable<GraphObject> has (this IEnumerable<GraphObject> graphobject, string key, string value) 
        {
            if (Properties[key] == value) 
            {
                return this;
            }
            return null;
        }*/
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
    /**
    * <summary>The GraphObject contains some common properties for nodes and edges to inhereit.</summary>
    */
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
    /**
    * <summary>GraphNodes are the vertices of the graph. These are for containing the records of the database.</summary>
    */
    public class GraphNode : GraphObject
    {
        public List<GraphRelation> Relationships = new List<GraphRelation>();
        //public List<string> Labels;
        public string[] Labels;
        public GraphNode (params string[] label) : this (Guid.NewGuid(),new Dictionary<string,string>(),label) { }
        public GraphNode (Guid guid, params string[] label) : this (guid, new Dictionary<string,string>(), label) { }
        public GraphNode (Dictionary<string,string> properties,params string[] label) : this(Guid.NewGuid(),properties,label) { }
        public GraphNode (Guid guid, Dictionary<string,string> properties, params string[] label) : base (guid,properties)
        {
            //Labels.AddRange(label);
            Labels = label;
        }
    }
    /// <summary>
    /// This class holds the nodes and edges of the database.
    /// </summary>
    /// <value>
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
    public class GraphRelation : GraphObject 
    {
        public GraphNode LeftNode;
        public GraphNode RightNode;
        public string RelationshipType;
        public GraphRelation (string relationshiptype, GraphNode leftnode, GraphNode rightnode) : this(Guid.NewGuid(),relationshiptype,new Dictionary<string,string>(),leftnode,rightnode) { }
        public GraphRelation (Guid id, string relationshiptype, GraphNode leftnode, GraphNode rightnode) : this(id, relationshiptype,new Dictionary<string,string>(),leftnode,rightnode) { }
        public GraphRelation (string relationshiptype, Dictionary<string, string> properties, GraphNode leftnode, GraphNode rightnode) : this(Guid.NewGuid(),relationshiptype,properties,leftnode,rightnode) { }
        public GraphRelation (Guid id, string relationshiptype, Dictionary<string, string> properties, GraphNode leftnode, GraphNode rightnode) : base(id,properties)
        {
            LeftNode = leftnode;
            RelationshipType = relationshiptype;
            RightNode = rightnode;
        }
    }
}