using System;
using System.Collections.Generic;
using System.Net.Http;
using Example.Models;
using Microsoft.AspNetCore.Mvc;
using SummerBoot.Core;
using System.Threading.Tasks;
using Example.Dto;
using Example.Feign;
using Example.Repository;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SummerBoot.Repository;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ICustomerRepository CustomerRepository { set; get; }

        private IOrderHeaderRepository OrderHeaderRepository { set; get; }

        private IOrderDetailRepository OrderDetailRepository { set; get; }

        private IUnitOfWork Uow { set; get; }
        
        public OrderController(ICustomerRepository customerRepository,
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IUnitOfWork uow
        )
        {
            this.CustomerRepository = customerRepository;
            this.OrderHeaderRepository = orderHeaderRepository;
            this.OrderDetailRepository = orderDetailRepository;
            this.Uow = uow;
        }
        
        /// <summary>
        /// 删库跑路
        /// </summary>
        /// <returns></returns>
        [HttpGet("DeleteDatabase")]
        public async Task DeleteDatabase()
        {
            await OrderHeaderRepository.DeleteAllOrder();
        }

        /// <summary>
        /// 根据会员编号取消订单
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [HttpGet("CancelOrderByCustomerNo")]
        public async Task<bool> CancelOrderByCustomerNo(string customerNo)
        {
            var count = await OrderHeaderRepository.CancelOrderByCustomerNoAsync(customerNo);
            return count > 0;
        }

        /// <summary>
        /// 分页，根据会员编号获取消费详情
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [HttpGet("QueryOrderDetailByCustomerNoByPage")]
        public async Task<Page<OrderDetail>> QueryOrderDetailByCustomerNoByPage(int pageNumber,int pageSize, string customerNo)
        {
            var page=new Pageable(pageNumber,pageSize);
            var result = await OrderDetailRepository.GetOrderDetailByCustomerNoByPageAsync(page,customerNo);
            return result;
        }

        /// <summary>
        /// 根据会员编号获取消费详情
        /// </summary>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [HttpGet("QueryOrderDetailByCustomerNo")]
        public async Task<List<OrderDetail>> QueryOrderDetailByCustomerNo(string customerNo)
        {
            var result= await OrderDetailRepository.GetOrderDetailByCustomerNoAsync(customerNo);
            return result;
        }

        /// <summary>
        /// 添加订单
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder([FromBody]AddOrderDto dto)
        {
            if (dto?.ProductList==null) return BadRequest("参数不能为空");

            Uow.BeginTransaction();
            try
            {
                var orderHeader = new OrderHeader
                {
                    CreateTime = DateTime.UtcNow,
                    CustomerNo = dto.CustomerNo,
                    State = 1,
                    OrderNo = Guid.NewGuid().ToString("N")
                };

                await OrderHeaderRepository.InsertAsync(orderHeader);

                var orderDetailList = new List<OrderDetail>();
                //总消费金额
                var totalAmount = 0m;
                dto.ProductList.ForEach(it =>
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderHeaderId = orderHeader.Id,
                        ProductNo = it.ProductNo,
                        ProductName = it.ProductName,
                        Quantity = it.Quantity,
                        Price = it.Price
                    };
                    orderDetailList.Add(orderDetail);

                    totalAmount += it.Quantity * it.Price;
                });

                await OrderDetailRepository.BatchInsertAsync(orderDetailList);
                //更新用户消费金额
                var success = await CustomerRepository.UpdateCustomerAmount(dto.CustomerNo, totalAmount);
                
                if (!success)
                {
                    Uow.RollBack();

                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                Uow.RollBack();
            }
           
            Uow.Commit();

            return Ok();
        }
    }
}