using System;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Contacts> Contacts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.email, u.phone, u.documentNumber })
                .IsUnique(true);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.email)
                .IsUnique(true);

            modelBuilder.Entity<Contacts>()
                .HasIndex(c => new { c.IdUser, c.documentNumber })
                .IsUnique(false);
        }

        public void SeedInitialData()
        {
            if (!Users.Any())
            {
                var user1 = new User
                {
                    firstName = "Kevin Andrey",
                    lastName = "Herrera Silva",
                    documentType = "CC",
                    documentNumber = "1032485121",
                    phone = "3185563353",
                    email = "kaherrera@hubbec.com",
                    password = "Password123!",
                    CreatedAt = DateTime.UtcNow
                };

                var user2 = new User
                {
                    firstName = "Diana",
                    lastName = "Prince",
                    documentType = "CC",
                    documentNumber = "1098765432",
                    phone = "3101234567",
                    email = "diana.prince@hubbec.com",
                    password = "Password123!",
                    CreatedAt = DateTime.UtcNow
                };

                var user3 = new User
                {
                    firstName = "Bruce",
                    lastName = "Wayne",
                    documentType = "CC",
                    documentNumber = "1087654321",
                    phone = "3209876543",
                    email = "bruce.wayne@hubbec.com",
                    password = "Password123!",
                    CreatedAt = DateTime.UtcNow
                };

                var user4 = new User
                {
                    firstName = "Clark",
                    lastName = "Kent",
                    documentType = "CC",
                    documentNumber = "1076543210",
                    phone = "3154567890",
                    email = "clark.kent@hubbec.com",
                    password = "Password123!",
                    CreatedAt = DateTime.UtcNow
                };

                Users.AddRange(user1, user2, user3, user4);
                SaveChanges();

                // Seed mutual contacts for testing:
                // User1 (Kevin) has User2 (Diana) and User3 (Bruce) as contacts
                // User2 (Diana) has User1 (Kevin) and User3 (Bruce) and User4 (Clark)
                // Mutual contact between Kevin and Diana: Bruce!
                Contacts.AddRange(
                    new Contacts
                    {
                        IdUser = user1.Id,
                        IdContact = user2.Id,
                        firstName = user2.firstName,
                        lastName = user2.lastName,
                        documentType = user2.documentType,
                        documentNumber = user2.documentNumber,
                        phone = user2.phone,
                        email = user2.email,
                        relationship = "Colega",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Contacts
                    {
                        IdUser = user1.Id,
                        IdContact = user3.Id,
                        firstName = user3.firstName,
                        lastName = user3.lastName,
                        documentType = user3.documentType,
                        documentNumber = user3.documentNumber,
                        phone = user3.phone,
                        email = user3.email,
                        relationship = "Amigo",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Contacts
                    {
                        IdUser = user2.Id,
                        IdContact = user1.Id,
                        firstName = user1.firstName,
                        lastName = user1.lastName,
                        documentType = user1.documentType,
                        documentNumber = user1.documentNumber,
                        phone = user1.phone,
                        email = user1.email,
                        relationship = "Colega",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Contacts
                    {
                        IdUser = user2.Id,
                        IdContact = user3.Id,
                        firstName = user3.firstName,
                        lastName = user3.lastName,
                        documentType = user3.documentType,
                        documentNumber = user3.documentNumber,
                        phone = user3.phone,
                        email = user3.email,
                        relationship = "Amigo",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Contacts
                    {
                        IdUser = user2.Id,
                        IdContact = user4.Id,
                        firstName = user4.firstName,
                        lastName = user4.lastName,
                        documentType = user4.documentType,
                        documentNumber = user4.documentNumber,
                        phone = user4.phone,
                        email = user4.email,
                        relationship = "Otro",
                        CreatedAt = DateTime.UtcNow
                    }
                );
                SaveChanges();
            }
        }
    }
}
