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

namespace LumiSoft.Net.STUN.Message
{
    /// <summary>
    /// This class implements STUN CHANGE-REQUEST attribute. Defined in RFC 3489 11.2.4.
    /// </summary>
    public class STUN_ChangeRequest
    {
        private readonly bool fChangeIP;
        private readonly bool fChangePort;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public STUN_ChangeRequest()
        {
            fChangeIP = true;
            fChangePort = true;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="changeIP">Specifies if STUN server must send response to different IP than request was received.</param>
        /// <param name="changePort">Specifies if STUN server must send response to different port than request was received.</param>
        public STUN_ChangeRequest(bool changeIP, bool changePort)
        {
            fChangeIP = changeIP;
            fChangePort = changePort;
        }

        /// <summary>
        /// Gets or sets if STUN server must send response to different IP than request was received.
        /// </summary>
        public bool ChangeIP
        {
            get { return fChangeIP; }
        }

        /// <summary>
        /// Gets or sets if STUN server must send response to different port than request was received.
        /// </summary>
        public bool ChangePort
        {
            get { return fChangePort; }
        }
    }
}
