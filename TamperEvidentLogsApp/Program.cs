using System;
using TamperEvidentLogs;
using TamperEvidentLogs.Aggregators;

namespace TamperEvidentLogsApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HashTree hashTree = new HashTree(new SHA256Aggregator());
            for (int i = 0; i < 10; ++i)
            {
                hashTree.Append(Console.ReadLine());
            }

            for (int i = 0; i < 10; ++i)
            {
                Console.WriteLine(hashTree.GenerateMembershipProof(i));
            }

            Console.ReadLine();
        }
    }
}
