using Scenes.Ingame.Stage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;


namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// マップの視線の通り方とマップのどのあたりをどのくらい確認したかを記録してゆくクラス。敵キャラがマップを認識するのに使用されるクラス。
    /// </summary>
    public class EnemyVisibilityMap : MonoBehaviour
    {
        public List<List<List<VisivilityArea>>> visivilityAreaGrid;//Unityの座標系を優先、一個目がx軸二個目がy軸二個目がz軸のイメージ左下が[0][0]左上が[0][max]
        public float maxVisivilityRange;//この距離を超えているエリアは見えることはないものとする
        public bool debugMode;
        public float gridRange;
        public Vector3 centerPosition;//いちばん左下のグリッドの中央

        //メモ、これら構造体はもはや巨大化しすぎておりクラスにした方が高速であると考えられる


        /// <summary>マス目の位置を2つのbyteで表し疎のマス目までの距離をfoatであらわしている</summary>
        [StructLayout(LayoutKind.Auto)]
        public struct TripleByteAndMonoFloat
        {//位置と距離
            public byte x;
            public byte y;
            public byte z;
            public float range;

            public List<StageDoor> needOpenDoor;
            public List<StageDoor> needCloseDoor;

            public TripleByteAndMonoFloat(byte sX, byte sY, byte sZ, float sRange)
            {
                x = sX;
                y = sY;
                z = sZ;
                range = sRange;
                needOpenDoor = new List<StageDoor>();
                needCloseDoor = new List<StageDoor>();
            }

            public TripleByteAndMonoFloat(byte sX, byte sY, byte sZ, float sRange, List<StageDoor> sNeedOpenDoor, List<StageDoor> sNeedCloseDoor)
            {
                x = sX;
                y = sY;
                z = sZ;
                range = sRange;
                needOpenDoor = sNeedOpenDoor;
                needCloseDoor = sNeedCloseDoor;
            }
        }

        /// <summary>
        /// マス目が何度見られたかをbyteで記録し、このマス目から視線の通るマス目をListで記録している
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct VisivilityArea
        {
            public byte watchNum;//このエリアを見た回数
            public List<TripleByteAndMonoFloat> canVisivleAreaPosition;
            public VisivilityArea(byte sWatchNum)
            {
                watchNum = sWatchNum;
                canVisivleAreaPosition = new List<TripleByteAndMonoFloat>();
            }
            public VisivilityArea(byte sWatchNum, List<TripleByteAndMonoFloat> sDoubleByteAndFloat)
            {
                watchNum = sWatchNum;
                canVisivleAreaPosition = new List<TripleByteAndMonoFloat>(sDoubleByteAndFloat);
            }
        }

        private void Start()
        {
            /*
            GridMake(9,9,5.8f,new Vector3(-46.4f,1,-43.7f));
            MapScan();
            */
        }

        /// <summary>
        /// マス目の集合である二次元Listを作成する。
        /// </summary>
        /// <param name="x">x座標方向にマス目をいくつ並べるか</param>
        /// <param name="z">z座標方向にマス目をいくつ並べるか</param>
        /// <param name="range">この距離以上の視線は通らないものと考えてシミュレートされる距離</param>
        /// <param name="setCenterPosition">左下のマス目の中心位置</param>
        public void GridMake(byte x, byte y, byte z, float range, Vector3 setCenterPosition)
        { //マップを作成。xとzはグリッドの配置数。rangeはグリッドの距離。centerPositionは左下の位置
            if (debugMode) Debug.Log("グリッド作成開始");
            visivilityAreaGrid = new List<List<List<VisivilityArea>>>();
            gridRange = range;
            centerPosition = setCenterPosition;
            for (byte i = 0; i < x; i++)
            { //配列の要素を作成

                List<List<VisivilityArea>> itemy = new List<List<VisivilityArea>>();
                for (byte j = 0; j < y; j++)
                {
                    List<VisivilityArea> itemz = new List<VisivilityArea>();
                    for (byte k = 0; k < z; k++)
                    {

                        if (debugMode) Debug.DrawLine(setCenterPosition + ToVector3(i, j, k) * range, setCenterPosition + ToVector3(i, j, k) * range + ToVector3(0, 10, 0), Color.yellow, 10);//グリッドの位置を表示
                        itemz.Add(new VisivilityArea(0));
                    }
                    itemy.Add(itemz);
                }
                visivilityAreaGrid.Add(itemy);
            }
            if (debugMode) Debug.Log("firstSize(x)" + visivilityAreaGrid.Count());
            if (debugMode) Debug.Log("SecondSize(y)" + visivilityAreaGrid[0].Count());
            if (debugMode) Debug.Log("SecondSize(z)" + visivilityAreaGrid[0][0].Count());
        }

        /// <summary>
        /// マップをスキャンしてマス目同士での視界の通っている情報を決定する
        /// </summary>
        public void MapScan()
        {//マップをスキャンして実際の視界がどのように通っているかを設定
            if (debugMode) Debug.Log("マップスキャン開始");
            //各マス目へとアクセス
            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {

                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {

                        //対象のマスから他のマス目が見えるかを確認
                        for (byte vX = 0; vX < visivilityAreaGrid.Count(); vX++)
                        {
                            for (byte vY = 0; vY < visivilityAreaGrid[0].Count(); vY++)
                            {
                                for (byte vZ = 0; vZ < visivilityAreaGrid[0][0].Count(); vZ++)
                                {
                                    if ((x != vX) || (y != vY) || (z != vZ))
                                    { //自分自身ではない場合                               
                                        float range2 = Mathf.Pow((x - vX) * gridRange, 2) + Mathf.Pow((y - vY) * gridRange, 2) + Mathf.Pow((z - vZ) * gridRange, 2);
                                        if (range2 <= Mathf.Pow(maxVisivilityRange, 2))
                                        { //視界が通るとされる距離でない場合
                                            float range = Mathf.Sqrt(range2);//平方根を求めるのはすごくコストが重いらしいので確実に計算が必要になってからしてます
                                                                             //視界が通るか＝Rayが通るか
                                            bool hit;//ドアとかインタラクトできるのも関連以外
                                            Ray ray = new Ray(centerPosition + ToVector3(x * gridRange, y * gridRange, z * gridRange), ToVector3(vX - x, vY - y, vZ - z));
                                            hit = Physics.Raycast(ray,  range, ~(LayerMask.GetMask("StageIntract", "Player")), QueryTriggerInteraction.Collide);
                                            if (!hit)
                                            { //何にもあたっていなかった場合
                                                if (debugMode) Debug.DrawRay(ray.origin, ray.direction * range, Color.green, 10);
                                                visivilityAreaGrid[x][y][z].canVisivleAreaPosition.Add(new TripleByteAndMonoFloat(vX, vY, vZ, range));//追加
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

            }
            //ここまで来てマップスキャンが終わる
            if (debugMode) Debug.Log("マップのスキャンが完了しました");
        }

        /// <summary>
        /// ドアをスキャンして解放状態でないと視界の通らない判定を作る
        /// </summary>
        public void NeedOpenDoorScan()
        {
            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {
                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {

                        foreach (TripleByteAndMonoFloat visivilityAreaPosition in visivilityAreaGrid[x][y][z].canVisivleAreaPosition)//各マス目ごとの見えるであろうマスにアクセス
                        {

                            float range = Mathf.Sqrt(Mathf.Pow((x - visivilityAreaPosition.x) * gridRange, 2) + Mathf.Pow((y - visivilityAreaPosition.y) * gridRange, 2) + Mathf.Pow((z - visivilityAreaPosition.z) * gridRange, 2));
                            //各エリアのグリッドにアクセス
                            Ray ray = new Ray(centerPosition + ToVector3(x * gridRange, y * gridRange, z * gridRange), ToVector3(visivilityAreaPosition.x - x, visivilityAreaPosition.y - y, visivilityAreaPosition.z - z));
                            foreach (RaycastHit doorHit in Physics.RaycastAll(ray.origin, ray.direction, range, LayerMask.GetMask("StageIntract"), QueryTriggerInteraction.Collide).ToArray<RaycastHit>())
                            {//命中したすべてのドアにアクセス
                                if (doorHit.collider.gameObject.TryGetComponent<StageDoor>(out StageDoor stageDoorCs))
                                {
                                    if (debugMode) Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10);
                                    visivilityAreaPosition.needOpenDoor.Add(stageDoorCs);

                                }
                            }
                        }
                    }
                }


            }

        }

        /// <summary>
        /// ドアをスキャンして閉鎖状態でないと視界の通らない判定を作る
        /// </summary>
        public void NeedCloseDoorScan()
        {
            Debug.Log("閉じていなくてはならないドアをスキャンします");
            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {


                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {
                        foreach (TripleByteAndMonoFloat visivilityAreaPosition in visivilityAreaGrid[x][y][z].canVisivleAreaPosition)//各マス目ごとの見えるであろうマスにアクセス
                        {
                            float range = Mathf.Sqrt(Mathf.Pow((x - visivilityAreaPosition.x) * gridRange, 2) + Mathf.Pow((y - visivilityAreaPosition.y) * gridRange, 2) + Mathf.Pow((z - visivilityAreaPosition.z) * gridRange, 2));
                            //各エリアのグリッドにアクセス
                            Ray ray = new Ray(centerPosition + ToVector3(x * gridRange, y * gridRange, z * gridRange), ToVector3(visivilityAreaPosition.x - x, visivilityAreaPosition.y - y, visivilityAreaPosition.z - z));
                            foreach (RaycastHit doorHit in Physics.RaycastAll(ray.origin, ray.direction, range, LayerMask.GetMask("StageIntract"), QueryTriggerInteraction.Collide).ToArray<RaycastHit>())
                            {//命中したすべてのドアにアクセス
                                if (doorHit.collider.gameObject.TryGetComponent<StageDoor>(out StageDoor stageDoorCs))
                                {
                                    if (debugMode) Debug.DrawRay(ray.origin, ray.direction * range, Color.blue, 10);
                                    visivilityAreaPosition.needCloseDoor.Add(stageDoorCs);

                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("閉じていなくてはならないドアをスキャンが完了しました");
        }


        /// <summary>
        /// 自身のディープコピーを作成して返す
        /// </summary>
        /// <returns>自身のディープコピー</returns>
        public EnemyVisibilityMap DeepCopy()
        {
            if (debugMode) Debug.Log("ディープコピー開始");
            EnemyVisibilityMap copy;
            copy = new EnemyVisibilityMap();
            copy.visivilityAreaGrid = new List<List<List<VisivilityArea>>>();

            foreach (List<List<VisivilityArea>> item in visivilityAreaGrid)//3次元リストをコピー
            {
                List<List<VisivilityArea>> secondVisivilityArea = new List<List<VisivilityArea>>();//二次元配列

                foreach (List<VisivilityArea> item2 in item)
                {

                    List<VisivilityArea> therdVisivilityarea = new List<VisivilityArea>();

                    foreach (VisivilityArea item3 in item2)
                    {

                        List<TripleByteAndMonoFloat> addCanVisivilityAndMonoFloat = new List<TripleByteAndMonoFloat>();

                        foreach (TripleByteAndMonoFloat value in item3.canVisivleAreaPosition)
                        {


                            addCanVisivilityAndMonoFloat.Add(new TripleByteAndMonoFloat(value.x, value.y, value.z, value.range, new List<StageDoor>(value.needOpenDoor), new List<StageDoor>(value.needCloseDoor)));
                        }
                        therdVisivilityarea.Add(new VisivilityArea(item3.watchNum, addCanVisivilityAndMonoFloat));
                    }
                    secondVisivilityArea.Add(therdVisivilityarea);
                }
                copy.visivilityAreaGrid.Add(secondVisivilityArea);//二次元Listを三次元にAddする
            }




            copy.gridRange = gridRange;
            copy.maxVisivilityRange = maxVisivilityRange;
            copy.debugMode = debugMode;
            copy.centerPosition = centerPosition;

            if (debugMode)
            { //マス目の情報が正常にコピーできているかを表示する
                for (byte x = 0; x < copy.visivilityAreaGrid.Count(); x++)
                {
                    for (byte y = 0; y < copy.visivilityAreaGrid[0].Count(); y++)
                    {
                        for (byte z = 0; z < copy.visivilityAreaGrid[0][0].Count(); z++)
                        {
                            Debug.DrawLine(copy.centerPosition + ToVector3(x, y, z) * copy.gridRange, copy.centerPosition + ToVector3(x, y, z) * copy.gridRange + ToVector3(0, 10, 0), Color.green, 10);
                        }
                    }

                }
            }

            if (visivilityAreaGrid.Count() != copy.visivilityAreaGrid.Count()) { Debug.LogWarning("数が違う1"); } else { Debug.Log("数は同じ"); }
            if (visivilityAreaGrid[0].Count() != copy.visivilityAreaGrid[0].Count()) { Debug.LogWarning("数が違う2"); }
            if (visivilityAreaGrid[0][0].Count() != copy.visivilityAreaGrid[0][0].Count()) { Debug.LogWarning("数が違う3"); }
            return copy;

        }

        /// <summary>
        /// 自信のシャローコピーを返す
        /// </summary>
        /// <returns>自身のシャローコピー</returns>
        public EnemyVisibilityMap SharrowCopy() { 
        return (EnemyVisibilityMap)this.MemberwiseClone();
        }




        /// <summary>
        /// 次に確認すべき最も見ておらず最も近い位置を取得。
        /// </summary>
        /// <param name="nowPosition">現在のcharacterの座標</param>
        /// <returns>次に行くべき座標</returns>
        public Vector3 GetNextNearWatchPosition(Vector3 nowPosition)
        {
            if (debugMode) Debug.Log("次の移動先を取得");
            List<byte> nextPositionX = new List<byte>();
            List<byte> nextPositionY = new List<byte>();
            List<byte> nextPositionZ = new List<byte>();
            byte smallestWatchNum = byte.MaxValue;
            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {
                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {
                        if (smallestWatchNum > visivilityAreaGrid[x][y][z].watchNum) { smallestWatchNum = visivilityAreaGrid[x][y][z].watchNum; }
                    }
                }

            }


            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {
                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {
                        if (visivilityAreaGrid[x][y][z].watchNum == smallestWatchNum)
                        { //最も小さい場合
                            nextPositionX.Add(x);
                            nextPositionY.Add(y);
                            nextPositionZ.Add(z);
                        }
                        VisivilityArea newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[x][y][z].watchNum - smallestWatchNum), visivilityAreaGrid[x][y][z].canVisivleAreaPosition); ;
                        visivilityAreaGrid[x][y][z] = newVisivilityArea;
                    }
                }

            }
            //最も近い要素を考える
            float nearDistance = float.MaxValue;
            byte nearPositionX = 0; byte nearPositionY = 0; byte nearPositionZ = 0;//Y方向の評価値を10倍している
            for (int i = 0; i < nextPositionX.Count; i++)
            {
                if (nearDistance > Vector3.Magnitude(nowPosition - (centerPosition + ToVector3(nextPositionX[i], nextPositionY[i] * 10, nextPositionZ[i]) * gridRange)))
                {
                    nearDistance = Vector3.Magnitude(nowPosition - (centerPosition + ToVector3(nextPositionX[i], nextPositionY[i] * 10, nextPositionZ[i]) * gridRange));
                    nearPositionX = nextPositionX[i];
                    nearPositionY = nextPositionY[i];
                    nearPositionZ = nextPositionZ[i];
                }
            }

            //実際に次ぎに行くべき座標を示す
            Vector3 nextPosition = (ToVector3(nearPositionX, nearPositionY, nearPositionZ) * gridRange) + centerPosition;
            if (debugMode)
            {//次に行くべき位置を描画
                Debug.DrawLine(nextPosition, nextPosition + ToVector3(0, 20, 0), Color.magenta, 3);
            }
            return nextPosition;
        }

        /// <summary>
        /// 今いる場所から見れるマス目の見た回数のカウントを増加させる
        /// </summary>
        /// <param name="nowPosition">現在の座標</param>
        /// <param name="visivilityRange">視界の長さ</param>
        public void CheckVisivility(Vector3 nowPosition, float visivilityRange)
        {
            if (debugMode) Debug.Log("視界の通りをチェック");
            VisivilityArea newVisivilityArea;
            if ((nowPosition.x < centerPosition.x + (visivilityAreaGrid.Count - 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < nowPosition.x))
            {//x座標がマップの範囲内であるかどうか

                if ((nowPosition.y < centerPosition.y + (visivilityAreaGrid[0].Count - 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < nowPosition.y))
                {//y座標
                    if ((nowPosition.z < centerPosition.z + (visivilityAreaGrid[0][0].Count - 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < nowPosition.z)) //z座標がマップの範囲内であるかどうか
                    {
                        if (debugMode) Debug.Log("マップの範囲内です");
                        byte myPositionX, myPositionY, myPositionZ;//自分がどこのグリッドにいるかを確認する
                        myPositionX = (byte)Mathf.FloorToInt((float)(nowPosition.x - centerPosition.x + 0.5 * gridRange) / gridRange);
                        myPositionY = (byte)Mathf.FloorToInt((float)(nowPosition.y - centerPosition.y + 0.5 * gridRange) / gridRange);
                        myPositionZ = (byte)Mathf.FloorToInt((float)(nowPosition.z - centerPosition.z + 0.5 * gridRange) / gridRange);
                        foreach (TripleByteAndMonoFloat item in visivilityAreaGrid[myPositionX][myPositionY][myPositionZ].canVisivleAreaPosition)
                        {
                            if (item.range < visivilityRange)
                            { //見える距離
                              //ドアに関連して見える条件にあるか調べる
                                bool noDoor = true;
                                foreach (StageDoor needOpen in item.needOpenDoor) //開いていなければならないドアは開いているかチェック
                                {
                                    if (needOpen.ReturnIsOpen == false)
                                    {
                                        noDoor = false; break;
                                    }
                                }
                                if (noDoor)
                                {
                                    foreach (StageDoor needClose in item.needCloseDoor)//閉じていなければならないドアは閉じているかチェック
                                    {
                                        if (needClose.ReturnIsOpen == true)
                                        {
                                            noDoor = false;
                                            break;
                                        }
                                    }
                                }

                                if (noDoor)
                                {
                                    //見た回数を足す。ただし構造体をListのFor文の中でいじれないのでコピーしていじって書き換える。オーバーフローしない場合
                                    if ((byte)(visivilityAreaGrid[item.x][item.y][item.z].watchNum) < byte.MaxValue)
                                    {
                                        newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[item.x][item.y][item.z].watchNum + 1), visivilityAreaGrid[item.x][item.y][item.z].canVisivleAreaPosition);
                                        visivilityAreaGrid[item.x][item.y][item.z] = newVisivilityArea;
                                    }
                                    if (debugMode)
                                    {//見たエリアを線で表示
                                        Debug.DrawLine(centerPosition + ToVector3(myPositionX, myPositionY, myPositionZ) * gridRange, centerPosition + ToVector3(item.x, item.y, item.z) * gridRange, Color.green, 1f);
                                    }
                                }

                            }
                        }
                        //自分が今いる場所に見た回数を足す。ただし構造体をListのFor文の中でいじれないのでコピーしていじって書き換える
                        if ((byte)(visivilityAreaGrid[myPositionX][myPositionY][myPositionZ].watchNum) < byte.MaxValue)
                        {
                            newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[myPositionX][myPositionY][myPositionZ].watchNum + 1), visivilityAreaGrid[myPositionX][myPositionY][myPositionZ].canVisivleAreaPosition);
                            visivilityAreaGrid[myPositionX][myPositionY][myPositionZ] = newVisivilityArea;
                        }
                    }
                    else
                    {
                        Debug.LogError("z座標がマップからはみ出ています");
                    }
                }
                else
                {
                    Debug.LogError("z座標がマップからはみ出ています");
                }
            }
            else
            {
                Debug.LogError("x座標がマップからはみ出ています");
            }

            if (debugMode)
            { //各マス目がどれだけ見られているかを確認する
                for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
                {
                    for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                    {
                        for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                        {
                            Color drawColor;
                            if (visivilityAreaGrid[x][y][z].watchNum < 25) { drawColor = new Color32((byte)(10 * visivilityAreaGrid[x][y][z].watchNum), 0, (byte)(byte.MaxValue - (10 * visivilityAreaGrid[x][y][z].watchNum)), byte.MaxValue); }
                            else
                            {
                                drawColor = Color.red;
                            }

                            Debug.DrawLine(centerPosition + ToVector3(x, y, z) * gridRange, centerPosition + ToVector3(x, y, z) * gridRange + ToVector3(0, 10, 0), drawColor, 1f);
                        }

                    }

                }
            }
        }

        /// <summary>
        /// 特定の位置から音が聞こえてきた場合の処理
        /// </summary>
        /// <param name="position">音源の座標</param>
        /// <param name="resetRange">音源が存在するであろうという事で対象とする範囲</param>
        /// <param name="periodic">定期的なチェックによって呼び出されたのかどうか</param>
        public void HearingSound(Vector3 position, float resetRange, bool periodic)
        {
            if (debugMode) Debug.Log("特定位置から聞こえてきた音について対処");
            VisivilityArea newVisivilityArea;
            if ((position.x < centerPosition.x + (visivilityAreaGrid.Count - 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < position.x))
            {//x座標がマップの範囲内であるかどうか
                if ((position.y < centerPosition.y + (visivilityAreaGrid[0].Count - 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < position.y))
                {
                    if ((position.z < centerPosition.z + (visivilityAreaGrid[0][0].Count - 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < position.z)) //z座標がマップの範囲内であるかどうか
                    {
                        for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
                        {
                            for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                            {
                                for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                                {
                                    //マスが対象範囲か調べる                          
                                    if (resetRange > Vector3.Magnitude(position - (centerPosition + ToVector3(x, y, z) * gridRange)))
                                    {
                                        //対象内の場合見た回数を0とする
                                        newVisivilityArea = ToVisivilityArea((byte)(0), visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                        visivilityAreaGrid[x][y][z] = newVisivilityArea;
                                        if (debugMode) { DrawCross((centerPosition + ToVector3(x, y, z) * gridRange), 5, Color.magenta, 2f); }

                                    }
                                    else
                                    {
                                        //対象でない場合見た回数を1追加する(何度も音を聞いた場合に最も新しい音を対象とするため)
                                        if (periodic)
                                        {//細かく走りまくることで音のしていないエリアが極端に捜索先にならないようにするグリッチの対策
                                            newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[x][y][z].watchNum + 1), visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                            visivilityAreaGrid[x][y][z] = newVisivilityArea;
                                        }
                                    }
                                }


                            }

                        }
                    }
                    else
                    {
                        Debug.LogError("z座標がマップからはみ出ています");
                    }
                }
                else { Debug.LogError("y座標がマップからはみ出ています"); }


            }
            else
            {
                Debug.LogError("x座標がマップからはみ出ています");
            }
        }

        /// <summary>
        /// プレイヤーの光が見えているかどうかを検出する
        /// </summary>
        /// <param name="enemyPosition">敵の居場所</param>
        /// <param name="playerPosition">プレイヤーの居場所</param>
        /// <param name="visivilityRange">敵の視界の距離</param>
        /// <param name="lightRange">プレイヤーの視界の距離</param>
        /// <param name="NextPosition">参照渡しで最も強い光の見えた位置を返される</param>
        /// <returns>光は見えたかどうか</returns>
        public bool RightCheck(Vector3 enemyPosition, Vector3 playerPosition, float visivilityRange, float lightRange, ref Vector3 NextPosition)
        {
            if (!((enemyPosition.x < centerPosition.x + (visivilityAreaGrid.Count - 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < enemyPosition.x)))
            {
                Debug.LogError("EnemyPosition.xが範囲外です");
                return false;
            }
            if (!((enemyPosition.y < centerPosition.y + (visivilityAreaGrid[0].Count - 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < enemyPosition.y)))
            {
                Debug.LogError("EnemyPosition.yが範囲外です");
                return false;
            }
            if (!((enemyPosition.z < centerPosition.z + (visivilityAreaGrid[0][0].Count - 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < enemyPosition.z)))
            {
                Debug.LogError("EnemyPosition.zが範囲外です");
                return false;
            }
            if (!((playerPosition.x < centerPosition.x + (visivilityAreaGrid.Count - 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < playerPosition.x)))
            {
                Debug.LogError("PlayerPosition.xが範囲外です");
                return false;
            }
            if (!((playerPosition.y < centerPosition.y + (visivilityAreaGrid[0].Count - 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < playerPosition.y)))
            {
                Debug.LogError("PlayerPosition.yが範囲外です");
                return false;
            }
            if (!((playerPosition.z < centerPosition.z + (visivilityAreaGrid[0][0].Count - 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < playerPosition.z)))
            {
                Debug.LogError("EPlayerPosition.zが範囲外です");
                return false;
            }
            //Enemyから見れる可能性のあるマスを取得
            byte enemyGridPositionX, enemyGridPositionY, enemyGridPositionZ;
            enemyGridPositionX = (byte)Mathf.FloorToInt((float)(enemyPosition.x - centerPosition.x + 0.5 * gridRange) / gridRange);
            enemyGridPositionY = (byte)Mathf.FloorToInt((float)(enemyPosition.y - centerPosition.y + 0.5 * gridRange) / gridRange);
            enemyGridPositionZ = (byte)Mathf.FloorToInt((float)(enemyPosition.z - centerPosition.z + 0.5 * gridRange) / gridRange);

            List<TripleByteAndMonoFloat> enemyVisivilityGridPosition = new List<TripleByteAndMonoFloat>();//今してることはこの先において敵からドアの問題なく見えるマスだけを抽出すること

            foreach (TripleByteAndMonoFloat item in visivilityAreaGrid[enemyGridPositionX][enemyGridPositionY][enemyGridPositionZ].canVisivleAreaPosition)
            {
                //ドアに関連して見える条件にあるか調べる
                bool noDoor = true;
                foreach (StageDoor needOpen in item.needOpenDoor) //開いていなければならないドアは開いているかチェック
                {
                    if (needOpen.ReturnIsOpen == false)
                    {
                        noDoor = false; break;
                    }
                }
                if (noDoor)
                {
                    foreach (StageDoor needClose in item.needCloseDoor)//閉じていなければならないドアは閉じているかチェック
                    {
                        if (needClose.ReturnIsOpen == true)
                        {
                            noDoor = false;
                            break;
                        }
                    }
                }
                if (noDoor)
                {
                    enemyVisivilityGridPosition.Add(item);
                }
            }

            if (debugMode)
            {
                for (int e = 0; e < enemyVisivilityGridPosition.Count; e++)
                {
                    if (enemyVisivilityGridPosition[e].range < visivilityRange) Debug.DrawLine((ToVector3(enemyGridPositionX, enemyGridPositionY, enemyGridPositionZ) * gridRange) + centerPosition, (ToVector3(enemyVisivilityGridPosition[e].x, enemyVisivilityGridPosition[e].y, enemyVisivilityGridPosition[e].z) * gridRange) + centerPosition, Color.green, 1f);
                }
            }
            //光が届く可能性のあるマスを取得
            byte rightGridPositionX, rightGridPositionY, rightGridPositionZ;
            rightGridPositionX = (byte)Mathf.FloorToInt((float)(playerPosition.x - centerPosition.x + 0.5 * gridRange) / gridRange);
            rightGridPositionY = (byte)Mathf.FloorToInt((float)(playerPosition.y - centerPosition.y + 0.5 * gridRange) / gridRange);
            rightGridPositionZ = (byte)Mathf.FloorToInt((float)(playerPosition.z - centerPosition.z + 0.5 * gridRange) / gridRange);
            List<TripleByteAndMonoFloat> rightingGridPosition = new List<TripleByteAndMonoFloat>();//今してることはこの先においてプレイヤーからドアの問題なく見えるマスだけを抽出すること
            foreach (TripleByteAndMonoFloat item in visivilityAreaGrid[rightGridPositionX][rightGridPositionY][rightGridPositionZ].canVisivleAreaPosition)
            {
                //ドアに関連して見える条件にあるか調べる
                bool noDoor = true;
                foreach (StageDoor needOpen in item.needOpenDoor) //開いていなければならないドアは開いているかチェック
                {
                    if (needOpen.ReturnIsOpen == false)
                    {
                        noDoor = false; break;
                    }
                }
                if (noDoor)
                {
                    foreach (StageDoor needClose in item.needCloseDoor)//閉じていなければならないドアは閉じているかチェック
                    {
                        if (needClose.ReturnIsOpen == true)
                        {
                            noDoor = false;
                            break;
                        }
                    }
                }
                if (noDoor)
                {
                    rightingGridPosition.Add(item);
                }
            }


            if (debugMode)//光を描画
            {
                for (int r = 0; r < rightingGridPosition.Count; r++)
                {
                    if (rightingGridPosition[r].range < lightRange) { Debug.DrawLine((ToVector3(rightGridPositionX, rightGridPositionY, rightGridPositionZ) * gridRange) + centerPosition, (ToVector3(rightingGridPosition[r].x, rightingGridPosition[r].y, rightingGridPosition[r].z) * gridRange) + centerPosition, Color.yellow, 1f); }
                    
                }
            }







            //見ることのできる最も明るいマスを決定
            bool canLookLight = false;
            byte mostShiningGridPositionX = 0, mostShiningGridPositionY = 0, mostShiningGridPositionZ = 0;
            float shining = 0;
            for (int e = 0; e < enemyVisivilityGridPosition.Count; e++)
            {
                for (int r = 0; r < rightingGridPosition.Count; r++)
                {
                    if (enemyVisivilityGridPosition[e].x == rightingGridPosition[r].x && enemyVisivilityGridPosition[e].z == rightingGridPosition[r].z)
                    {//光が届く可能性があり見えているマスを取得
                        if (enemyVisivilityGridPosition[e].range < visivilityRange && rightingGridPosition[r].range < lightRange)
                        { //見える上に光も届く
                            if (debugMode) { DrawCross((ToVector3(rightingGridPosition[r].x, rightingGridPosition[r].y, rightingGridPosition[r].z) * gridRange) + centerPosition, 2, Color.yellow, 1); }
                            if (shining < lightRange - rightingGridPosition[r].range)//最も明るいマスである
                            {
                                mostShiningGridPositionX = rightingGridPosition[r].x;
                                mostShiningGridPositionY = rightingGridPosition[r].y;
                                mostShiningGridPositionZ = rightingGridPosition[r].z;
                                shining = lightRange - rightingGridPosition[r].range;
                                canLookLight = true;
                            }
                        }
                    }
                }
            }

            //情報を返す
            if (canLookLight)
            {
                NextPosition = (ToVector3(mostShiningGridPositionX, mostShiningGridPositionY, mostShiningGridPositionZ) * gridRange) + centerPosition;
                if (debugMode) { DrawCross(NextPosition, 5, Color.yellow, 1); Debug.Log("光が見えた！"); Debug.DrawLine(NextPosition, NextPosition + ToVector3(0, 20, 0), Color.magenta, 3); }
                return true;
            }
            else
            {
                if (debugMode) { Debug.Log("光は見えなかった"); }
                return false;
            }
        }

        /// <summary>
        /// 全てのマス目の見た回数を規定回数変更する
        /// </summary>
        /// <param name="change">変化させる数</param>
        /// /// <param name="plus">足すならtrue、引くならfalse</param>
        public void ChangeEveryGridWatchNum(byte change, bool plus)
        {
            VisivilityArea newVisivilityArea;
            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {
                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {
                        if (plus)
                        {
                            if ((byte)(visivilityAreaGrid[x][y][z].watchNum) < byte.MaxValue - change)
                            {
                                newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[x][y][z].watchNum + change), visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                visivilityAreaGrid[x][y][z] = newVisivilityArea;
                            }
                            else
                            {
                                newVisivilityArea = ToVisivilityArea(byte.MaxValue, visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                visivilityAreaGrid[x][y][z] = newVisivilityArea;
                            }
                        }
                        else
                        {
                            if ((byte)(visivilityAreaGrid[x][y][z].watchNum) < byte.MinValue + change)
                            {
                                newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[x][y][z].watchNum - change), visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                visivilityAreaGrid[x][y][z] = newVisivilityArea;
                            }
                            else
                            {
                                newVisivilityArea = ToVisivilityArea(byte.MinValue, visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                visivilityAreaGrid[x][y][z] = newVisivilityArea;
                            }

                        }
                    }
                }

            }
        }

        /// <summary>
        /// 全てのマス目の見た回数をセットする
        /// </summary>
        /// <param name="num"></param>
        public void SetEveryGridWatchNum(byte num)
        {
            VisivilityArea newVisivilityArea;
            for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
            {
                for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                {
                    for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                    {
                        newVisivilityArea = ToVisivilityArea(num, visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                        visivilityAreaGrid[x][y][z] = newVisivilityArea;
                    }
                }
            }
        }

        /// <summary>
        /// 特定のグリッドの見た回数をセットする
        /// </summary>
        /// <param name="position">マスのある位置</param>
        /// <param name="num">セットする数</param>
        public void SetGridWatchNum(Vector3 position, byte num)
        {
            VisivilityArea newVisivilityArea;
            if (!(position.x < centerPosition.x + (visivilityAreaGrid.Count - 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < position.x))
            {
                Debug.LogError("Position.xが範囲外です");
            }
            if (!(position.y < centerPosition.y + (visivilityAreaGrid[0].Count - 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < position.x))
            {
                Debug.LogError("positionYが範囲外です");
            }
            if (!(position.z < centerPosition.z + (visivilityAreaGrid[0][0].Count - 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < position.z))
            {
                Debug.LogError("Position.zが範囲外です");
            }
            byte gridPositionX, gridPositionY, gridPositionZ;
            gridPositionX = (byte)Mathf.FloorToInt((float)(position.x - centerPosition.x + 0.5 * gridRange) / gridRange);
            gridPositionY = (byte)Mathf.FloorToInt((float)(position.y - centerPosition.y + 0.5 * gridRange) / gridRange);
            gridPositionZ = (byte)Mathf.FloorToInt((float)(position.z - centerPosition.z + 0.5 * gridRange) / gridRange);
            newVisivilityArea = ToVisivilityArea(num, visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].canVisivleAreaPosition);
            visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ] = newVisivilityArea;
        }

        public void ChangeGridWatchNum(Vector3 position, byte num,bool plus) {
            VisivilityArea newVisivilityArea;
            if (!(position.x < centerPosition.x + (visivilityAreaGrid.Count - 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < position.x))
            {
                Debug.LogError("Position.xが範囲外です");
            }
            if (!(position.y < centerPosition.y + (visivilityAreaGrid[0].Count - 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < position.x))
            {
                Debug.LogError("positionYが範囲外です");
            }
            if (!(position.z < centerPosition.z + (visivilityAreaGrid[0][0].Count - 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < position.z))
            {
                Debug.LogError("Position.zが範囲外です");
            }
            byte gridPositionX, gridPositionY, gridPositionZ;
            gridPositionX = (byte)Mathf.FloorToInt((float)(position.x - centerPosition.x + 0.5 * gridRange) / gridRange);
            gridPositionY = (byte)Mathf.FloorToInt((float)(position.y - centerPosition.y + 0.5 * gridRange) / gridRange);
            gridPositionZ = (byte)Mathf.FloorToInt((float)(position.z - centerPosition.z + 0.5 * gridRange) / gridRange);
            if (plus)
            {
                if (visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].watchNum + num <= byte.MaxValue)
                {
                    newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].watchNum + num), visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].canVisivleAreaPosition);
                }
                else 
                {
                    newVisivilityArea = ToVisivilityArea(byte.MaxValue, visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].canVisivleAreaPosition);
                }
            }
            else
            {
                if (visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].watchNum - num >= byte.MinValue)
                {
                    newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].watchNum - num), visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].canVisivleAreaPosition);
                }
                else 
                {
                    newVisivilityArea = ToVisivilityArea(byte.MinValue, visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ].canVisivleAreaPosition);
                }
            }
            visivilityAreaGrid[gridPositionX][gridPositionY][gridPositionZ] = newVisivilityArea;
        }



        /// <summary>
        /// プレイヤーの周辺に最初近づかないようにするために使用
        /// </summary>
        public void DontApproachPlayer()
        {
            Vector3 playerPosition = GameObject.FindWithTag("Player").transform.position;
            if (debugMode) Debug.Log("プレイヤーにスポーン直後接近しないように対処");
            VisivilityArea newVisivilityArea;
            if ((playerPosition.x < centerPosition.x + (visivilityAreaGrid.Count + 0.5) * gridRange) && (centerPosition.x - 0.5 * gridRange < playerPosition.x))
            {//x座標がマップの範囲内であるかどうか
                if ((playerPosition.y < centerPosition.y + (visivilityAreaGrid[0].Count + 0.5) * gridRange) && (centerPosition.y - 0.5 * gridRange < playerPosition.y))
                {
                    if ((playerPosition.z < centerPosition.z + (visivilityAreaGrid[0][0].Count + 0.5) * gridRange) && (centerPosition.z - 0.5 * gridRange < playerPosition.z)) //z座標がマップの範囲内であるかどうか
                    {
                        for (byte x = 0; x < visivilityAreaGrid.Count(); x++)
                        {
                            for (byte y = 0; y < visivilityAreaGrid[0].Count(); y++)
                            {
                                for (byte z = 0; z < visivilityAreaGrid[0][0].Count(); z++)
                                {
                                    //マスが対象範囲(ハードコードで50にしてある)か調べる                          
                                    if (50 > Vector3.Magnitude(playerPosition - (centerPosition + ToVector3(x, y, z) * gridRange)))
                                    {
                                        //対象内の場合見た回数を0とする
                                        newVisivilityArea = ToVisivilityArea((byte)(visivilityAreaGrid[x][y][z].watchNum + 1), visivilityAreaGrid[x][y][z].canVisivleAreaPosition);
                                        visivilityAreaGrid[x][y][z] = newVisivilityArea;
                                        if (debugMode) { DrawCross((centerPosition + ToVector3(x, y, z) * gridRange), 5, Color.magenta, 2f); }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("z座標がマップからはみ出ています");
                    }
                }
                else
                {
                    Debug.LogError("y座標がマップからはみ出ています");
                }
            }
            else
            {
                Debug.LogError("x座標がマップからはみ出ています");
            }

        }

        public int GetYPosition(Vector3 position) {
            return (int)Mathf.FloorToInt((float)(position.y - centerPosition.y + 0.5 * gridRange) / gridRange);
        }


        //###################################
        //便利な関数たち
        //###################################

        Vector2 translation2 = Vector2.zero;
        Vector3 translation3 = Vector3.zero;
        VisivilityArea vA = new VisivilityArea((byte)0);
        private Vector2 ToVector2(float x, float y)
        {
            translation2.x = x;
            translation2.y = y;
            return translation2;
        }
        private Vector3 ToVector3(float x, float y, float z)
        {
            translation3.x = x;
            translation3.y = y;
            translation3.z = z;
            return translation3;
        }
        private VisivilityArea ToVisivilityArea(byte setNum, List<TripleByteAndMonoFloat> setList)
        {
            vA.watchNum = setNum;
            vA.canVisivleAreaPosition = setList;
            return vA;
        }

        private void DrawCross(Vector3 position, float size, Color color, float time)
        {
            Debug.DrawLine(position + ToVector3(size / 2, 0, size / 2), position + ToVector3(-size, 0, -size), color, time);
            Debug.DrawLine(position + ToVector3(-size / 2, 0, size / 2), position + ToVector3(size / 2, 0, -size / 2), color, time);
        }


    }
}