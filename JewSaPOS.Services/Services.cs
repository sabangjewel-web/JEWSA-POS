using System.Collections.Generic;
using JewSaPOS.Data;
using JewSaPOS.Domain;

namespace JewSaPOS.Services
{
    public sealed class AuthService
    {
        private readonly UserRepository users = new UserRepository();

        /// <summary>Returns the user when credentials are valid and active; otherwise null (generic failure).</summary>
        public User Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) return null;
            var rec = users.GetLogin(username.Trim());
            if (rec == null || !rec.User.IsActive) return null;
            if (!PasswordHasher.Verify(rec.Salt, password, rec.PasswordHash)) return null;
            users.TouchLogin(rec.User.UserId);
            return rec.User;
        }
    }

    public sealed class DashboardService
    {
        private readonly ReportRepository reports = new ReportRepository();
        public DashboardStats Stats() { return reports.Dashboard(); }
        public List<Product> LowStock() { return reports.LowStock(); }
    }

    public sealed class InventoryService
    {
        private readonly ProductRepository products = new ProductRepository();
        private readonly CategoryRepository categories = new CategoryRepository();
        private readonly InventoryRepository inventory = new InventoryRepository();
        private readonly User actor;

        public InventoryService(User actor) { this.actor = actor; }

        public bool CanModify { get { return actor.CanManageInventory; } }

        private void RequireManage()
        {
            if (!actor.CanManageInventory)
                throw new PosException("You do not have permission to change inventory.");
        }

        public List<Product> Search(string text, bool lowStockOnly, bool includeInactive)
        {
            return products.Search((text ?? "").Trim(), lowStockOnly, includeInactive);
        }

        public List<Category> Categories() { return categories.List(); }

        public int Add(Product p)
        {
            RequireManage(); Normalize(p); Validate(p, true);
            return products.Insert(p, actor.UserId);
        }

        public void Update(Product p)
        {
            RequireManage(); Normalize(p); Validate(p, false);
            products.Update(p);
        }

        public void Delete(int productId)
        {
            RequireManage();
            products.Delete(productId);
        }

        public int Adjust(int productId, int change, string reason)
        {
            RequireManage();
            if (change == 0) throw new PosException("Enter a non-zero quantity (positive adds stock, negative removes).");
            if (string.IsNullOrWhiteSpace(reason)) throw new PosException("Please give a reason for the adjustment.");
            return inventory.Adjust(productId, change, reason.Trim(), actor.UserId);
        }

        public List<InventoryLogEntry> History(int productId) { return inventory.History(productId); }

        private static void Normalize(Product p)
        {
            p.Sku = (p.Sku ?? "").Trim();
            p.Name = (p.Name ?? "").Trim();
            p.Barcode = string.IsNullOrWhiteSpace(p.Barcode) ? null : p.Barcode.Trim();
        }

        private static void Validate(Product p, bool isNew)
        {
            if (p.Sku.Length == 0) throw new PosException("SKU is required.");
            if (p.Sku.Length > 30) throw new PosException("SKU must be 30 characters or fewer.");
            if (p.Name.Length == 0) throw new PosException("Product name is required.");
            if (p.Name.Length > 150) throw new PosException("Product name must be 150 characters or fewer.");
            if (p.Barcode != null && p.Barcode.Length > 50) throw new PosException("Barcode must be 50 characters or fewer.");
            if (p.CategoryId <= 0) throw new PosException("Please choose a category.");
            if (p.Price < 0 || p.Cost < 0) throw new PosException("Price and cost cannot be negative.");
            if (p.ReorderLevel < 0) throw new PosException("Reorder level cannot be negative.");
            if (isNew && p.StockQty < 0) throw new PosException("Initial stock cannot be negative.");
        }
    }
}
