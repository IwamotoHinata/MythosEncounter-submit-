using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

namespace Network.HostMigration
{
    public class HostMigrationObservable : NetworkBehaviour
    {
        [Networked] [HideInInspector] public string token { get; set; } = "None";//オブジェクトの所有者を設定する
        [Networked] [HideInInspector] public Vector3 position { get; set; } //Transformを記録する
        [Networked] [HideInInspector] public Quaternion rotation { get; set; }

        public override void Spawned()
        {
            if (!Runner.IsServer) return;

            //HostMigrationでプレイヤーが復元された場合、PlayerJoinedなどで復元されたプレイヤーは削除する
            if (!Runner.IsResume) //HostMigration時ではない
            {
                //プレイヤーオブジェクトがすでにある場合（既存プレイヤー）は削除
                var connectionToken = new Guid(Runner.GetPlayerConnectionToken(Object.InputAuthority)).GetHashCode().ToString();
                if (HostMigrationHandler.Instance.ResumePlayerDict.ContainsKey(connectionToken))
                {
                    Runner.Despawn(Object);
                }
                else //これが初めてのプレイヤーオブジェクトである場合（新規プレイヤー）はRunnerに登録
                {
                    if (!Runner.TryGetPlayerObject(Object.InputAuthority, out var _))
                        Runner.SetPlayerObject(Object.InputAuthority, Object);

                    //接続トークンを設定する
                    if (Object.InputAuthority == Runner.LocalPlayer) token = "Host";
                    else token = connectionToken;
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Runner.IsServer) return;

            position = transform.position;
            rotation = transform.rotation;
        }
    }
}
