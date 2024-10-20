using NewFace.Models;
using NewFace.Models.User;
using NewFace.Models.Actor;
using NewFace.Models.Entertainment;

namespace NewFace.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }

    #region user
    public DbSet<User> Users { get; set; }
    public DbSet<UserAuth> UserAuth { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<UserFile> UserFile { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<UserLike> UserLikes { get; set; }
    #endregion


    #region actor
    public DbSet<Actor> Actors { get; set; }
    public DbSet<ActorEducation> ActorEducations { get; set; }
    public DbSet<ActorExperience> ActorExperiences { get; set; }
    public DbSet<ActorLink> ActorLinks { get; set; }
    public DbSet<ActorDemoStar> ActorDemoStars { get; set; }
    public DbSet<ActorImage> ActorImages { get; set; }
    #endregion

    public DbSet<Entertainment> Entertainments { get; set; }

    public DbSet<SystemLog> SystemLogs { get; set; }

}
