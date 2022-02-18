using System;
using System.Collections.Generic;
using System.Linq;
using GKLocations.Common;

namespace GKLocations.Blockchain
{
    /// <summary>
    /// Blockchain.
    /// </summary>
    public class Chain
    {
        private IAlgorithm fAlgorithm = Helpers.GetDefaultAlgorithm();
        private IDataProvider fDataProvider;
        private INetwork fNetwork;
        private List<Block> fBlockChain = new List<Block>();
        private List<User> fUsers = new List<User>();
        private List<Data> fData = new List<Data>();


        public IEnumerable<Block> BlockChain
        {
            get {
                return fBlockChain;
            }
        }

        public Block PreviousBlock
        {
            get {
                return fBlockChain.Last();
            }
        }

        public IEnumerable<Data> Content
        {
            get {
                return fData;
            }
        }

        public IEnumerable<User> Users
        {
            get {
                return fUsers;
            }
        }

        public int Length
        {
            get {
                return fBlockChain.Count;
            }
        }


        /// <summary>
        /// Create a new block chain instance.
        /// </summary>
        public Chain(INetwork network, IDataProvider dataProvider)
        {
            fNetwork = network;
            fDataProvider = dataProvider;
        }

        /// <summary>
        /// Get data from the local chain.
        /// </summary>
        private void LoadDataFromLocalChain(Chain localChain)
        {
            if (localChain == null) {
                throw new ArgumentNullException(nameof(localChain));
            }

            foreach (var block in localChain.fBlockChain) {
                fBlockChain.Add(block);
                AddDataInList(block);
                SendBlockToGlobalChain(block);
            }
        }

        /// <summary>
        /// Replace the local data chain with blocks from the global chain.
        /// </summary>
        private void ReplaceLocalChainFromGlobalChain(Chain globalChain)
        {
            if (globalChain == null) {
                throw new ArgumentNullException(nameof(globalChain));
            }

            // TODO: Develop a merge algorithm
            fDataProvider.ClearBlocks();

            foreach (var block in globalChain.fBlockChain) {
                AddBlock(block);
            }
        }

