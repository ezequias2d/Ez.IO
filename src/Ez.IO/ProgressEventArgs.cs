using System;
using System.Collections.Generic;
using System.Text;

namespace Ez.IO
{
    public sealed class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(float progress)
        {
            Progress = progress;
        }

        public float Progress { get; }
    }
}
