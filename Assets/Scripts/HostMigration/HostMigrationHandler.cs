using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

namespace Network.HostMigration
{
    public class HostMigrationHandler
    {
        public static HostMigrationHandler Instance;

        public GameObject RunnerClone; //Runner�̃N���[�����i�[���Ă���
        public Dictionary<string, NetworkObject> ResumePlayerDict = new Dictionary<string, NetworkObject>(); //�����v���C���[���i�[

        /// <summary>
        /// Runner���ċN��������
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="hostMigrationToken"></param>
        public async void RebootRunner(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            //�ڑ��g�[�N���̎擾
            var connectionToken = runner.GetPlayerConnectionToken(runner.LocalPlayer);

            //Runner���~����
            await runner.Shutdown(true, ShutdownReason.HostMigration);

            //RunnerObject�𐶐�����
            var runnerObject = UnityEngine.Object.Instantiate(RunnerClone); //�N���[������Runner�𐶐�
            runnerObject.name = "Runner"; //���O��ύX
            runnerObject.SetActive(true); //Runner��L����

            //Runner���N������
            runner = runnerObject.GetComponent<NetworkRunner>();
            runner.ProvideInput = true; //���͌�����t�^

            var args = new StartGameArgs
            {
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
                ConnectionToken = connectionToken,
            };

            await runner.StartGame(args);
        }

        /// <summary>
        /// �V�z�X�g�����ɌĂяo�����
        /// </summary>
        /// <param name="runner"></param>
        private void HostMigrationResume(NetworkRunner runner)
        {
            ResumePlayerDict.Clear();

            foreach (NetworkObject resumeObj in runner.GetResumeSnapshotNetworkObjects())
            {
                //HostMigrationObservable�R���|�[�l���g�����Ă��Ȃ���΁A�I�u�W�F�N�g�𕜌����Ȃ�
                if (!resumeObj.TryGetComponent<HostMigrationObservable>(out var hostMigrationObservable)) continue;

                //���z�X�g�I�u�W�F�N�g�͕��������Ȃ�
                if (hostMigrationObservable.token == "Host")
                {
                    continue;
                }
                else
                {
                    //�I�u�W�F�N�g�̏�Ԃ𕜌����ăX�|�[������
                    Vector3 position = hostMigrationObservable.position;
                    Quaternion rotation = hostMigrationObservable.rotation;

                    var spawnObj = runner.Spawn(resumeObj, position, rotation, null, (_, obj) =>
                    {
                        obj.CopyStateFrom(resumeObj);
                    });

                    //�v���C���[���L�^����
                    if (hostMigrationObservable.token != "None")
                    {
                        ResumePlayerDict.Add(hostMigrationObservable.token, spawnObj);
                    }
                }
            }
        }
    }
}
