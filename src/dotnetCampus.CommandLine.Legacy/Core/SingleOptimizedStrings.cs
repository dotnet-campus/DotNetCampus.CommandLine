namespace dotnetCampus.Cli.Core
{
    /// <summary>
    /// 为 0 个和 1 个值特殊优化性能的字符串列表。
    /// </summary>
    internal class SingleOptimizedStrings : SingleOptimizedList<string>
    {
        public SingleOptimizedStrings(string firstValue) : base(firstValue)
        {
        }
    }
}