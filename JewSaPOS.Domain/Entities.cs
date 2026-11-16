using System;

namespace JewSaPOS.Domain
{
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Cashier = "Cashier";
    }

    /// <summary>Friendly error that is safe to show to the user.</summary>
    public class PosException : Exception
    {
        public PosException(string message) : base(message) { }
    }

    public sealed class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get { return RoleName == RoleNames.Admin; } }
        public bool CanManageInventory { get { return RoleName == RoleNames.Admin || RoleName == RoleNames.Manager; } }
    }

    public sealed class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public override string ToString() { return Name; }
    }

    public sealed class Product
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int StockQty { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsLowStock { get { return StockQty <= ReorderLevel; } }
        public string Status { get { return IsActive ? "Active" : "Inactive"; } }
    }

    public sealed class DashboardStats
    {
        public decimal TodaySales { get; set; }
        public int TodayOrders { get; set; }
        public int LowStockCount { get; set; }
        public int ActiveProducts { get; set; }
    }

    public sealed class InventoryLogEntry
    {
        public DateTime CreatedAt { get; set; }
        public int ChangeQty { get; set; }
        public string Reason { get; set; }
        public string UserName { get; set; }
    }
}
