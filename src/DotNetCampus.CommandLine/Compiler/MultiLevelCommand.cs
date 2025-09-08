using System.Collections;
using System.Runtime.CompilerServices;
using DotNetCampus.Cli.Utils.Collections;

namespace DotNetCampus.Cli.Compiler;

/// <summary>
/// 表示一个多级命令（主命令、子命令、三级命令等）。
/// </summary>
#if NET8_0_OR_GREATER
[CollectionBuilder(typeof(MultiLevelCommandBuilder), nameof(MultiLevelCommandBuilder.Create))]
#endif
public readonly record struct MultiLevelCommand : IReadOnlyList<string>
{
    /// <summary>
    /// 获取主命令名称。如果为 <see langword="null"/>，表示这是一个默认命令（没有任何主命令或子命令）。
    /// </summary>
    public string? MainCommand { get; }

    /// <summary>
    /// 获取子命令名称。如果为 <see langword="null"/>，表示没有子命令。
    /// </summary>
    public string? SubCommand { get; }

    /// <summary>
    /// 获取三级命令名称。如果为 <see langword="null"/>，表示没有三级命令。
    /// </summary>
    public string? Level3Command { get; }

    /// <summary>
    /// 获取溢出命令名称数组。如果为 <see langword="null"/>，表示没有溢出命令。
    /// </summary>
    private readonly string[]? _overflowCommands;

    /// <summary>
    /// 初始化一个只有主命令的多级命令，或者一个默认命令（没有任何主命令或子命令）。
    /// </summary>
    /// <param name="mainCommandName">
    /// 主命令名称。如果为 <see langword="null"/>，表示这是一个默认命令（没有任何主命令或子命令）。
    /// </param>
    public MultiLevelCommand(string? mainCommandName)
    {
        MainCommand = mainCommandName;
        SubCommand = null;
        Level3Command = null;
        _overflowCommands = null;
        Count = mainCommandName is null ? 0 : 1;
    }

    /// <summary>
    /// 初始化 <see cref="MultiLevelCommand"/> 类的新实例。
    /// </summary>
    /// <param name="commandNames">根据参数个数依次表示主命令、子命令、三级命令等命令名称的数组。</param>
    public MultiLevelCommand(in ReadOnlySpan<string> commandNames)
    {
        if (commandNames.Length == 0)
        {
            MainCommand = null;
            SubCommand = null;
            Level3Command = null;
            _overflowCommands = null;
            Count = 0;
            return;
        }
        MainCommand = commandNames[0];
        if (commandNames.Length == 1)
        {
            SubCommand = null;
            Level3Command = null;
            _overflowCommands = null;
            Count = 1;
            return;
        }
        SubCommand = commandNames[1];
        if (commandNames.Length == 2)
        {
            Level3Command = null;
            _overflowCommands = null;
            Count = 2;
            return;
        }
        Level3Command = commandNames[2];
        if (commandNames.Length == 3)
        {
            _overflowCommands = null;
            Count = 3;
            return;
        }
        var overflowCount = commandNames.Length - 3;
        _overflowCommands = new string[overflowCount];
        for (var i = 0; i < overflowCount; i++)
        {
            _overflowCommands[i] = commandNames[i + 3];
        }
        Count = 3 + overflowCount;
    }

    /// <summary>
    /// 判断是否为默认命令（没有任何主命令或子命令）。
    /// </summary>
    public bool IsDefault => MainCommand is null;

    /// <inheritdoc />
    public int Count { get; }

    /// <inheritdoc />
    public string this[int index] => index switch
    {
        0 => MainCommand!,
        1 => SubCommand!,
        2 => Level3Command!,
        _ => _overflowCommands![index - 3],
    };

    /// <summary>
    /// 尝试匹配 <paramref name="possibleCommandNames"/> 的前几个项，找到与其匹配的命令（默认命令、主命令、子命令等）。
    /// </summary>
    /// <param name="possibleCommandNames">前几项可能是命令名称的命令行参数集合。</param>
    /// <returns>如果匹配成功，返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    internal bool Match(ReadOnlyListRange<string> possibleCommandNames)
    {
        if (possibleCommandNames.Count < Count)
        {
            return false;
        }
        for (var i = 0; i < Count; i++)
        {
            if (this[i] != possibleCommandNames[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (MainCommand is null)
        {
            return "";
        }
        if (SubCommand is null)
        {
            return MainCommand;
        }
        if (Level3Command is null)
        {
            return $"{MainCommand} {SubCommand}";
        }
        if (_overflowCommands is null || _overflowCommands.Length == 0)
        {
            return $"{MainCommand} {SubCommand} {Level3Command}";
        }
        return $"{MainCommand} {SubCommand} {Level3Command} {string.Join(" ", _overflowCommands)}";
    }

    /// <inheritdoc />
    public bool Equals(MultiLevelCommand other)
    {
        if (Count != other.Count)
        {
            return false;
        }
        if (MainCommand != other.MainCommand)
        {
            return false;
        }
        if (SubCommand != other.SubCommand)
        {
            return false;
        }
        if (Level3Command != other.Level3Command)
        {
            return false;
        }
        if (_overflowCommands is null && other._overflowCommands is null)
        {
            return true;
        }
        if (_overflowCommands is null || other._overflowCommands is null)
        {
            return false;
        }
        if (_overflowCommands.Length != other._overflowCommands.Length)
        {
            return false;
        }
        for (var i = 0; i < _overflowCommands.Length; i++)
        {
            if (_overflowCommands[i] != other._overflowCommands[i]) return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (MainCommand != null ? MainCommand.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (SubCommand != null ? SubCommand.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Level3Command != null ? Level3Command.GetHashCode() : 0);
            if (_overflowCommands is { } overflowCommands)
            {
                for (var i = 0; i < overflowCommands.Length; i++)
                {
                    var command = overflowCommands[i];
                    hashCode = (hashCode * 397) ^ command.GetHashCode();
                }
            }
            return hashCode;
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(in this);

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator() => new Enumerator(in this);

    /// <summary>
    /// 为 <see cref="MultiLevelCommand"/> 提供枚举器。
    /// </summary>
    public struct Enumerator : IEnumerator<string>, IEnumerator
    {
        private readonly MultiLevelCommand _command;
        private int _index;

        internal Enumerator(in MultiLevelCommand command)
        {
            _index = -1;
            _command = command;
        }

        /// <inheritdoc />
        public string Current => _command[_index];

        object IEnumerator.Current => _command[_index];

        /// <inheritdoc />
        public void Dispose()
        {
            _index = _command.Count;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            _index++;
            return _index < _command.Count;
        }

        /// <inheritdoc />
        public void Reset() => _index = -1;
    }
}

/// <summary>
/// 用于创建 <see cref="MultiLevelCommand"/> 的构建器。
/// </summary>
public static class MultiLevelCommandBuilder
{
    /// <summary>
    /// 创建一个多级命令。
    /// </summary>
    /// <param name="commandNames">主命令、子命令、三级命令等命令名称数组。</param>
    /// <returns>多级命令。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MultiLevelCommand Create(ReadOnlySpan<string> commandNames)
    {
        return new MultiLevelCommand(in commandNames);
    }
}
