using DDACASSSIGNMENT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DDACASSSIGNMENT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Period> Periods { get; set; }

        public DbSet<Size> Sizes { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Grouping> Groupings { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Brochure> Brochures { get; set; }
    }
}


