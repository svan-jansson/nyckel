using ByteSizeLib;
using LiteDB;
using Microsoft.Extensions.Caching.Memory;
using Nyckel.Core.ValueObjects;
using OneOf.Monads;
using System.Collections;
using System.Reflection;

namespace Nyckel.Core;

public class LiteDb : INyckel, IDisposable
{
    private readonly ILiteDatabase _liteDb;
    private readonly string _dataFile;
    public LiteDb()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var dataPath = Path.Combine(path, "data");
        Directory.CreateDirectory(dataPath);
        _dataFile = Path.Combine(dataPath, $"{nameof(LiteDb)}.db");
        _liteDb = new LiteDatabase(_dataFile);

        Records().EnsureIndex(x => x.Key);
    }

    private class Record
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
    }

    private ILiteCollection<Record> Records() => _liteDb.GetCollection<Record>();

    public Option<Value> Get(Key key) =>
        Try.Catching(() =>
        {
            var internalKey = key.Get();
            var record = Records().FindOne(r => r.Key == internalKey);
            if (record == null)
            {
                throw new KeyNotFoundException($"Key not found: {key}");
            }

            return Value.Create(record.Value);
        }).ToOption();

    public Option<Value> Set(Key key, Value value)
        => Try.Catching(() =>
        {
            var internalKey = key.Get();
            var internalValue = value.Get();
            var record = new Record { Key = internalKey, Value = internalValue };
            var records = Records();

            var existing = records.FindOne(r => r.Key == internalKey);
            if (existing == null)
            {
                records.Insert(record);
            }
            else
            {
                records.Update(existing.Id, record);
            }

            return Value.Create(internalValue);
        }).ToOption();

    public Option<Value> Delete(Key key)
        => Try.Catching(() =>
        {
            var internalKey = key.Get();

            var record = Records().FindOne(r => r.Key == internalKey);

            if (record == null)
            {
                throw new KeyNotFoundException();
            }

            Records().Delete(record.Id);

            return Value.Create(record.Value);
        })
        .ToOption();

    public INyckel Map(Func<Key, Value, bool> mapContinue)
        => Try.Catching<INyckel>(() =>
        {
            var records = Records().FindAll();

            foreach (var record in records)
            {
                if (!mapContinue(Key.Create(record.Key), Value.Create(record.Value)))
                {
                    break;
                }
            }

            return this;
        }).Unwrap(_ => this);

    public int Count() => Records().Count();

    public Dictionary<string, string> Info() => new()
    {
        { "Database Size", ByteSize.FromBytes(new FileInfo(_dataFile).Length).ToString() },
        { "Last Modified", new FileInfo(_dataFile).LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm") }
    };

    public void Dispose()
    {
        _liteDb.Dispose();
    }
}
