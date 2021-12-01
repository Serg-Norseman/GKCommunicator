# GKCommunicator

This is a sub-project of GEDKeeper (application and plugin). The distributed, decentralized, serverless, peer-to-peer (P2P) chat client. 

## Roadmap

### Implemented

- [x] Kademlia (Mainline) DHT
- [x] STUN detection of public endpoint and NAT's type
- [x] UPnP port's mapping (not used yet)
- [x] IPv4 and IPv6 (untested)
- [x] SQLite database support
- [x] Simple chat with formatting of history
- [x] Simple user’s profile
- [x] UDP hole-punching
- [x] Working DHT peer search, connection and simple chat
- [x] Storage of DHT nodes, user and peers profiles
- [x] Keys generation, simple user identification and authentication
- [x] RSA traffic encryption

### Planned

- [ ] Stabilize the chat and discovery of peers
- [ ] A simple bulletin board of investigations and search of kins (only in the implementation of the plugin)

### Distant future

- [ ] TCP
- [ ] Mutual identity and signing of user profiles (trusted and unchecked network members)
- [ ] "Web of Trust" elements with PGP certificates (https://en.wikipedia.org/wiki/Web_of_trust)
- [ ] "Friend-to-friend" network elements (https://en.wikipedia.org/wiki/Friend-to-friend)
