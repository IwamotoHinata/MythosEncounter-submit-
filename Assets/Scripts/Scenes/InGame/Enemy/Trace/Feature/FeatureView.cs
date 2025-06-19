using UnityEngine;
using System.Linq;
using Scenes.Ingame.Player;
using UniRx;
using System;
using Scenes.Ingame.Stage;
using System.Collections;
using EPOOutline;

namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class FeatureView : MonoBehaviour
    {
        GameObject _enemy;//TODO:敵が複数体だった場合の対応
        private const float RANGE = 5;
        private const float NEAR_RANGE = 1;
        private GameObject[] _stageInteracts;
        private GameObject[] nearStageObject = null;
        private GameObject[] moreNearStageObject = null;
        private GameObject interactTarget = null;
        private GameObject nearInteractTarget = null;
        private GameObject _floor;
        private Subject<Unit> _onDestroy = new Subject<Unit>();
        public IObservable<Unit> onDestroy { get => _onDestroy; }
        private ReactiveProperty<StageTile> _stageTile = new ReactiveProperty<StageTile>();
        public IObservable<StageTile> OnStageTileChange { get { return _stageTile; } }
        private Vector3 direction = new Vector3(0, -1, 0);
        private Vector3 direction_wall = new Vector3(1, 0, 0);
        [SerializeField] private GameObject _trackSprite;
        private int randomValue;
        private int traceSelect;
        private int _openInterval = 5;
        private int _maxTraceType = 2;
        private EnemyStatus _enemyStatus;

        public void Init()
        {

            _enemy = GameObject.FindWithTag("Enemy");
            _enemyStatus = _enemy.GetComponent<EnemyStatus>();
            _stageInteracts = GameObject.FindGameObjectsWithTag("StageIntract");
           
            StartCoroutine(FloorCheck());
        }

        IEnumerator FloorCheck()
        {
            LayerMask floorMask = LayerMask.GetMask("Floor");
            RaycastHit hit;
            while (true)
            {
                Ray ray = new Ray(_enemy.transform.position, direction);
                // レイをデバッグ表示
                Debug.DrawRay(ray.origin, ray.direction * 1.0f, Color.red, 2.0f);
                if (Physics.Raycast(ray.origin, ray.direction, out hit, 1.0f, floorMask))
                {
                    _floor = hit.collider.gameObject;
                    _stageTile.Value = _floor.GetComponent<StageTile>();
                }    
                yield return new WaitForSeconds(1f);
            }
        }

        public void Temperature(float change)
        {
            _stageTile.Value.TemperatureChange(change);
            Debug.Log(_stageTile.Value.Temperature);
        }

        public void Msv(int change)
        {
            _stageTile.Value.MsvChange(change);
        }

        public void Breath()
        {
            //敵の状態に応じて呼吸音を変更
            if (_enemyStatus.State == EnemyState.Chase || _enemyStatus.State == EnemyState.Attack)
                SoundManager.Instance.PlaySe("se_blessing00", _enemy.transform.position);
            else
                SoundManager.Instance.PlaySe("se_blessing01", _enemy.transform.position);
        }
        public void Grow()
        {
            if (_enemyStatus.State != EnemyState.Chase && _enemyStatus.State != EnemyState.Attack)
            {
                SoundManager.Instance.PlaySe("se_screaming00", _enemy.transform.position);
            }
        }


        public void TryInteract()
        {
            //5マス,1マスいないのインタラクトできるオブジェクトをそれぞれ格納
            nearStageObject = _stageInteracts.Where(target => Vector3.Distance(target.transform.position, _enemy.transform.position) < RANGE).ToArray();
            moreNearStageObject = _stageInteracts.Where(target => Vector3.Distance(target.transform.position, _enemy.transform.position) < NEAR_RANGE).ToArray();
            if (nearStageObject.Length > 0)
            {
                interactTarget = nearStageObject[UnityEngine.Random.Range(0, nearStageObject.Length)];
                nearInteractTarget = nearStageObject[UnityEngine.Random.Range(0, moreNearStageObject.Length)];
                if (interactTarget.TryGetComponent(out IInteractable act))
                {
                    randomValue = UnityEngine.Random.Range(0, 100);
                    //randomValue < 10 で10%の確率でイントラクトされるよう設定
                    if (interactTarget.TryGetComponent(out StageLight light) && randomValue < 10)
                    {
                        Debug.Log("照明にインタラクトしました");
                        act.Intract(null, true);
                    }
                    else if (interactTarget.TryGetComponent(out StageDoor door_open) && door_open.ReturnIsOpen == false)
                        act.Intract(null, true);
                    //randomValue < 5 で5%の確率でイントラクトされるよう設定
                    if ((nearInteractTarget.TryGetComponent(out StageDoor door_close) || nearInteractTarget.TryGetComponent(out StageRack Rack)) && randomValue < 5)
                    {
                        Debug.Log("ドアか棚にインタラクトしました");
                        act.Intract(null, true);
                    }
                }
            }
        }
        public void InstanceTrackSprite()
        {
            Debug.Log("動いている");
            //近くの壁を格納
            LayerMask wallMask = LayerMask.GetMask("Wall");
            RaycastHit hit;
            traceSelect = UnityEngine.Random.Range(0, _maxTraceType);
            //敵の左右3以内にある壁を取得
            Vector3 nearWallCheck = new Vector3(_enemy.transform.position.x - 3, _enemy.transform.position.y + 2, _enemy.transform.position.z);
            Ray ray = new Ray(nearWallCheck, direction_wall);
            // レイをデバッグ表示
            #if UNITY_EDITOR
                Debug.DrawRay(ray.origin, ray.direction * 6.0f, Color.red, 2.0f);
            #endif
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 6.0f, wallMask))
            {
                //どこに痕跡をつけるかを決定
                switch (traceSelect)
                {

                    case 0:
                        //壁に傷跡をつける
                        GameObject cutTrackSprite = Instantiate(_trackSprite, hit.point, Quaternion.LookRotation(hit.normal));
                        Debug.Log("壁に痕跡をつけました");
                        break;
                    case 1:
                        //壁に粘液をつける
                        GameObject mucusTrackSprite_Wall = Instantiate(_trackSprite, hit.point, Quaternion.LookRotation(hit.normal));
                        Debug.Log("壁に粘液をつけました");
                        break;
                }
            }
            else
            {
                switch (traceSelect)
                {
                    case 0:
                        //床に痕跡をつける
                        GameObject trackSprite = Instantiate(_trackSprite, _enemy.transform.position, Quaternion.Euler(90, 0, 0));
                        Debug.Log("床に痕跡をつけました");
                        break;
                    case 1:
                        //床に粘液をつける
                        GameObject mucusTrackSprite_Floor = Instantiate(_trackSprite, _enemy.transform.position, Quaternion.Euler(90, 0, 0));
                        Debug.Log("床に粘液をつけました");
                        break;
                }
            }
            Debug.Log(traceSelect);

        }
        private void OnDestroy()
        {
            _onDestroy.OnNext(default);
        }
    }
}