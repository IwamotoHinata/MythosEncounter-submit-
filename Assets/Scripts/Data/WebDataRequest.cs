using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using Scenes.Ingame.Enemy.Trace;
using Scenes.Ingame.Journal;

public enum DataType
{
    ItemTable = 0,
    SpellTable = 1,
    CharacterTable = 2,
    EnemyTable = 3,
    EnemyAttackTable = 4
}

public enum ItemFormat
{
    id = 0,
    name = 1,
    explaranation = 2,
    category = 3,
    price = 4
}

public enum SpellFormat
{
    id = 0,
    name = 1,
    explaranation = 2,
    unlockExplaranation = 3
}
public enum PlayerFormat
{
    id = 0,
    name = 1,
    careate_date = 2,
    end_date = 3,
    money = 4,
    item = 5,
    enemy = 6,
    epell = 7
}
public enum EnemyFormat
{
    id = 0,
    name = 1,
    hp = 2,
    stamina = 3,
    armor = 4,
    walkSpeed = 5,
    dashSpeed = 6,
    attack = 7,
    actionCooltime = 8,
    hearing = 9,
    visoin = 10,
    spell = 11,
    san = 12,
    feature = 13,
    attackMethod = 14
}
public enum EnemyAttackFormat
{
    id = 0,
    name = 1,
    directDamage = 2,
    bleedingDamage = 3,
    stanTime = 4,
    range = 5,
    attackProbability = 6,
    accuracy = 7,
    speed = 8,
}

