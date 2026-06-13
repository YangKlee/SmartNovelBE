using SmartNovelBE.Models;

namespace SmartNovelBE.Services
{
    public class MenuDashboardService
    {
        public List<MenuDashboard> GetMenuDashboardByRole(string roleId)
        {
            var menu = new List<MenuDashboard>();

            switch (roleId)
            {
                case "1": // Admin
                    menu.Add(new MenuDashboard { Title = "Dashboard", ActionUrl = "/dashboard/admin", Icon = "dashboard" });
                    menu.Add(new MenuDashboard { Title = "Quản lý người dùng", ActionUrl = "/dashboard/user-manager", Icon = "people" });
                    menu.Add(new MenuDashboard { Title = "Quản lý truyện", ActionUrl = "/dashboard/novel-manager", Icon = "menu_book" });
                    menu.Add(new MenuDashboard { Title = "Cài đặt hệ thống", ActionUrl = "/dashboard/settings", Icon = "settings" });
                    break;
                case "2": // Mod/Censor
                    menu.Add(new MenuDashboard { Title = "Dashboard", ActionUrl = "/dashboard/admin", Icon = "dashboard" });
                    menu.Add(new MenuDashboard { Title = "Duyệt truyện", ActionUrl = "/dashboard/censor", Icon = "fact_check" });
                    menu.Add(new MenuDashboard { Title = "Báo cáo", ActionUrl = "/dashboard/reports", Icon = "report" });
                    break;
                case "3": // Author
                    menu.Add(new MenuDashboard { Title = "Dashboard", ActionUrl = "/dashboard/author", Icon = "dashboard" });
                    menu.Add(new MenuDashboard { Title = "Truyện của tôi", ActionUrl = "/dashboard/author/novel-manager", Icon = "book" });
                    menu.Add(new MenuDashboard { Title = "Thêm truyện mới", ActionUrl = "/dashboard/author/novel-manager/create-novel", Icon = "add_box" });
                    menu.Add(new MenuDashboard { Title = "Bình luận độc giả", ActionUrl = "/dashboard/author/comment-reader", Icon = "comment" });
                    break;

                default:
                    break;
            }

            return menu;
        }
    }
}
