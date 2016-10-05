# SimpleVpn

This is a simple VPN that allows data to be sent from one computer to another computer over a protected channel. The channel provides mutual authentication and key establishment. It also provides confidentiality and integrity protection using the shared secret value computed at both ends by the key establishment protocol.

The program provides a custom implementation of the mutual authentication, key establishment, as well as the confidentiality and integrity protection, using third-party implementations of cryptographic primitives and modes of operation. We do not use full or partial third-party implementations of protected channels, e.g., SSL, TLS.
