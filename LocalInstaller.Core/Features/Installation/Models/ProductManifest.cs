namespace LocalInstaller.Core.Features.Installation.Models;

public sealed partial record ProductManifest(
    string ProductName,
    string Slug,
    string EnvironmentPrefix)
{
    public IReadOnlyList<ProductComponent> Components { get; init; } = [];
    public IReadOnlyList<ProductComponent> InstallerOptions { get; init; } = [];
    public string? Manufacturer { get; init; }
    public string? WindowsUpgradeCode { get; init; }

    public string ServiceName => $"{Slug}-server";
    public string CliCommandName => Slug;
    public string FakeInstallerVariable => $"{EnvironmentPrefix}_INSTALLER_FAKE";
    public string PayloadRootVariable => $"{EnvironmentPrefix}_INSTALLER_PAYLOAD_ROOT";

    public string DefaultInstallRoot()
    {
        if (OperatingSystem.IsWindows())
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (string.IsNullOrWhiteSpace(programFiles))
                programFiles = @"C:\Program Files";
            return System.IO.Path.Join(programFiles, ProductName);
        }

        if (OperatingSystem.IsMacOS())
            return $"/Applications/{ProductName}.app";

        return $"/opt/{Slug}";
    }

    public IReadOnlyList<ProductComponent> InstallableComponents
        => InstallerOptions.Count == 0 ? Components : InstallerOptions;

    /// <summary>
    /// Extra environment variables the Server component's manifest declared, applied when the platform
    /// adapter registers the native service. Empty when the Server component declares none.
    /// </summary>
    public IReadOnlyDictionary<string, string> ServerEnvironmentVariables
        => InstallableComponents.FirstOrDefault(component => component.Target == InstallerComponentTarget.Server)
               ?.EnvironmentVariables
           ?? new Dictionary<string, string>();
}
