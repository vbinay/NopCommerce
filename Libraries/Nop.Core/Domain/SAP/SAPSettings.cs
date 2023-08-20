using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.SAP
{
    public class SAPSettings : ISettings
    {
        public string Protocol { get; set; }

        public string Hostname { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int PortNumber { get; set; }

        public string SshHostKeyFingerprint { get; set; }

        public string localPath { get; set; }

        public string remotePath { get; set; }

        public bool remove { get; set; }

        public string TestOutput { get; set; }
    }
}
