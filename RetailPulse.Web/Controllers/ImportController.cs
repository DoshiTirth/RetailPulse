using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Models.ViewModels;
using RetailPulse.Web.Services;
using System.Globalization;

namespace RetailPulse.Web.Controllers;

public class ImportController : Controller
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public ImportController(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public IActionResult Products()
    {
        if (!PermissionService.HasPermission(User, "Products", "Add"))
            return RedirectToAction("AccessDenied", "Auth");

        ViewData["Title"] = "Import Products";
        ViewData["ActivePage"] = "Products";
        return View(new CsvImportViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(IFormFile file)
    {
        if (!PermissionService.HasPermission(User, "Products", "Add"))
            return RedirectToAction("AccessDenied", "Auth");

        ViewData["Title"] = "Import Products";
        ViewData["ActivePage"] = "Products";

        var vm = new CsvImportViewModel();

        if (file == null || file.Length == 0)
        {
            vm.Errors.Add("Please select a CSV file.");
            return View("Products", vm);
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            vm.Errors.Add("Only CSV files are supported.");
            return View("Products", vm);
        }

        try
        {
            var categories = await _db.Categories.ToListAsync();
            var suppliers = await _db.Suppliers.Where(s => s.IsActive).ToListAsync();
            var existingSkus = await _db.Products.Select(p => p.SKU).ToHashSetAsync();

            using var reader = new StreamReader(file.OpenReadStream());
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var row = new CsvProductRow
                {
                    Name = csv.GetField("Name") ?? "",
                    SKU = csv.GetField("SKU") ?? "",
                    CategoryName = csv.GetField("Category") ?? "",
                    SupplierName = csv.GetField("Supplier") ?? "",
                    UnitPrice = decimal.TryParse(csv.GetField("UnitPrice"),
                                        out var price) ? price : 0,
                    StockQuantity = int.TryParse(csv.GetField("StockQuantity"),
                                        out var stock) ? stock : 0,
                    ReorderLevel = int.TryParse(csv.GetField("ReorderLevel"),
                                        out var reorder) ? reorder : 10,
                };

                // Validate
                if (string.IsNullOrWhiteSpace(row.Name))
                    row.ErrorMessage = "Name is required";
                else if (string.IsNullOrWhiteSpace(row.SKU))
                    row.ErrorMessage = "SKU is required";
                else if (existingSkus.Contains(row.SKU))
                    row.ErrorMessage = $"SKU '{row.SKU}' already exists";
                else if (!categories.Any(c => c.Name.Equals(row.CategoryName,
                             StringComparison.OrdinalIgnoreCase)))
                    row.ErrorMessage = $"Category '{row.CategoryName}' not found";
                else if (!suppliers.Any(s => s.Name.Equals(row.SupplierName,
                             StringComparison.OrdinalIgnoreCase)))
                    row.ErrorMessage = $"Supplier '{row.SupplierName}' not found";
                else if (row.UnitPrice <= 0)
                    row.ErrorMessage = "Unit price must be greater than 0";

                if (row.ErrorMessage != null)
                    row.IsValid = false;

                vm.Preview.Add(row);
            }

            vm.ValidCount = vm.Preview.Count(r => r.IsValid);
            vm.HasPreview = true;

            // Store in TempData for import
            TempData["CsvPreview"] = System.Text.Json.JsonSerializer.Serialize(vm.Preview);
        }
        catch (Exception ex)
        {
            vm.Errors.Add($"Error parsing CSV: {ex.Message}");
        }

        return View("Products", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import()
    {
        if (!PermissionService.HasPermission(User, "Products", "Add"))
            return RedirectToAction("AccessDenied", "Auth");

        var json = TempData["CsvPreview"]?.ToString();
        if (string.IsNullOrEmpty(json))
        {
            TempData["Error"] = "Import session expired. Please upload the file again.";
            return RedirectToAction(nameof(Products));
        }

        var rows = System.Text.Json.JsonSerializer
            .Deserialize<List<CsvProductRow>>(json) ?? new();

        var categories = await _db.Categories.ToListAsync();
        var suppliers = await _db.Suppliers.ToListAsync();
        var imported = 0;

        foreach (var row in rows.Where(r => r.IsValid))
        {
            var category = categories.First(c =>
                c.Name.Equals(row.CategoryName, StringComparison.OrdinalIgnoreCase));
            var supplier = suppliers.First(s =>
                s.Name.Equals(row.SupplierName, StringComparison.OrdinalIgnoreCase));

            var product = new Product
            {
                Name = row.Name,
                SKU = row.SKU,
                CategoryId = category.CategoryId,
                SupplierId = supplier.SupplierId,
                UnitPrice = row.UnitPrice,
                StockQuantity = row.StockQuantity,
                ReorderLevel = row.ReorderLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Products", "Import",
                product.ProductId, product.Name,
                newValues: new { product.Name, product.SKU, product.UnitPrice });

            imported++;
        }

        TempData["Success"] = $"Successfully imported {imported} products.";
        return RedirectToAction("Index", "Products");
    }
}