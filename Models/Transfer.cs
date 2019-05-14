using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceManager.Models
{
    // If it changes, ExportController should be modified too.
    public class Transfer
    {
        [Key]
        public int TransferId { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public string Description { get; set; }
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