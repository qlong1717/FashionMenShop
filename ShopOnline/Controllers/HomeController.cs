using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ShopOnline.Models;

namespace shopOnline.Controllers
{

    // Session là 1 trong những thuộc tính của Interface Controller 
    public class HomeController : Controller
    {
        // tạo đối tượng database 
        menfashionEntities db = new menfashionEntities();

        // index của Home
        public ActionResult Index()
        {
            return View();
        }

        // trang about của Home
        public ActionResult About()
        {
            return View();
        }

        // trang Contact của Home
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        // hàm khi nhập liên hệ vào 
        public ActionResult Contact(Contact contact)
        {
            // lấy ngày hiện tại cho cái liên hệ;
            contact.dateContact = DateTime.Now;

            //thêm liên hệ đó vào database
            db.Contacts.Add(contact);
            //tạo 1 biến result để bắt kết quả lưu trữ
            var result = db.SaveChanges();
            if (result > 0)
            {
                // kết quả là thành công thì sẽ thông báo nhật được tin 
                ViewBag.MessageSuccess = "Your message has been received by us. We will contact you as soon as possible.";
            }
            return View(contact);
        }
        
        // trang error nếu mà có vấn đề về trang Home
        public ActionResult Error()
        {
            return View();
        }    
        // trang hiện chỉnh sửa hồ sơ 
        [HttpGet]
        public ActionResult EditProfie(string userName)
        {
            // hàm chỉnh sủa hồ sơ nhận biến userName
            // tạo 1 biến tên member có kiểu dữ liệu là Member và gán vô giá trị userName của người dùng vào để đối chiếu trong database
            Member member = db.Members.Find(userName);
            // trả về thông tin của member nếu người dùng được tìm thấy 
            Session["imgPath"] = member.avatar;
            return View(member);
        }            
        // trang sẽ thay đổi hồ sơ 
        [HttpPost]
        public ActionResult EditProfie(Member member, HttpPostedFileBase uploadFile)
        {
            //HttpPostedFileBase là 1 giao thức cho phép người dùng có thể truy xuất file ở bên ngoài 
            // tạo 2 biến member và uploadFile 

            // try để bắt lỗi
            try
            {
                if (ModelState.IsValid)
                {
                    // nếu như là trạng thái model Member là có giá trị thay đổi 
                    if (uploadFile != null)
                    {
                        // biến hình ảnh khác với null 
                        // thiết lập những thuộc tính cần thiết để lưu trữ file hình ảnh người dùng thay đổi
                        // => gán tên File vào fileName
                        var fileName = Path.GetFileName(uploadFile.FileName);
                        //=> gán thành phần đường dẫn mà mình muốn lưu vào 
                        var path = Path.Combine(Server.MapPath("~/Content/img/avatar"), fileName);

                        // khi mà gán vô xong thì ta sẽ gán hình ảnh vào thuộc tính avatar của người dùng
                        member.avatar = "~/Content/img/avatar/" + fileName;

                        // trạng thái đối tượng menber sẽ được chỉnh sửa 
                        db.Entry(member).State = EntityState.Modified;

                        
                        string oldImgPath = Request.MapPath(Session["imgPath"].ToString()); // Lấy đường dẫn ảnh (absolute path) đường dẫn cụ thể 
                        var avatarName = Session["imgPath"].ToString(); // Lấy đường dẫn ảnh (relative path)
                        var checkAvatart = db.Members.Where(model => model.avatar == avatarName).ToList(); // Kiểm tra ảnh có trùng với avatar của member nào không

                        if (db.SaveChanges() > 0)
                        {
                            // thay đổi được thì lưu đường dẫn thành phần 
                            uploadFile.SaveAs(path);

                            if (System.IO.File.Exists(oldImgPath) && checkAvatart.Count < 2) // Nếu tồn tại hình trong folder và không member nào có hình này thì xóa ra khỏi folder
                            {
                                System.IO.File.Delete(oldImgPath);
                            }
                            
                            var info = db.Members.Where(model => model.userName == member.userName).SingleOrDefault();// Lấy thông tin mới cập nhập lưu vào session
                            Session["info"] = info;
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        // hình ảnh bằng null 
                        // vẫn là session hình ảnh hiện tại 
                        member.avatar = Session["imgPath"].ToString();
                        // thiết lập trang thái cho cái thông tin của đối tượng member được thay đổi
                        db.Entry(member).State = EntityState.Modified;
                        if (db.SaveChanges() > 0)
                        {
                            var info = db.Members.Where(model => model.userName == member.userName).SingleOrDefault();// Lấy thông tin mới cập nhập lưu vào session
                            Session["info"] = info;
                            return RedirectToAction("Index");
                        }
                    }
                }
                // gọi hàm tham chiếu tới vai trò của member
                ViewBag.roleId = new SelectList(db.Roles, "roleId", "roleName", member.roleId);
                return View(member);
            }
            catch (Exception ex)
            {
                //nếu mà có lỗi trong quá trình thay đổi hồ sơ thì bắt lỗi và điều hướng qua index
                TempData["msgEditProfieFailed"] = "Edit failed! " + ex.Message;
                return RedirectToAction("Index");
            }
        }    
        [HttpGet]
        public ActionResult ChangePassword(string userName)
        {
            //tham chiếu tới người dùng hiện tại
            Member member = db.Members.Find(userName);
            return View();
        }        
        [HttpPost]
        public ActionResult ChangePassword(Member member, FormCollection collection)
        {
            try
            {
                // khởi tạo 2 biến mật khẩu hiện tại và mật khẩu mới 
                var CurrentPassword = collection["CurPassword"]; // Mật khẩu hiện tại
                var NewPassword = collection["NewPassword"]; // Mật khẩu mới
                //var ConfirmPassword = collection["Confirm"];

                //  mã hóa mật khẩu hiện tại và dùng Trim() để mà xóa khoảng cách trong text 
                CurrentPassword = Encryptor.MD5Hash(CurrentPassword.Trim());

                // mã hóa mật khẩu mới 
                NewPassword = Encryptor.MD5Hash(NewPassword.Trim());

                // kiểm tra coi người dùng và mật khẩu có đúng không ?
                var check = db.Members.Where(model => model.password == CurrentPassword && model.userName == member.userName).FirstOrDefault();
                if (check != null)
                {

                    // nếu mà kiểm tra đúng thì sẽ lưu mật khẩu mới 

                    check.password = NewPassword;
                    db.SaveChanges();
                    TempData["msgChangePassword"] = "Successfully change password!";
                    return RedirectToAction("index");
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect your password!");
                    return View(member);
                }
            }
            catch (Exception ex)
            {
                TempData["msgChangePasswordFailed"] = "Edit failed! " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public ActionResult MyOrder(string userName)
        {
            // sắp xếp theo thứ tự giảm dần của hóa đơn của người dùng 
            var orders = db.Invoinces.OrderByDescending(model => model.dateOrder).Where(model => model.userName == userName).ToList();
            return View(orders);
        }
        [HttpGet]
        public ActionResult InvoinceDetail(string invoinceNo)
        {
            // tạo biến chi tiết là kiểm tra Id hóa đơn của sản phẩm.
            var detail = db.InvoinceDetails.Where(model => model.invoinceNo == invoinceNo).Include(model => model.Product).ToList();
            // tạo thông tin hóa đơn của người dùng 
            var infor = db.Invoinces.Where(model => model.invoinceNo == invoinceNo).Include(model => model.Member).FirstOrDefault();
            ViewBag.invoinceNo = invoinceNo;
            Session["information"] = infor;

            return View(detail);
        }
        public ActionResult Delete(string id)
        {
            // khởi tạo 1 danh sách chi tiết hóa đơn đối chiếu tới id hóa đơn 
            List<InvoinceDetail> ctdh = db.InvoinceDetails.Where(model => model.invoinceNo == id).ToList();
            foreach (var i in ctdh)
            {
                // quét tất cả các thành phần của danh sách chi tiết hóa đơn và xóa theo id 
                db.InvoinceDetails.Remove(i);
            }
            db.SaveChanges();
            // tạo biến hóa đơn theo id 
            Invoince invoince = db.Invoinces.Find(id);
            // xóa hóa đơn theo id 
            db.Invoinces.Remove(invoince);

            TempData["msgDeleteOrder"] = "Successfully delete order!";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public PartialViewResult ProductHome()
        {

            // xuất ra danh sách sản phẩm theo thứ dự giảm dần của ngày tạo theo ngày tháng năm 
            var list = db.Products.OrderByDescending(model => model.dateCreate).Take(8).ToList();
            // trả về 1 khung view danh sách đó 
            return PartialView(list);
        }
        public PartialViewResult BlogHome()
        {
            // xuất ra danh sách blog theo thứ tự giảm dần của ngày đăng lấy ngày tháng 
            var list = db.Articles.OrderByDescending(model => model.publicDate).Take(3).ToList();
            return PartialView(list);
        }
    }
}