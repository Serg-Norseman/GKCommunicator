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
    /// This enum specifies STUN message type.
    /// </summary>
    public enum STUN_MessageType
    {
        /// <summary>
        /// STUN message is binding request.
        /// </summary>
        BindingRequest = 0x0001,

        /// <summary>
        /// STUN message is binding request response.
        /// </summary>
        BindingResponse = 0x0101,

        /// <summary>
        /// STUN message is binding requesr error response.
        /// </summary>
        BindingErrorResponse = 0x0111,

        /// <summary>
        /// STUN message is "shared secret" request.
        /// </summary>
        SharedSecretRequest = 0x0002,

        /// <summary>
        /// STUN message is "shared secret" request response.
        /// </summary>
        SharedSecretResponse = 0x0102,

        /// <summary>
        /// STUN message is "shared secret" request error response.
        /// </summary>
        SharedSecretErrorResponse = 0x0112,
    }
}
