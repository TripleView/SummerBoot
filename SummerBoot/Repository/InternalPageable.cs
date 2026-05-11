namespace SummerBoot.Repository;

internal class InternalPageable
{
    public InternalPageable(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }
    public int Skip { get; set; }

    public int Take { get; set; }
}