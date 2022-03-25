using Microsoft.EntityFrameworkCore;

namespace MailService.DBContexts
{
    public class MailServiceDatabaseContext : DbContext
    {
        /// <summary>
        /// Constructor of the UserServiceDatabaseContext class
        /// </summary>
        public MailServiceDatabaseContext()
        {
        }

        /// <summary>
        /// Constructor of the UserServiceDatabaseContext class with options, used for Unittesting
        /// Database options can be given, to switch between local and remote database
        /// </summary>
        /// <param name="options">Database options</param>
        public MailServiceDatabaseContext(DbContextOptions<MailServiceDatabaseContext> options) : base(options)
        {
        }

        /// <summary>
        /// OnConfiguring builds the connection between the database and the API using the given connection string
        /// </summary>
        /// <param name="optionsBuilder">Used for adding options to the database to configure the connection.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbString = Environment.GetEnvironmentVariable("kwetter_db_string");
                if (string.IsNullOrWhiteSpace(dbString))
                {
                    throw new MissingFieldException("Database environment variable not found.");
                }

                optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("kwetter_db_string").Replace("DATABASE_NAME", "MailService"));
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
