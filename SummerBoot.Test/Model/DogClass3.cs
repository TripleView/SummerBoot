namespace SummerBoot.Test.Model
{
    public enum DogKind
    {
        lbld = 1,
        jm = 2
    }
    public class DogClass3
    {
        public string Name { get; set; }
        public int? Age { get; set; }
        public DogKind? Kind { get; set; }
    }

    public struct DogStruct3
    {
        public string Name { get; set; }
        public int? Age { get; set; }
        public DogKind? Kind { get; set; }
    }
}