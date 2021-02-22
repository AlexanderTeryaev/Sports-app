using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetascanHelper
{
    public enum MetaScanScanStatus
    {
        NotFound,
        GeneralError,
        ProccessNotCompleted,
        ValidSizeError,
        ThreatsFound,
        ExtensionNotAllowed,
        Valid
    }
}
