using System;

namespace Example.Models
{
    public class OuterClass
    {
        static OuterClass()
        {
            Console.WriteLine("爸爸类静态初始化");
        }
        
        private int happinessValue = 1;

        public OuterClass()
        {
            Console.WriteLine("爸爸初始化");
        }

        public void DoWork()
        {
            Console.WriteLine("爸爸在干活");
            new InnerClass(this).Play();
            Console.WriteLine("爸爸很开心啊,"+ happinessValue);
        }

        public class InnerClass
        {
            private readonly OuterClass outClass;

            public InnerClass(OuterClass outClass)
            {
                this.outClass = outClass;
            }

            public void Play()
            {
                Console.WriteLine("爸爸辛苦了");
                outClass.happinessValue += 5;
            }
        }
    }
}