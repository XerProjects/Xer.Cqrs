//using MongoDB.Bson;
//using MongoDB.Driver;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Xer.Cqrs.QueryStack.Projections;
//using Xunit;

//namespace Xer.Cqrs.Tests.Projections
//{
//    public class ProjectionTests
//    {
//        public class LoadMethod
//        {
//            [Fact]
//            public void SaveProjection()
//            {
//                IMongoClient mongoClient = new MongoClient("mongodb://localhost:27017");

//                const string cqrsTestDatabaseName = "CqrsTestDatabase";
//                string testStringsCollectionName = typeof(TestProjectionDataSource).Name;

//                var database = mongoClient.GetDatabase(cqrsTestDatabaseName);
//                database.DropCollection(testStringsCollectionName);

//                var testProjectionDataCollection = database.GetCollection<TestProjectionDataSource>(testStringsCollectionName);

//                var testProjectionData = new TestProjectionDataSource(Guid.NewGuid(), "Name1", 0, DateTime.Now);
//                testProjectionDataCollection.InsertOne(testProjectionData);

//                TestMongoProjection projection = new TestMongoProjection(mongoClient);
                
//                Assert.Equal(testProjectionData.Name, projection.Name);
//                Assert.Equal(testProjectionData.CurrentDate, projection.Date);

//                testProjectionDataCollection.UpdateOne(t => t.Id == testProjectionData.Id, Builders<TestProjectionDataSource>.Update.Set(t => t.CurrentDate, DateTime.Now));

//                projection.UpdateAsync().GetAwaiter().GetResult();

//                var result = testProjectionDataCollection.Find(Builders<TestProjectionDataSource>.Filter.Eq(d => d.Id, testProjectionData.Id));
//                Assert.Equal(result.First().CurrentDate, projection.Date);
//            }
//        }
//    }

//    class TestProjectionDataSource
//    {
//        public Guid Id { get; private set; }
//        public string Name { get; private set; }
//        public DateTime CurrentDate { get; private set; }

//        public TestProjectionDataSource(Guid id, string name, int numberOfRun, DateTime currentDate)
//        {
//            Id = id;
//            Name = name ?? throw new ArgumentNullException(nameof(name));
//            CurrentDate = currentDate;
//        }
//    }

//    class TestMongoProjection : MongoDbProjection<Guid, TestMongoProjection>
//    {
//        public override Guid ProjectionId => throw new NotImplementedException();
//        public string Name { get; private set; }
//        public int NumberOfRun { get; private set; }
//        public DateTime Date { get; private set; }
        
//        public TestMongoProjection(IMongoClient mongoClient) 
//            : base(mongoClient)
//        {
//        }

//        // For test only.
//        public TestMongoProjection()
//            : base(new MongoClient())
//        {
//        }

//        public override Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken))
//        {
//            IMongoDatabase database = MongoClient.GetDatabase("CqrsTestDatabase");
//            var projections = database.GetCollection<TestMongoProjection>(typeof(TestMongoProjection).Name);

//            var update = Builders<TestMongoProjection>.Update.Set(t => t.Name, Name)
//                                                             .Set(t => t.NumberOfRun, NumberOfRun++)
//                                                             .Set(t => t.Date, DateTime.Now);

//            return projections.UpdateOneAsync(t => t.ProjectionId == ProjectionId, update);
//        }

//        public override async Task<TestMongoProjection> GetAsync(Guid projectionId, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            try
//            {
//                IMongoDatabase database = MongoClient.GetDatabase("CqrsTestDatabase");

//                var dataSourceCollection = database.GetCollection<TestProjectionDataSource>(typeof(TestProjectionDataSource).Name);
//                var cursor = await dataSourceCollection.FindAsync((p) => p.Name == "Name1");
//                var result = await cursor.FirstOrDefaultAsync();

//                return new TestMongoProjection()
//                {
//                    Name = result.Name,
//                    Date = result.CurrentDate
//                };
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }
                
//        public override bool Equals(TestMongoProjection other)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    abstract class MongoDbProjection<TId, TViewModel> : Projection<TId, TViewModel> where TViewModel : class
//    {
//        protected IMongoClient MongoClient { get; }
        
//        public MongoDbProjection(IMongoClient mongoClient)
//        {
//            MongoClient = mongoClient;
//        }
//    }
//}
