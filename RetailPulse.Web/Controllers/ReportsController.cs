using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models.ViewModels;
using RetailPulse.Web.Services;
using System.Text;

namespace RetailPulse.Web.Controllers;

public class ReportsController : Controller
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var vm = await BuildReportViewModel();
        ViewData["Title"] = "Reports";
        ViewData["ActivePage"] = "Reports";
        return View(vm);
    }

    // PREVIEW — returns partial with report table
    [HttpPost]
    public async Task<IActionResult> Preview(string reportType)
    {
        if (!PermissionService.HasPermission(User, "Reports", "View"))
            return RedirectToAction("AccessDenied", "Auth");
        var vm = await BuildReportViewModel();
        vm.SelectedReport = reportType;
        ViewData["Title"] = "Reports";
        ViewData["ActivePage"] = "Reports";
        return View("Index", vm);
    }

    // DOWNLOAD CSV
    public async Task<IActionResult> DownloadCsv(string reportType)
    {
        if (!PermissionService.HasPermission(User, "Reports", "Download"))
            return RedirectToAction("AccessDenied", "Auth");
        var csv = await GenerateCsv(reportType);
        var bytes = Encoding.UTF8.GetBytes(csv);
        var fileName = $"{reportType}_{DateTime.Now:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    // DOWNLOAD PDF
    public async Task<IActionResult> DownloadPdf(string reportType)
    {
        if (!PermissionService.HasPermission(User, "Reports", "Download"))
            return RedirectToAction("AccessDenied", "Auth");
        var vm = await BuildReportViewModel();
        vm.SelectedReport = reportType;

        var pdfBytes = GeneratePdf(vm, reportType);
        var fileName = $"{reportType}_{DateTime.Now:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // ─── Private Helpers ──────────────────────────────────

    private async Task<ReportsViewModel> BuildReportViewModel()
    {
        // Monthly revenue (last 12 months)
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
        var monthlyRevenue = await _db.SalesOrders
            .Where(o => o.Status != "Cancelled" && o.OrderDate >= twelveMonthsAgo)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new MonthlyRevenuePoint
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        // Top products by revenue
        var topProducts = await _db.SalesOrderItems
            .Include(i => i.Product)
            .GroupBy(i => new { i.ProductId, i.Product.Name })
            .Select(g => new TopProductPoint
            {
                ProductName = g.Key.Name,
                UnitsSold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(8)
            .ToListAsync();

        // Sales by category
        var byCategory = await _db.SalesOrderItems
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .GroupBy(i => i.Product.Category.Name)
            .Select(g => new CategoryRevenuePoint
            {
                CategoryName = g.Key,
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
                Orders = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        // Low stock
        var lowStock = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive && p.StockQuantity <= p.ReorderLevel)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        return new ReportsViewModel
        {
            MonthlyRevenue = monthlyRevenue,
            TopProducts = topProducts,
            ByCategory = byCategory,
            LowStock = lowStock,
            TotalRevenue = monthlyRevenue.Sum(x => x.Revenue),
            TotalOrders = monthlyRevenue.Sum(x => x.Orders)
        };
    }

    private async Task<string> GenerateCsv(string reportType)
    {
        var sb = new StringBuilder();
        var vm = await BuildReportViewModel();

        switch (reportType)
        {
            case "revenue":
                sb.AppendLine("Month,Year,Orders,Revenue");
                foreach (var r in vm.MonthlyRevenue)
                    sb.AppendLine($"{r.Label},{r.Orders},{r.Revenue:F2}");
                break;

            case "products":
                sb.AppendLine("Product,Units Sold,Revenue");
                foreach (var p in vm.TopProducts)
                    sb.AppendLine($"\"{p.ProductName}\",{p.UnitsSold},{p.Revenue:F2}");
                break;

            case "category":
                sb.AppendLine("Category,Revenue,Transactions");
                foreach (var c in vm.ByCategory)
                    sb.AppendLine($"\"{c.CategoryName}\",{c.Revenue:F2},{c.Orders}");
                break;

            case "lowstock":
                sb.AppendLine("Product,Category,Supplier,Stock,Reorder Level");
                foreach (var p in vm.LowStock)
                    sb.AppendLine($"\"{p.Name}\",\"{p.Category.Name}\",\"{p.Supplier.Name}\",{p.StockQuantity},{p.ReorderLevel}");
                break;
        }

        return sb.ToString();
    }

    private byte[] GeneratePdf(ReportsViewModel vm, string reportType)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var title = reportType switch
        {
            "revenue" => "Monthly Revenue Report",
            "products" => "Top Products Report",
            "category" => "Sales by Category Report",
            "lowstock" => "Low Stock Report",
            _ => "Report"
        };

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("RetailPulse").FontSize(20).Bold().FontColor("#4f8ef7");
                            c.Item().Text("ERP Management System").FontSize(10).FontColor("#9099b0");
                        });
                        row.ConstantItem(150).AlignRight().Column(c =>
                        {
                            c.Item().Text(title).FontSize(12).Bold();
                            c.Item().Text($"Generated: {DateTime.Now:MMM d, yyyy}").FontSize(9).FontColor("#9099b0");
                        });
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#2a3050");
                });

                page.Content().PaddingTop(20).Column(col =>
                {
                    if (reportType == "revenue")
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#1e2235").Padding(8).Text("Month").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Orders").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Revenue").Bold().FontColor("#9099b0");
                            });

                            foreach (var r in vm.MonthlyRevenue)
                            {
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(r.Label);
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(r.Orders.ToString());
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text($"${r.Revenue:N2}");
                            }

                            table.Cell().ColumnSpan(2).Padding(8).Text("Total Revenue").Bold();
                            table.Cell().Padding(8).Text($"${vm.TotalRevenue:N2}").Bold().FontColor("#4f8ef7");
                        });
                    }
                    else if (reportType == "products")
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#1e2235").Padding(8).Text("Product").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Units Sold").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Revenue").Bold().FontColor("#9099b0");
                            });

                            foreach (var p in vm.TopProducts)
                            {
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.ProductName);
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.UnitsSold.ToString());
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text($"${p.Revenue:N2}");
                            }
                        });
                    }
                    else if (reportType == "category")
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#1e2235").Padding(8).Text("Category").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Transactions").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Revenue").Bold().FontColor("#9099b0");
                            });

                            foreach (var c in vm.ByCategory)
                            {
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(c.CategoryName);
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(c.Orders.ToString());
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text($"${c.Revenue:N2}");
                            }
                        });
                    }
                    else if (reportType == "lowstock")
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#1e2235").Padding(8).Text("Product").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Category").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Supplier").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Stock").Bold().FontColor("#9099b0");
                                h.Cell().Background("#1e2235").Padding(8).Text("Reorder At").Bold().FontColor("#9099b0");
                            });

                            foreach (var p in vm.LowStock)
                            {
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.Name);
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.Category.Name);
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.Supplier.Name);
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.StockQuantity.ToString()).FontColor("#f75f5f");
                                table.Cell().BorderBottom(1).BorderColor("#1e2235").Padding(8).Text(p.ReorderLevel.ToString());
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("RetailPulse ERP — ").FontColor("#555e78");
                    x.Span($"Page ").FontColor("#555e78");
                    x.CurrentPageNumber().FontColor("#555e78");
                    x.Span(" of ").FontColor("#555e78");
                    x.TotalPages().FontColor("#555e78");
                });
            });
        });

        return document.GeneratePdf();
    }
}