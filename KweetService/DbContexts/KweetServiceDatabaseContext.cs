using KweetService.Models;
using Microsoft.EntityFrameworkCore;

namespace KweetService.DbContexts
{
    public class KweetServiceDatabaseContext : DbContext
    {
        /// <summary>
        /// Constructor of the KweetServiceDatabaseContext class
        /// </summary>
        public KweetServiceDatabaseContext()
        {
        }

        /// <summary>
        /// Constructor of the KweetServiceDatabaseContext class with options, used for Unittesting
        /// Database options can be given, to switch between local and remote database
        /// </summary>
        /// <param name="options">Database options</param>
        public KweetServiceDatabaseContext(DbContextOptions<KweetServiceDatabaseContext> options) : base(options)
        {
        }

        public DbSet<Kweet> Kweets { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbString = Environment.GetEnvironmentVariable("kwetter_db_string");
                if (string.IsNullOrWhiteSpace(dbString))
                {
                    throw new MissingFieldException("Database environment variable not found.");
                }

                optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("kwetter_db_string").Replace("DATABASE_NAME", "kweetservice"));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Like>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}
