using LocalInstaller.Core.Features.Installation.Models;
using LocalInstaller.Core.Features.MacOsInstallation.Models;

namespace LocalInstaller.Core.Tests.Features.MacOsInstallation.Model;

[TestFixture]
public class MacOsInstallerManifestEnvironmentVariablesTests
{
    [Test]
    public void LaunchDaemonPlist_includesServerComponentEnvironmentVariables()
    {
        var product = new ProductManifest("Acme Studio", "acme-studio", "ACMESTUDIO")
        {
            Components =
            [
                ProductComponent.Server with
                {
                    EnvironmentVariables = new Dictionary<string, string> { ["EXAMPLE_FLAG"] = "true" }
                }
            ]
        };

        var manifest = MacOsInstallerManifest.From(product, "1.0.0");
        var plist = new MacOsInstallerPlistGenerator(manifest).LaunchDaemonPlist();

        Assert.That(plist, Does.Contain("<key>EXAMPLE_FLAG</key>"));
        Assert.That(plist, Does.Contain("<string>true</string>"));
    }

    [Test]
    public void LaunchDaemonPlist_omitsExtraEnvironmentVariablesWhenNoneDeclared()
    {
        var product = new ProductManifest("Acme Studio", "acme-studio", "ACMESTUDIO")
        {
            Components = [ProductComponent.Server]
        };

        var manifest = MacOsInstallerManifest.From(product, "1.0.0");
        var plist = new MacOsInstallerPlistGenerator(manifest).LaunchDaemonPlist();

        Assert.That(plist, Does.Contain("ASPNETCORE_URLS"));
        Assert.That(plist, Does.Not.Contain("EXAMPLE_FLAG"));
    }
}
