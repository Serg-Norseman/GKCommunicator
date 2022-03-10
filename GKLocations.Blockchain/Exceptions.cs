/*
 *  This file is part of the "GKLocations".
 *  Copyright (C) 2022 by Sergey V. Zhdanovskih.
 *  This program is licensed under the GNU General Public License.
 */

using System;

namespace GKLocations.Blockchain
{
    public class MethodArgumentException : ArgumentException
    {
        public MethodArgumentException(string paramName, string message)
            : base($"The argument {paramName} does not match the conditions. {message}", paramName)
        {
        }
    }

    public class MethodResultException : ArgumentException
    {
        public MethodResultException(string paramName, string message)
            : base($"The result {paramName} does not match the conditions. {message}", paramName)
        {
        }
    }
}
