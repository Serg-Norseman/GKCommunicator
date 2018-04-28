/*
 * Copyright: LumiSoft <ivar@lumisoft.ee>
 * 
 * General usage terms:
 * 
 *   *) If you use/redistribute compiled binary, there are no restrictions.
 *      You can use it in any project, commercial and no-commercial.
 *      Redistributing compiled binary not limited any way.
 * 
 *   *) It's allowed to complile source code parts to your application,
 *      but then you may not rename class names and namespaces.
 * 
 *   *) Anything is possible, if special agreement between LumiSoft.
 * 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace LumiSoft.Net.STUN.Client
{
    /// <summary>
    /// Specifies UDP network type.
    /// </summary>
    public enum STUN_NetType
    {
        /// <summary>
        /// UDP is always blocked.
        /// </summary>
        UdpBlocked,

        /// <summary>
        /// No NAT, public IP, no firewall.
        /// </summary>
        OpenInternet,

        /// <summary>
        /// No NAT, public IP, but symmetric UDP firewall.
        /// </summary>
        SymmetricUdpFirewall,

        /// <summary>
        /// A full cone NAT is one where all requests from the same internal IP address and port are 
        /// mapped to the same external IP address and port. Furthermore, any external host can send 
        /// a packet to the internal host, by sending a packet to the mapped external address.
        /// </summary>
        FullCone,

        /// <summary>
        /// A restricted cone NAT is one where all requests from the same internal IP address and 
        /// port are mapped to the same external IP address and port. Unlike a full cone NAT, an external
        /// host (with IP address X) can send a packet to the internal host only if the internal host 
        /// had previously sent a packet to IP address X.
        /// </summary>
        RestrictedCone,

        /// <summary>
        /// A port restricted cone NAT is like a restricted cone NAT, but the restriction 
        /// includes port numbers. Specifically, an external host can send a packet, with source IP
        /// address X and source port P, to the internal host only if the internal host had previously 
        /// sent a packet to IP address X and port P.
        /// </summary>
        PortRestrictedCone,

        /// <summary>
        /// A symmetric NAT is one where all requests from the same internal IP address and port, 
        /// to a specific destination IP address and port, are mapped to the same external IP address and
        /// port.  If the same host sends a packet with the same source address and port, but to 
        /// a different destination, a different mapping is used. Furthermore, only the external host that
        /// receives a packet can send a UDP packet back to the internal host.
        /// </summary>
        Symmetric
    }
}
