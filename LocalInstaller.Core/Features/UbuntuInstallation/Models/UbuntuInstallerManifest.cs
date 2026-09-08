using LocalInstaller.Core.Features.Installation.Models;

namespace LocalInstaller.Core.Features.UbuntuInstallation.Models;

public sealed partial record UbuntuInstallerManifest(
    string PackageName,
    string ServiceUnitName,
    string CliCommandName,
    string DesktopApplicationName,
    string DesktopExecutableName,
    string ServerExecutableName,
    string CliExecutableName,
    string TrayExecutableName,
    IReadOnlyDictionary<string, string>? EnvironmentVariables = null)
{
    public static UbuntuInstallerManifest ForProduct(ProductManifest manifest)
        => new(
            PackageName: manifest.Slug,
            ServiceUnitName: $"{manifest.ServiceName}.service",
            CliCommandName: manifest.CliCommandName,
            DesktopApplicationName: manifest.ProductName,
            DesktopExecutableName: ExecutableName(manifest, InstallerComponentTarget.Desktop, "desktop"),
            ServerExecutableName: ExecutableName(manifest, InstallerComponentTarget.Server, "server"),
            CliExecutableName: ExecutableName(manifest, InstallerComponentTarget.Cli, "cli"),
            TrayExecutableName: ExecutableName(manifest, InstallerComponentTarget.Tray, "tray"),
            EnvironmentVariables: manifest.ServerEnvironmentVariables);

    /// <summary>
    /// Extra systemd drop-in `Environment=` lines for <see cref="EnvironmentVariables"/>, empty when the
    /// Server manifest declared none. Each assignment is quoted per systemd.exec(5) config quoting rules
    /// so values containing spaces, quotes, or backslashes parse as a single assignment.
    /// </summary>
    public string EnvironmentOverrideConf()
        => string.Concat((EnvironmentVariables ?? new Dictionary<string, string>())
            .Select(pair => $"Environment={SystemdAssignment(pair.Key, pair.Value)}" + Environment.NewLine));

    private static string SystemdAssignment(string key, string value)
    {
        if (value.Any(c => c is '\n' or '\r' || char.IsControl(c)))
            throw new ArgumentException(
                $"Environment variable '{key}' value must not contain control characters.", nameof(value));

        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{key}={escaped}\"";
    }

    public string DesktopEntryText(string executablePath, string version)
    {
        var versionKey = DesktopApplicationName.Replace("-", "").Replace(" ", "");
        return $"""
               [Desktop Entry]
               Type=Application
               Name={DesktopApplicationName}
               Comment={DesktopApplicationName} desktop workspace client
               Exec={executablePath}
               Icon={PackageName}
               Terminal=false
               Categories=Development;
               StartupNotify=true
               StartupWMClass={DesktopExecutableName}
               X-{versionKey}-Version={version}
               """ + Environment.NewLine;
    }

    public string PostInstallScript()
        => $"""
           #!/usr/bin/env bash
           set -e
           mkdir -p /var/lib/{PackageName}
           touch /var/log/{PackageName}-server.log /var/log/{PackageName}-server.err.log
           chmod +x /opt/{PackageName}/desktop/{DesktopExecutableName} /opt/{PackageName}/server/{ServerExecutableName} /opt/{PackageName}/cli/{CliExecutableName}
           systemctl daemon-reload
           systemctl enable --now {ServiceUnitName}
           if command -v update-desktop-database >/dev/null 2>&1; then
             update-desktop-database /usr/share/applications || true
           fi
           """ + Environment.NewLine;

    public string PreRemoveScript()
        => $"""
           #!/usr/bin/env bash
           set -e
           systemctl disable --now {ServiceUnitName} 2>/dev/null || true
           """ + Environment.NewLine;

    public static string PostRemoveScript()
        => """
           #!/usr/bin/env bash
           set -e
           systemctl daemon-reload
           if command -v update-desktop-database >/dev/null 2>&1; then
             update-desktop-database /usr/share/applications || true
           fi
           """ + Environment.NewLine;

    private static string ExecutableName(ProductManifest manifest, InstallerComponentTarget target, string fallback)
        => manifest.InstallableComponents.FirstOrDefault(component => component.Target == target)?.ExecutableName ?? fallback;
}
