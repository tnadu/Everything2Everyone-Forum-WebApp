using Everything2Everyone.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Everything2Everyone.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ArticleVersion> ArticleVersions { get; set; }
        public DbSet<ChapterVersion> ChapterVersions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ArticleVersions composite key
            modelBuilder.Entity<ArticleVersion>()
            .HasKey(av => new
            {
                av.ArticleID,
                av.VersionID
            });

            // ArticleVersions (m) - Articles (1)
            modelBuilder.Entity<ArticleVersion>()
            .HasOne(av => av.Article)
            .WithMany(a => a.Versions)
            .HasForeignKey(av => av.ArticleID);

            // ArticleVersions (1) - ChapterVersions (m)
            modelBuilder.Entity<ArticleVersion>()
            .HasMany(av => av.Chapters)
            .WithOne(c => c.Article)
            .HasForeignKey(av => new
            {
                av.ArticleID,
                av.VersionID
            });

            // ArticleVersions (m) - Category (1)
            modelBuilder.Entity<ArticleVersion>()
            .HasOne(av => av.Category)
            .WithMany(c => c.ArticleVersions)
            .HasForeignKey(av => av.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);


            // Chapters composite key
            modelBuilder.Entity<Chapter>()
            .HasKey(c => new
            {
                c.ChapterID,
                c.ArticleID
            });

            // Chapters (m) - Article (1)
            modelBuilder.Entity<Chapter>()
            .HasOne(c => c.Article)
            .WithMany(a => a.Chapters)
            .HasForeignKey(c => c.ArticleID);


            // ChapterVersions composite key
            modelBuilder.Entity<ChapterVersion>()
            .HasKey(c => new
            {
                c.ChapterID,
                c.ArticleID,
                c.VersionID
            });


            // Users (1) - Articles (m)
            modelBuilder.Entity<User>()
            .HasMany(u => u.Articles)
            .WithOne(a => a.User)
            .HasForeignKey(u => u.UserID)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}