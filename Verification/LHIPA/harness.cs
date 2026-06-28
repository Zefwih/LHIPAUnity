// Verification driver: runs the REAL lhipa.LHIPA.CalculateLHIPA on the shared test
// battery (signals.csv) and prints "name,lhipa" to stdout.
// When compiled with /define:LHIPA_TEST it also dumps the normalized cD_H / cD_L bands
// (via the test seam in LHIPA.cs) to the json path given as args[1], so Python can
// compare them against pywt.downcoef.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;

public static class Harness
{
    public static int Main(string[] args)
    {
        var ci = CultureInfo.InvariantCulture;
        string signalsPath = args.Length > 0 ? args[0] : "signals.csv";
        string bandsPath = args.Length > 1 ? args[1] : null;

        var bands = new StringBuilder();
        bands.Append("{");
        bool first = true;

        foreach (var line in File.ReadAllLines(signalsPath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var tok = line.Split(',');
            string name = tok[0];
            float sr = float.Parse(tok[1], ci);
            int n = tok.Length - 2;
            float[] sig = new float[n];
            for (int i = 0; i < n; i++) sig[i] = float.Parse(tok[i + 2], ci);
            float tt = n / sr;

            float result;
            try { result = global::lhipa.LHIPA.CalculateLHIPA(sig, tt, 0f, false); }
            catch (Exception e) { Console.Error.WriteLine(name + " EXC: " + e.Message); result = float.NaN; }
            Console.WriteLine(name + "," + result.ToString("R", ci));

#if LHIPA_TEST
            double[] cdH, cdL;
            global::lhipa.LHIPA.ComputeBandsForTest(sig, out cdH, out cdL);
            if (!first) bands.Append(",");
            first = false;
            bands.Append("\"").Append(name).Append("\":{\"cD_H\":[");
            for (int i = 0; i < cdH.Length; i++) { if (i>0) bands.Append(","); bands.Append(cdH[i].ToString("R", ci)); }
            bands.Append("],\"cD_L\":[");
            for (int i = 0; i < cdL.Length; i++) { if (i>0) bands.Append(","); bands.Append(cdL[i].ToString("R", ci)); }
            bands.Append("]}");
#endif
        }

        bands.Append("}");
        if (bandsPath != null)
            File.WriteAllText(bandsPath, bands.ToString());
        return 0;
    }
}
