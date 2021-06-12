using System;
using System.Collections.Generic;
using System.Text;
using Prism.Common;

namespace HusicBasic.Models.Tasks
{
    public class TaskParameters : ParametersBase
    {
        public TaskParameters() : base() { }
        public TaskParameters(string query) : base(query) { }
    }
}