        /// <summary>
        /// Create a block chain from a data provider block list.
        /// </summary>
        private Chain(List<SerializableBlock> blocks)
        {
            if (blocks == null) {
                throw new ArgumentNullException(nameof(blocks));
            }

            foreach (var block in blocks) {
                var b = new Block(block);
                fBlockChain.Add(b);

                AddDataInList(b);
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        /// <summary>
        /// Creation of a chain of blocks from blocks of data.
        /// </summary>
        private Chain(List<Block> blocks)
        {
            if (blocks == null) {
                throw new ArgumentNullException(nameof(blocks));
            }

            foreach (var block in blocks) {
                fBlockChain.Add(block);

                AddDataInList(block);
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        /// <summary>
        /// Get the current user of the system.
        /// </summary>
        public User GetCurrentUser()
        {
            User result = fNetwork.GetCurrentUser();

            // FIXME
            if (result == null) {
                result = new User("user", "password", UserRole.Reader);
            }

            return result;
        }

        /// <summary>
        /// Create a new empty block chain.
        /// </summary>
        private void CreateNewBlockChain()
        {
            fDataProvider.ClearBlocks();
            fBlockChain = new List<Block>();

            var genesisBlock = Block.CreateGenesisBlock(GetCurrentUser(), fAlgorithm);
            AddBlock(genesisBlock);
        }

        /// <summary>
        /// Check the correctness of the block chain.
        /// </summary>
        public bool CheckCorrect()
        {
            foreach (var block in fBlockChain) {
                if (!block.IsCorrect(fAlgorithm)) {
                    return false;
                }
            }

            return true;
        }

        public void ReceivedGlobalBlockchain(string jsonResponse)
        {
            List<Block> blocks;
            if (string.IsNullOrEmpty(jsonResponse)) {
                blocks = null;
            } else {
                blocks = DeserializeCollectionBlocks(jsonResponse);
            }

            Chain globalChain = null;
            if (blocks != null && blocks.Count > 0) {
                globalChain = new Chain(blocks);
            }

            if (globalChain == null)
                return;

            var localChain = GetLocalChain();

            if (globalChain != null && localChain != null) {
                if (globalChain.Length > localChain.Length) {
                    ReplaceLocalChainFromGlobalChain(globalChain);
                } else {
                    LoadDataFromLocalChain(localChain);
                }
            } else if (globalChain != null) {
                ReplaceLocalChainFromGlobalChain(globalChain);
            } else if (localChain != null) {
                LoadDataFromLocalChain(localChain);
            } else {
                CreateNewBlockChain();
            }

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "Error creating block chain. The chain is incorrect.");
            }
        }

        /// <summary>
        /// Getting a chain of blocks from local storage.
        /// </summary>
        private Chain GetLocalChain()
        {
            var blocks = fDataProvider.GetBlocks();
            if (blocks.Count > 0) {
                var chain = new Chain(blocks);
                return chain;
            }

            return null;
        }

        /// <summary>
        ///Add data to the block chain.
        /// </summary>
        public Block AddContent(string text)
        {
            if (string.IsNullOrEmpty(text)) {
                throw new ArgumentNullException(nameof(text));
            }

            var data = new Data(text, DataType.Content);
            var block = new Block(PreviousBlock, data, GetCurrentUser(), fAlgorithm);

            AddBlock(block);

            return block;
        }

        /// <summary>
        /// Add user data to the chain.
        /// </summary>
        public Block AddUser(string login, string password, UserRole role = UserRole.Reader)
        {
            if (string.IsNullOrEmpty(login)) {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrEmpty(password)) {
                throw new ArgumentNullException(nameof(password));
            }

            if (Users.Any(b => b.Login == login)) {
                return null;
            }

            var user = new User(login, password, role);
            var data = user.GetData();
            var block = new Block(PreviousBlock, data, GetCurrentUser());
            AddBlock(block);
            return block;
        }

        /// <summary>
        /// Adding global chain address information.
        /// </summary>
        public Block AddHost(string ip)
        {
            if (string.IsNullOrEmpty(ip)) {
                throw new ArgumentNullException(nameof(ip));
            }

            var data = new Data(ip, DataType.Node);
            var block = new Block(PreviousBlock, data, GetCurrentUser(), fAlgorithm);
            AddBlock(block);

            return block;
        }

        /// <summary>
        /// Log in as a network user.
        /// </summary>
        public User LoginUser(string login, string password)
        {
            if (string.IsNullOrEmpty(login)) {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrEmpty(password)) {
                throw new ArgumentNullException(nameof(password));
            }

            var user = Users.SingleOrDefault(b => b.Login == login);
            if (user == null) {
                return null;
            }

            var passwordHash = password.GetHash();
            if (user.PasswordHash != passwordHash) {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Add block.
        /// </summary>
        private void AddBlock(Block block)
        {
            if (block == null) {
                throw new ArgumentNullException(nameof(block));
            }

            if (!block.IsCorrect()) {
                throw new MethodArgumentException(nameof(block), "The block is invalid.");
            }

            // Do not add an existing block
            if (fBlockChain.Any(b => b.Hash == block.Hash)) {
                return;
            }

            fBlockChain.Add(block);
            fDataProvider.AddBlock(new SerializableBlock(block));
            AddDataInList(block);
            SendBlockToGlobalChain(block);

            if (!CheckCorrect()) {
                throw new MethodResultException(nameof(Chain), "The correctness was violated after adding the block.");
            }
        }

        /// <summary>
        /// Adding data from blocks to quick access lists.
        /// </summary>
        private void AddDataInList(Block block)
        {
            switch (block.Data.Type) {
                case DataType.Content:
                    fData.Add(block.Data);
                    foreach (var host in fNetwork.Hosts) {
                        SendBlockToHosts(host, "AddData", block.Data.Content);
                    }
                    break;

                case DataType.User:
                    var user = new User(block);
                    fUsers.Add(user);
                    foreach (var host in fNetwork.Hosts) {
                        SendBlockToHosts(host, "AddUser", $"{user.Login}&{user.PasswordHash}&{user.Role}");
                    }
                    break;

                case DataType.Node:
                    fNetwork.Hosts.Add(block.Data.Content);
                    foreach (var host in fNetwork.Hosts) {
                        SendBlockToHosts(host, "AddHost", block.Data.Content);
                    }
                    break;

                default:
                    throw new MethodArgumentException(nameof(block), "Unknown block.");
            }
        }

        /// <summary>
        /// Adding data from blocks to quick access lists.
        /// </summary>
        private void SendBlockToGlobalChain(Block block)
        {
            switch (block.Data.Type) {
                case DataType.Content:
                    foreach (var host in fNetwork.Hosts) {
                        SendBlockToHosts(host, "AddData", block.Data.Content);
                    }
                    break;

                case DataType.User:
                    var user = new User(block);
                    foreach (var host in fNetwork.Hosts) {
                        // FIXME
                        SendBlockToHosts(host, "AddUser", $"{user.Login}&{user.PasswordHash}&{user.Role}");
                    }
                    break;

                case DataType.Node:
                    fNetwork.Hosts.Add(block.Data.Content);
                    foreach (var host in fNetwork.Hosts) {
                        SendBlockToHosts(host, "AddHost", block.Data.Content);
                    }
                    break;

                default:
                    throw new MethodArgumentException(nameof(block), "Unknown block.");
            }
        }

        /// <summary>
        /// Formation of a list of blocks based on the received json response from the host.
        /// </summary>
        private static List<Block> DeserializeCollectionBlocks(string json)
        {
            var requestResult = JsonHelper.DeserializeObject<SerializableChain>(json);

            var result = new List<Block>();
            foreach (var block in requestResult.Chain) {
                result.Add(new Block(block));
            }
            return result;
        }

        /// <summary>
        /// Request to the host to add a block of data.
        /// </summary>
        private bool SendBlockToHosts(string ip, string method, string data)
        {
            return fNetwork.SendBlockToHost(ip, method, data);
        }
    }
}
