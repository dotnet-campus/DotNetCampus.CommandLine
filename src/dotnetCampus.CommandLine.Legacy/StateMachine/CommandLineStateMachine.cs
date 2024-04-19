#pragma warning disable CA1303 // 请不要将文本作为本地化参数传递

using System;
using System.Collections.Generic;

using dotnetCampus.Cli.Compatibility;
using dotnetCampus.Cli.Core;

namespace dotnetCampus.Cli.StateMachine
{
    /// <summary>
    /// 使用状态机的方式解析命令行参数。
    /// </summary>
    internal class CommandLineStateMachine
    {
        /// <summary>
        /// 当构造此状态机时，储存下来的命令行参数列表。状态机在执行时，将在此参数列表中移动。
        /// </summary>
        private readonly IReadOnlyList<string> _commandLineArgs;

        /// <summary>
        /// 当构造此状态机时，储存选项型参数的前缀。
        /// </summary>
        private readonly char _optionPrefix;

        /// <summary>
        /// 状态机执行时，如果有新的选项和相关的值生成完成，则调用此委托。此委托仅在状态机开始执行时才会初始化。
        /// </summary>
        private Action<string, SingleOptimizedStrings?>? _optionCollectedAction;

        /// <summary>
        /// 状态机执行时，如果碰到了 -- 选项结束符，那么随后的所有参数都视为值。
        /// </summary>
        private bool _isOptionSectionEnded;

        /// <summary>
        /// 状态机执行时，如果有新的选项生成，则设置到此字段中。此字段仅在状态机开始执行时才会初始化。
        /// </summary>
        private string? _currentOption;

        /// <summary>
        /// 状态机执行时，如果有新的值生成，则添加到此集合中。此集合仅在状态机开始执行时才会初始化。
        /// </summary>
        private SingleOptimizedStrings? _currentValues;

        /// <summary>
        /// 创建此命令行状态机的新实例。
        /// </summary>
        /// <param name="args">命令行参数列表。</param>
        /// <param name="optionPrefix"></param>
        public CommandLineStateMachine(IReadOnlyList<string> args, char optionPrefix)
        {
            _commandLineArgs = args ?? throw new ArgumentNullException(nameof(args));
            if (optionPrefix is '-' || optionPrefix is '/')
            {
                _optionPrefix = optionPrefix;
            }
            else
            {
                throw new NotSupportedException("仅支持 - 或者 / 作为选项的前缀。");
            }
        }

        /// <summary>
        /// 开始执行状态机，以便得到命令行解析后的字典集合。
        /// 执行此状态机仅会原封不动地将参数进行选项和值的分组，不会修改任何参数字符。
        /// </summary>
        /// <returns>包含命令行解析的字典集合。</returns>
        public ListGroup<SingleOptimizedStrings> Run()
        {
            // 准备初始参数。
            var parsedArgs = new ListGroup<SingleOptimizedStrings>();

            // 初始化状态机运行的所有状态。
            _currentOption = null;
            _currentValues = null;
            _optionCollectedAction = OnOptionCollected;

            // 执行状态机。
            foreach (var arg in _commandLineArgs)
            {
                if (_isOptionSectionEnded)
                {
                    // 如果已使用 -- 结束选项，则后续全视为值。
                    AppendValue(arg);
                }
                else if (string.IsNullOrEmpty(arg))
                {
                    // 如果此参数是空字符串，则将值加入。
                    AppendValue(arg);
                }
                else if (arg[0] == _optionPrefix)
                {
                    if (_optionPrefix == '-' && arg.Length == 2 && arg[1] == _optionPrefix)
                    {
                        // 如果此参数已被 -- 分隔，则标记选项结束。
                        _isOptionSectionEnded = true;
                        Commit();
                        SetOption(null);
                    }
                    else
                    {
                        // 如果此参数是选项，则作为选项。
                        Commit();
                        SetOption(arg);
                    }
                }
                else
                {
                    // 如果此参数不是选项，则作为值。
                    AppendValue(arg);
                }
            }

            Commit();

            // 清除此次状态机运行的所有状态。
            _currentOption = null;
            _currentValues = null;
            _optionCollectedAction = null;
            _isOptionSectionEnded = false;

            // 返回结果。
            return parsedArgs;

            // 当状态机运行触发选项生成时，将此选项更新到集合中。
            void OnOptionCollected(string option, SingleOptimizedStrings? values)
            {
                if (values != null && values.Count > 0)
                {
                    parsedArgs.Add(option ?? "", values);
                }
                else if (!string.IsNullOrEmpty(option))
                {
                    parsedArgs.Add(option, values);
                }
            }
        }

        private void SetOption(string? option)
        {
            if (option is null)
            {
                _currentOption = null;
                _currentValues = null;
                return;
            }

            var valueSplitIndex = option.IndexOf(':', StringComparison.Ordinal);
            if (valueSplitIndex < 0 || valueSplitIndex >= option.Length - 1)
            {
                // -k value
                _currentOption = option;
                _currentValues = null;
            }
            else
            {
                // -k:value
                _currentOption = option.Substring(0, valueSplitIndex);
                _currentValues = new SingleOptimizedStrings(option.Substring(valueSplitIndex + 1, option.Length - valueSplitIndex - 1));
            }
        }

        private void AppendValue(string value)
        {
            if (_currentValues == null)
            {
                _currentValues = new SingleOptimizedStrings(value);
            }
            else
            {
                _currentValues.Add(value);
            }
        }

        private void Commit()
        {
            _optionCollectedAction!(_currentOption!, _currentValues);
        }
    }
}