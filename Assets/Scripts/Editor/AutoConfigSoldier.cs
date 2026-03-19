#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AutoConfigSoldier : EditorWindow
{
    [MenuItem("Custom Tools/군인(Soldier) 자동 세팅 및 스포너 연결")]
    public static void SetupSoldierAndSpawner()
    {
        // 1. 유저가 선택한 오브젝트 가져오기
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            Debug.LogError("<color=red>[에러]</color> 군인(Soldier)으로 만들 오브젝트를 마우스로 클릭(선택)한 상태에서 이 메뉴를 눌러주세요!");
            EditorUtility.DisplayDialog("선택 확인", "군인으로 만들 캐릭터를 좌측 씬(Hierarchy)에서 먼저 한 번 클릭(선택)한 뒤 마법사를 실행해주세요.", "확인");
            return;
        }

        // 2. 군인 필수 컴포넌트 자동 장착
        BoxCollider2D collider = selectedObj.GetComponent<BoxCollider2D>();
        if (collider == null) collider = selectedObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Soldier soldierScript = selectedObj.GetComponent<Soldier>();
        if (soldierScript == null) soldierScript = selectedObj.AddComponent<Soldier>();

        // 태그 등록 시도 (만약 Project Settings에 soldier 태그가 만들어져 있다면)
        try
        {
            selectedObj.tag = "soldier";
        }
        catch (UnityException)
        {
            Debug.LogWarning("<color=orange>[주의]</color> 'soldier' 태그가 유니티에 등록되지 않아 기본 태그로 둡니다. 가급적 Edit -> Project Settings -> Tags and Layers에서 soldier 태그를 수동으로 추가해주세요!");
        }

        // 3. 프리팹(Prefab)으로 автомати 굽기
        GameObject prefabObj = selectedObj;
        
        // 만약 하이어라키(씬)에 있는 평범한 게임오브젝트라면 프리팹 파일로 저장합니다.
        if (!PrefabUtility.IsPartOfAnyPrefab(selectedObj))
        {
            string localPath = "Assets/soldier_auto_prefab.prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            prefabObj = PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObj, localPath, InteractionMode.UserAction);
            Debug.Log($"<color=green>[프리팹 생성]</color> 군인이 프리팹으로 안전하게 저장되었습니다: {localPath}");
        }
        else 
        {
            // 이미 프리팹이라면 원본 프리팹 에셋을 가져옵니다.
            if (PrefabUtility.GetPrefabAssetType(selectedObj) != PrefabAssetType.NotAPrefab)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(selectedObj) as GameObject;
                if (source != null) prefabObj = source;
            }
        }

        // 4. 스포너 생성 및 연결
        GameObject spawnerObj = GameObject.Find("soldier spawner");
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("soldier spawner");
            // 화면 오른쪽 대기 지점 (대략 x=15, y=-2 부근)
            spawnerObj.transform.position = new Vector3(15f, -2f, 0f);
        }

        SoldierSpawner spawnerScript = spawnerObj.GetComponent<SoldierSpawner>();
        if (spawnerScript == null) spawnerScript = spawnerObj.AddComponent<SoldierSpawner>();

        // 스포너에 방금 완성된 프리팹을 쏙 넣어줍니다!
        spawnerScript.soldierPrefabs = new GameObject[1] { prefabObj };

        EditorUtility.SetDirty(spawnerObj);

        // 완료 알림
        Debug.Log("<color=cyan>[세팅 완료]</color> 군인 컴포넌트 부착부터 스포너 자동 등록까지 순식간에 완료되었습니다! 이제 게임을 재생하세요.");
        EditorUtility.DisplayDialog("완벽합니다!", "군인 프리팹 완성 및 스포너 등록이 자동으로 끝났습니다.\n\n이제 재생(▶) 버튼을 누르고 플레이해보세요!", "확인");
    }
}
#endif
