using System.Threading.Tasks;
using DotNetCampus.Cli.Compiler;
using DotNetCampus.Cli.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace DotNetCampus.Cli.Tests;

/// <summary>
/// 测试子命令（SubCommand）功能，包括二级子命令、多级子命令和嵌套子命令。
/// </summary>
[TestClass]
public class SubcommandTests
{
    private CommandLineParsingOptions Flexible { get; } = CommandLineParsingOptions.Flexible;

    #region 1. 基本子命令测试

    [TestMethod("1.1. 二级子命令匹配")]
    public void BasicSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["remote", "add", "origin", "https://github.com/user/repo.git"];
        string? capturedRemoteName = null;
        string? capturedRemoteUrl = null;
        bool otherHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<GitRemoteAddOptions>(o =>
            {
                capturedRemoteName = o.RemoteName;
                capturedRemoteUrl = o.RemoteUrl;
            })
            .AddHandler<GitRemoteListOptions>(_ => otherHandlerCalled = true)
            .Run();

        // Assert
        Assert.AreEqual("origin", capturedRemoteName);
        Assert.AreEqual("https://github.com/user/repo.git", capturedRemoteUrl);
        Assert.IsFalse(otherHandlerCalled);
    }

    [TestMethod("1.2. 另一个二级子命令匹配")]
    public void AnotherBasicSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["container", "run", "--name", "test-container", "nginx"];
        string? capturedContainerName = null;
        string? capturedImageName = null;
        bool otherHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DockerContainerRunOptions>(o =>
            {
                capturedContainerName = o.ContainerName;
                capturedImageName = o.ImageName;
            })
            .AddHandler<DockerContainerListOptions>(_ => otherHandlerCalled = true)
            .Run();

        // Assert
        Assert.AreEqual("test-container", capturedContainerName);
        Assert.AreEqual("nginx", capturedImageName);
        Assert.IsFalse(otherHandlerCalled);
    }

    [TestMethod("1.3. 单级命令与二级子命令共存")]
    public void SingleCommandAndSubcommand_Coexist()
    {
        // Arrange - 测试主命令
        string[] mainArgs = ["status"];
        bool statusHandlerCalled = false;
        bool subcommandHandlerCalled = false;

        // Act - 执行主命令
        CommandLine.Parse(mainArgs, Flexible)
            .AddHandler<GitStatusOptions>(_ => statusHandlerCalled = true)
            .AddHandler<GitRemoteAddOptions>(_ => subcommandHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsTrue(statusHandlerCalled);
        Assert.IsFalse(subcommandHandlerCalled);

        // Reset
        statusHandlerCalled = false;
        subcommandHandlerCalled = false;

        // Arrange - 测试子命令
        string[] subArgs = ["remote", "add", "origin", "https://example.com"];

        // Act - 执行子命令
        CommandLine.Parse(subArgs, Flexible)
            .AddHandler<GitStatusOptions>(_ => statusHandlerCalled = true)
            .AddHandler<GitRemoteAddOptions>(_ => subcommandHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsFalse(statusHandlerCalled);
        Assert.IsTrue(subcommandHandlerCalled);
    }

    #endregion

    #region 2. 多级子命令测试

    [TestMethod("2.1. 三级子命令匹配")]
    public void ThreeLevelSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["container", "image", "list"];
        bool handlerCalled = false;
        bool otherHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DockerContainerImageListOptions>(_ => handlerCalled = true)
            .AddHandler<DockerContainerRunOptions>(_ => otherHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsTrue(handlerCalled);
        Assert.IsFalse(otherHandlerCalled);
    }

    [TestMethod("2.2. 另一个三级子命令匹配")]
    public void AnotherThreeLevelSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["cluster", "node", "delete", "worker-node-1"];
        string? capturedNodeName = null;
        bool otherHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<KubernetesClusterNodeDeleteOptions>(o =>
            {
                capturedNodeName = o.NodeName;
            })
            .AddHandler<DockerContainerImageListOptions>(_ => otherHandlerCalled = true)
            .Run();

        // Assert
        Assert.AreEqual("worker-node-1", capturedNodeName);
        Assert.IsFalse(otherHandlerCalled);
    }

    [TestMethod("2.3. 四级子命令匹配")]
    public void FourLevelSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["config", "user", "profile", "set", "development"];
        string? capturedProfile = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<ConfigUserProfileSetOptions>(o =>
            {
                capturedProfile = o.ProfileName;
            })
            .Run();

        // Assert
        Assert.AreEqual("development", capturedProfile);
    }

    [TestMethod("2.4. kebab-case 命名的子命令匹配")]
    public void KebabCaseSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["get-info", "user", "123"];
        string? capturedUserId = null;
        bool otherHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<GetInfoUserOptions>(o =>
            {
                capturedUserId = o.UserId;
            })
            .AddHandler<GetInfoSystemOptions>(_ => otherHandlerCalled = true)
            .Run();

        // Assert
        Assert.AreEqual("123", capturedUserId);
        Assert.IsFalse(otherHandlerCalled);
    }

    [TestMethod("2.5. 混合 kebab-case 和普通命名的多级子命令")]
    public void MixedKebabCaseAndNormalSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["user-management", "create-account", "--username", "john", "--email", "john@example.com"];
        string? capturedUsername = null;
        string? capturedEmail = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<UserManagementCreateAccountOptions>(o =>
            {
                capturedUsername = o.Username;
                capturedEmail = o.Email;
            })
            .Run();

        // Assert
        Assert.AreEqual("john", capturedUsername);
        Assert.AreEqual("john@example.com", capturedEmail);
    }

    [TestMethod("2.6. 复杂的 kebab-case 三级子命令")]
    public void ComplexKebabCaseThreeLevelSubcommand_MatchesCorrectly()
    {
        // Arrange
        string[] args = ["cloud-service", "auto-scaling", "set-policy", "scale-up"];
        string? capturedPolicy = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<CloudServiceAutoScalingSetPolicyOptions>(o =>
            {
                capturedPolicy = o.PolicyName;
            })
            .Run();

        // Assert
        Assert.AreEqual("scale-up", capturedPolicy);
    }

    #endregion

    #region 3. 子命令优先级与匹配规则测试

    [TestMethod("3.1. 更具体的子命令优先匹配")]
    public void MoreSpecificSubcommand_TakesPriority()
    {
        // Arrange
        string[] args = ["remote", "add", "origin", "https://example.com"];
        bool genericRemoteHandlerCalled = false;
        bool specificRemoteAddHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<GitRemoteOptions>(_ => genericRemoteHandlerCalled = true)
            .AddHandler<GitRemoteAddOptions>(_ => specificRemoteAddHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsFalse(genericRemoteHandlerCalled);
        Assert.IsTrue(specificRemoteAddHandlerCalled);
    }

    [TestMethod("3.2. 部分匹配子命令的处理")]
    public void PartialSubcommandMatch_MatchesLongestPath()
    {
        // Arrange
        string[] args = ["container", "run", "nginx"];
        bool containerHandlerCalled = false;
        bool containerRunHandlerCalled = false;
        bool containerImageHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DockerContainerOptions>(_ => containerHandlerCalled = true)
            .AddHandler<DockerContainerRunOptions>(_ => containerRunHandlerCalled = true)
            .AddHandler<DockerContainerImageOptions>(_ => containerImageHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsFalse(containerHandlerCalled);
        Assert.IsTrue(containerRunHandlerCalled);
        Assert.IsFalse(containerImageHandlerCalled);
    }

    [TestMethod("3.3. 注册顺序不影响子命令匹配优先级")]
    public void RegistrationOrder_DoesNotAffectSubcommandPriority()
    {
        // Arrange
        string[] args = ["remote", "add", "origin", "https://example.com"];
        bool genericRemoteHandlerCalled = false;
        bool specificRemoteAddHandlerCalled = false;

        // Act - 先注册具体的，再注册通用的
        CommandLine.Parse(args, Flexible)
            .AddHandler<GitRemoteAddOptions>(_ => specificRemoteAddHandlerCalled = true)
            .AddHandler<GitRemoteOptions>(_ => genericRemoteHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsFalse(genericRemoteHandlerCalled);
        Assert.IsTrue(specificRemoteAddHandlerCalled);
    }

    [TestMethod("3.4. 最长路径匹配 - 基本情况")]
    public void LongestPathMatching_BasicCase()
    {
        // Arrange - 测试 "git", "git remote", "git remote add" 的优先级
        string[] args = ["git", "remote", "add", "origin", "https://example.com"];
        bool gitHandlerCalled = false;
        bool gitRemoteHandlerCalled = false;
        bool gitRemoteAddHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<GitBaseOptions>(_ => gitHandlerCalled = true)
            .AddHandler<GitRemoteOptionsNew>(_ => gitRemoteHandlerCalled = true)
            .AddHandler<GitRemoteAddOptionsNew>(_ => gitRemoteAddHandlerCalled = true)
            .Run();

        // Assert - 应该匹配最长的 "git remote add"
        Assert.IsFalse(gitHandlerCalled);
        Assert.IsFalse(gitRemoteHandlerCalled);
        Assert.IsTrue(gitRemoteAddHandlerCalled);
    }

    [TestMethod("3.5. 最长路径匹配 - 复杂情况")]
    public void LongestPathMatching_ComplexCase()
    {
        // Arrange - 测试多个不同长度的命令路径
        string[] args = ["cluster", "config", "set-context", "my-context"];
        bool clusterHandlerCalled = false;
        bool clusterConfigHandlerCalled = false;
        bool clusterConfigSetHandlerCalled = false;
        bool clusterConfigSetContextHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<KubernetesClusterOptions>(_ => clusterHandlerCalled = true)
            .AddHandler<KubernetesClusterConfigOptions>(_ => clusterConfigHandlerCalled = true)
            .AddHandler<KubernetesClusterConfigSetOptions>(_ => clusterConfigSetHandlerCalled = true)
            .AddHandler<KubernetesClusterConfigSetContextOptions>(_ => clusterConfigSetContextHandlerCalled = true)
            .Run();

        // Assert - 应该匹配最长的 "cluster config set-context"
        Assert.IsFalse(clusterHandlerCalled);
        Assert.IsFalse(clusterConfigHandlerCalled);
        Assert.IsFalse(clusterConfigSetHandlerCalled);
        Assert.IsTrue(clusterConfigSetContextHandlerCalled);
    }

    [TestMethod("3.6. 最长路径匹配 - 前缀匹配但非完整匹配")]
    public void LongestPathMatching_PrefixButNotComplete()
    {
        // Arrange - "remote addx" 不应该匹配 "remote add"
        string[] args = ["remote", "addx", "test"];
        bool remoteHandlerCalled = false;
        bool remoteAddHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<GitRemoteOptions>(_ => remoteHandlerCalled = true)
            .AddHandler<GitRemoteNullableAddOptions>(_ => remoteAddHandlerCalled = true)
            .Run();

        // Assert - addx 不能匹配 add，所以只有 remote 是匹配的
        Assert.IsTrue(remoteHandlerCalled);
        Assert.IsFalse(remoteAddHandlerCalled);
    }

    [TestMethod("3.7. 最长路径匹配 - 大小写不敏感")]
    public void LongestPathMatching_CaseInsensitive()
    {
        // Arrange
        string[] args = ["Remote", "ADD", "origin", "https://example.com"];
        bool remoteHandlerCalled = false;
        bool remoteAddHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible) // Flexible 默认大小写不敏感
            .AddHandler<GitRemoteOptions>(_ => remoteHandlerCalled = true)
            .AddHandler<GitRemoteAddOptions>(_ => remoteAddHandlerCalled = true)
            .Run();

        // Assert
        Assert.IsFalse(remoteHandlerCalled);
        Assert.IsTrue(remoteAddHandlerCalled);
    }

    [TestMethod("3.8. 最长路径匹配 - 单个字符差异")]
    public void LongestPathMatching_SingleCharacterDifference()
    {
        // Arrange - 测试命令名称相似但不同的情况
        string[] args = ["config", "users", "list"];
        bool configUserHandlerCalled = false;
        bool configUsersHandlerCalled = false;
        bool configUserListHandlerCalled = false;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<ConfigUserOptions>(_ => configUserHandlerCalled = true)
            .AddHandler<ConfigUsersOptions>(_ => configUsersHandlerCalled = true)
            .AddHandler<ConfigUserListOptions>(_ => configUserListHandlerCalled = true)
            .Run();

        // Assert - 应该匹配 "config users" 而不是其他
        Assert.IsFalse(configUserHandlerCalled);
        Assert.IsTrue(configUsersHandlerCalled);
        Assert.IsFalse(configUserListHandlerCalled);
    }

    #endregion

    #region 4. 子命令参数与选项测试

    [TestMethod("4.1. 子命令带选项参数")]
    public void Subcommand_WithOptions_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["remote", "add", "origin", "https://example.com", "--fetch", "--tags"];
        bool fetchEnabled = false;
        bool tagsEnabled = false;
        string? remoteName = null;
        string? remoteUrl = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<GitRemoteAddOptionsWithFlags>(o =>
            {
                fetchEnabled = o.EnableFetch;
                tagsEnabled = o.EnableTags;
                remoteName = o.RemoteName;
                remoteUrl = o.RemoteUrl;
            })
            .Run();

        // Assert
        Assert.IsTrue(fetchEnabled);
        Assert.IsTrue(tagsEnabled);
        Assert.AreEqual("origin", remoteName);
        Assert.AreEqual("https://example.com", remoteUrl);
    }

    [TestMethod("4.2. 子命令带位置参数和选项混合")]
    public void Subcommand_WithMixedPositionalAndOptions_ParsedCorrectly()
    {
        // Arrange
        string[] args = ["container", "run", "--detach", "--publish", "8080:80", "nginx", "nginx:latest"];
        bool detached = false;
        string? portMapping = null;
        string? containerName = null;
        string? imageName = null;

        // Act
        CommandLine.Parse(args, Flexible)
            .AddHandler<DockerContainerRunOptionsDetailed>(o =>
            {
                detached = o.Detach;
                portMapping = o.Publish;
                containerName = o.ContainerName;
                imageName = o.ImageName;
            })
            .Run();

        // Assert
        Assert.IsTrue(detached);
        Assert.AreEqual("8080:80", portMapping);
        Assert.AreEqual("nginx", containerName);
        Assert.AreEqual("nginx:latest", imageName);
    }

    #endregion

    #region 5. 子命令错误处理测试

    [TestMethod("5.1. 未知子命令抛出异常")]
    public void UnknownSubcommand_ThrowsCommandNameNotFoundException()
    {
        // Arrange
        string[] args = ["unknown", "subcommand"];

        // Act & Assert
        var exception = Assert.ThrowsExactly<CommandNameNotFoundException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<GitRemoteAddOptions>(_ => { })
                .AddHandler<DockerContainerRunOptions>(_ => { })
                .Run();
        });

        // 确认异常包含正确的子命令信息
        Assert.IsTrue(exception.Message.Contains("unknown"));
    }

    [TestMethod("5.2. 子命令缺少必需参数抛出异常")]
    public void Subcommand_MissingRequiredParameter_ThrowsException()
    {
        // Arrange
        string[] args = ["remote", "add"]; // 缺少 remote name 和 URL

        // Act & Assert
        Assert.ThrowsExactly<RequiredPropertyNotAssignedException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<GitRemoteAddOptions>(_ => { })
                .Run();
        });
    }

    [TestMethod("5.3. 部分匹配但无完全匹配的子命令")]
    public void PartialSubcommandMatch_NoExactMatch_ThrowsException()
    {
        // Arrange - "remote" 存在，但 "remote unknown" 不存在
        string[] args = ["remote", "unknown"];

        // Act & Assert
        var exception = Assert.ThrowsExactly<CommandNameNotFoundException>(() =>
        {
            CommandLine.Parse(args, Flexible)
                .AddHandler<GitRemoteAddOptions>(_ => { })
                .AddHandler<GitRemoteListOptions>(_ => { })
                .Run();
        });

        Assert.IsTrue(exception.Message.Contains("remote"));
    }

    #endregion

    #region 6. 异步子命令处理测试

    [TestMethod("6.1. 异步子命令处理")]
    public async Task AsyncSubcommand_ExecutesSuccessfully()
    {
        // Arrange
        string[] args = ["remote", "sync", "origin"];
        string? capturedRemoteName = null;
        bool asyncOperationCompleted = false;

        // Act
        await CommandLine.Parse(args, Flexible)
            .AddHandler<GitRemoteSyncOptions>(async o =>
            {
                await Task.Delay(10); // 模拟异步操作
                capturedRemoteName = o.RemoteName;
                asyncOperationCompleted = true;
                return 0;
            })
            .RunAsync();

        // Assert
        Assert.AreEqual("origin", capturedRemoteName);
        Assert.IsTrue(asyncOperationCompleted);
    }

    [TestMethod("6.2. 混合同步异步子命令处理")]
    public async Task MixedSyncAsyncSubcommands_ExecuteCorrectly()
    {
        // Arrange
        string[] args = ["container", "build", ".", "--tag", "myapp"];
        string? capturedTag = null;
        string? capturedPath = null;
        bool otherHandlerCalled = false;

        // Act
        await CommandLine.Parse(args, Flexible)
            .AddHandler<DockerContainerBuildOptions>(o =>
            {
                capturedPath = o.BuildPath;
                capturedTag = o.Tag;
            })
            .AddHandler<DockerContainerRunOptions>(_ => Task.FromResult(otherHandlerCalled = true))
            .RunAsync();

        // Assert
        Assert.AreEqual(".", capturedPath);
        Assert.AreEqual("myapp", capturedTag);
        Assert.IsFalse(otherHandlerCalled);
    }

    #endregion

    #region 7. ICommandHandler 接口子命令测试

    [TestMethod("7.1. ICommandHandler 接口实现的子命令")]
    public async Task ICommandHandler_Subcommand_ExecutesCorrectly()
    {
        // Arrange
        string[] args = ["service", "start", "web-api"];

        // Act
        int exitCode = await CommandLine.Parse(args, Flexible)
            .AddHandler<ServiceStartCommandHandler>()
            .RunAsync();

        // Assert
        Assert.AreEqual(ServiceStartCommandHandler.ExpectedExitCode, exitCode);
        Assert.IsTrue(ServiceStartCommandHandler.WasHandlerCalled);
        Assert.AreEqual("web-api", ServiceStartCommandHandler.CapturedServiceName);

        // Reset static state for other tests
        ServiceStartCommandHandler.ResetState();
    }

    #endregion
}

