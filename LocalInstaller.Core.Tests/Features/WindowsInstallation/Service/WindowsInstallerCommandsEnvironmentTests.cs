using LocalInstaller.Core.Features.WindowsInstallation.Models;
using LocalInstaller.Core.Features.WindowsInstallation.Services;

namespace LocalInstaller.Core.Tests.Features.WindowsInstallation.Service;

[TestFixture]
public class WindowsInstallerCommandsEnvironmentTests
{
    [Test]
    public void ServiceEnvironmentPowerShell_returnsNullWhenManifestDeclaresNoEnvironmentVariables()
    {
        var manifest = Manifest(null);

        Assert.That(WindowsInstallerCommands.ServiceEnvironmentPowerShell(manifest), Is.Null);
    }

    [Test]
    public void ServiceEnvironmentPowerShell_writesRegistryEnvironmentValueWhenDeclared()
    {
        var manifest = Manifest(new Dictionary<string, string> { ["EXAMPLE_FLAG"] = "true" });

        var script = WindowsInstallerCommands.ServiceEnvironmentPowerShell(manifest);

        Assert.That(script, Is.Not.Null);
        Assert.That(script, Does.Contain($"HKLM:\\SYSTEM\\CurrentControlSet\\Services\\{manifest.ServiceName}"));
        Assert.That(script, Does.Contain("-Name Environment"));
        Assert.That(script, Does.Contain("-PropertyType MultiString"));
        Assert.That(script, Does.Contain("EXAMPLE_FLAG=true"));
    }

    private static WindowsInstallerManifest Manifest(IReadOnlyDictionary<string, string>? environmentVariables)
        => new(
            ProductName: "Acme Studio",
            Manufacturer: "Acme Labs",
            Version: "1.0.0",
            UpgradeCode: "16357A7B-6C17-4635-85D5-28E74D12F4F3",
            ServiceName: "acme-studio-server",
            CliShimName: "acme-studio.cmd",
            BundleName: "Acme Studio",
            ServerUrl: "http://127.0.0.1:5000",
            EnvironmentVariables: environmentVariables);
}