public class WebDataRequest : MonoBehaviour
{
    // ?f?[?^?x?[?X??????????????????????URL
    private string[] databaseUrl =
        { "https://igc.deca.jp/mythos-encounter/item-get.php",
          "https://igc.deca.jp/mythos-encounter/spell-get.php",
          "https://igc.deca.jp/mythos-encounter/player-get.php",
          "https://igc.deca.jp/mythos-encounter/enemy-get.php",
          "https://igc.deca.jp/mythos-encounter/enemy-attack-get.php"};
    private const String PLAYERPUTPHP = "https://igc.deca.jp/mythos-encounter/player-put.php";
    private static List<EnemyDataStruct> EnemyDataArrayList = new List<EnemyDataStruct>();
    private static List<ItemDataStruct> ItemDataArrayList = new List<ItemDataStruct>();
    private static List<SpellStruct> SpellDataArrayList = new List<SpellStruct>();
    private static List<PlayerDataStruct> PlayerDataArrayList = new List<PlayerDataStruct>();
    private static List<EnemyAttackStruct> EnemyAttacArrayList = new List<EnemyAttackStruct>();
    private CancellationTokenSource _timeOutToken;
    private CancellationTokenSource _loadSuccessToken;
    private const int TIMEOUTMILISECOND = 10000;//?^?C???A?E?g????10?b(?~???P??)
    private List<string[]>[] DataArrayList;
    private bool debugMode = true;
    public static WebDataRequest instance;
    private Subject<Unit> _endLoad = new Subject<Unit>();
    public IObservable<Unit> OnEndLoad { get { return _endLoad; } }
    public static List<ItemDataStruct> GetItemDataArrayList { get => ItemDataArrayList; }
    public static List<SpellStruct> GetSpellDataArrayList { get => SpellDataArrayList; }
    public static List<PlayerDataStruct> GetPlayerDataArrayList { get => PlayerDataArrayList; }
    public static List<EnemyDataStruct> GetEnemyDataArrayList { get => EnemyDataArrayList; }
    public static List<EnemyAttackStruct> GetEnemyAttacArrayList { get => EnemyAttacArrayList; }
    public static bool OnCompleteLoadData = false;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        _timeOutToken = new CancellationTokenSource();
        _loadSuccessToken = new CancellationTokenSource();
        TimeOutTimer(_loadSuccessToken.Token).Forget();
        GetData(_timeOutToken.Token).Forget();
    }

    private async UniTaskVoid GetData(CancellationToken token)
    {
        Debug.Log($"Table count = {databaseUrl.Length}");

        DataArrayList = new List<string[]>[databaseUrl.Length];

        UnityWebRequest[] request = new UnityWebRequest[databaseUrl.Length];
        //WebRequest??????
        for (int i = 0; i < databaseUrl.Length; i++)
        {
            request[i] = UnityWebRequest.Get(databaseUrl[i]);
        }
        //?f?[?^???????????@
        for (int i = 0; i < databaseUrl.Length; i++)
        {
            await request[i].SendWebRequest();
        }

        //error????
        foreach (var requestResult in request)
        {
            if (requestResult.result == UnityWebRequest.Result.ConnectionError)
            {
                throw new ApplicationException("?T?[?o?[?????????????s????????");
            }
            else if (requestResult.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new ApplicationException("Status 500 ,?T?[?o?[???????f?[?^?????????s????????");
            }
        }

        for (int i = 0; i < databaseUrl.Length; i++)
        {
            DataArrayList[i] = ConvertToArrayListFrom(System.Web.HttpUtility.HtmlDecode(request[i].downloadHandler.text));
        }
        ConvertStringToItemData(DataArrayList[(int)DataType.ItemTable]);
        ConvertStringToSpellData(DataArrayList[(int)DataType.SpellTable]);
        ConvertStringToPlayerData(DataArrayList[(int)DataType.CharacterTable]);
        ConvertStringToEnemyData(DataArrayList[(int)DataType.EnemyTable]);
        ConvertStringToEnemyAttackData(DataArrayList[(int)DataType.EnemyAttackTable]);
        OnCompleteLoadData = true;
        _loadSuccessToken.Cancel();
        _endLoad.OnNext(default);
    }
    public static async UniTaskVoid PutPlayerData(PlayerDataStruct sendData)
    {
        var token = new CancellationTokenSource().Token;

        string jsonData = JsonMapper.ToJson(sendData);
        WWWForm form = new WWWForm();
        form.AddField("user", jsonData);

        using (UnityWebRequest www = UnityWebRequest.Post(PLAYERPUTPHP, form))
        {
            var asyncOp = www.SendWebRequest();

            while (!asyncOp.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    www.Abort();
                    throw new OperationCanceledException();
                }
                await UniTask.Yield(); // ?????I???????????f
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("error: " + www.error);
            }
            else
            {
                Debug.Log($"SEND JSON = {jsonData}");
                Debug.Log("text: " + www.downloadHandler.text);
                PlayerDataStruct user = JsonMapper.ToObject<PlayerDataStruct>(www.downloadHandler.text);
            }
        }
    }
    /// <summary>
    /// ???????????I???????????????^?C???A?E?g???????????????f????
    /// </summary>
    private async UniTaskVoid TimeOutTimer(CancellationToken token)
    {
        await UniTask.Delay(TIMEOUTMILISECOND, cancellationToken: token);
        _timeOutToken.Cancel();
        throw new TimeoutException();
    }
    /// <summary>
    /// ???????????X?v???b?g?V?[?g???e?v?f???z????????
    /// </summary>
    static List<string[]> ConvertToArrayListFrom(string text)
    {
        List<string[]> cardDataStringsList = new List<string[]>();
        text = text.Replace("<br>", "\n");
        StringReader reader = new StringReader(text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            if (line != null)
            {
                string[] elements = line.Split(':');
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] == "\"\"")
                    {
                        continue;
                    }

                    elements[i] = elements[i].TrimStart('"').TrimEnd('"');
                }
                cardDataStringsList.Add(elements);
            }
        }

        return cardDataStringsList;
    }
    /// <summary>
    /// ?z?????f?[?^??EnemyDataStruct???^?????X??????
    /// </summary>
    private void ConvertStringToEnemyData(List<string[]> _dataArray)
    {
        EnemyDataArrayList.Clear();
        EnemyDataStruct inputTempData = new EnemyDataStruct();
        List<TraceType> trace = new List<TraceType>();
        foreach (var dataRecord in _dataArray)
        {
            string[] spell = dataRecord[(int)EnemyFormat.spell].Split(',');
            string[] attachMethod = dataRecord[(int)EnemyFormat.attackMethod].Split(',');

            trace.Clear();
            var traceCode = dataRecord[(int)EnemyFormat.feature];
            traceCode = traceCode.Length == 7 ? traceCode : $"0{traceCode}";
            for (int i = 0; i < traceCode.Length; i++)
            {
                if (traceCode[i].ToString() == "1")
                {
                    trace.Add((TraceType)Enum.ToObject(typeof(TraceType), i));
                }
            }

            inputTempData.EnemyDataSet(
                int.Parse(dataRecord[(int)EnemyFormat.id]),//ID
                dataRecord[(int)EnemyFormat.name],//???O
                int.Parse(dataRecord[(int)EnemyFormat.hp]),//hp
                int.Parse(dataRecord[(int)EnemyFormat.stamina]),//stamina
                int.Parse(dataRecord[(int)EnemyFormat.armor]),//armor
                float.Parse(dataRecord[(int)EnemyFormat.walkSpeed]),//walkSpeed
                float.Parse(dataRecord[(int)EnemyFormat.dashSpeed]),//dashSpeed
                int.Parse(dataRecord[(int)EnemyFormat.attack]),//attack
                int.Parse(dataRecord[(int)EnemyFormat.hearing]),//hearing
                int.Parse(dataRecord[(int)EnemyFormat.visoin]),//vision
                int.Parse(dataRecord[(int)EnemyFormat.actionCooltime]),//actionCooltime
                spell,//spell
                float.Parse(dataRecord[(int)EnemyFormat.san]),//san
                trace.ToArray(),
                attachMethod
                );
            EnemyDataArrayList.Add(inputTempData);
        }
        if (debugMode) Debug.Log($"EnemyDataLoadEnd : {EnemyDataArrayList.Count}");
    }


    private void ConvertStringToEnemyAttackData(List<string[]> _dataArray)
    {
        EnemyAttacArrayList.Clear();
        EnemyAttackStruct inputTempData;
        foreach (var dataRecord in _dataArray)
        {
            inputTempData = new EnemyAttackStruct(
                int.Parse(dataRecord[(int)EnemyAttackFormat.id]),
                dataRecord[(int)EnemyAttackFormat.name],
                int.Parse(dataRecord[(int)EnemyAttackFormat.directDamage]),
                int.Parse(dataRecord[(int)EnemyAttackFormat.bleedingDamage]),
                float.Parse(dataRecord[(int)EnemyAttackFormat.stanTime]),
                float.Parse(dataRecord[(int)EnemyAttackFormat.range]),
                int.Parse(dataRecord[(int)EnemyAttackFormat.attackProbability]),
                int.Parse(dataRecord[(int)EnemyAttackFormat.accuracy]),
                float.Parse(dataRecord[(int)EnemyAttackFormat.speed]));
            EnemyAttacArrayList.Add(inputTempData);
        }
        if (debugMode) Debug.Log($"EnemyAttackLoadEnd : {EnemyDataArrayList.Count}");
    }


    /// <summary>
    /// ?z?????f?[?^??ItemDataStruct???^?????X??????
    /// </summary>
    private void ConvertStringToItemData(List<string[]> _dataArray)
    {
        ItemDataArrayList.Clear();
        ItemDataStruct inputTempData = new ItemDataStruct();
        ItemCategory _itemCategory = new ItemCategory();
        foreach (var dataRecord in _dataArray)
        {
            switch (dataRecord[(int)ItemFormat.category])
            {
                case "shop":
                    _itemCategory = ItemCategory.Shop;
                    break;
                case "stage":
                    _itemCategory = ItemCategory.Stage;
                    break;
                case "unique":
                    _itemCategory = ItemCategory.Unique;
                    break;
                default:
                    break;
            }
            inputTempData.ItemDataSet(
                int.Parse(dataRecord[(int)ItemFormat.id]),//ID
                dataRecord[(int)ItemFormat.name],//???O
                dataRecord[(int)ItemFormat.explaranation],//????
                _itemCategory,//category
                int.Parse(dataRecord[(int)ItemFormat.price])//price
                );
            ItemDataArrayList.Add(inputTempData);
        }
        if (debugMode) Debug.Log($"ItemDataLoadEnd : {ItemDataArrayList.Count}");
    }
    private void ConvertStringToSpellData(List<string[]> _dataArray)
    {
        SpellDataArrayList.Clear();
        SpellStruct inputTempData = new SpellStruct();
        foreach (var dataRecord in _dataArray)
        {
            inputTempData.SpellDataSet(
                int.Parse(dataRecord[(int)SpellFormat.id]),//ID
                dataRecord[(int)SpellFormat.name],//???O
                dataRecord[(int)SpellFormat.explaranation],//????
                dataRecord[(int)SpellFormat.unlockExplaranation]//????
                );
            SpellDataArrayList.Add(inputTempData);
        }
        if (debugMode) Debug.Log($"SpellDataLoadEnd : {SpellDataArrayList.Count}");
    }
    /// <summary>
    /// ?z?????f?[?^??PlayerDataStruct???^?????X??????
    /// </summary>
    private void ConvertStringToPlayerData(List<string[]> _dataArray)
    {
        PlayerDataArrayList.Clear();
        PlayerDataStruct inputTempData = new PlayerDataStruct();
        foreach (var dataRecord in _dataArray)
        {
            string[] items = dataRecord[(int)PlayerFormat.item].Split(',');
            string[] spell = dataRecord[(int)PlayerFormat.epell].Split(',');
            string[] enemy = dataRecord[(int)PlayerFormat.enemy].Split(',');

            inputTempData.PlayerDataSet(
                int.Parse(dataRecord[(int)PlayerFormat.id]),//ID
                dataRecord[(int)PlayerFormat.name],//???O
                DateTime.Parse(dataRecord[(int)PlayerFormat.careate_date]),//createdDate
                DateTime.Parse(dataRecord[(int)PlayerFormat.end_date]),//endDate
                int.Parse(dataRecord[(int)PlayerFormat.money]),//money
                items,//items
                enemy,//mythPoint
                spell,//spell
                enemy.Length//escape
                );
            PlayerDataArrayList.Add(inputTempData);
        }
        if (debugMode) Debug.Log($"PlayerDataLoadEnd : {PlayerDataArrayList.Count}");
    }
    private void OnDestroy()
    {
        _loadSuccessToken.Cancel();
        _loadSuccessToken.Dispose();
        _timeOutToken.Cancel();
        _timeOutToken.Dispose();
    }
}
