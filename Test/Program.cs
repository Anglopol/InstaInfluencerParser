using System;
using System.Linq;
using Neo4jClient;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
//            var client = new GraphClient(new Uri("http://188.68.210.25:7474/db/data"),
//                "neo4j", "J9xMXjhMsZKn8$");
//            client.Connect();
//            var users = client.Cypher
//                .Match("(location:Location)")
//                .Where((Location location) => location.Owner == "ivanselsky")
//                .Return(location => new
//                {
//                    Loc = location.As<Location>()
//                })
//                .Results;
//
//            foreach (var value in users)
//            {
//                Console.WriteLine($"parent: {value.Loc.Name}");
//            }
            DateTime date = DateTime.UtcNow;
            Console.WriteLine(date.ToString());
        }
    }
}