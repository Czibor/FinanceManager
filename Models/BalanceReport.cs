using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManager.Models
{
    // If it changes, ExportController should be modified too.
    public class BalanceReport
    {
        [Key]
        public int BalanceReportId { get; set; }
        public DateTime Date { get; set; }
        public int Balance { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }

        [NotMapped]
        public string Year
        {
            get
            {
                return Date.ToString("yyyy");
            }
        }

        [NotMapped]
        public string Month
        {
            get
            {
                return Date.ToString("MM MMMM");
            }
        }
    }
}