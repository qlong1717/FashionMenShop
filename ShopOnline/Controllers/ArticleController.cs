using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShopOnline.Models;
using System.IO;
using PagedList;
using PagedList.Mvc;

namespace ShopOnline.Controllers
{
    public class ArticleController : Controller
    {
        
        menfashionEntities db = new menfashionEntities();
        public ActionResult ArticleList(int? page)
        {
            // số dòng = 6 và tên trang đầu là 1 
            int pageSize = 6;
            int pageNum = (page ?? 1);

            // danh sách blog theo giảm dần ngày xuất bản 
            var ListBlog = db.Articles.OrderByDescending(model => model.publicDate).ToPagedList(pageNum, pageSize);
            return View(ListBlog);
        }
        public ActionResult ArticleDetail(int? id)
        {
            // chi tiết blog 
            if (id == null)
            {
                // không có blog thì lỗi 
                return RedirectToAction("Error", "Home");
            }
            // tạo biến item gán blog dựa vào id và có dựa trên người tạo  
            var item = db.Articles.Where(model => model.articleId == id).Include(model => model.Member).Single(); // Single là so 1 cái blog thôi 
            if (item == null)
            {
                // nếu không có blog thì lỗi 
                return RedirectToAction("Error", "Home");
            }
            // trả về  view
            return View(item);
        }
    }
}