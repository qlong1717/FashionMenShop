using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopOnline.Models;
using PayPal.Api;

namespace shopOnline.Controllers
{
    public class CartController : Controller
    {
        menfashionEntities db = new menfashionEntities();

        // 
        public List<Cart> getCart() // tạo danh sách giỏ hàng và truyền vào phiên giỏ hàng 
        {
            // thiết lập phiên giỏ hàng là cả danh sách giỏ hàng 
            List<Cart> listCart = Session["Cart"] as List<Cart>;
            if (listCart == null)
            {
                // nếu mà giỏ hàng/ phiên giỏ hàng chưa có sản phẩm thì ta sẽ khởi tạo giỏ hàng để sử dụng 
                listCart = new List<Cart>();
                Session["Cart"] = listCart;
            }
            return listCart;
        }
        public ActionResult AddToCart(int id, string strURL) //  Add item in cart 

        {
            // tạo phiên giỏ hàng và danh sách giỏ hàng  
            List<Cart> listCart = getCart();
            // tạo đối tượng sản phảm theo id có kiểu dữ liệu hiện thị là Cart
            Cart item = listCart.Find(model => model.IdItem == id);
            if (item == null)
            {
                // tạo sản phẩm mới theo id  
                item = new Cart(id);
                // chuyền id sản phẩm vào danh sách giỏ hàng 
                listCart.Add(item);
                return Redirect(strURL); // lấy hình ảnh sản phẩm để hiện bên hóa đơn
            }
            else
            { 
                // nếu mà trong giỏ hàng đã có sản phẩm rồi thì ta tăng số lượng sản phẩm lên
                item.Quantity++;
                return Redirect(strURL); 
            }
        }
        private int Quanlity() // Lấy tổng số sản phẩm giỏ hàng hiện tại
        {
            // tại biến tổng là 0
            int amount = 0;
            // tạo phiên giỏ hàng là bằng 
            List<Cart> listCart = Session["Cart"] as List<Cart>;
       
            if (listCart != null)
            {
                // danh sách giỏ hàng tồn tại thì tính tổng số lượng 
                amount = listCart.Sum(model => model.Quantity);
            }
            return amount;
        }
        private double TotalPrice() //  Lấy tổng số tiền sản phẩm
        {
            // biến tổng = 0
            double total = 0;
            List<Cart> listCart = Session["Cart"] as List<Cart>;
            if (listCart != null)
            {
                // tính tổng giá tiền 
                total = listCart.Sum(model => model.PriceTotal);
            }
            return total;
        }
        public PartialViewResult Navbar() // Hiển thị số lượng sản phẩm và tiền trên navbar
        {
            ViewBag.quanlityItem = Quanlity();
            ViewBag.totalPrice = TotalPrice();
            return PartialView();
        }
        // trả về view khi mà không có giỏ hàng 
        public ActionResult NoICart()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult Cart()
        {
            // lấy phiên giỏ hàng 
            List<Cart> listCart = getCart();
            Session["Cart"] = listCart;
            if (listCart.Count == 0)
            {
                // nếu mà rỗng giỏ hàng thì trả về view không giỏ hàng  
                return RedirectToAction("NoICart", "Cart");
            }
            ViewBag.quanlityItem = Quanlity();
            ViewBag.totalPrice = TotalPrice();
            return View(listCart);
        }
        public ActionResult DeteteCart(int id)
        {
            
            List<Cart> listCart = getCart();
            // tại đối tượng sản phầm theo id kiểu Cart
            Cart item = listCart.SingleOrDefault(model => model.IdItem == id);
            if (item != null)
            {
                // nếu mà sản phẩm tồn tại thì xóa sản phẩm theo id trong danh sách
                listCart.RemoveAll(model => model.IdItem == id);
                return RedirectToAction("Cart", "Cart");
            }
            if (listCart.Count == 0)
            {
                // nếu mà giỏ hàng rỗng thì chuyền ra index
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Cart", "Cart");
        }
        public ActionResult UpdateCart(FormCollection form)
        {
            // tạo 1 mảng số lượng và gán số lượng từ form qua
            string[] qualities = form.GetValues("quanlity");
            List<Cart> listCart = getCart();
            // quét tất cả các sản phẩm trong danh sách 
            for (int i = 0; i < listCart.Count; i++)
            {
                // gán số lượng từ form qua danh sách và theo  thành kiểu int
                listCart[i].Quantity = Convert.ToInt32(qualities[i]);
            }
            if (Quanlity() == 0)
            {
                // số lượng không thay đổi 
                Session["Cart"] = null;
                // hủy bỏ phiên 
                return RedirectToAction("NoICart", "Cart");
            }
            else
            {
                return RedirectToAction("Cart", "Cart");
            }
        }
        public static string ConvertTimeTo24(string hour) // hàm chuyền đổi giờ sang kiểu 24g
        {
            string h = "";
            switch (hour)
            {
                case "1":
                    h = "13";
                    break;
                case "2":
                    h = "14";
                    break;
                case "3":
                    h = "15";
                    break;
                case "4":
                    h = "16";
                    break;
                case "5":
                    h = "17";
                    break;
                case "6":
                    h = "18";
                    break;
                case "7":
                    h = "19";
                    break;
                case "8":
                    h = "20";
                    break;
                case "9":
                    h = "21";
                    break;
                case "10":
                    h = "22";
                    break;
                case "11":
                    h = "23";
                    break;
                case "12":
                    h = "0";
                    break;
            }
            return h;
        }
        public static string CreateKey(string tiento) // Tạo chuỗi mã hóa ngày - giờ cho mã hóa đơn - Id của Order
        {
            
            // chuyền vào tiền tố giờ vào 
            string key = tiento;
            // mảng ngày 
            string[] partsDay;
            partsDay = DateTime.Now.ToShortDateString().Split('/'); // chuyền vào ngày giờ hiện tại và chia bằng "/"
            //Ví dụ 07/08/2009
            // tạo biến d theo format 3 là gồm part Tháng , part Ngày, part Năm   
            string d = String.Format("{0}{1}{2}", partsDay[0], partsDay[1], partsDay[2]);

            // cái key = tiền tố + thángngàynăm
            key = key + d;

            string[] partsTime;
            partsTime = DateTime.Now.ToLongTimeString().Split(':');
            //Ví dụ 7:08:03 PM hoặc 7:08:03 AM

            // giờ : phút : giây : phần giờ 
            if (partsTime[2].Substring(3, 2) == "PM")
                // nếu mà format giờ  mà PM thì sẽ đổi sang kiểu 24g
                partsTime[0] = ConvertTimeTo24(partsTime[0]);
            if (partsTime[2].Substring(3, 2) == "AM")
                // nếu mà giờ mà AM
                if (partsTime[0].Length == 1)
                    // độ dài của phút = 1 thì đồ dài phút sẽ bằng thêm số 0 vào khúc đầu ví 8 phút = 08 phút
                    partsTime[0] = "0" + partsTime[0];
            //Xóa ký tự trắng và PM hoặc AM
            partsTime[2] = partsTime[2].Remove(2, 3);
            string t;
            t = String.Format("_{0}{1}{2}", partsTime[0], partsTime[1], partsTime[2]);
            key = key + t;
            return key;
        }
        
        // Trang checkout khi đã đăng nhập tài khoản
        [HttpGet]
        public ActionResult Checkout()
        {
            List<Cart> listCart = getCart();
            ViewBag.quanlityItem = Quanlity();
            ViewBag.totalPrice = TotalPrice();

            return View(listCart);
        }
        [HttpPost]
        public ActionResult Checkout(FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // trang thái giỏ hàng thay đổi 
                    // tạo bill kiểu hóa đơn
                    Invoince bill = new Invoince();

                    Member member = (Member)Session["info"]; // Lấy thông tin tài khoản từ session 
                    // lấy phiên giỏ hàng 
                    List<Cart> listCart = getCart();
                    //gán key cho Id giỏ hàng có thêm HD làm tiền tố 
                    bill.invoinceNo = CreateKey("HD");
                    bill.userName = member.userName;
                    bill.dateOrder = DateTime.Now;
                    bill.status = true;
                    bill.deliveryDate = null;
                    bill.deliveryStatus = false;

                    // Biến totalmney lưu tổng tiền sản phẩm từ giỏ hàng
                    int totalmoney = 0;
                    foreach (var item in listCart)
                    {
                        totalmoney += Convert.ToInt32(item.PriceTotal);
                    }
                    bill.totalMoney = totalmoney;
                    db.Invoinces.Add(bill);
                    db.SaveChanges();
                    foreach (var item in listCart)
                    {
                        //tạo cthd 
                        InvoinceDetail ctdh = new InvoinceDetail();
                        ctdh.invoinceNo = bill.invoinceNo;
                        ctdh.productId = item.IdItem;
                        ctdh.quanlityProduct = item.Quantity;
                        ctdh.unitPrice = item.unitPrice;
                        ctdh.totalPrice = (int?)(long)item.PriceTotal;
                        ctdh.totalDiscount = item.Discount * item.Quantity;
                        db.InvoinceDetails.Add(ctdh);
                    }
                    db.SaveChanges();
                    return RedirectToAction("SubmitBill", "Cart");
                }
                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        
        //Trang checkout khi không đăng nhập tài khoản
        [HttpGet]
        public ActionResult CheckoutNoAccount()
        {
            
            List<Cart> listCart = getCart();
            ViewBag.quanlityItem = Quanlity();
            ViewBag.totalPrice = TotalPrice();
            return View(listCart);
        }
        [HttpPost]
        public ActionResult CheckoutNoAccount(Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    var check = db.Customers.Where(model => model.lastName == customer.lastName && model.email == customer.email).FirstOrDefault();
                    // Kiểm tra xem tên khách hàng có email nhập đã tồn tại hay chưa
                    if (check == null) // Nếu chưa thì lưu lại thông tin vào bảng customer
                    {
                        db.Customers.Add(customer);
                        db.SaveChanges();
                        Invoince bill = new Invoince();
                        List<Cart> listCart = getCart();
                        bill.customerId = customer.customerId;
                        bill.invoinceNo = CreateKey("HD");
                        bill.dateOrder = DateTime.Now;
                        bill.status = true;
                        bill.deliveryDate = null;
                        bill.deliveryStatus = false;
                        int totalmoney = 0;
                        foreach (var item in listCart)
                        {
                            totalmoney += Convert.ToInt32(item.PriceTotal);
                        }
                        bill.totalMoney = totalmoney;
                        db.Invoinces.Add(bill);
                        db.SaveChanges();
                        foreach (var item in listCart)
                        {
                            InvoinceDetail ctdh = new InvoinceDetail();
                            ctdh.invoinceNo = bill.invoinceNo;
                            ctdh.productId = item.IdItem;
                            ctdh.quanlityProduct = item.Quantity;
                            ctdh.unitPrice = item.unitPrice;
                            ctdh.totalPrice = (int?)(long)item.PriceTotal;
                            ctdh.totalDiscount = item.Discount * item.Quantity;
                            db.InvoinceDetails.Add(ctdh);
                        }
                        db.SaveChanges();
                        return RedirectToAction("SubmitBill", "Cart");
                    }
                    else
                    {
                        Invoince bill = new Invoince();
                        List<Cart> listCart = getCart();
                        bill.customerId = check.customerId;
                        bill.invoinceNo = CreateKey("HD");
                        bill.dateOrder = DateTime.Now;
                        bill.status = true;
                        bill.deliveryDate = null;
                        bill.deliveryStatus = false;
                        int totalmoney = 0;
                        foreach (var item in listCart)
                        {
                            totalmoney += Convert.ToInt32(item.PriceTotal);
                        }
                        bill.totalMoney = totalmoney;
                        db.Invoinces.Add(bill);
                        db.SaveChanges();
                        foreach (var item in listCart)
                        {
                            InvoinceDetail ctdh = new InvoinceDetail();
                            ctdh.invoinceNo = bill.invoinceNo;
                            ctdh.productId = item.IdItem;
                            ctdh.quanlityProduct = item.Quantity;
                            ctdh.unitPrice = item.unitPrice;
                            ctdh.totalPrice = (int?)(long)item.PriceTotal;
                            ctdh.totalDiscount = item.Discount * item.Quantity;
                            db.InvoinceDetails.Add(ctdh);
                        }
                        db.SaveChanges();
                        return RedirectToAction("SubmitBill", "Cart");
                    }

                }

                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        public ActionResult SubmitBill()
        {
            ViewBag.quanlityItem = Quanlity();
            ViewBag.totalPrice = TotalPrice();
            return View();
        }
        [HttpPost]
        public ActionResult SubmitBill(FormCollection form)
        {
            Session.Remove("Cart");
            //Session["Cart"] = null;
            return RedirectToAction("Index", "Home");
        }

        // Paypal 
        private Payment payment;
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var listItems = new ItemList() { items = new List<Item>() };
            List<Cart> listCart = getCart();
            foreach(var cart in listCart)
            {
                // chuyền các sản phẩm từ giỏ hàng vào 1 danh sách API của Paypal mới  
                listItems.items.Add(new Item()
                {
                    name = cart.NameItem,
                    currency = "USD",
                    price = cart.PriceItem.ToString(),
                    quantity = cart.Quantity.ToString(),
                    sku = "sku" //gán cho paypal theo dõi hóa đơn sản phẩm 
                });
            }
            // tạo đối tượng thanh toán Paypal
            var payer = new Payer() { payment_method = "paypal"};

            // tạo đường dẫn điều hướng 
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl, // chuyền vào đường dẫn 
                return_url = redirectUrl // đường dẫn trả về 
            };