#region 测试用数据模型

// Git 相关子命令选项类

[Command("status")]
internal class GitStatusOptions
{
    [Option("short")]
    public bool Short { get; init; }
}

[Command("remote")]
internal class GitRemoteOptions
{
    [Option("verbose")]
    public bool Verbose { get; init; }
}

[Command("remote add")]
internal class GitRemoteAddOptions
{
    [Value(0)]
    public required string RemoteName { get; init; }

    [Value(1)]
    public required string RemoteUrl { get; init; }
}

[Command("remote add")]
internal class GitRemoteNullableAddOptions
{
    [Value(0)]
    public string? RemoteName { get; init; }

    [Value(1)]
    public string? RemoteUrl { get; init; }
}

[Command("remote list")]
internal class GitRemoteListOptions
{
    [Option("verbose")]
    public bool Verbose { get; init; }
}

[Command("remote add")]
internal class GitRemoteAddOptionsWithFlags
{
    [Option("fetch")]
    public bool EnableFetch { get; init; }

    [Option("tags")]
    public bool EnableTags { get; init; }

    [Value(0)]
    public required string RemoteName { get; init; }

    [Value(1)]
    public required string RemoteUrl { get; init; }
}

[Command("remote sync")]
internal class GitRemoteSyncOptions
{
    [Value(0)]
    public required string RemoteName { get; init; }
}

