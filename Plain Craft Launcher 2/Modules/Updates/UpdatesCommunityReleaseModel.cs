using System.Text.Json.Serialization;
using PCL.Core.App.Localization;
using PCL.Core.Utils;
using PCL.Network;
using PCL.Network.Loaders;

namespace PCL;

/// <summary>
/// 社区版自己的更新源：读取本仓库 GitHub Release（tag 固定为 community-latest）里的 update.json。
/// 该文件由 .github/workflows/sync-upstream.yml 在构建时生成，包含版本号、SHA256 与更新说明。
/// 直接走 Release 资源直链而非 GitHub API，避免未授权访问的每小时限额。
/// </summary>
public class UpdatesCommunityReleaseModel : IUpdateSource
{
    private const string ReleaseBaseUrl =
        "https://github.com/a13621173445/PCL-CE/releases/download/community-latest/";

    private const string DefaultAssetUrl = ReleaseBaseUrl + "PCL2_CE_Community.exe";

    private CommunityReleaseInfo _info;

    public string SourceName { get; set; } = "CommunityRelease";

    public bool IsAvailable()
    {
        return true;
    }

    public bool RefreshCache()
    {
        var content = Requester.FetchString(ReleaseBaseUrl + "update.json", RequestParam.WithRetry);
        var info = ModBase.GetJson(content).ToObject<CommunityReleaseInfo>();
        if (info is null || string.IsNullOrWhiteSpace(info.Version))
            throw new Exception("社区版 update.json 内容无效");
        _info = info;
        return true;
    }

    public VersionDataModel GetLatestVersion(UpdateChannel channel, UpdateArch arch)
    {
        var info = EnsureCache();
        return new VersionDataModel
        {
            VersionName = info.Version,
            VersionCode = info.Code,
            Sha256 = info.Sha256,
            Source = SourceName,
            Changelog = string.IsNullOrWhiteSpace(info.Changelog) ? "同步主仓库最新代码。" : info.Changelog
        };
    }

    public bool IsLatest(UpdateChannel channel, UpdateArch arch, SemVer currentVersion, int currentVersionCode)
    {
        return currentVersion >= SemVer.Parse(EnsureCache().Version);
    }

    public VersionAnnouncementDataModel GetAnnouncementList()
    {
        return new VersionAnnouncementDataModel { Content = [] };
    }

    public List<ModLoader.LoaderBase> GetDownloadLoader(UpdateChannel channel, UpdateArch arch, string output)
    {
        var info = EnsureCache();
        var url = string.IsNullOrWhiteSpace(info.Url) ? DefaultAssetUrl : info.Url;
        return
        [
            new LoaderDownload(Lang.Text("Update.Task.DownloadFile"),
                [new DownloadFile([url], output)])
        ];
    }

    private CommunityReleaseInfo EnsureCache()
    {
        if (_info is null)
            RefreshCache();
        return _info;
    }

    private class CommunityReleaseInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }

        [JsonPropertyName("changelog")]
        public string Changelog { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
