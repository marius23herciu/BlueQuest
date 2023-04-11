using BlueQuest.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BlueQuest.Data
{
    public class BlueQuestDbContext: DbContext
    {
        public BlueQuestDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<Badge> Badges { get; set; }
    }
}
