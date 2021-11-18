/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2021 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Net;
using System.Net.Sockets;
using GKNet.Logging;
using LumiSoft.Net.STUN.Client;

namespace GKNet.STUN
{
    public static class STUNUtility
    {
        private static readonly ILogger fLogger = LogManager.GetLogger(ProtocolHelper.LOG_FILE, ProtocolHelper.LOG_LEVEL, "STUNUtility");

        public static STUN_Result Detect(int portNumber)
        {
            fLogger.WriteInfo("STUN detecting started");

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, portNumber));

            fLogger.WriteInfo("Local Endpoint: " + socket.LocalEndPoint);

            STUN_Result result = STUN_Client.Query("stun1.l.google.com", 19302, socket);
            if (result.NetType == STUN_NetType.UdpBlocked) {
                foreach (var item in STUNProviders) {
                    var res = STUN_Client.Query(item.Address, item.Port, socket);
                    if (res.NetType != STUN_NetType.UdpBlocked) {
                        result = res;
                        break;
                    }
                }
            }

            fLogger.WriteInfo("NAT Type: " + result.NetType.ToString());
            if (result.NetType != STUN_NetType.UdpBlocked) {
                fLogger.WriteInfo("Public Endpoint: " + result.PublicEndPoint.ToString());
            }

            socket.Close();

            fLogger.WriteInfo("STUN detecting finished");

