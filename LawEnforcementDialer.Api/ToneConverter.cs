using System.Text;
using Azure.Communication.CallAutomation;

namespace LawEnforcementDialer.Api;

public static class ToneConverter
{
    public static string ToString(this IReadOnlyList<DtmfTone> tones)
    {
        var builder = new StringBuilder(tones.Count);
        foreach (var dtmfTone in tones)
        {
            if (dtmfTone == DtmfTone.Zero)
            {
                builder.Append("0");
                continue;
            }

            if (dtmfTone == DtmfTone.One)
            {
                builder.Append("1");
                continue;
            }

            if (dtmfTone == DtmfTone.Two)
            {
                builder.Append("2");
                continue;
            }

            if (dtmfTone == DtmfTone.Three)
            {
                builder.Append("3");
                continue;
            }

            if (dtmfTone == DtmfTone.Four)
            {
                builder.Append("4");
                continue;
            }

            if (dtmfTone == DtmfTone.Five)
            {
                builder.Append("5");
                continue;
            }

            if (dtmfTone == DtmfTone.Six)
            {
                builder.Append("6");
                continue;
            }

            if (dtmfTone == DtmfTone.Seven)
            {
                builder.Append("7");
                continue;
            }

            if (dtmfTone == DtmfTone.Eight)
            {
                builder.Append("8");
                continue;
            }

            if (dtmfTone == DtmfTone.Nine)
            {
                builder.Append("9");
                continue;
            }
        }

        return builder.ToString();
    }
}