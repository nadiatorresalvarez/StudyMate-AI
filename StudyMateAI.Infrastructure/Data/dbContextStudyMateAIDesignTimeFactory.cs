using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql;

namespace StudyMateAI.Infrastructure.Data
{
    public class dbContextStudyMateAIDesignTimeFactory : IDesignTimeDbContextFactory<dbContextStudyMateAI>
    {
        public dbContextStudyMateAI CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<dbContextStudyMateAI>();

            // Try to load connection string from the web project's appsettings
            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "StudyMateAI"));
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Use a placeholder connection at design time to avoid attempting to connect.
            // EF will override the connection when --connection is provided during update.
            var designConn = "Server=localhost;Port=3306;Database=design_time;User=design;Password=design;";
            optionsBuilder.UseMySql(designConn, new MySqlServerVersion(new Version(8, 0, 36)));
            return new dbContextStudyMateAI(optionsBuilder.Options);
        }
    }
}
