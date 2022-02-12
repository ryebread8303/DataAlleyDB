using System;
using System.Linq;
using Xunit;
using GraphLibrary;
using System.Collections.Generic;
using System.Xml;

namespace UnitTests
{
    public class UnitTest1
    {
        Guid BobsGuid = new Guid("00000000-0000-0000-0000-000000000001");
        Guid AlicesGuid = new Guid("00000000-0000-0000-0000-000000000002");
        Guid RelationGuid = new Guid("00000000-0000-0000-0000-000000000003");
        Guid BadGuid = Guid.NewGuid();
        XmlDocument SerializedDatabase = new XmlDocument();
        Dictionary<string,string> BobsProps = new Dictionary<string, string>();
        GraphDatabase Database = new GraphDatabase();

        Dictionary<string,string> AlicesProps = new Dictionary<string, string>();
        public UnitTest1 ()
        {
            BobsProps.Add("Name","Bob");
            BobsProps.Add("Hobby","Sportsball");
            AlicesProps.Add("Name","Alice");
            AlicesProps.Add("Hobby","trains");
            AlicesProps.Add("Bloodtype","A+");
            GraphNode Bob = new GraphNode(BobsGuid, BobsProps, "Person");
            GraphNode Alice = new GraphNode(AlicesGuid,AlicesProps, "Person");
            GraphRelation Relation = new GraphRelation(RelationGuid,"test",Bob,Alice);
            Database.AddNode(Bob);
            Database.AddNode(Alice);
            Database.AddRelationship(Relation);
            string xmlrepresentation = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Graph Version=""1.0.0"" Application=""DataAlley"">
	<Node Id=""00000000-0000-0000-0000-000000000001"">
		<Label>Person</Label>
		<Property Name=""Name"">Bob</Property>
		<Property Name=""Hobby"">Sportsball</Property>
	</Node>
	<Node Id=""00000000-0000-0000-0000-000000000002"">
		<Label>Person</Label>
		<Property Name=""Name"">Alice</Property>
		<Property Name=""Hobby"">trains</Property>
		<Property Name=""Bloodtype"">A+</Property>
	</Node>
	<Relation Id=""00000000-0000-0000-0000-000000000003"" Type=""test"" LeftNode=""00000000-0000-0000-0000-000000000001"" RightNode=""00000000-0000-0000-0000-000000000002"" />
</Graph>";
            SerializedDatabase.LoadXml(xmlrepresentation);
        }
        [Fact]
        public void GraphDatabase()
        {
            Assert.IsType<GraphDatabase>(Database);
            Assert.Equal(2,Database.V().Count);
            //make sure that a relation can be traversed
            Assert.Equal("Alice",Database.V().hasID(BobsGuid).outV().values("Name").Single());
        }
        [Fact]
        public void Filter_By_ID()
        {
            Assert.Single(Database.V().hasID(BobsGuid));
            Assert.Equal(BobsGuid,Database.V().hasID(BobsGuid).Select(v => v.ID).Single());
            Assert.NotEqual(BobsGuid,Database.V().hasID(AlicesGuid).Select(v => v.ID).Single());
        }
        [Fact]
        public void Filter_By_Label()
        {
            Assert.Equal(2, Database.V().hasLabel("Person").Count());
            Assert.Empty(Database.V().hasLabel("Train"));
        }
        [Fact]
        public void Filter_By_Presence_of_Property()
        {
            Assert.Single(Database.V().hasKey("Bloodtype"));
            Assert.Empty(Database.V().hasKey("doodleheimer"));
        }
        [Fact]
        public void Serialize_database()
        {
            //Given
            Database.Serialize("testexport.xml");

            //When
            XmlDocument XMLDatabase = new XmlDocument();
            XMLDatabase.Load("testexport.xml");
            //Then
            Assert.Equal(XMLDatabase, SerializedDatabase);
        }
        [Fact]
        public void Deserialize_database()
        {
            //Given
            //testexport.xml already exists
            //When
            GraphDatabase DatabaseCopy = new GraphDatabase();
            DatabaseCopy.LoadXML("testexport.xml");
            //serialize the copy so that I can compare with the first export manually
            DatabaseCopy.Serialize("testexportcopy.xml");
            //Then
            Assert.Equal(Database.V(), DatabaseCopy.V());
        }
    }//endclass
}//endnamespace