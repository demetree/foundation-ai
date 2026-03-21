// ============================================================================
//
// IpBlockList.cs — IP allow/deny list with CIDR range support.
//
// Evaluates whether an IP address falls within configured CIDR ranges
// for allow or deny list enforcement.
//
// AI-Developed | Gemini
//
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Foundation.Networking.Skynet.Firewall
{
    /// <summary>
    ///
    /// IP address matcher with CIDR range support.
    ///
    /// Supports individual IPs ("192.168.1.1") and CIDR ranges
    /// ("192.168.1.0/24", "10.0.0.0/8").
    ///
    /// </summary>
    public class IpBlockList
    {
        private readonly List<IpRange> _ranges;


        public IpBlockList()
        {
            _ranges = new List<IpRange>();
        }


        /// <summary>
        /// Creates an IpBlockList from a list of IP addresses and/or CIDR ranges.
        /// </summary>
        public IpBlockList(IEnumerable<string> ipRanges)
        {
            _ranges = new List<IpRange>();

            foreach (string range in ipRanges)
            {
                AddRange(range);
            }
        }


        /// <summary>
        /// Number of configured ranges.
        /// </summary>
        public int Count => _ranges.Count;


        /// <summary>
        /// Adds an IP address or CIDR range to the list.
        /// </summary>
        public void AddRange(string ipOrCidr)
        {
            if (string.IsNullOrWhiteSpace(ipOrCidr))
            {
                return;
            }

            string trimmed = ipOrCidr.Trim();

            if (trimmed.Contains("/"))
            {
                //
                // CIDR notation: "192.168.1.0/24"
                //
                string[] parts = trimmed.Split('/');

                if (parts.Length == 2 &&
                    IPAddress.TryParse(parts[0], out IPAddress networkAddress) &&
                    int.TryParse(parts[1], out int prefixLength))
                {
                    _ranges.Add(new IpRange(networkAddress, prefixLength));
                }
            }
            else
            {
                //
                // Single IP: treat as /32 (IPv4) or /128 (IPv6)
                //
                if (IPAddress.TryParse(trimmed, out IPAddress address))
                {
                    int prefixLength = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
                    _ranges.Add(new IpRange(address, prefixLength));
                }
            }
        }


        /// <summary>
        /// Removes all ranges from the list.
        /// </summary>
        public void Clear()
        {
            _ranges.Clear();
        }


        /// <summary>
        /// Checks whether the given IP address matches any range in the list.
        /// </summary>
        public bool Contains(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            if (IPAddress.TryParse(ipAddress.Trim(), out IPAddress address) == false)
            {
                return false;
            }

            return Contains(address);
        }


        /// <summary>
        /// Checks whether the given IP address matches any range in the list.
        /// </summary>
        public bool Contains(IPAddress address)
        {
            foreach (IpRange range in _ranges)
            {
                if (range.Contains(address) == true)
                {
                    return true;
                }
            }

            return false;
        }


        // ── Internal ──────────────────────────────────────────────────────


        /// <summary>
        /// Represents a single IP range defined by a network address and prefix length.
        /// </summary>
        private class IpRange
        {
            private readonly byte[] _networkBytes;
            private readonly byte[] _maskBytes;


            public IpRange(IPAddress networkAddress, int prefixLength)
            {
                _networkBytes = networkAddress.GetAddressBytes();
                _maskBytes = CreateMask(_networkBytes.Length, prefixLength);

                //
                // Apply the mask to the network address to normalize
                //
                for (int i = 0; i < _networkBytes.Length; i++)
                {
                    _networkBytes[i] = (byte)(_networkBytes[i] & _maskBytes[i]);
                }
            }


            public bool Contains(IPAddress address)
            {
                byte[] addressBytes = address.GetAddressBytes();

                //
                // Must be the same address family (IPv4 vs IPv6)
                //
                if (addressBytes.Length != _networkBytes.Length)
                {
                    return false;
                }

                //
                // Apply the mask and compare
                //
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    if ((addressBytes[i] & _maskBytes[i]) != _networkBytes[i])
                    {
                        return false;
                    }
                }

                return true;
            }


            private static byte[] CreateMask(int addressLength, int prefixLength)
            {
                byte[] mask = new byte[addressLength];
                int remainingBits = prefixLength;

                for (int i = 0; i < mask.Length; i++)
                {
                    if (remainingBits >= 8)
                    {
                        mask[i] = 0xFF;
                        remainingBits -= 8;
                    }
                    else if (remainingBits > 0)
                    {
                        mask[i] = (byte)(0xFF << (8 - remainingBits));
                        remainingBits = 0;
                    }
                    else
                    {
                        mask[i] = 0x00;
                    }
                }

                return mask;
            }
        }
    }
}
