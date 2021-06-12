using System;
using System.Collections.Generic;
using System.Text;
using TNO.Json.Runtime;

namespace HusicBasic.Services.Interfaces
{
    public interface IYoutubeCLI
    {
        #region Methods
        JsonValue ParseAsJson(string arguments);
        #endregion
    }
}
