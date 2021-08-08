using System;
using System.Linq;
using Xunit;
using GraphLibrary;
using System.Collections.Generic;

namespace UnitTests
{
    public class UnitTest1
    {
        Guid BobsGuid = new Guid("00000000-0000-0000-0000-000000000001");
        Guid AlicesGuid = new Guid("00000000-0000-0000-0000-000000000002");
        Guid BadGuid = Guid.NewGuid();
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
            GraphRelation Relation = new GraphRelation("test",Bob,Alice);
            Database.AddNode(Bob);
            Database.AddNode(Alice);
            Database.AddRelationship(Relation);
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
    }//endclass
}//endnamespace