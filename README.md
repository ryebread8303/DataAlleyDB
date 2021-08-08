# DataAlleyDB
## summary
This project aims to create a C# library that creates and manages a graph database in memory. Adjacency is obtained by directly referencing adjacent node and edge objects. Persistence is planned through serializing the database to an XML file format. Queries are performed using a subset of [Gremlin](https://tinkerpop.apache.org/gremlin.html). 

## todo
* Serialize and deserialize to/from xml files for persistance.
* Support atomicity via a transaction log. Will probably rely on deserializing xml files added to a folder. This should allow the application using the library to watch a folder for added xml files and add the changes to the in memory database. Once the whole database has been serialized into a new xml file the transactions should be deleted.