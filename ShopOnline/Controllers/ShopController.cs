using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopOnline.Models;
using PagedList;

namespace shopOnline.Controllers
{
    public class ShopController : Controller
    {
        menfashionEntities db = new menfashionEntities();
        // GET: Shop lấy danh sách giảm phẩm 
        public ActionResult ProductList()
        {
            return View();
        }
        // xuất ra sản phẩm 
        public PartialViewResult ListItem(string brand,int? categories,int? page, string searching) // Show product
        {
            // đặt 2 biến là pageNumber và Size để cấu hình khung sản phẩm 
            var pageNumber = page ?? 1; // mặc định trang đầu 
            var pageSize = 9; // thiết lập 9 dòng 
            if(searching != null)
            {
                // nếu mà tìm kiếm khác null thì sẽ xuất ra những những sản phẩm theo thứ tự giảm dần  của ngày tạo 
                
                ViewBag.categories = categories;
                // xuất ra danh sách chứ từ khóa tìm kiếm hoặc là không tìm kiếm luôn 
                var list = db.Products.Where(model => model.productName.Contains(searching) || searching == null && model.status == true).OrderByDescending(model => model.dateCreate).ToPagedList(pageNumber, pageSize);
                return PartialView(list);
            }
            else
            {
                // nếu mà tìm kiếm theo thương hiệu 
                if (brand != null && categories == null)
                {
                    ViewBag.categories = categories;
                    var list = db.Products.OrderByDescending(model => model.dateCreate).Where(model => model.brand == brand && model.status == true).ToPagedList(pageNumber, pageSize);
                    return PartialView(list);
                }
                else if (brand == null && categories != null)
                {
                    // tìm kiếm theo loại sản phẩm 
                    ViewBag.categories = categories;
                    var list = db.Products.OrderByDescending(model => model.dateCreate).Where(model => model.categoryId == categories && model.status == true).ToPagedList(pageNumber, pageSize);
                    return PartialView(list);
                }
                else
                {
                    // xuất ra danh sách sản phẩm mặc định theo ngày ngày tạo giảm dần 
                    var list = db.Products.OrderByDescending(model => model.dateCreate).Where(model => model.status == true).ToPagedList(pageNumber, pageSize);
                    return PartialView(list);
                }
            }
        }
        public PartialViewResult Categories() // List categories
        {
            var list = db.ProductCategories.ToList();
            return PartialView(list);
        }
        public PartialViewResult Brand() // List brand
        {
            List<String> brand = new List<string>();
            foreach (Product i in this.db.Products)
            {
                if (!brand.Contains(i.brand.Trim()))
                    brand.Add(i.brand.Trim());
            }
            return PartialView(brand);
        }        
        public PartialViewResult RelationProduct(int? category) // List brand
        {
            // lấy ra 4 sản phẩm có cùng loại sản phẩm 
            var categories = db.Products.Where(model => model.categoryId == category).Take(4).ToList();
            return PartialView(categories);
        }

        public ActionResult ProductDetail(int? id, int? category)
        {
            // chuyền vào id sản phẩm và loại sản phẩm 
            if (id == null)
            {
               // id sản phầm mà null => sản phẩm không tồn tại và lỗi 
                return RedirectToAction("Error", "Home");
            }
            // tạo biến chi tiết theo id sản phẩm 
            var detail = db.Products.Where(model => model.productId == id).Single();

            // hiện giá sản phẩm = giá trong db - ( giá trong db   * giảm giá ) / 100

            // công thức giảm giá = giá cũ - ( giá cũ * giảm giá)/100 = giá mới 
            ViewBag.NewPrice = detail.price - ((detail.price * detail.discount) / 100);
            if (detail == null)
            {
                // nếu mà không có sản phẩm thì báo lỗi
                return RedirectToAction("Error", "Home");
            }
            return View(detail);
        }
    }
}