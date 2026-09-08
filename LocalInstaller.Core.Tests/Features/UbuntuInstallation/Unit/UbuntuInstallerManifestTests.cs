using LocalInstaller.Core.Features.UbuntuInstallation.Models;
using LocalInstaller.Core.Tests.Support;

namespace LocalInstaller.Core.Tests.Features.UbuntuInstallation.Unit;

[TestFixture]
public class UbuntuInstallerManifestTests
{
    [Test]
    public void PostInstallScript_registersAndStartsService()
    {
        var script = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()).PostInstallScript();

        Assert.That(script, Does.Contain("systemctl enable --now agent-up-server.service"));
    }

    [Test]
    public void PostInstallScript_doesNotRunInstallCore()
    {
        var script = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()).PostInstallScript();

        Assert.That(script, Does.Not.Contain("--install-core"));
    }

    [Test]
    public void EnvironmentOverrideConf_isEmptyWhenNoVariablesDeclared()
    {
        var conf = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()).EnvironmentOverrideConf();

        Assert.That(conf, Is.Empty);
    }

    [Test]
    public void EnvironmentOverrideConf_quotesValuesContainingSpaces()
    {
        var manifest = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()) with
        {
            EnvironmentVariables = new Dictionary<string, string> { ["EXAMPLE_MESSAGE"] = "hello world" }
        };

        var conf = manifest.EnvironmentOverrideConf();

        Assert.That(conf, Does.Contain("Environment=\"EXAMPLE_MESSAGE=hello world\""));
    }

    [Test]
    public void EnvironmentOverrideConf_doublesEmbeddedBackslashes()
    {
        var manifest = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()) with
        {
            EnvironmentVariables = new Dictionary<string, string> { ["EXAMPLE_PATH"] = @"C:\tools" }
        };

        var conf = manifest.EnvironmentOverrideConf();

        Assert.That(conf, Does.Contain(@"Environment=""EXAMPLE_PATH=C:\\tools"""));
    }

    [Test]
    public void EnvironmentOverrideConf_escapesEmbeddedQuotes()
    {
        var manifest = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()) with
        {
            EnvironmentVariables = new Dictionary<string, string> { ["EXAMPLE_MESSAGE"] = "say \"hi\"" }
        };

        var conf = manifest.EnvironmentOverrideConf();

        const string q = "\"";
        Assert.That(conf, Does.Contain($"Environment={q}EXAMPLE_MESSAGE=say \\{q}hi\\{q}{q}"));
    }

    [Test]
    public void EnvironmentOverrideConf_rejectsValuesContainingNewlines()
    {
        var manifest = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()) with
        {
            EnvironmentVariables = new Dictionary<string, string> { ["EXAMPLE_FLAG"] = "line1\nline2" }
        };

        Assert.That(() => manifest.EnvironmentOverrideConf(), Throws.ArgumentException);
    }

    [Test]
    public void EnvironmentOverrideConf_rejectsKeysContainingNewlines()
    {
        var manifest = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product()) with
        {
            EnvironmentVariables = new Dictionary<string, string> { ["EXAMPLE_FLAG\nSERVICE_ENVIRONMENT"] = "true" }
        };

        Assert.That(() => manifest.EnvironmentOverrideConf(), Throws.ArgumentException);
    }

    [Test]
    public void DesktopEntryText_declaresStartupWmClassForUbuntuTaskbarIcon()
    {
        var text = UbuntuInstallerManifest.ForProduct(AgentUpTestManifests.Product())
            .DesktopEntryText("/opt/agent-up/desktop/AgentUp.Desktop", "1.2.3");

        Assert.That(text, Does.Contain("Icon=agent-up"));
        Assert.That(text, Does.Contain("StartupWMClass=AgentUp.Desktop"));
    }
}
