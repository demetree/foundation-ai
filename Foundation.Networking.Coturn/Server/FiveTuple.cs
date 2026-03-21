// ============================================================================
//
// FiveTuple.cs — Identifies a TURN allocation by its 5-tuple.
//
// Per RFC 5766, each allocation is uniquely identified by a 5-tuple:
//   (client IP, client port, server IP, server port, transport protocol)
//
// On the server side, the client address is the server-reflexive address
// (i.e., the address as seen through the NAT).
//
// ============================================================================

using System;
using System.Net;

namespace Foundation.Networking.Coturn.Server
{
    /// <summary>
    ///
    /// Five-tuple that uniquely identifies a TURN allocation.
    ///
    /// Used as a dictionary key in AllocationManager, so it implements
    /// IEquatable and provides a good hash code.
    ///
    /// </summary>
    public struct FiveTuple : IEquatable<FiveTuple>
    {
        //
        // The client's address as seen by the server (server-reflexive)
        //
        public IPEndPoint ClientEndPoint { get; set; }

        //
        // The server's listening address
        //
        public IPEndPoint ServerEndPoint { get; set; }

        //
        // Transport protocol (UDP = 17, TCP = 6)
        //
        public byte Transport { get; set; }


        public FiveTuple(IPEndPoint clientEndPoint, IPEndPoint serverEndPoint, byte transport)
        {
            ClientEndPoint = clientEndPoint;
            ServerEndPoint = serverEndPoint;
            Transport = transport;
        }


        public bool Equals(FiveTuple other)
        {
            return Transport == other.Transport
                && EndPointEquals(ClientEndPoint, other.ClientEndPoint)
                && EndPointEquals(ServerEndPoint, other.ServerEndPoint);
        }


        public override bool Equals(object obj)
        {
            if (obj is FiveTuple other)
            {
                return Equals(other);
            }

            return false;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 31 + Transport.GetHashCode();

                if (ClientEndPoint != null)
                {
                    hash = hash * 31 + ClientEndPoint.Address.GetHashCode();
                    hash = hash * 31 + ClientEndPoint.Port.GetHashCode();
                }

                if (ServerEndPoint != null)
                {
                    hash = hash * 31 + ServerEndPoint.Address.GetHashCode();
                    hash = hash * 31 + ServerEndPoint.Port.GetHashCode();
                }

                return hash;
            }
        }


        public override string ToString()
        {
            string transportName = (Transport == 17) ? "UDP" : (Transport == 6) ? "TCP" : Transport.ToString();

            return $"[{transportName}] {ClientEndPoint} → {ServerEndPoint}";
        }


        public static bool operator ==(FiveTuple left, FiveTuple right)
        {
            return left.Equals(right);
        }


        public static bool operator !=(FiveTuple left, FiveTuple right)
        {
            return left.Equals(right) == false;
        }


        // ── Private ──────────────────────────────────────────────────────


        private static bool EndPointEquals(IPEndPoint a, IPEndPoint b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            return a.Port == b.Port && a.Address.Equals(b.Address);
        }
    }
}
