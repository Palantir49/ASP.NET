using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.Context;

public class PromoCodeFactoryDbContext(DbContextOptions<PromoCodeFactoryDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Preference> Preferences { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(e => e.Role).WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(255);
        });

        //Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasMany(e => e.PromoCodes)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            //automate many-to-many relationship
            entity.HasMany(e => e.Preferences).WithMany(p => p.Customers)
                .UsingEntity(j => j.ToTable("CustomerPreference"));
        });

        //preference
        modelBuilder.Entity<Preference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
        });


        //PromoCode
        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code);
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(255);
            entity.Ignore(e => e.PartnerManager);
            /*entity.Property(e => e.PartnerManager)
                .HasMaxLength(128);*/
            entity.Property(e => e.ServiceInfo)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasOne(e => e.Preference)
                .WithMany()
                .HasForeignKey(e => e.PreferenceId);
        });
    }
}