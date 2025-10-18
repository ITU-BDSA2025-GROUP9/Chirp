using FluentAssertions;
namespace SimpleDB.Tests;

public class IntegrationTest
{
    private record TestRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    
    private TestRecord FromLine(string line)
    {
        var parts = line.Split(',');
        return new TestRecord {
            Id = int.Parse(parts[0]),
            Name = parts[1]
        };
    }
    
    private CSVDatabase<TestRecord> SetUpDatabase(string tempFile)
    {
        return CSVDatabase<TestRecord>.GetInstance(
            tempFile,
            FromLine,
            r => $"{r.Id},{r.Name}",
            r => r.Id
        );
    }
    
    
    [Fact]
    public void StoreNewEntry_Test()
    {
        var tempFile = Path.GetTempFileName();
        
        try { 
            var db = SetUpDatabase(tempFile);

            var testRecord = new TestRecord { Id = 1, Name = "Bob" };
            db.Add(testRecord);
            
            var lines = File.ReadAllLines(tempFile);
            lines[0].Should().Be("1,Bob");
        }
        finally {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
    
    
    [Fact]
    public void RetrieveEntry_Test()
    {
        var tempFile = Path.GetTempFileName();
        
        try { 
            var db = SetUpDatabase(tempFile);

            var testRecord = new TestRecord { Id = 1, Name = "Bob" };
            db.Add(testRecord);
            
            var retrieved = db.FindById(1);
            Assert.Equal(testRecord, retrieved);
            
            var retrievedTestRecords = db.GetAll().ToList();
            
            retrievedTestRecords.Should().HaveCount(1);
            retrievedTestRecords[0].Should().Be(testRecord);
        }
        finally {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
    
    
    [Fact]
    public void RemoveEntry_Test()
    {
        var tempFile = Path.GetTempFileName();
        
        try { 
            var db = SetUpDatabase(tempFile);

            var testRecord = new TestRecord { Id = 1, Name = "Bob" };
            db.Add(testRecord);
            
            db.Remove(1);
            
            var retrievedTestRecords = db.GetAll().ToList();
            retrievedTestRecords.Should().HaveCount(0);
        }
        finally {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}