// Copyright (c) 2021 ezequias2d <ezequiasmoises@gmail.com> and the Ez contributors
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ez.IO
{
    /// <summary>
    /// Helper functions for path manipulation.
    /// </summary>
    public static class PathHelper
    {
        private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Split the path by separator.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <returns>Split path.</returns>
        public static string[] SeparatePath(string path)
        {      
            return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Get folder name from path.
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>The name of last folder of path(considers folder up to the last folder separator, example: c:/path/foo = path. c:/path/foo/ = foo.).</returns>
        public static string GetFolderName(string path)
        {
            return Path.GetFileName(Path.GetDirectoryName(path));
        }
    }
}