            return result;
        }

        private struct STUNProvider
        {
            public string Address;
            public int Port;

            public STUNProvider(string address, int port)
            {
                Address = address;
                Port = port;
            }
        }

        private static readonly STUNProvider[] STUNProviders = new STUNProvider[] {
            new STUNProvider("iphone-stun.strato-iphone.de",3478),
            new STUNProvider("numb.viagenie.ca",3478),
            new STUNProvider("sip1.lakedestiny.cordiaip.com",3478),
            new STUNProvider("stun.12connect.com",3478),
            new STUNProvider("stun.12voip.com",3478),
            new STUNProvider("stun.1cbit.ru",3478),
            new STUNProvider("stun.1und1.de",3478),
            new STUNProvider("stun.2talk.co.nz",3478),
            new STUNProvider("stun.2talk.com",3478),
            new STUNProvider("stun.3clogic.com",3478),
            new STUNProvider("stun.3cx.com",3478),
            new STUNProvider("stun.726.com",3478),
            new STUNProvider("stun.a-mm.tv",3478),
            new STUNProvider("stun.aa.net.uk",3478),
            new STUNProvider("stun.aceweb.com",3478),
            new STUNProvider("stun.acrobits.cz",3478),
            new STUNProvider("stun.acronis.com",3478),
            new STUNProvider("stun.actionvoip.com",3478),
            new STUNProvider("stun.advfn.com",3478),
            new STUNProvider("stun.aeta-audio.com",3478),
            new STUNProvider("stun.aeta.com",3478),
            new STUNProvider("stun.allflac.com",3478),
            new STUNProvider("stun.anlx.net",3478),
            new STUNProvider("stun.antisip.com",3478),
            new STUNProvider("stun.avigora.com",3478),
            new STUNProvider("stun.avigora.fr",3478),
            new STUNProvider("stun.b2b2c.ca",3478),
            new STUNProvider("stun.bahnhof.net",3478),
            new STUNProvider("stun.barracuda.com",3478),
            new STUNProvider("stun.beam.pro",3478),
            new STUNProvider("stun.bitburger.de",3478),
            new STUNProvider("stun.bluesip.net",3478),
            new STUNProvider("stun.bomgar.com",3478),
            new STUNProvider("stun.botonakis.com",3478),
            new STUNProvider("stun.budgetphone.nl",3478),
            new STUNProvider("stun.budgetsip.com",3478),
            new STUNProvider("stun.cablenet-as.net",3478),
            new STUNProvider("stun.callromania.ro",3478),
            new STUNProvider("stun.callwithus.com",3478),
            new STUNProvider("stun.cheapvoip.com",3478),
            new STUNProvider("stun.cloopen.com",3478),
            new STUNProvider("stun.cognitoys.com",3478),
            new STUNProvider("stun.comfi.com",3478),
            new STUNProvider("stun.commpeak.com",3478),
            new STUNProvider("stun.communigate.com",3478),
            new STUNProvider("stun.comrex.com",3478),
            new STUNProvider("stun.comtube.com",3478),
            new STUNProvider("stun.comtube.ru",3478),
            new STUNProvider("stun.connecteddata.com",3478),
            new STUNProvider("stun.cope.es",3478),
            new STUNProvider("stun.counterpath.com",3478),
            new STUNProvider("stun.counterpath.net",3478),
            new STUNProvider("stun.crimeastar.net",3478),
            new STUNProvider("stun.dcalling.de",3478),
            new STUNProvider("stun.demos.ru",3478),
            new STUNProvider("stun.demos.su",3478),
            new STUNProvider("stun.dls.net",3478),
            new STUNProvider("stun.dokom.net",3478),
            new STUNProvider("stun.dowlatow.ru",3478),
            new STUNProvider("stun.duocom.es",3478),
            new STUNProvider("stun.dus.net",3478),
            new STUNProvider("stun.e-fon.ch",3478),
            new STUNProvider("stun.easemob.com",3478),
            new STUNProvider("stun.easycall.pl",3478),
            new STUNProvider("stun.easyvoip.com",3478),
            new STUNProvider("stun.eibach.de",3478),
            new STUNProvider("stun.ekiga.net",3478),
            new STUNProvider("stun.ekir.de",3478),
            new STUNProvider("stun.elitetele.com",3478),
            new STUNProvider("stun.emu.ee",3478),
            new STUNProvider("stun.engineeredarts.co.uk",3478),
            new STUNProvider("stun.eoni.com",3478),
            new STUNProvider("stun.epygi.com",3478),
            new STUNProvider("stun.faktortel.com.au",3478),
            new STUNProvider("stun.fbsbx.com",3478),
            new STUNProvider("stun.fh-stralsund.de",3478),
            new STUNProvider("stun.fmbaros.ru",3478),
            new STUNProvider("stun.fmo.de",3478),
            new STUNProvider("stun.freecall.com",3478),
            new STUNProvider("stun.freeswitch.org",3478),
            new STUNProvider("stun.freevoipdeal.com",3478),
            new STUNProvider("stun.genymotion.com",3478),
            new STUNProvider("stun.gmx.de",3478),
            new STUNProvider("stun.gmx.net",3478),
            new STUNProvider("stun.gnunet.org",3478),
            new STUNProvider("stun.gradwell.com",3478),
            new STUNProvider("stun.halonet.pl",3478),
            new STUNProvider("stun.highfidelity.io",3478),
            new STUNProvider("stun.hoiio.com",3478),
            new STUNProvider("stun.hosteurope.de",3478),
            new STUNProvider("stun.i-stroy.ru",3478),
            new STUNProvider("stun.ideasip.com",3478),
            new STUNProvider("stun.imweb.io",3478),
            new STUNProvider("stun.infra.net",3478),
            new STUNProvider("stun.innovaphone.com",3478),
            new STUNProvider("stun.instantteleseminar.com",3478),
            new STUNProvider("stun.internetcalls.com",3478),
            new STUNProvider("stun.intervoip.com",3478),
            new STUNProvider("stun.ipcomms.net",3478),
            new STUNProvider("stun.ipfire.org",3478),
            new STUNProvider("stun.ippi.com",3478),
            new STUNProvider("stun.ippi.fr",3478),
            new STUNProvider("stun.it1.hr",3478),
            new STUNProvider("stun.ivao.aero",3478),
            new STUNProvider("stun.jabbim.cz",3478),
            new STUNProvider("stun.jumblo.com",3478),
            new STUNProvider("stun.justvoip.com",3478),
            new STUNProvider("stun.kaospilot.dk",3478),
            new STUNProvider("stun.kaseya.com",3478),
            new STUNProvider("stun.kaznpu.kz",3478),
            new STUNProvider("stun.kiwilink.co.nz",3478),
            new STUNProvider("stun.kuaibo.com",3478),
            new STUNProvider("stun.l.google.com",19302),
            new STUNProvider("stun.lamobo.org",3478),
            new STUNProvider("stun.levigo.de",3478),
            new STUNProvider("stun.lindab.com",3478),
            new STUNProvider("stun.linphone.org",3478),
            new STUNProvider("stun.linx.net",3478),
            new STUNProvider("stun.liveo.fr",3478),
            new STUNProvider("stun.lowratevoip.com",3478),
            new STUNProvider("stun.lundimatin.fr",3478),
            new STUNProvider("stun.maestroconference.com",3478),
            new STUNProvider("stun.mangotele.com",3478),
            new STUNProvider("stun.mgn.ru",3478),
            new STUNProvider("stun.mit.de",3478),
            new STUNProvider("stun.miwifi.com",3478),
            new STUNProvider("stun.mixer.com",3478),
            new STUNProvider("stun.modulus.gr",3478),
            new STUNProvider("stun.mrmondialisation.org",3478),
            new STUNProvider("stun.myfreecams.com",3478),
            new STUNProvider("stun.myvoiptraffic.com",3478),
            new STUNProvider("stun.mywatson.it",3478),
            new STUNProvider("stun.nacsworld.com",3478),
            new STUNProvider("stun.nas.net",3478),
            new STUNProvider("stun.nautile.nc",3478),
            new STUNProvider("stun.netappel.com",3478),
            new STUNProvider("stun.nextcloud.com",3478),
            new STUNProvider("stun.nfon.net",3478),
            new STUNProvider("stun.ngine.de",3478),
            new STUNProvider("stun.node4.co.uk",3478),
            new STUNProvider("stun.nonoh.net",3478),
            new STUNProvider("stun.nottingham.ac.uk",3478),
            new STUNProvider("stun.nova.is",3478),
            new STUNProvider("stun.onesuite.com",3478),
            new STUNProvider("stun.onthenet.com.au",3478),
            new STUNProvider("stun.ooma.com",3478),
            new STUNProvider("stun.oovoo.com",3478),
            new STUNProvider("stun.ozekiphone.com",3478),
            new STUNProvider("stun.personal-voip.de",3478),
            new STUNProvider("stun.petcube.com",3478),
            new STUNProvider("stun.pexip.com",3478),
            new STUNProvider("stun.phone.com",3478),
            new STUNProvider("stun.pidgin.im",3478),
            new STUNProvider("stun.pjsip.org",3478),
            new STUNProvider("stun.planete.net",3478),
            new STUNProvider("stun.poivy.com",3478),
            new STUNProvider("stun.powervoip.com",3478),
            new STUNProvider("stun.ppdi.com",3478),
            new STUNProvider("stun.rackco.com",3478),
            new STUNProvider("stun.redworks.nl",3478),
            new STUNProvider("stun.ringostat.com",3478),
            new STUNProvider("stun.rmf.pl",3478),
            new STUNProvider("stun.rockenstein.de",3478),
            new STUNProvider("stun.rolmail.net",3478),
            new STUNProvider("stun.rudtp.ru",3478),
            new STUNProvider("stun.russian-club.net",3478),
            new STUNProvider("stun.rynga.com",3478),
            new STUNProvider("stun.sainf.ru",3478),
            new STUNProvider("stun.schlund.de",3478),
            new STUNProvider("stun.sigmavoip.com",3478),
            new STUNProvider("stun.sip.us",3478),
            new STUNProvider("stun.sipdiscount.com",3478),
            new STUNProvider("stun.sipgate.net",10000),
            new STUNProvider("stun.sipgate.net",3478),
            new STUNProvider("stun.siplogin.de",3478),
            new STUNProvider("stun.sipnet.net",3478),
            new STUNProvider("stun.sipnet.ru",3478),
            new STUNProvider("stun.siportal.it",3478),
            new STUNProvider("stun.sippeer.dk",3478),
            new STUNProvider("stun.siptraffic.com",3478),
            new STUNProvider("stun.sma.de",3478),
            new STUNProvider("stun.smartvoip.com",3478),
            new STUNProvider("stun.smsdiscount.com",3478),
            new STUNProvider("stun.snafu.de",3478),
            new STUNProvider("stun.solcon.nl",3478),
            new STUNProvider("stun.solnet.ch",3478),
            new STUNProvider("stun.sonetel.com",3478),
            new STUNProvider("stun.sonetel.net",3478),
            new STUNProvider("stun.sovtest.ru",3478),
            new STUNProvider("stun.speedy.com.ar",3478),
            new STUNProvider("stun.spoiltheprincess.com",3478),
            new STUNProvider("stun.srce.hr",3478),
            new STUNProvider("stun.ssl7.net",3478),
            new STUNProvider("stun.stunprotocol.org",3478),
            new STUNProvider("stun.swissquote.com",3478),
            new STUNProvider("stun.t-online.de",3478),
            new STUNProvider("stun.talks.by",3478),
            new STUNProvider("stun.tel.lu",3478),
            new STUNProvider("stun.telbo.com",3478),
            new STUNProvider("stun.telefacil.com",3478),
            new STUNProvider("stun.threema.ch",3478),
            new STUNProvider("stun.tng.de",3478),
            new STUNProvider("stun.trueconf.ru",3478),
            new STUNProvider("stun.twt.it",3478),
            new STUNProvider("stun.ucallweconn.net",3478),
            new STUNProvider("stun.ucsb.edu",3478),
            new STUNProvider("stun.ucw.cz",3478),
            new STUNProvider("stun.uiscom.ru",3478),
            new STUNProvider("stun.uls.co.za",3478),
            new STUNProvider("stun.unseen.is",3478),
            new STUNProvider("stun.up.edu.ph",3478),
            new STUNProvider("stun.usfamily.net",3478),
            new STUNProvider("stun.uucall.com",3478),
            new STUNProvider("stun.veoh.com",3478),
            new STUNProvider("stun.vipgroup.net",3478),
            new STUNProvider("stun.viva.gr",3478),
            new STUNProvider("stun.vivox.com",3478),
            new STUNProvider("stun.vline.com",3478),
            new STUNProvider("stun.vmi.se",3478),
            new STUNProvider("stun.vo.lu",3478),
            new STUNProvider("stun.vodafone.ro",3478),
            new STUNProvider("stun.voicetrading.com",3478),
            new STUNProvider("stun.voip.aebc.com",3478),
            new STUNProvider("stun.voip.blackberry.com",3478),
            new STUNProvider("stun.voip.eutelia.it",3478),
            new STUNProvider("stun.voiparound.com",3478),
            new STUNProvider("stun.voipblast.com",3478),
            new STUNProvider("stun.voipbuster.com",3478),
            new STUNProvider("stun.voipbusterpro.com",3478),
            new STUNProvider("stun.voipcheap.co.uk",3478),
            new STUNProvider("stun.voipcheap.com",3478),
            new STUNProvider("stun.voipdiscount.com",3478),
            new STUNProvider("stun.voipfibre.com",3478),
            new STUNProvider("stun.voipgain.com",3478),
            new STUNProvider("stun.voipgate.com",3478),
            new STUNProvider("stun.voipinfocenter.com",3478),
            new STUNProvider("stun.voipplanet.nl",3478),
            new STUNProvider("stun.voippro.com",3478),
            new STUNProvider("stun.voipraider.com",3478),
            new STUNProvider("stun.voipstunt.com",3478),
            new STUNProvider("stun.voipwise.com",3478),
            new STUNProvider("stun.voipzoom.com",3478),
            new STUNProvider("stun.voxgratia.org",3478),
            new STUNProvider("stun.voxox.com",3478),
            new STUNProvider("stun.voztele.com",3478),
            new STUNProvider("stun.wcoil.com",3478),
            new STUNProvider("stun.webcalldirect.com",3478),
            new STUNProvider("stun.whc.net",3478),
            new STUNProvider("stun.whoi.edu",3478),
            new STUNProvider("stun.wifirst.net",3478),
            new STUNProvider("stun.wwdl.net",3478),
            new STUNProvider("stun.xn----8sbcoa5btidn9i.xn--p1ai",3478),
            new STUNProvider("stun.xten.com",3478),
            new STUNProvider("stun.xtratelecom.es",3478),
            new STUNProvider("stun.yy.com",3478),
            new STUNProvider("stun.zadarma.com",3478),
            new STUNProvider("stun.zepter.ru",3478),
            new STUNProvider("stun.zoiper.com",3478),
            new STUNProvider("stun1.faktortel.com.au",3478),
            new STUNProvider("stun1.l.google.com",19302),
            new STUNProvider("stun2.l.google.com",19302),
            new STUNProvider("stun3.l.google.com",19302),
            new STUNProvider("stun4.l.google.com",19302),
            new STUNProvider("stun.zoiper.com",3478)
        };
    }
}
