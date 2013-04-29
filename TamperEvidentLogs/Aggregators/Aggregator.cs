
namespace TamperEvidentLogs.Aggregators
{
    public interface Aggregator
    {
        byte[] AggregateChildren(byte[] left, byte[] right);

        byte[] HashLeaf(byte[] data);

        string Name { get; }
    }
}
