using Microsoft.EntityFrameworkCore;
using System;

namespace RandomNumberService.RandomNumberDatabase
{
    public class RandomNumberDbContext : DbContext
    {
        public DbSet<RandomNumber> RandomNumbers { get; set; }

        //if we weren't using sql lite the retry logic would go here in the configuration
        //the configuration in entity framework core can specify how many retries should be attempted and the how long you show wait
        protected override void OnConfiguring(DbContextOptionsBuilder options)
          => options.UseSqlite("Filename=./RandomNumbers.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RandomNumber>()
                .HasKey(r => r.RandomNumberId)
                .HasName("PrimaryKey_RandomNumberId");
        }
        
    }

    public class RandomNumber
    {
        public int RandomNumberId { get; set; }
        public int Value { get; set; }
        public DateTime GeneratedTime { get; set; }
        public bool ServiceCallRetryRequired { get; set; }
    }
}
