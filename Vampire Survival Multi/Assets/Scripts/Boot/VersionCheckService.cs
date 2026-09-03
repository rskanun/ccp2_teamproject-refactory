using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class VersionData
{
    public string client_version;
    public bool is_maintenance;
    public string maintenance_msg;
}

public enum VersionCheckResult
{
    Success,
    NeedsUpdate,
    Maintenance,
}

public static class VersionCheckService
{
    private const string CDN_URL = "https://rskanun.github.io/ccp2_teamproject-refactory/version.json";

    public static async UniTask<(VersionCheckResult result, VersionData data)> FetchVersionAsync(CancellationToken ct)
    {
        // 같은 버전을 1분 단위로 묶어 캐시 공유
        long currentTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute;
        string requestUrl = $"{CDN_URL}?v={Application.version}&t={currentTime}";

        using var request = UnityWebRequest.Get(requestUrl);

        // 타임아웃 설정
        request.timeout = 5;

        // 버전 정보 체크
        try
        {
            await request.SendWebRequest().WithCancellation(ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[VersionCheck] 웹 요청 예외: {e.Message}");
            throw new Exception($"네트워크 연결에 실패했습니다: {e.Message}", e);
        }

        // 통신 성공 여부 체크
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[VersionCheck] HTTP 통신 실패: {request.error}");
            throw new Exception($"서버 응답 오류({request.responseCode}): {request.error}");
        }

        // 받은 Json 데이터 역직렬화
        VersionData versionData;
        try
        {
            string verText = request.downloadHandler.text;
            versionData = JsonUtility.FromJson<VersionData>(verText);
        }
        catch (Exception e)
        {
            Debug.LogError($"[VersionCheck] Json 파싱 실패: {e.Message}");
            throw new Exception($"서버 버전 데이터 형식이 올바르지 않습니다.", e);
        }

        // 버전 데이터 유효 확인
        if (versionData == null)
        {
            Debug.LogError("[VersionCheck] 데이터 Null 오류");
            throw new Exception("서버로부터 버전 데이터를 불러올 수 없습니다.");
        }

        // 점검 여부
        if (versionData.is_maintenance)
        {
            return (VersionCheckResult.Maintenance, versionData);
        }

        // 업데이트 필요 여부
        if (versionData.client_version != Application.version)
        {
            return (VersionCheckResult.NeedsUpdate, versionData);
        }

        return (VersionCheckResult.Success, versionData);
    }
}