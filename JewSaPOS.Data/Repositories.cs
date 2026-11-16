using System;
using System.Collections.Generic;
using System.Data;
using JewSaPOS.Domain;

namespace JewSaPOS.Data
{
    public sealed class LoginRecord
    {
        public User User;
        public string PasswordHash;
        public string Salt;
    }

    public sealed class UserRepository
    {
        public LoginRecord GetLogin(string username)
        {
            var dt = Db.Query("dbo.usp_Login", Db.P("@Username", username));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new LoginRecord
            {
                PasswordHash = Convert.ToString(r["PasswordHash"]),
                Salt = Convert.ToString(r["Salt"]),
                User = new User
                {
                    UserId = Convert.ToInt32(r["UserId"]),
                    Username = Convert.ToString(r["Username"]),
                    FullName = Convert.ToString(r["FullName"]),
                    RoleId = Convert.ToInt32(r["RoleId"]),
                    RoleName = Convert.ToString(r["RoleName"]),
                    IsActive = Convert.ToBoolean(r["IsActive"])
                }
            };
        }

        public void TouchLogin(int userId)
        {
            Db.NonQuery("dbo.usp_User_TouchLogin", Db.P("@UserId", userId));
        }
    }

    public sealed class CategoryRepository
    {
        public List<Category> List()
        {
            var list = new List<Category>();
            foreach (DataRow r in Db.Query("dbo.usp_Category_List").Rows)
                list.Add(new Category { CategoryId = Convert.ToInt32(r["CategoryId"]), Name = Convert.ToString(r["Name"]) });
            return list;
        }
    }

    public sealed class ProductRepository
    {
        public List<Product> Search(string text, bool lowStockOnly, bool includeInactive)
        {
            var dt = Db.Query("dbo.usp_Product_Search",
                Db.P("@Search", string.IsNullOrWhiteSpace(text) ? null : text),
                Db.P("@LowStockOnly", lowStockOnly),
                Db.P("@IncludeInactive", includeInactive));
            var list = new List<Product>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new Product
                {
                    ProductId = Convert.ToInt32(r["ProductId"]),
                    Sku = Convert.ToString(r["Sku"]),
                    Barcode = r["Barcode"] == DBNull.Value ? null : Convert.ToString(r["Barcode"]),
                    Name = Convert.ToString(r["Name"]),
                    CategoryId = Convert.ToInt32(r["CategoryId"]),
                    CategoryName = Convert.ToString(r["CategoryName"]),
                    Price = Convert.ToDecimal(r["Price"]),
                    Cost = Convert.ToDecimal(r["Cost"]),
                    StockQty = Convert.ToInt32(r["StockQty"]),
                    ReorderLevel = Convert.ToInt32(r["ReorderLevel"]),
                    IsActive = Convert.ToBoolean(r["IsActive"])
                });
            }
            return list;
        }

        public int Insert(Product p, int userId)
        {
            return Convert.ToInt32(Db.Scalar("dbo.usp_Product_Insert",
                Db.P("@Sku", p.Sku), Db.P("@Barcode", p.Barcode), Db.P("@Name", p.Name),
                Db.P("@CategoryId", p.CategoryId), Db.P("@Price", p.Price), Db.P("@Cost", p.Cost),
                Db.P("@StockQty", p.StockQty), Db.P("@ReorderLevel", p.ReorderLevel), Db.P("@UserId", userId)));
        }

        public void Update(Product p)
        {
            Db.NonQuery("dbo.usp_Product_Update",
                Db.P("@ProductId", p.ProductId), Db.P("@Sku", p.Sku), Db.P("@Barcode", p.Barcode),
                Db.P("@Name", p.Name), Db.P("@CategoryId", p.CategoryId), Db.P("@Price", p.Price),
                Db.P("@Cost", p.Cost), Db.P("@ReorderLevel", p.ReorderLevel), Db.P("@IsActive", p.IsActive));
        }

        public void Delete(int productId)
        {
            Db.NonQuery("dbo.usp_Product_Delete", Db.P("@ProductId", productId));
        }
    }

    public sealed class InventoryRepository
    {
        public int Adjust(int productId, int changeQty, string reason, int userId)
        {
            return Convert.ToInt32(Db.Scalar("dbo.usp_Inventory_Adjust",
                Db.P("@ProductId", productId), Db.P("@ChangeQty", changeQty),
                Db.P("@Reason", reason), Db.P("@UserId", userId)));
        }

        public List<InventoryLogEntry> History(int productId)
        {
            var list = new List<InventoryLogEntry>();
            foreach (DataRow r in Db.Query("dbo.usp_Inventory_History", Db.P("@ProductId", productId)).Rows)
                list.Add(new InventoryLogEntry
                {
                    CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                    ChangeQty = Convert.ToInt32(r["ChangeQty"]),
                    Reason = Convert.ToString(r["Reason"]),
                    UserName = Convert.ToString(r["UserName"])
                });
            return list;
        }
    }

    public sealed class ReportRepository
    {
        public DashboardStats Dashboard()
        {
            var dt = Db.Query("dbo.usp_Report_Dashboard");
            var r = dt.Rows[0];
            return new DashboardStats
            {
                TodaySales = Convert.ToDecimal(r["TodaySales"]),
                TodayOrders = Convert.ToInt32(r["TodayOrders"]),
                LowStockCount = Convert.ToInt32(r["LowStockCount"]),
                ActiveProducts = Convert.ToInt32(r["ActiveProducts"])
            };
        }

        public List<Product> LowStock()
        {
            var list = new List<Product>();
            foreach (DataRow r in Db.Query("dbo.usp_Report_LowStock").Rows)
                list.Add(new Product
                {
                    ProductId = Convert.ToInt32(r["ProductId"]),
                    Sku = Convert.ToString(r["Sku"]),
                    Name = Convert.ToString(r["Name"]),
                    StockQty = Convert.ToInt32(r["StockQty"]),
                    ReorderLevel = Convert.ToInt32(r["ReorderLevel"]),
                    IsActive = true
                });
            return list;
        }
    }
}
