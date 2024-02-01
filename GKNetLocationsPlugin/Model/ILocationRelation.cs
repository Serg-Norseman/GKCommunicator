/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GKCommunicator".
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

namespace GKNetLocationsPlugin.Model
{
    public interface ILocationRelation
    {
        string GUID { get; set; }

        string LocationGUID { get; set; }

        string OwnerGUID { get; set; }

        /// <summary>
        /// [P]olitical, [R]eligious, [G]eographic, [C]ultural
        /// </summary>
        string RelationType { get; set; }

        /// <summary>
        /// Perfect notation - GEDCOM date period format ("FROM 10 JUL 1805 TO 20 AUG 1917").
        /// </summary>
        string ActualDates { get; set; }
    }
}
