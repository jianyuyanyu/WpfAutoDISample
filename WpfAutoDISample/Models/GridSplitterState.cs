using LiteDB;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace WpfAutoDISample.Models;

internal sealed class GridSplitterState
{
    [BsonId]
    public required ObjectId Id { get; set; } // LiteDB需要一个Id字段作为主键

    public double Value { get; init; } // 保存的位置值，可以是宽度或高度
}