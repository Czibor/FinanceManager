using FinanceManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public virtual DbSet<BalanceReport> BalanceReports { get; set; }
        public virtual DbSet<Transfer> Transfers { get; set; }
        public virtual DbSet<RecurringTransfer> RecurringTransfers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
    }
}