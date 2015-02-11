using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CleverDomeCommon
{
    public enum PermissionLevel
    {
        None = 0,
        View = 100,
        Modify = 200,
        Reupload = 300,
        Delete = 400,
        Share = 500,
        QualityReview = 600,
        OSJReview = 700,
        BOProcessing = 800,
        Compliance = 900,
        Admin = 1000
    }
}
