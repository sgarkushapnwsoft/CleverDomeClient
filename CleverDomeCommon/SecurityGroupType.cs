using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleverDomeCommon
{
    public enum SecurityGroupType
    {
        Application = 0,
        Supervisor,
        Owner,
        Client,
        Custom
    }
}
