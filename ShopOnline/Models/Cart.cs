using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopOnline.Models
{
    public class Cart
    {
        menfashionEntities db = new menfashionEntities();
        public int IdItem { get; set; }
        public string NameItem { get; set; }
        public string ImageItem { get; set; }
        public int PriceItem { get; set; }
        public int unitPrice { get; set; }
        public int Quantity { get; set; }
        public int Discount { get; set; }
        public Double PriceTotal
        {
            get
            {
                return Quantity * PriceItem;
            }
        }
        public Cart(int idProduct)
        {
            // hàm giỏ hàng chuyền vô id sản phẩm 
            this.IdItem = idProduct; // id sản phẩm 
            Product item = db.Products.Single(model => model.productId == IdItem); // kiểm tra id sản phẩm có trong db không ?
            this.NameItem = item.productName;
            this.ImageItem = item.image;
            this.unitPrice = int.Parse(item.price.ToString());
            this.PriceItem = int.Parse(item.price.ToString()) - ((int.Parse(item.price.ToString()) * int.Parse(item.discount.ToString())) / 100); // theo công thức tính giá 
            this.Quantity = 1; // mặc định số lượng sản phầm ban đầu là 1 
            this.Discount = ((int.Parse(item.price.ToString()) * int.Parse(item.discount.ToString())) / 100) * this.Quantity; // giảm giá để theo % 
        }
    }
}