// Docker 相关子命令选项类

[Command("container run")]
internal class DockerContainerRunOptions
{
    [Option("name")]
    public string? ContainerName { get; init; }

    [Value(0)]
    public required string ImageName { get; init; }
}

[Command("container list")]
internal class DockerContainerListOptions
{
    [Option("all")]
    public bool ShowAll { get; init; }
}

[Command("container")]
internal class DockerContainerOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("container image")]
internal class DockerContainerImageOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("container image list")]
internal class DockerContainerImageListOptions
{
    [Option("all")]
    public bool ShowAll { get; init; }
}

[Command("container run")]
internal class DockerContainerRunOptionsDetailed
{
    [Option("detach")]
    public bool Detach { get; init; }

    [Option("publish")]
    public string? Publish { get; init; }

    [Value(0)]
    public required string ContainerName { get; init; }

    [Value(1)]
    public required string ImageName { get; init; }
}

[Command("container build")]
internal class DockerContainerBuildOptions
{
    [Value(0)]
    public required string BuildPath { get; init; }

    [Option("tag")]
    public string? Tag { get; init; }
}

// Kubernetes 相关子命令选项类

[Command("cluster node delete")]
internal class KubernetesClusterNodeDeleteOptions
{
    [Value(0)]
    public required string NodeName { get; init; }
}

