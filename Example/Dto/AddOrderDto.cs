using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Dto
{
    public class AddOrderDto
    {
        /// <summary>
        /// 会员号
        /// </summary>
        public string CustomerNo { set; get; }
        /// <summary>
        /// 商品详情
        /// </summary>
        public List<ProductDto> ProductList { set; get; }
    }

    public class ProductDto
    {
        /// <summary>
        /// 商品号
        /// </summary>
        public string ProductNo { get; set; }
        /// <summary>
        /// 商品名称    
        /// </summary>
        public string ProductName { set; get; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { set; get; }
        
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { set; get; }
    }
}
