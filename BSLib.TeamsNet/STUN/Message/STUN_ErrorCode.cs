/*
 * Copyright: LumiSoft <ivar@lumisoft.ee>
 * 
 * General usage terms:
 * 
 *   *) If you use/redistribute compiled binary, there are no restrictions.
 *      You can use it in any project, commercial and no-commercial.
 *      Redistributing compiled binary not limited any way.
 * 
 *   *) It's allowed to compile source code parts to your application,
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
    /// This class implements STUN ERROR-CODE. Defined in RFC 3489 11.2.9.
    /// </summary>
    public class STUN_ErrorCode
    {
        private readonly int fCode;
        private readonly string fReasonText;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="reasonText">Reason text.</param>
        public STUN_ErrorCode(int code, string reasonText)
        {
            fCode = code;
            fReasonText = reasonText;
        }

        /// <summary>
        /// Gets or sets error code.
        /// </summary>
        public int Code
        {
            get { return fCode; }
        }

        /// <summary>
        /// Gets reason text.
        /// </summary>
        public string ReasonText
        {
            get { return fReasonText; }
        }
    }
}
