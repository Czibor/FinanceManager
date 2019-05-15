using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace FinanceManager.Controllers
{
    [Authorize]
    public class DataController : ControllerBase
    {
        private ApplicationDbContext ApplicationDb { get; set; }

        private string _userId;
        private string UserId
        {
            get
            {
                return _userId ?? (_userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            }
        }

        public DataController(ApplicationDbContext dbContext)
        {
            //TODO: (2/1) Date localization.
            //TODO: (?/2) User management.
            ApplicationDb = dbContext;
        }

        private IQueryable<BalanceReport> GetBalanceReports()
        {
            return ApplicationDb.BalanceReports.Where(x => x.UserId == UserId);
        }

        [HttpGet]
        public object GetBalanceReports(DataSourceLoadOptions loadOptions)
        {
            return DataSourceLoader.Load(GetBalanceReports(), loadOptions);
        }

        [HttpPost]
        public IActionResult CreateBalanceReport(string values)
        {
            BalanceReport balanceReport = new BalanceReport();
            JsonConvert.PopulateObject(values, balanceReport);

            if (TryValidateModel(balanceReport))
            {
                balanceReport.UserId = UserId;
                ApplicationDb.BalanceReports.Add(balanceReport);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public IActionResult UpdateBalanceReport(int key, string values)
        {
            BalanceReport balanceReport = GetBalanceReports().FirstOrDefault(x => x.BalanceReportId == key);
            int id = balanceReport.BalanceReportId;
            string userId = balanceReport.UserId;
            JsonConvert.PopulateObject(values, balanceReport);

            if (TryValidateModel(balanceReport))
            {
                balanceReport.BalanceReportId = id;
                balanceReport.UserId = userId;
                ApplicationDb.BalanceReports.Update(balanceReport);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private IQueryable<Transfer> GetTransfers()
        {
            return ApplicationDb.Transfers.Where(x => x.UserId == UserId);
        }

        /// <summary>
        /// Gets all available transfers or transfers related to a BalanceReport.
        /// </summary>
        /// <param name="key">BalanceReportId.</param>
        [HttpGet]
        public object GetTransfers(DataSourceLoadOptions loadOptions, int? key)
        {
            CheckRecurringTransfers();

            // If key is given, then filter transfers based on BalanceReport date.
            if (key != null)
            {
                BalanceReport balanceReport = GetBalanceReports().Where(x => x.BalanceReportId == key).FirstOrDefault();
                BalanceReport previousBalanceReport = GetBalanceReports().OrderByDescending(x => x.Date).Where(y => y.Date < balanceReport.Date).FirstOrDefault();

                if (balanceReport != null)
                {
                    //TODO: (?/2) Future transfers can't be retrieved by key.
                    DateTime startDate = previousBalanceReport?.Date ?? DateTime.MinValue;
                    DateTime endDate = balanceReport.Date;
                    IQueryable<Transfer> transfers = GetTransfers().Where(x => startDate < x.Date && x.Date <= endDate);
                    return DataSourceLoader.Load(transfers, loadOptions);
                }
                else
                {
                    return DataSourceLoader.Load(Enumerable.Empty<Transfer>().AsQueryable(), loadOptions);
                }
            }
            else
            {
                return DataSourceLoader.Load(GetTransfers(), loadOptions);
            }
        }

        [HttpPost]
        public IActionResult CreateTransfer(string values)
        {
            Transfer transfer = new Transfer();
            JsonConvert.PopulateObject(values, transfer);

            if (TryValidateModel(transfer))
            {
                transfer.UserId = UserId;
                ApplicationDb.Transfers.Add(transfer);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public IActionResult UpdateTransfer(int key, string values)
        {
            Transfer transfer = GetTransfers().FirstOrDefault(x => x.TransferId == key);
            int id = transfer.TransferId;
            string userId = transfer.UserId;
            JsonConvert.PopulateObject(values, transfer);

            if (TryValidateModel(transfer))
            {
                transfer.TransferId = id;
                transfer.UserId = userId;
                ApplicationDb.Transfers.Update(transfer);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete]
        public IActionResult DeleteTransfer(int key)
        {
            Transfer transfer = GetTransfers().FirstOrDefault(x => x.TransferId == key);
            ApplicationDb.Transfers.Remove(transfer);
            ApplicationDb.SaveChanges();
            return Ok();
        }

        private IQueryable<RecurringTransfer> GetRecurringTransfers()
        {
            return ApplicationDb.RecurringTransfers.Where(x => x.UserId == UserId);
        }

        [HttpGet]
        public object GetRecurringTransfers(DataSourceLoadOptions loadOptions)
        {
            return DataSourceLoader.Load(GetRecurringTransfers(), loadOptions);
        }

        [HttpGet]
        public object GetRecurrenceTypes()
        {
            List<object> enums = new List<object>();

            foreach (var item in Enum.GetValues(typeof(RecurrenceType)))
            {
                enums.Add(new { Id = (int)item, Name = item.ToString() });
            }

            return enums;
        }

        [HttpPost]
        public IActionResult CreateRecurringTransfer(string values)
        {
            RecurringTransfer recurringTransfer = new RecurringTransfer();
            JsonConvert.PopulateObject(values, recurringTransfer);
            ValidateStartDate(recurringTransfer);
            ValidateRecurrenceDay(recurringTransfer);

            if (TryValidateModel(recurringTransfer))
            {
                recurringTransfer.UserId = UserId;
                ApplicationDb.RecurringTransfers.Add(recurringTransfer);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public IActionResult UpdateRecurringTransfer(int key, string values)
        {
            RecurringTransfer recurringTransfer = GetRecurringTransfers().FirstOrDefault(x => x.RecurringTransferId == key);
            int id = recurringTransfer.RecurringTransferId;
            string userId = recurringTransfer.UserId;
            JsonConvert.PopulateObject(values, recurringTransfer);
            ValidateStartDate(recurringTransfer);
            ValidateRecurrenceDay(recurringTransfer);

            if (TryValidateModel(recurringTransfer))
            {
                recurringTransfer.CategoryId = id;
                recurringTransfer.UserId = userId;
                ApplicationDb.RecurringTransfers.Update(recurringTransfer);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private void ValidateStartDate(RecurringTransfer recurringTransfer)
        {
            if (recurringTransfer.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "StartDate can't be in the past.");
            }
        }

        private void ValidateRecurrenceDay(RecurringTransfer recurringTransfer)
        {
            //TODO: (3/1) Inform user how recurrence day works.
            if (recurringTransfer.RecurrenceType == RecurrenceType.Daily)
            {
                if (recurringTransfer.RecurrenceDay != 1)
                {
                    ModelState.AddModelError("", "Day for daily recurrence must be 1.");
                }
            }
            else if (recurringTransfer.RecurrenceType == RecurrenceType.Weekly)
            {
                if (!Enumerable.Range(1, 7).Contains(recurringTransfer.RecurrenceDay))
                {
                    ModelState.AddModelError("", "Day for weekly recurrence must be between 1 and 7.");
                }
            }
            else if (recurringTransfer.RecurrenceType == RecurrenceType.Monthly)
            {
                if (!Enumerable.Range(1, 31).Contains(recurringTransfer.RecurrenceDay))
                {
                    ModelState.AddModelError("", "Day for monthly recurrence must be between 1 and 31 and can't be 0.");
                }
            }
        }

        [HttpDelete]
        public IActionResult DeleteRecurringTransfer(int key)
        {
            RecurringTransfer recurringTransfer = GetRecurringTransfers().FirstOrDefault(x => x.RecurringTransferId == key);
            ApplicationDb.RecurringTransfers.Remove(recurringTransfer);
            ApplicationDb.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Checks if transfer should be added since last check and adds them.
        /// </summary>
        public void CheckRecurringTransfers()
        {
            foreach (RecurringTransfer recurringTransfer in GetRecurringTransfers())
            {
                if (recurringTransfer.IsActive)
                {
                    //DateTime startDate = recurringTransfer.LastRunDate == null ? recurringTransfer.StartDate : (DateTime)recurringTransfer.LastRunDate;
                    DateTime startDate = recurringTransfer.LastRunDate ?? recurringTransfer.StartDate;

                    if (startDate < DateTime.Today)
                    {
                        DateTime endDate = recurringTransfer.EndDate != null && recurringTransfer.EndDate < DateTime.Today ? (DateTime)recurringTransfer.EndDate : DateTime.Today;
                        List<DateTime> dates = GetRecurringDates(startDate, endDate, recurringTransfer);

                        foreach (DateTime date in dates)
                        {
                            Transfer transfer = new Transfer()
                            {
                                UserId = recurringTransfer.UserId,
                                Date = date,
                                Value = recurringTransfer.Value,
                                Category = recurringTransfer.Category,
                                Description = recurringTransfer.Description
                            };
                        }

                        recurringTransfer.LastRunDate = DateTime.Now;

                        if (recurringTransfer.EndDate != null && recurringTransfer.LastRunDate > recurringTransfer.EndDate)
                        {
                            recurringTransfer.IsActive = false;
                        }

                        ApplicationDb.RecurringTransfers.Update(recurringTransfer);
                        ApplicationDb.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Gets dates between a given range filtered by the conditions in recurringTransfer.
        /// </summary>
        private static List<DateTime> GetRecurringDates(DateTime startDate, DateTime endDate, RecurringTransfer recurringTransfer)
        {
            List<DateTime> dates = new List<DateTime>();

            if (recurringTransfer.RecurrenceType == RecurrenceType.Daily)
            {
                for (DateTime date = startDate; date.Date <= endDate; date = date.AddDays(recurringTransfer.SkippedDays + 1))
                {
                    dates.Add(date);
                }
            }
            else if (recurringTransfer.RecurrenceType == RecurrenceType.Weekly)
            {
                int daysUntilMonday = ((int)DayOfWeek.Monday - (int)startDate.DayOfWeek + 7) % 7;
                DateTime firstRecurrence = startDate.AddDays(daysUntilMonday);

                for (DateTime date = firstRecurrence; date.Date <= endDate; date = date.AddDays(7 * (recurringTransfer.SkippedDays + 1)))
                {
                    dates.Add(date);
                }
            }
            else if (recurringTransfer.RecurrenceType == RecurrenceType.Monthly)
            {
                int recurrenceDay = recurringTransfer.RecurrenceDay;
                DateTime temp = recurrenceDay > startDate.Day ? startDate : startDate.AddMonths(1);
                DateTime firstRecurrence = GetNextDate(temp, recurrenceDay);

                for (DateTime date = firstRecurrence; date.Date <= endDate; date = GetNextDate(date.AddMonths(recurringTransfer.SkippedDays + 1), recurrenceDay))
                {
                    dates.Add(date);
                }
            }

            return dates;
        }

        private static DateTime GetNextDate(DateTime date, int recurrenceDay)
        {
            int day = Math.Min(recurrenceDay, DateTime.DaysInMonth(date.Year, date.Month));
            return new DateTime(date.Year, date.Month, day);
        }

        private IQueryable<Category> GetCategories()
        {
            return ApplicationDb.Categories.Where(x => x.UserId == UserId);
        }

        [HttpGet]
        public object GetCategories(DataSourceLoadOptions loadOptions)
        {
            return DataSourceLoader.Load(GetCategories(), loadOptions);
        }

        [HttpPost]
        public IActionResult CreateCategory(string values)
        {
            Category category = new Category();
            JsonConvert.PopulateObject(values, category);

            if (TryValidateModel(category))
            {
                category.UserId = UserId;
                ApplicationDb.Categories.Add(category);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public IActionResult UpdateCategory(int key, string values)
        {
            Category category = GetCategories().FirstOrDefault(x => x.CategoryId == key);
            int id = category.CategoryId;
            string userId = category.UserId;
            JsonConvert.PopulateObject(values, category);

            if (TryValidateModel(category))
            {
                category.CategoryId = id;
                category.UserId = userId;
                ApplicationDb.Categories.Update(category);
                ApplicationDb.SaveChanges();
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete]
        public IActionResult DeleteCategory(int key)
        {
            Category category = GetCategories().FirstOrDefault(x => x.CategoryId == key);
            ApplicationDb.Categories.Remove(category);
            ApplicationDb.SaveChanges();
            return Ok();
        }

        //TODO: (3/2) Client-side option to set year.
        [HttpGet]
        public object GetMonthlyTransfers(int? year)
        {
            List<ChartValue> chartValues = new List<ChartValue>();
            year = year ?? DateTime.Now.Year;
            var transfers = GetTransfers().Where(x => x.Date.Year == year);
            DateTimeFormatInfo formatInfo = new DateTimeFormatInfo();

            for (int i = 1; i <= 12; ++i)
            {
                string monthName = formatInfo.GetMonthName(i).ToString();
                var currentMonthTransfers = transfers.Where(x => x.Date.Month == i);

                ChartValue income = new ChartValue
                {
                    Name = monthName,
                    Legend = "Income",
                    Value = currentMonthTransfers.Where(x => x.Value > 0).Sum(y => y.Value)
                };

                ChartValue expense = new ChartValue
                {
                    Name = monthName,
                    Legend = "Expense",
                    Value = 0 - currentMonthTransfers.Where(x => x.Value < 0).Sum(y => y.Value)
                };

                chartValues.Add(income);
                chartValues.Add(expense);
            }

            return chartValues;
        }

        [HttpGet]
        public object GetYearlyTransfers(int? year)
        {
            List<ChartValue> chartValues = new List<ChartValue>();
            var transfers = GetTransfers();

            if (GetTransfers().Count() != 0)
            {
                int startYear = transfers.Min(x => x.Date).Year;

                for (int i = startYear; i <= DateTime.Now.Year; ++i)
                {
                    var currentYearTransfers = transfers.Where(x => x.Date.Year == i);

                    ChartValue income = new ChartValue
                    {
                        Name = i.ToString(),
                        Legend = "Income",
                        Value = currentYearTransfers.Where(x => x.Value > 0).Sum(y => y.Value)
                    };

                    ChartValue expense = new ChartValue
                    {
                        Name = i.ToString(),
                        Legend = "Expense",
                        Value = 0 - currentYearTransfers.Where(x => x.Value < 0).Sum(y => y.Value)
                    };

                    chartValues.Add(income);
                    chartValues.Add(expense);
                }
            }

            return chartValues;
        }

        [HttpGet]
        public object GetMonthlyBalanceReports(int? year)
        {
            List<ChartValue> chartValues = new List<ChartValue>();
            year = year ?? DateTime.Now.Year;
            var balanceReports = GetBalanceReports().Where(x => x.Date.Year == year).OrderBy(y => y.Date);
            DateTimeFormatInfo formatInfo = new DateTimeFormatInfo();

            for (int i = 1; i <= 12; ++i)
            {
                BalanceReport balanceReport = balanceReports.Where(x => x.Date.Month == i).LastOrDefault();
                string monthName = formatInfo.GetMonthName(i).ToString();

                ChartValue chartValue = new ChartValue
                {
                    Name = monthName,
                    Value = balanceReport?.Balance ?? 0
                };

                chartValues.Add(chartValue);
            }

            return chartValues;
        }

        [HttpGet]
        public object GetYearlyBalanceReports()
        {
            List<ChartValue> chartValues = new List<ChartValue>();
            var balanceReports = GetBalanceReports().OrderBy(x => x.Date);

            if (balanceReports.Count() != 0)
            {
                int startYear = GetBalanceReports().Min(x => x.Date).Year;

                for (int i = startYear; i <= DateTime.Now.Year; ++i)
                {
                    BalanceReport balanceReport = GetBalanceReports().Where(x => x.Date.Year == i).LastOrDefault();

                    ChartValue chartValue = new ChartValue
                    {
                        Name = i.ToString(),
                        Value = balanceReport?.Balance ?? 0
                    };

                    chartValues.Add(chartValue);
                }
            }

            return chartValues;
        }

        [HttpDelete]
        public IActionResult RemoveData()
        {
            ApplicationDb.BalanceReports.RemoveRange(GetBalanceReports());
            ApplicationDb.Transfers.RemoveRange(GetTransfers());
            ApplicationDb.RecurringTransfers.RemoveRange(GetRecurringTransfers());
            ApplicationDb.Categories.RemoveRange(GetCategories());
            ApplicationDb.SaveChanges();
            return Ok();
        }
    }
}
