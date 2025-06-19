using System.Collections;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// ���E�ɂ���P�^�C���ȓ��̏ꏊ�ɏ�ǂ��쐬�������
    /// </summary>
    public class CreateBarricadeMagic : Magic
    {
        private readonly float _tileLength = 5.85f;//�P�^�C���̒���
        private readonly float _wallLength = 5.8f;//�ǂ̍���
        private GameObject _mainCamera;
        private GameObject _barricadePrefab;
        [SerializeField] private GameObject _CreatedBarricade;
        [SerializeField] private bool isCanCreate = false;

        //�����̏����ŕK�v�ȕϐ�
        int _defaultlayerMask;//��v��Layer�ȊO�E�ǂɔ�������悤�ɂ���
        int _floorlayerMask;//���ɂ�����������悤�ɂ���
        RaycastHit _hit;
        RaycastHit _leftHit, _rightHit;

        //�o���P�[�h�̏��
        bool _isCatchCreate = false;
        Vector3 _center;

        //�f�o�b�N�֘A�̕ϐ�
        [Header("�f�o�b�N�֘A")]
        private bool _debugMode = false;
        

        public override void ChangeFieldValue()
        {
            chantTime = 5f;
            consumeSanValue = 10;
            Debug.Log("�������Ă���������FCreateBarricadeMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);
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

            //�U����H������ۂɂ��̃R���[�`����j��              
            if (cancelMagic == true)
            {
                cancelMagic = false;
                yield break;
            }

            Debug.Log("cancelMagic:" + cancelMagic);
            yield break;
        }

        /// <summary>
        /// �ǂ̃v���r���[���[�h�ɂ���
        /// </summary>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_SettingPreview()
        {
            startTime = Time.time;
            _mainCamera = GameObject.FindWithTag("MainCamera").gameObject;
            _barricadePrefab = (GameObject)Resources.Load("Prefab/Magic/Barricade");

            _defaultlayerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Wall");//��v��Layer�ȊO�E�ǂɔ�������悤�ɂ���
            _floorlayerMask = LayerMask.GetMask("Floor");//���ɂ�����������悤�ɂ���

            StartCoroutine(SettingPreview());
        }

        /// <summary>
        /// InputAuthority�����v���C���[�݂̂ɔ����B�ǐ����̃v���r���[��\��
        /// </summary>
        /// <returns></returns>
        private IEnumerator SettingPreview()
        {
            Debug.Log("�v���r���[���[�h");
            _isCatchCreate = false;
            //��ȏ���(�u�N���b�N����Ă��Ȃ��ԁv���u�L�����Z������������Ȃ��ԁv������)
            while (true)
            {
                yield return null;
                Debug.Log("�v���r���[���[�h��");
                if (_debugMode)
                    Debug.Log(Time.time - startTime);

                //���Ɍ�����Ray���΂�
                Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out _hit, _tileLength, _floorlayerMask);
                Debug.DrawRay(_mainCamera.transform.position, _mainCamera.transform.forward * _tileLength, Color.black);

                //���ɂԂ����Ă�����
                if (_hit.collider != null)
                {
                    //���e�n�_���獶�E��Ray���΂��BVector3.up * 0.15f��offset(�O�~�΍�)
                    Physics.Raycast(_hit.point + Vector3.up * 0.15f, this.transform.right * -1, out _leftHit, Mathf.Infinity, _defaultlayerMask);
                    Physics.Raycast(_hit.point + Vector3.up * 0.15f, this.transform.right, out _rightHit, Mathf.Infinity, _defaultlayerMask);

                    Debug.DrawRay(_hit.point + Vector3.up * 0.15f, this.transform.right * -100, Color.red);
                    Debug.DrawRay(_hit.point + Vector3.up * 0.15f, this.transform.right * 100, Color.blue);

                    //���E�ɏ�Q�����������Ƃ�
                    if (_leftHit.collider != null && _rightHit.collider != null)
                    {
                        _center = (_leftHit.point + _rightHit.point) / 2 + new Vector3(0, _wallLength, 0) / 2;
                        isCanCreate = true;

                        //�v���r���[�̍쐬�E�ړ�
                        if (_CreatedBarricade == null)//�v���r���[���Ȃ��Ƃ��͍쐬
                        {
                            _CreatedBarricade = Instantiate(_barricadePrefab, _center, _barricadePrefab.transform.rotation);
                            _CreatedBarricade.GetComponent<BoxCollider>().enabled = false;
                        }
                        else//�v���r���[������Έړ�
                        {
                            _CreatedBarricade.transform.position = _center;
                        }

                        //��ǂ̌����ƃT�C�Y�𒲐�
                        _CreatedBarricade.transform.rotation = this.gameObject.transform.rotation;

                        float distance = Vector3.Distance(_leftHit.point, _rightHit.point);


                        if (Mathf.Abs(_leftHit.point.x - _rightHit.point.x) >= _tileLength / 2)//z��������Player�������Ă���Ƃ�
                            _CreatedBarricade.transform.localScale = new Vector3(distance, _wallLength, 1);
                        else//x��������Player�������Ă���Ƃ�
                            _CreatedBarricade.transform.localScale = new Vector3(distance, _wallLength, 1);

                    }

                    //�����I���̖��߂��o���ꂽ�Ƃ�
                    if (cancelMagic == true)
                    {
                        if (_CreatedBarricade != null)//�v���r���[���쐬����Ă�����j��
                        {
                            Destroy(_CreatedBarricade);
                        }
                        RPC_SetIsCatchCreate();
                        break;
                    }
                        
                    //���������̃L�[�������ꂽ�Ƃ��A�����\�̏�Ԃł����while�����甲���o��(5�b�ȏソ���Ă��邱�Ƃ������̈��)
                    //�����������L�����Z�����ꂽ�Ƃ���������
                    if ((Input.GetMouseButtonDown(0) && isCanCreate && Time.time - startTime >= chantTime))
                    {
                        Debug.Log("�L�����Z��Bool�F" + cancelMagic);
                        //�L�����Z��������������Εǐ���
                        RPC_CreateBarricade(_center, _CreatedBarricade.transform.localScale, _CreatedBarricade.transform.rotation);

                        if (_CreatedBarricade != null)//�v���r���[���쐬����Ă�����j��
                        {
                            Destroy(_CreatedBarricade);
                        }

                        RPC_SetIsCatchCreate();
                        break;
                    }
                }
                else//���ɍŏ���Ray���������Ă��Ȃ������Ƃ�
                {
                    isCanCreate = false;
                    if (_CreatedBarricade != null)//�v���r���[���쐬����Ă�����j��
                    {
                        Destroy(_CreatedBarricade);
                    }
                }
            }
            Debug.Log("SettingPreview()�I��");
        }

        /// <summary>
        /// �z�X�g�Ƀo���P�[�h�̏���`�B���ǂ����
        /// </summary>
        /// <param name="center">���S���W</param>
        /// <param name="Scale">�傫��</param>
        /// <param name="rotation">����</param>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_CreateBarricade(Vector3 center, Vector3 Scale, Quaternion rotation)
        {
            Debug.Log("�Ǎ��܂�");
            //�ǂ̐���
            var barricade = RunnerSpawner.RunnerInstance.Spawn(_barricadePrefab, center, rotation);
            barricade.transform.localScale = Scale;

            //������̏���
            //SAN�l����
            myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

            //�������g���Ȃ��悤�ɂ���
            myPlayerMagic.ChangeCanUseMagicBool(false);

            //���������r���̏I����ʒm
            myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);

            Debug.Log("�㏈������");

        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetIsCatchCreate()
        {
            _isCatchCreate = true;
        }

    }
}
