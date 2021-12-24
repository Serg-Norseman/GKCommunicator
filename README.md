# GKCommunicator

This is a sub-project of GEDKeeper (application and plugin). The distributed, decentralized, serverless, peer-to-peer (P2P) chat client. 

## Roadmap

### Implemented

- [x] Kademlia (Mainline) DHT
- [x] STUN detection of public endpoint and NAT's type
- [x] UPnP port's mapping (not used yet)
- [x] IPv4 and IPv6 (untested)
- [x] UDP hole-punching
- [x] SQLite storage of DHT nodes, peer profiles and messages
- [x] Keys generation, simple authentication and RSA traffic encryption
- [x] Stable DHT peer discovery
- [x] Chat with presence statuses, message history and delivery statuses

### Planned

- [ ] A simple bulletin board of investigations and search of kins (only in the implementation of the plugin)

### Distant future

- [ ] TCP
- [ ] Mutual identity and signing of user profiles (trusted and unchecked network members)
- [ ] "Web of Trust" elements with PGP certificates (https://en.wikipedia.org/wiki/Web_of_trust)
- [ ] "Friend-to-friend" network elements (https://en.wikipedia.org/wiki/Friend-to-friend)
