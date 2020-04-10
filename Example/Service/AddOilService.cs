using Example.Models;
using SummerBoot.Core;
using SummerBoot.Repository;

namespace Example.Service
{
    public class AddOilService : IAddOilService
    {
        [Autowired]
        public IRepository<CashBalance> CashBalanceRepository { set; get; }

        [Autowired]
        public IRepository<OilQuantity> OilQuantityRepository { set; get; }

        [Transactional]
        public void AddOil()
        {
            //初始化金额
            var cashBalance = CashBalanceRepository.Insert(new CashBalance() { Balance = 100 });
            //初始化油量
            var oilQuantity = OilQuantityRepository.Insert(new OilQuantity() { Quantity = 5 });

            cashBalance.Balance -= 95;
            oilQuantity.Quantity += 50;

            //CashBalanceRepository.Update(cashBalance);
            ////throw new Exception("throw err");
            //OilQuantityRepository.Update(oilQuantity);
        }
    }
}