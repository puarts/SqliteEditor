using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor
{
    public record StringConversionInfo(string Source, string Destination)
    {
    }

    public record ApplicationSetting(
        string? DatabasePath,
        ImmutableList<StringConversionInfo> StringConversionInfos)
    {
    }
}