// 配置管理相关子命令选项类

[Command("config user profile set")]
internal class ConfigUserProfileSetOptions
{
    [Value(0)]
    public required string ProfileName { get; init; }
}

// 服务管理相关子命令选项类

[Command("service start")]
internal class ServiceStartCommandHandler : ICommandHandler
{
    public static bool WasHandlerCalled { get; private set; }
    public static string? CapturedServiceName { get; private set; }
    public const int ExpectedExitCode = 100;

    [Value(0)]
    public required string ServiceName { get; init; }

    public Task<int> RunAsync()
    {
        WasHandlerCalled = true;
        CapturedServiceName = ServiceName;
        return Task.FromResult(ExpectedExitCode);
    }

    public static void ResetState()
    {
        WasHandlerCalled = false;
        CapturedServiceName = null;
    }
}

// kebab-case 命名相关子命令选项类

[Command("get-info user")]
internal class GetInfoUserOptions
{
    [Value(0)]
    public required string UserId { get; init; }
}

[Command("get-info system")]
internal class GetInfoSystemOptions
{
    [Option("verbose")]
    public bool Verbose { get; init; }
}

[Command("user-management create-account")]
internal class UserManagementCreateAccountOptions
{
    [Option("username")]
    public required string Username { get; init; }

    [Option("email")]
    public required string Email { get; init; }

