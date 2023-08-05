using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Server.Models
{
    public partial class InsuranceContext : DbContext
    {
        public InsuranceContext()
        {
        }

        public InsuranceContext(DbContextOptions<InsuranceContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Claim> Claims { get; set; } = null!;
        public virtual DbSet<ClaimImage> ClaimImages { get; set; } = null!;
        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Policy> Policies { get; set; } = null!;
        public virtual DbSet<PolicyOnUser> PolicyOnUsers { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server=.; database=Project3; trusted_connection=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.ToTable("Claim");

                entity.Property(e => e.ClaimId)
                    .ValueGeneratedNever()
                    .HasColumnName("ClaimID");

                entity.Property(e => e.AppAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreateDate).HasColumnType("date");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.PolicyId).HasColumnName("PolicyID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Policy)
                    .WithMany(p => p.Claims)
                    .HasForeignKey(d => d.PolicyId)
                    .HasConstraintName("FK_Claim_Policy");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Claims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Claim_User");
            });

            modelBuilder.Entity<ClaimImage>(entity =>
            {
                entity.HasKey(e => e.ImageId);

                entity.ToTable("ClaimImage");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.ClaimId).HasColumnName("ClaimID");

                entity.Property(e => e.Url)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("URL");

                entity.HasOne(d => d.Claim)
                    .WithMany(p => p.ClaimImages)
                    .HasForeignKey(d => d.ClaimId)
                    .HasConstraintName("FK_ClaimImage_Claim");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company");

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CompanyName).HasMaxLength(100);

                entity.Property(e => e.CompanyPhone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Logo).IsUnicode(false);

                entity.Property(e => e.Url)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("URL");
            });

            modelBuilder.Entity<Policy>(entity =>
            {
                entity.ToTable("Policy");

                entity.Property(e => e.PolicyId)
                    .ValueGeneratedNever()
                    .HasColumnName("PolicyID");

                entity.Property(e => e.Banner).IsUnicode(false);

                entity.Property(e => e.CompanyId).HasColumnName("CompanyID");

                entity.Property(e => e.Desciption).IsUnicode(false);

                entity.Property(e => e.Duration).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PolicyName).HasMaxLength(150);

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Policies)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_Policy_Company");
            });

            modelBuilder.Entity<PolicyOnUser>(entity =>
            {
                entity.ToTable("PolicyOnUser");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AvaibleAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.PolicyId).HasColumnName("PolicyID");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Policy)
                    .WithMany(p => p.PolicyOnUsers)
                    .HasForeignKey(d => d.PolicyId)
                    .HasConstraintName("FK_PolicyOnUser_Policy");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PolicyOnUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_PolicyOnUser_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.Avatar).IsUnicode(false);

                entity.Property(e => e.Dob).HasColumnType("date");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Password)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
