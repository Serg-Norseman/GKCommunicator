/*
 *  "BSLib.TeamsNet", the serverless peer-to-peer network library.
 *  Copyright (C) 2018-2025 by Sergey V. Zhdanovskih.
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

using System;
using System.Reflection;
using BSLib.TeamsNet;
using BSLib.TeamsNet.UI;

[assembly: AssemblyTitle("BSLib.TeamsNet.App")]
[assembly: AssemblyDescription(CommunicatorCore.APP_DESC)]
[assembly: AssemblyProduct(CommunicatorCore.APP_NAME)]
[assembly: AssemblyCopyright(CommunicatorCore.APP_COPYRIGHT)]
[assembly: AssemblyVersion(CommunicatorCore.APP_VERSION)]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#elif RELEASE
[assembly: AssemblyConfiguration("Release")]
#endif

namespace BSLib.TeamsNet.App
{
#if !ETO
    using System.Windows.Forms;
#else
    using Eto.Forms;
#endif

    /// <summary>
    /// The main startup class of application.
    /// </summary>
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
#if !ETO
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChatForm());
#else
            UIHelper.InitCommonStyles();
            var application = new Application();
            application.Run(new ChatForm());
#endif
        }
    }
}
