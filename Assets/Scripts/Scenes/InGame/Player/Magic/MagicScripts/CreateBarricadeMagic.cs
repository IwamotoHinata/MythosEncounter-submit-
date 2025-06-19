using System.Collections;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 視界にある１タイル以内の場所に障壁を作成する呪文
    /// </summary>
    public class CreateBarricadeMagic : Magic
    {
        private readonly float _tileLength = 5.85f;//１タイルの長さ
        private readonly float _wallLength = 5.8f;//壁の高さ
        private GameObject _mainCamera;
        private GameObject _barricadePrefab;
        [SerializeField] private GameObject _CreatedBarricade;
        [SerializeField] private bool isCanCreate = false;

        //呪文の処理で必要な変数
        int _defaultlayerMask;//主要なLayer以外・壁に反応するようにする
        int _floorlayerMask;//床にだけ反応するようにする
        RaycastHit _hit;
        RaycastHit _leftHit, _rightHit;

        //バリケードの情報
        bool _isCatchCreate = false;
        Vector3 _center;

        //デバック関連の変数
        [Header("デバック関連")]
        private bool _debugMode = false;
        

        public override void ChangeFieldValue()
        {
            chantTime = 5f;
            consumeSanValue = 10;
            Debug.Log("装備している呪文名：CreateBarricadeMagic" + "\n装備してる呪文の詠唱時間：" + chantTime + "\n装備してる呪文のSAN値消費量：" + consumeSanValue);
        }

        public override void MagicEffect()
        {
            _isCatchCreate = false;
            StartCoroutine(Magic());
        }

        private IEnumerator Magic()
        {
            _barricadePrefab = (GameObject)Resources.Load("Prefab/Magic/Barricade");

            RPC_SettingPreview();
            yield return new WaitUntil(() => _isCatchCreate);

            //攻撃を食らった際にこのコルーチンを破棄              
            if (cancelMagic == true)
            {
                cancelMagic = false;
                yield break;
            }

            Debug.Log("cancelMagic:" + cancelMagic);
            yield break;
        }

        /// <summary>
        /// 壁のプレビューモードにする
        /// </summary>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_SettingPreview()
        {
            startTime = Time.time;
            _mainCamera = GameObject.FindWithTag("MainCamera").gameObject;
            _barricadePrefab = (GameObject)Resources.Load("Prefab/Magic/Barricade");

            _defaultlayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Wall");//主要なLayer以外・壁に反応するようにする
            _floorlayerMask = LayerMask.GetMask("Floor");//床にだけ反応するようにする

            StartCoroutine(SettingPreview());
        }

        /// <summary>
        /// InputAuthorityを持つプレイヤーのみに発動。壁生成のプレビューを表示
        /// </summary>
        /// <returns></returns>
        private IEnumerator SettingPreview()
        {
            Debug.Log("プレビューモード");
            _isCatchCreate = false;
            //主な処理(「クリックされていない間」かつ「キャンセル処理が入らない間」ずっと)
            while (true)
            {
                yield return null;
                Debug.Log("プレビューモード中");
                if (_debugMode)
                    Debug.Log(Time.time - startTime);

                //床に向けてRayを飛ばす
                Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out _hit, _tileLength, _floorlayerMask);
                Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward * _tileLength, Color.black);

                //床にぶつかっていた時
                if (_hit.collider != null)
                {
                    //着弾地点から左右にRayを飛ばす。Vector3.up * 0.15fはoffset(絨毯対策)
                    Physics.Raycast(_hit.point + Vector3.up * 0.15f, this.transform.right * -1, out _leftHit, Mathf.Infinity, _defaultlayerMask);
                    Physics.Raycast(_hit.point + Vector3.up * 0.15f, this.transform.right, out _rightHit, Mathf.Infinity, _defaultlayerMask);

                    Debug.DrawRay(_hit.point + Vector3.up * 0.15f, this.transform.right * -100, Color.red);
                    Debug.DrawRay(_hit.point + Vector3.up * 0.15f, this.transform.right * 100, Color.blue);

                    //左右に障害物があったとき
                    if (_leftHit.collider != null && _rightHit.collider != null)
                    {
                        _center = (_leftHit.point + _rightHit.point) / 2 + new Vector3(0, _wallLength, 0) / 2;
                        isCanCreate = true;

                        //プレビューの作成・移動
                        if (_CreatedBarricade == null)//プレビューがないときは作成
                        {
                            _CreatedBarricade = Instantiate(_barricadePrefab, _center, _barricadePrefab.transform.rotation);
                            _CreatedBarricade.GetComponent<BoxCollider>().enabled = false;
                        }
                        else//プレビューがあれば移動
                        {
                            _CreatedBarricade.transform.position = _center;
                        }

                        //障壁の向きとサイズを調整
                        _CreatedBarricade.transform.rotation = this.gameObject.transform.rotation;

                        float distance = Vector3.Distance(_leftHit.point, _rightHit.point);


                        if (Mathf.Abs(_leftHit.point.x - _rightHit.point.x) >= _tileLength / 2)//z軸方向にPlayerが向いているとき
                            _CreatedBarricade.transform.localScale = new Vector3(distance, _wallLength, 1);
                        else//x軸方向にPlayerが向いているとき
                            _CreatedBarricade.transform.localScale = new Vector3(distance, _wallLength, 1);

                    }

                    //強制終了の命令が出されたとき
                    if (cancelMagic == true)
                    {
                        if (_CreatedBarricade != null)//プレビューが作成されていたら破壊
                        {
                            Destroy(_CreatedBarricade);
                        }
                        RPC_SetIsCatchCreate();
                        break;
                    }
                        
                    //呪文発動のキーを押されたとき、生成可能の状態であればwhile分から抜け出す(5秒以上たっていることも条件の一つ)
                    //呪文処理をキャンセルされたときも取り消し
                    if ((Input.GetMouseButtonDown(0) && isCanCreate && Time.time - startTime >= chantTime))
                    {
                        Debug.Log("キャンセルBool：" + cancelMagic);
                        //キャンセル処理が無ければ壁生成
                        RPC_CreateBarricade(_center, _CreatedBarricade.transform.localScale, _CreatedBarricade.transform.rotation);

                        if (_CreatedBarricade != null)//プレビューが作成されていたら破壊
                        {
                            Destroy(_CreatedBarricade);
                        }

                        RPC_SetIsCatchCreate();
                        break;
                    }
                }
                else//床に最初のRayが当たっていなかったとき
                {
                    isCanCreate = false;
                    if (_CreatedBarricade != null)//プレビューが作成されていたら破壊
                    {
                        Destroy(_CreatedBarricade);
                    }
                }
            }
            Debug.Log("SettingPreview()終了");
        }

        /// <summary>
        /// ホストにバリケードの情報を伝達し壁を作る
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="Scale">大きさ</param>
        /// <param name="rotation">向き</param>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_CreateBarricade(Vector3 center, Vector3 Scale, Quaternion rotation)
        {
            Debug.Log("壁作ります");
            //壁の生成
            var barricade = RunnerSpawner.RunnerInstance.Spawn(_barricadePrefab, center, rotation);
            barricade.transform.localScale = Scale;

            //生成後の処理
            //SAN値減少
            myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

            //呪文を使えないようにする
            myPlayerMagic.ChangeCanUseMagicBool(false);

            //成功した詠唱の終了を通知
            myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);

            Debug.Log("後処理完了");

        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetIsCatchCreate()
        {
            _isCatchCreate = true;
        }

    }
}
