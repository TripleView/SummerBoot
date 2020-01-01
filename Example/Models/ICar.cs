using System.Collections.Generic;

namespace Example.Models
{

    public interface ICar
    {
        Engine Engine { set; get; }

        IEnumerable<WheelA> GetWheelAs();

        int GetWheelNum(int requestNum);

        void Fire();
    }
}