    [Option("role")]
    public string Role { get; init; } = "user";
}

[Command("cloud-service auto-scaling set-policy")]
internal class CloudServiceAutoScalingSetPolicyOptions
{
    [Value(0)]
    public required string PolicyName { get; init; }

    [Option("min-instances")]
    public int MinInstances { get; init; } = 1;

    [Option("max-instances")]
    public int MaxInstances { get; init; } = 10;
}

// 新增的测试数据模型类 - 用于最长路径匹配测试

[Command("git")]
internal class GitBaseOptions
{
    [Option("version")]
    public bool ShowVersion { get; init; }
}

[Command("cluster")]
internal class KubernetesClusterOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("cluster config")]
internal class KubernetesClusterConfigOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("cluster config set")]
internal class KubernetesClusterConfigSetOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("cluster config set-context")]
internal class KubernetesClusterConfigSetContextOptions
{
    [Value(0)]
    public required string ContextName { get; init; }
}

[Command("config user")]
internal class ConfigUserOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("config users")]
internal class ConfigUsersOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

[Command("config user list")]
internal class ConfigUserListOptions
{
    [Option("help")]
    public bool ShowHelp { get; init; }
}

// 新增用于最长路径匹配测试的 Git 命令类

[Command("git remote")]
internal class GitRemoteOptionsNew
{
    [Option("verbose")]
    public bool Verbose { get; init; }
}

[Command("git remote add")]
internal class GitRemoteAddOptionsNew
{
    [Value(0)]
    public required string RemoteName { get; init; }

    [Value(1)]
    public required string RemoteUrl { get; init; }
}

#endregion
