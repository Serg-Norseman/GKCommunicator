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

### Planned

- [ ] A simple bulletin board of investigations and search of kins (only in the implementation of the plugin)
- [ ] TCP


# GKLocations

This project is a distributed, decentralized, serverless, peer-to-peer knowledge base of the history of locations 
(cities, villages and other settlements). Will be implemented as a plugin for GKCommunicator.

## Roadmap

- [x] SQLite storage of location, names and relations with history
- [x] GEDCOM format of dates of locations history
- [x] Pool of local data modification transaction
- [x] Simple primitive blockchain without network features

- [ ] Target data model
- [ ] Local transactions pool
- [ ] Simple data editing user interface
- [ ] Distribution of user profiles in a peer-to-peer network (blockchain-based)
- [ ] Distribution of data change transactions in a peer-to-peer network (blockchain-based)
- [ ] Consensus strategy for combining transactions of different nodes
- [ ] Integration with GKCommunicator (peer-to-peer communication)
