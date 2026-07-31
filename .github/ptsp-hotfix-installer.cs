using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

internal static class Program
{
    private const string TargetVersion = "__VERSION__";
    private const string PayloadBase64 =
            __PAYLOAD_BASE64__;

    [STAThread]
    private static int Main(string[] args)
    {
        bool silent = args.Any(a => string.Equals(a, "--silent", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(a, "/silent", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(a, "/s", StringComparison.OrdinalIgnoreCase));
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string root = Path.Combine(local, "PTSP Assistant");
        string logDir = Path.Combine(root, "Logs");
        Directory.CreateDirectory(logDir);
        string logPath = Path.Combine(logDir, "hotfix-v" + TargetVersion + ".log");

        try
        {
            Log(logPath, "Memulai hotfix v" + TargetVersion);
            string extensionPath = FindExtensionPath(root);
            if (extensionPath == null)
                throw new InvalidOperationException("Folder extension PTSP Assistant tidak ditemukan. Jalankan installer FULL terlebih dahulu.");

            string currentVersion = ReadManifestVersion(Path.Combine(extensionPath, "manifest.json"));
            Log(logPath, "Target extension: " + extensionPath);
            Log(logPath, "Versi sebelumnya: " + (currentVersion ?? "tidak terbaca"));

            if (currentVersion != null && CompareVersions(currentVersion, "3.0.10") < 0)
                throw new InvalidOperationException("Hotfix otomatis ini memerlukan PTSP Assistant minimal v3.0.10. Pasang installer FULL terbaru terlebih dahulu.");

            string temp = Path.Combine(Path.GetTempPath(), "ptsp-hotfix-" + TargetVersion + "-" + Guid.NewGuid().ToString("N"));
            string payloadDir = Path.Combine(temp, "payload");
            Directory.CreateDirectory(payloadDir);

            byte[] zipBytes = Convert.FromBase64String(PayloadBase64);
            string zipPath = Path.Combine(temp, "payload.zip");
            File.WriteAllBytes(zipPath, zipBytes);
            ZipFile.ExtractToDirectory(zipPath, payloadDir);

            string backupRoot = Path.Combine(root, "Backups", "extension-" + Sanitize(currentVersion ?? "unknown") + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(backupRoot);

            foreach (string source in Directory.GetFiles(payloadDir, "*", SearchOption.AllDirectories))
            {
                string relative = source.Substring(payloadDir.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destination = Path.Combine(extensionPath, relative);
                string backup = Path.Combine(backupRoot, relative);

                if (File.Exists(destination))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backup));
                    File.Copy(destination, backup, true);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, true);
                Log(logPath, "Diperbarui: " + relative);
            }

            string installedVersion = ReadManifestVersion(Path.Combine(extensionPath, "manifest.json"));
            if (!string.Equals(installedVersion, TargetVersion, StringComparison.Ordinal))
                throw new InvalidOperationException("Verifikasi versi gagal. Manifest terbaca: " + (installedVersion ?? "null"));

            TryDeleteDirectory(temp);
            Log(logPath, "Hotfix selesai. Versi terpasang: " + installedVersion);

            if (!silent)
            {
                MessageBox.Show(
                    "PTSP Assistant v" + TargetVersion + " berhasil dipasang.\n\n" +
                    "Buka chrome://extensions lalu tekan Reload pada PTSP Assistant, atau mulai ulang Chrome.",
                    "PTSP Assistant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Log(logPath, "GAGAL: " + ex);
            if (!silent)
            {
                MessageBox.Show(
                    "Pembaruan PTSP Assistant gagal.\n\n" + ex.Message + "\n\nLog: " + logPath,
                    "PTSP Assistant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return 1;
        }
    }

    private static string FindExtensionPath(string root)
    {
        var candidates = new List<string>
        {
            Path.Combine(root, "Extension"),
            Path.Combine(root, "extension")
        };

        if (Directory.Exists(root))
        {
            candidates.AddRange(Directory.GetDirectories(root, "Extension-v*")
                .OrderByDescending(Directory.GetLastWriteTimeUtc));
            candidates.AddRange(Directory.GetDirectories(root, "extension-v*")
                .OrderByDescending(Directory.GetLastWriteTimeUtc));
        }

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(Path.Combine(candidate, "manifest.json"))
                && File.Exists(Path.Combine(candidate, "background.js")))
                return candidate;
        }
        return null;
    }

    private static string ReadManifestVersion(string path)
    {
        if (!File.Exists(path)) return null;
        string text = File.ReadAllText(path, Encoding.UTF8);
        Match match = Regex.Match(text, "\\\"version\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static int CompareVersions(string left, string right)
    {
        int[] a = ParseVersion(left);
        int[] b = ParseVersion(right);
        int length = Math.Max(a.Length, b.Length);
        for (int i = 0; i < length; i++)
        {
            int av = i < a.Length ? a[i] : 0;
            int bv = i < b.Length ? b[i] : 0;
            if (av != bv) return av.CompareTo(bv);
        }
        return 0;
    }

    private static int[] ParseVersion(string value)
    {
        return (value ?? "0").Split('.')
            .Select(part => { int n; return int.TryParse(part, out n) ? n : 0; })
            .ToArray();
    }

    private static string Sanitize(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
        return value;
    }

    private static void TryDeleteDirectory(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
    }

    private static void Log(string path, string message)
    {
        File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine, Encoding.UTF8);
    }
}
