using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace TamperEvidentLogs
{
    [DataContract]
    public class ProofNode
    {
        public ProofNode()
        {
        }

        public ProofNode(int index, string s)
        {
            this.Index = index;
            this.Hash = s;
        }

        [DataMember(Name = "Index")]
        public int Index { get; set; }

        [DataMember(Name = "Hash")]
        public string Hash { get; set; }
    }

    [DataContract]
    public class MembershipProof
    {
        private byte[] _commitment;
        private string _commitmentValue;
        private byte[] _memberData;
        private string _memberDataValue;
        private List<Tuple<int, byte[]>> _prunedTree;
        private List<ProofNode> _prunedTreeValue;

        [IgnoreDataMember]
        public byte[] Commitment
        {
            get
            {
                return _commitment;
            }
            set
            {
                this._commitment = value;
                this._commitmentValue = Encoding.EncodeBytes(this._commitment);
            }
        }

        [DataMember(Name = "Commitment")]
        public string CommitmentValue
        {
            get
            {
                return _commitmentValue;
            }
            set
            {
                this._commitmentValue = value;
                this._commitment = Encoding.DecodeString(this._commitmentValue);
            }
        }

        [IgnoreDataMember]
        public byte[] MemberData
        {
            get
            {
                return this._memberData;
            }
            set
            {
                this._memberData = value;
                this._memberDataValue = Encoding.EncodeBytes(this._memberData);
            }
        }

        [DataMember(Name = "MemberData")]
        public string MemberDataValue
        {
            get
            {
                return this._memberDataValue;
            }
            set
            {
                this._memberDataValue = value;
                this._memberData = Encoding.DecodeString(this._memberDataValue);
            }
        }

        [DataMember(Name = "MemberIndex")]
        public int MemberIndex { get; set; }

        [DataMember(Name = "Aggregator")]
        public string AggregatorName { get; set; }

        [DataMember(Name = "Encoding")]
        public string EncodingName
        {
            get
            {
                return Encoding.Name;
            }
            set
            {
                // Do Nothing, only defined because of DataMemberContract
            }
        }

        [IgnoreDataMember]
        public List<Tuple<int, byte[]>> PrunedTree
        {
            get
            {
                return this._prunedTree;
            }
            set
            {
                this._prunedTree = value;
                this._prunedTreeValue = this._prunedTree.Select(
                    x => new ProofNode(x.Item1, Encoding.EncodeBytes(x.Item2))
                    ).ToList();
            }
        }

        [DataMember(Name = "PrunedTree")]
        public List<ProofNode> PrunedTreeValue
        {
            get
            {
                return this._prunedTreeValue;
            }
            set
            {
                this._prunedTreeValue = value;
                this._prunedTree = this._prunedTreeValue.Select(
                    x => new Tuple<int, byte[]>(x.Index, Encoding.DecodeString(x.Hash))
                    ).ToList();
            }
        }

        public override string ToString()
        {
            DataContractJsonSerializer serializer =
                new DataContractJsonSerializer(typeof(MembershipProof));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, this);

                ms.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
