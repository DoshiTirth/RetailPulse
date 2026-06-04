namespace RetailPulse.Web.Models.ViewModels;

public class CsvImportViewModel
{
    public List<CsvProductRow> Preview { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public int ValidCount { get; set; }
    public bool HasPreview { get; set; }
}

public class CsvProductRow
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsValid { get; set; } = true;
    public string? ErrorMessage { get; set; }
}