using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using DotNetCampus.Cli.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCampus.Cli.Tests.ParsingStyles;

[TestClass]
public class PropertyTypeTests
{
    [TestMethod]
    public void SupportManyTypes()
    {
        // Arrange
        // "--boolean-property", "true",
        // "--immutable-array-property", "a,b,c",
        string[] args =
        [
            "--boolean-property", "true",
            "--byte-property", "1",
            "--sbyte-property", "1",
            "--decimal-property", "1.1",
            "--double-property", "1.1",
            "--single-property", "1.1",
            "--int32-property", "1",
            "--uint32-property", "1",
            "--int64-property", "1",
            "--uint64-property", "1",
            "--int16-property", "1",
            "--uint16-property", "1",
            "--char-property", "a",
            "--string-property", "value",
            "--array-property", "a,b,c",
            "--collection-property", "a,b,c",
            "--read-only-collection-property", "a,b,c",
            "--hash-set-property", "a,b,c",
            "--immutable-array-property", "a,b,c",
            "--immutable-list-property", "a,b,c",
            "--immutable-hash-set-property", "a,b,c",
            "--sorted-set-property", "a,b,c",
            "--immutable-sorted-set-property", "a,b,c",
            "--ienumerable-property", "a,b,c",
            "--icollection-property", "a,b,c",
            "--ilist-property", "a,b,c",
            "--iread-only-collection-property", "a,b,c",
            "--iread-only-list-property", "a,b,c",
            "--iset-property", "a,b,c",
            "--iimmutable-list-property", "a,b,c",
            "--iimmutable-set-property", "a,b,c",
            "--key-value-pair-property", "key:value",
            "--dictionary-property", "key:value,key2:value2",
            "--immutable-dictionary-property", "key:value,key2:value2",
            "--sorted-dictionary-property", "key:value,key2:value2",
            "--sorted-list-property", "key:value,key2:value2",
            "--immutable-sorted-dictionary-property", "key:value,key2:value2",
            "--idictionary-property", "key:value,key2:value2",
            "--iread-only-dictionary-property", "key:value,key2:value2",
        ];
        var commandLine = CommandLine.Parse(args, CommandLineParsingOptions.DotNet);

        // Act
        var options = commandLine.As<TestOptions>();

        // Assert
        Assert.IsNotNull(options);
        Assert.AreEqual(true, options.BooleanProperty);
        Assert.AreEqual((byte)1, options.ByteProperty);
        Assert.AreEqual((sbyte)1, options.SByteProperty);
        Assert.AreEqual(1.1m, options.DecimalProperty);
        Assert.AreEqual(1.1, options.DoubleProperty);
        Assert.AreEqual(1.1f, options.SingleProperty);
        Assert.AreEqual(1, options.Int32Property);
        Assert.AreEqual((uint)1, options.UInt32Property);
        Assert.AreEqual((long)1, options.Int64Property);
        Assert.AreEqual((ulong)1, options.UInt64Property);
        Assert.AreEqual((short)1, options.Int16Property);
        Assert.AreEqual((ushort)1, options.UInt16Property);
        Assert.AreEqual('a', options.CharProperty);
        Assert.AreEqual("value", options.StringProperty);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, options.ArrayProperty);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.CollectionProperty!);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.ReadOnlyCollectionProperty!);
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, options.HashSetProperty!.ToArray());
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, options.ImmutableArrayProperty.ToArray());
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.ImmutableListProperty!);
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, options.ImmutableHashSetProperty!.ToArray());
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, options.SortedSetProperty!.ToArray());
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, options.ImmutableSortedSetProperty!.ToArray());
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.IEnumerableProperty!);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.ICollectionProperty!);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.IListProperty!);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.IReadOnlyCollectionProperty!);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.IReadOnlyListProperty!);
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, options.ISetProperty!.ToArray());
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (ICollection)options.IImmutableListProperty!);
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, options.IImmutableSetProperty!.ToArray());
        Assert.AreEqual(new KeyValuePair<string, string>("key", "value"), options.KeyValuePairProperty);
        CollectionAssert.AreEquivalent(new Dictionary<string, string>
        {
            ["key"] = "value",
            ["key2"] = "value2",
        }, (ICollection)options.DictionaryProperty!);
    }

    public record TestOptions
    {
        [Option]
        public bool? BooleanProperty { get; set; }

        [Option]
        public byte? ByteProperty { get; set; }

        [Option]
        public sbyte? SByteProperty { get; set; }

        [Option]
        public decimal? DecimalProperty { get; set; }

        [Option]
        public double? DoubleProperty { get; set; }

        [Option]
        public float? SingleProperty { get; set; }

        [Option]
        public int? Int32Property { get; set; }

        [Option]
        public uint? UInt32Property { get; set; }

        [Option]
        public long? Int64Property { get; set; }

        [Option]
        public ulong? UInt64Property { get; set; }

        [Option]
        public short? Int16Property { get; set; }

        [Option]
        public ushort? UInt16Property { get; set; }

        [Option]
        public char? CharProperty { get; set; }

        [Option]
        public string? StringProperty { get; set; }

        [Option]
        public string[]? ArrayProperty { get; set; }

        [Option]
        public Collection<string>? CollectionProperty { get; set; }

        [Option]
        public ReadOnlyCollection<string>? ReadOnlyCollectionProperty { get; set; }

        [Option]
        public HashSet<string>? HashSetProperty { get; set; }

        [Option]
        public ImmutableArray<string> ImmutableArrayProperty { get; set; }

        [Option]
        public ImmutableList<string>? ImmutableListProperty { get; set; }

        [Option]
        public ImmutableHashSet<string>? ImmutableHashSetProperty { get; set; }

        [Option]
        public SortedSet<string>? SortedSetProperty { get; set; }

        [Option]
        public ImmutableSortedSet<string>? ImmutableSortedSetProperty { get; set; }

        [Option]
        public IEnumerable<string>? IEnumerableProperty { get; set; }

        [Option]
        public ICollection<string>? ICollectionProperty { get; set; }

        [Option]
        public IList<string>? IListProperty { get; set; }

        [Option]
        public IReadOnlyCollection<string>? IReadOnlyCollectionProperty { get; set; }

        [Option]
        public IReadOnlyList<string>? IReadOnlyListProperty { get; set; }

        [Option]
        public ISet<string>? ISetProperty { get; set; }

        [Option]
        public IImmutableList<string>? IImmutableListProperty { get; set; }

        [Option]
        public IImmutableSet<string>? IImmutableSetProperty { get; set; }

        [Option]
        public KeyValuePair<string, string>? KeyValuePairProperty { get; set; }

        [Option]
        public Dictionary<string, string>? DictionaryProperty { get; set; }

        [Option]
        public ImmutableDictionary<string, string>? ImmutableDictionaryProperty { get; set; }

        [Option]
        public SortedDictionary<string, string>? SortedDictionaryProperty { get; set; }

        [Option]
        public SortedList<string, string>? SortedListProperty { get; set; }

        [Option]
        public ImmutableSortedDictionary<string, string>? ImmutableSortedDictionaryProperty { get; set; }

        [Option]
        public IDictionary<string, string>? IDictionaryProperty { get; set; }

        [Option]
        public IReadOnlyDictionary<string, string>? IReadOnlyDictionaryProperty { get; set; }
    }
}
