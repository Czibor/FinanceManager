using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManager.Models
{
    public class RecurringTransfer
    {
        [Key]
        public int RecurringTransferId { get; set; }
        public int Value { get; set; }
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = null;
        public DateTime? LastRunDate { get; set; } = null;
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Monthly;
        /// <summary>
        /// Daily: Must be 1. Weekly: Day of week. Monthly: Day of month.
        /// </summary>
        [Range(1, 31)]
        public int RecurrenceDay { get; set; } = 1;
        [Range(0, 10)]
        public int SkippedDays { get; set; } = 0;
    }
}