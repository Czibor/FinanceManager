using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace FinanceManager.Controllers
{
    [Authorize]
    public class ExportController : ControllerBase
    {
        private ApplicationDbContext ApplicationDb { get; set; }

        private string _userId;
        private string UserId
        {
            get
            {
                return _userId ?? (_userId = User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
        }

        public ExportController(ApplicationDbContext dbContext)
        {
            ApplicationDb = dbContext;
        }

        private IQueryable<BalanceReport> GetBalanceReports()
        {
            return ApplicationDb.BalanceReports.Where(x => x.UserId == UserId);
        }

        private IQueryable<Transfer> GetTransfers()
        {
            return ApplicationDb.Transfers.Where(x => x.UserId == UserId);
        }

        public IActionResult ExportData()
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            CreateBalanceReportsSheet(workbook);
            CreateTransfersSheet(workbook);
            // TempStream is needed because the stream is closed after workbook.Write.
            MemoryStream memoryStream = new MemoryStream();

            using (MemoryStream tempStream = new MemoryStream())
            {
                workbook.Write(tempStream);
                byte[] byteArray = tempStream.ToArray();
                memoryStream.Write(byteArray, 0, byteArray.Length);
                memoryStream.Position = 0;
                string fileName = $"FileManager_{DateTime.Today.ToString("yyyyMMdd")}.xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        #region Disgusting.
        private void CreateBalanceReportsSheet(XSSFWorkbook workbook)
        {
            ISheet balanceReportsSheet = workbook.CreateSheet("Balance reports");
            IRow balanceReportsHeader = balanceReportsSheet.CreateRow(0);
            balanceReportsHeader.CreateCell(0).SetCellValue("ID");
            balanceReportsHeader.CreateCell(1).SetCellValue("User ID");
            balanceReportsHeader.CreateCell(2).SetCellValue("Date");
            balanceReportsHeader.CreateCell(3).SetCellValue("Value");
            balanceReportsHeader.CreateCell(3).SetCellValue("Comment");
            List<BalanceReport> balanceReports = GetBalanceReports().ToList();

            for (int i = 0; i < balanceReports.Count; ++i)
            {
                IRow row = balanceReportsSheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(balanceReports[i].BalanceReportId);
                row.CreateCell(1).SetCellValue(balanceReports[i].UserId);
                row.CreateCell(2).SetCellValue(balanceReports[i].Date);
                row.CreateCell(3).SetCellValue(balanceReports[i].Balance);
                row.CreateCell(4).SetCellValue(balanceReports[i].Comment);
            }
        }

        private void CreateTransfersSheet(XSSFWorkbook workbook)
        {
            ISheet transfersSheet = workbook.CreateSheet("Transfers");
            IRow transfersHeader = transfersSheet.CreateRow(0);
            transfersHeader.CreateCell(0).SetCellValue("ID");
            transfersHeader.CreateCell(1).SetCellValue("User ID");
            transfersHeader.CreateCell(2).SetCellValue("Date");
            transfersHeader.CreateCell(3).SetCellValue("Value");
            transfersHeader.CreateCell(4).SetCellValue("Category");
            transfersHeader.CreateCell(5).SetCellValue("Description");
            transfersHeader.CreateCell(6).SetCellValue("Comment");
            List<Transfer> transfers = GetTransfers().ToList();

            for (int i = 0; i < transfers.Count; ++i)
            {
                IRow row = transfersSheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(transfers[i].TransferId);
                row.CreateCell(1).SetCellValue(transfers[i].UserId);
                row.CreateCell(2).SetCellValue(transfers[i].Date);
                row.CreateCell(3).SetCellValue(transfers[i].Value);
                row.CreateCell(4).SetCellValue(transfers[i].Category?.Name);
                row.CreateCell(5).SetCellValue(transfers[i].Description);
                row.CreateCell(6).SetCellValue(transfers[i].Comment);
            }
        }
        #endregion
    }
}
