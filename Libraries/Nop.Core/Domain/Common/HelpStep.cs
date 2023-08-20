using System;
using Nop.Core.Domain.Directory;
using Newtonsoft.Json.Linq;

namespace Nop.Core.Domain.Common
{
    public partial class HelpStep : BaseEntity
    {
        public HelpStep()
        {

        }

        public HelpStep(string title, string content, JRaw target, string placement)
        {
            this.Title = title;
            this.Content = content;
            this.Target = target;
            this.Placement = placement;
        }
        public string Title { get; set; }
        public string Content { get; set; }
        public JRaw Target { get; set; }
        public JRaw OnPrev { get; set; }
        public JRaw OnNext { get; set; }
        public JRaw OnShow { get; set; }
        public string Placement { get; set; }
    }
}
