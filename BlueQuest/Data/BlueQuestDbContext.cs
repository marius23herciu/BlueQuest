using BlueQuest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Net;
using System.Reflection.Metadata;

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

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Quest>()
        //.HasMany(e => e.UsersWhoSolvedQuest)
        //.WithMany(e => e.SolvedQuests);
        //}
    }
}
