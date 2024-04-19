namespace dotnetCampus.Cli.StateMachine
{
    /// <summary>
    /// 表示谓词匹配结果。
    /// </summary>
    internal enum VerbMatchingResult
    {
        /// <summary>
        /// 谓词没有匹配上。
        /// </summary>
        NotMatch,

        /// <summary>
        /// 谓词匹配成功（包括无谓词的匹配）。
        /// </summary>
        Matched,

        /// <summary>
        /// 本类型无谓词，如果所有谓词均无法匹配，则可作为回退方案匹配。
        /// </summary>
        FallbackMatched,
    }
}
