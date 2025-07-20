# GKCommunicator

This project is a distributed, decentralized, serverless, peer-to-peer chat client for GEDKeeper.

## Roadmap

### Implemented

- [x] Kademlia (Mainline) DHT
- [x] STUN detection of public endpoint
- [x] UPnP port's mapping
- [x] UDP hole-punching
- [x] SQLite storage of DHT nodes, peer profiles and messages
- [x] Keys generation, simple authentication and RSA traffic encryption
- [x] Chat with presence statuses, message history and delivery statuses
- [x] Invite peers to join via emails
- [x] Simple blockchain

### Planned

- [ ] Local transactions pool (blockchain)
- [ ] Distribution of user profiles (blockchain)
- [ ] Distribution of data change transactions (blockchain)
- [ ] Consensus strategy for combining transactions of different nodes (PoS or PoI or PoA)
- [ ] A simple bulletin board of investigations and search of kins (only in the implementation of the plugin)
- [ ] TCP
