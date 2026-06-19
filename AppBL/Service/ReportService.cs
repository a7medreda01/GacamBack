using AppBL.IService;
using AppDAL.IRepos;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> GeneratePaymentsReportAsync()
        {
            var payments = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.User)
                .Include(p => p.VerifiedByUser)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Payments Log");

                // Headers
                ws.Cell(1, 1).Value = "Payment ID";
                ws.Cell(1, 2).Value = "User Name";
                ws.Cell(1, 3).Value = "User Email";
                ws.Cell(1, 4).Value = "Amount (CAD)";
                ws.Cell(1, 5).Value = "Sender Name";
                ws.Cell(1, 6).Value = "Reference Number";
                ws.Cell(1, 7).Value = "Payment Type";
                ws.Cell(1, 8).Value = "Related Record ID";
                ws.Cell(1, 9).Value = "Status";
                ws.Cell(1, 10).Value = "Submitted Date";
                ws.Cell(1, 11).Value = "Reviewed Date";
                ws.Cell(1, 12).Value = "Reviewer";
                ws.Cell(1, 13).Value = "Admin Notes";

                // Format Header Row
                var headerRow = ws.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#003F4A");
                headerRow.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var p in payments)
                {
                    ws.Cell(row, 1).Value = p.Id;
                    ws.Cell(row, 2).Value = p.User.FullName;
                    ws.Cell(row, 3).Value = p.User.Email;
                    ws.Cell(row, 4).Value = p.Amount;
                    ws.Cell(row, 5).Value = p.SenderName;
                    ws.Cell(row, 6).Value = p.ReferenceNumber;
                    ws.Cell(row, 7).Value = p.Type.ToString();
                    ws.Cell(row, 8).Value = p.RelatedRecordId;
                    ws.Cell(row, 9).Value = p.Status.ToString();
                    ws.Cell(row, 10).Value = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cell(row, 11).Value = p.VerifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                    ws.Cell(row, 12).Value = p.VerifiedByUser?.FullName ?? "N/A";
                    ws.Cell(row, 13).Value = p.AdminNotes ?? "";
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }

        public async Task<byte[]> GenerateAuditLogsReportAsync()
        {
            var logs = await _unitOfWork.AuditLogs.GetQueryable()
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Audit Logs");

                ws.Cell(1, 1).Value = "Log ID";
                ws.Cell(1, 2).Value = "User ID";
                ws.Cell(1, 3).Value = "User Email";
                ws.Cell(1, 4).Value = "Action";
                ws.Cell(1, 5).Value = "Table Name";
                ws.Cell(1, 6).Value = "Record ID";
                ws.Cell(1, 7).Value = "Old Values";
                ws.Cell(1, 8).Value = "New Values";
                ws.Cell(1, 9).Value = "Timestamp";
                ws.Cell(1, 10).Value = "IP Address";

                var headerRow = ws.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#003F4A");
                headerRow.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var l in logs)
                {
                    ws.Cell(row, 1).Value = l.Id;
                    ws.Cell(row, 2).Value = l.UserId?.ToString() ?? "Guest";
                    ws.Cell(row, 3).Value = l.UserEmail ?? "Guest";
                    ws.Cell(row, 4).Value = l.Action;
                    ws.Cell(row, 5).Value = l.TableName;
                    ws.Cell(row, 6).Value = l.RecordId ?? "";
                    ws.Cell(row, 7).Value = l.OldValues ?? "";
                    ws.Cell(row, 8).Value = l.NewValues ?? "";
                    ws.Cell(row, 9).Value = l.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                    ws.Cell(row, 10).Value = l.IpAddress ?? "";
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }

        public async Task<byte[]> GenerateUsersReportAsync()
        {
            var users = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Users List");

                ws.Cell(1, 1).Value = "User ID";
                ws.Cell(1, 2).Value = "Email";
                ws.Cell(1, 3).Value = "Full Name";
                ws.Cell(1, 4).Value = "Phone Number";
                ws.Cell(1, 5).Value = "Roles";
                ws.Cell(1, 6).Value = "Is Active";
                ws.Cell(1, 7).Value = "Registration Date";

                var headerRow = ws.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#003F4A");
                headerRow.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var u in users)
                {
                    ws.Cell(row, 1).Value = u.Id;
                    ws.Cell(row, 2).Value = u.Email;
                    ws.Cell(row, 3).Value = u.FullName;
                    ws.Cell(row, 4).Value = u.PhoneNumber ?? "";
                    ws.Cell(row, 5).Value = string.Join(", ", u.UserRoles.Select(ur => ur.Role.Name));
                    ws.Cell(row, 6).Value = u.IsActive ? "Yes" : "No";
                    ws.Cell(row, 7).Value = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