            var details = new Details()
            {
                tax = "0", // ko thuế
                shipping = "0", // ko  giao hàng 
                subtotal = listCart.Sum(model => model.Quantity * model.PriceItem).ToString() // tính tổng sản phẩm 
            };

            var amount = new Amount()
            {
                currency = "USD", // đơn vị là USD
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(), // tax + shipping + subtotal
                details = details
            };

            var transactionList = new List<Transaction>(); // khai báo số hóa đơn và người phải trả 
            transactionList.Add(new Transaction()
            {
                // thêm thông tin cho hóa đơn
                description = "Menfashion transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = listItems
            });

            payment = new Payment()
            {
                // thêm thông tin thanh toán 
                intent = "sale", // dạng thanh toán là bán hàng
                payer = payer, // phương thức là paypal
                transactions = transactionList, // thông tin hóa đơn
                redirect_urls = redirUrls // đường dẫn khi thanh toán xong
            };

            return payment.Create(apiContext);
        }
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            // khởi tạo xử lý 
            var paymentExecution = new PaymentExecution()
            {
                // gán id cho xử lý thanh toán
                payer_id = payerId
            };
            // khởi tạo thanh toán hóa đơn palpay = id hóa đơn 
            payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }
        public ActionResult PaymentWithPaypal()
        {
            APIContext apiContext = PaypalConfiguration.GetAPIContext(); // lấy dữ liệu PayPal API

            try
            {
                // yêu cầu mã Id thanh toán 
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    // thiết lập khung xương URL 
                    string baseURL = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000)); // random id 
                    var createdPayment = CreatePayment(apiContext, baseURL + "guid=" + guid); // tạo hóa đơn dựa vào Api chuyền vào URL và id 

                    
                    var links = createdPayment.links.GetEnumerator(); // khởi tạo và kiểm tra link thanh toán có đúng không 
                    string paypalRedirectUrl = string.Empty; // khởi tạo cho đường dẫn thanh toán Paypal ban đầu là rỗng

                    while (links.MoveNext())
                    {
                        // nhận sự kiện và gán link hiện tại vô 
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url")) // kiểm tra link = "/Cart/PaymentWithPaypal?"
                        {
                            // gán đường truyền thanh toán vào biến thanh toán paypal
                            paypalRedirectUrl = link.href;
                        }
                    }
                    // gán id  và điều hướng qua thanh toán paypal
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        Session.Remove("Cart");
                        return RedirectToAction("Error","Home");
                    }
                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                Session.Remove("Cart");
                return RedirectToAction("Error", "Home");
            }


            Member member = (Member)Session["info"];
            if(member!= null) // Nếu đã đăng nhập thì lấy thông tin thành viên trong session info
            {
                Invoince bill = new Invoince();
                List<Cart> listCart = getCart();
                bill.invoinceNo = CreateKey("HD");
                bill.userName = member.userName;
                bill.dateOrder = DateTime.Now;
                bill.status = true;
                bill.deliveryDate = null;
                bill.deliveryStatus = false;
                
                // Biến totalmoney lưu tổng tiền sản phẩm từ giỏ hàng
                int totalmoney = 0;
                foreach (var item in listCart)
                {
                    totalmoney += Convert.ToInt32(item.PriceTotal);
                }
                bill.totalMoney = totalmoney;
                db.Invoinces.Add(bill);
                db.SaveChanges();
                foreach (var item in listCart)
                {

                    InvoinceDetail ctdh = new InvoinceDetail();
                    ctdh.invoinceNo = bill.invoinceNo;
                    ctdh.productId = item.IdItem;
                    ctdh.quanlityProduct = item.Quantity;
                    ctdh.unitPrice = item.unitPrice;
                    ctdh.totalPrice = (int?)(long)item.PriceTotal;
                    ctdh.totalDiscount = item.Discount * item.Quantity;
                    db.InvoinceDetails.Add(ctdh);
                }
                db.SaveChanges();
                return RedirectToAction("SubmitBill", "Cart");
            }
            else
            {
                // Xử lí dữ liệu khách hàng nhập
                //Session.Remove("Cart");
                return RedirectToAction("SubmitBill", "Cart");
            }
        }
    }
}