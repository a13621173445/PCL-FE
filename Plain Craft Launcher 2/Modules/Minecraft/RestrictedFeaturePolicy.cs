// === 社区版修改：所有地区限制永久放开 ===
// 原版基于 RegionPolicy 判断中国大陆/未知地区，
// 对其限制功能（下载、安装整合包、修复等）。
// 社区版认为：这些功能与地区无关，全部放行。

namespace PCL;

internal static class RestrictedFeaturePolicy
{
    internal static bool IsRestricted => false;
    internal static bool Check(string feature, bool showHint = true) => true;
    internal static void ThrowIfRestricted(string feature) { }
}
