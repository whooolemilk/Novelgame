using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// MonoBehaviourを継承することでオブジェクトにコンポーネントとして
// アタッチすることができるようになる
public class GameManager : MonoBehaviour
{
    // パラメーター
    // SerializeFieldと書くとprivateなパラメーターでも
    // インスペクター上で値を変更できる
    [SerializeField]
    private Text mainText;
    [SerializeField]
    private Text nameText;

    private const char SEPARATE_MAIN_START = '「';
    private const char SEPARATE_MAIN_END = '」';

    private string _text =
       "!background_sprite=\"background_sprite1\"&みにに「Hello,World!」&みにに「これはテキスト表示のサンプルです」&!background_sprite=\"background_sprite2\"!background_color=\"255,0,255\"&名無し「こんにちは！」";


    private Queue<char> _charQueue;

    [SerializeField]
    private float captionSpeed = 0.2f;

    private const char SEPARATE_PAGE = '&';
    private Queue<string> _pageQueue;

    [SerializeField]
    private GameObject nextPageIcon;

    private const char SEPARATE_COMMAND = '!';
    private const char COMMAND_SEPARATE_PARAM = '=';
    private const string COMMAND_BACKGROUND = "background";
    private const string COMMAND_SPRITE = "_sprite";
    private const string COMMAND_COLOR = "_color";
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private string spritesDirectory = "Sprites/";


    // メソッド
    //文を1文字ごとに区切り、キューに格納したものを返す
    private Queue<char> SeparateString(string str)
    {
        // 文字列をchar型の配列にする = 1文字ごとに区切る
        char[] chars = str.ToCharArray();
        Queue<char> charQueue = new Queue<char>();
        // foreach文で配列charsに格納された文字を全て取り出し
        // キューに加える
        foreach (char c in chars) charQueue.Enqueue(c);
        return charQueue;
    }

    //1文字を出力する
    private bool OutputChar()
    {
        if (_charQueue.Count <= 0)
        {
            nextPageIcon.SetActive(true);
            return false;
        }
        // キューから値を取り出し、キュー内からは削除する
        mainText.text += _charQueue.Dequeue();
        return true;
    }

    // 出力
    private void ReadLine(string text)
    {
        // 最初が「!」だったら
        if (text[0].Equals(SEPARATE_COMMAND))
        {
            ReadCommand(text);
            ShowNextPage();
            return;
        }
        string[] ts = text.Split(SEPARATE_MAIN_START);
        string name = ts[0];
        string main = ts[1].Remove(ts[1].LastIndexOf(SEPARATE_MAIN_END));
        nameText.text = name;
        mainText.text = "";
        _charQueue = SeparateString(main);
        // コルーチンを呼び出す
        StartCoroutine(ShowChars(captionSpeed));
    }

    private void Start()
    {
        Init();
    }


    private IEnumerator ShowChars(float wait)
    {
        // OutputCharメソッドがfalseを返す(=キューが空になる)までループする
        while (OutputChar())
            // wait秒だけ待機
            yield return new WaitForSeconds(wait);
        // コルーチンを抜け出す
        yield break;
    }

    //全文を表示
    private void OutputAllChar()
    {
        // コルーチンをストップ
        StopCoroutine(ShowChars(captionSpeed));
        while (OutputChar()) ;
        // キューが空になるまで表示
        nextPageIcon.SetActive(true);
    }

    //クリックしたときの処理
    private void OnClick()
    {
        if (_charQueue.Count > 0) OutputAllChar();
        else
        {
            if (!ShowNextPage())
                // UnityエディタのPlayモードを終了する
                UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    // MonoBehaviourを継承している場合限定で
    // 毎フレーム呼ばれる 
    private void Update()
    {
        // 左(=0)クリックされたらOnClickメソッドを呼び出し
        if (Input.GetMouseButtonDown(0)) OnClick();
    }

    //文字列を指定した区切り文字ごとに区切り、キューに格納したものを返す
    private Queue<string> SeparateString(string str, char sep)
    {
        string[] strs = str.Split(sep);
        Queue<string> queue = new Queue<string>();
        foreach (string l in strs) queue.Enqueue(l);
        return queue;
    }

    //初期化する
    private void Init()
    {
        _pageQueue = SeparateString(_text, SEPARATE_PAGE);
        ShowNextPage();
    }

    //次のページを表示する
    private bool ShowNextPage()
    {
        if (_pageQueue.Count <= 0) return false;
        // オブジェクトの表示/非表示を設定する
        nextPageIcon.SetActive(false);
        ReadLine(_pageQueue.Dequeue());
        return true;
    }

    //背景の設定
    private void SetBackgroundImage(string cmd, string parameter)
    {
        // 空白を削除し、背景コマンドの文字列も削除する
        cmd = cmd.Replace(" ", "").Replace(COMMAND_BACKGROUND, "");
        // ダブルクォーテーションで囲われた部分だけを取り出す
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        switch (cmd)
        {
            case COMMAND_SPRITE:
                // Resourcesフォルダからスプライトを読み込み、インスタンス化する
                Sprite sp = Instantiate(Resources.Load<Sprite>(spritesDirectory + parameter));
                // 背景画像にインスタンス化したスプライトを設定する
                backgroundImage.sprite = sp;
                break;
            case COMMAND_COLOR:
                // 空白を削除し、カンマで文字を分ける
                string[] ps = parameter.Replace(" ", "").Split(',');
                // 分けた文字列(=引数)が4つ以上あるなら
                if (ps.Length > 3)
                    // 透明度も設定する
                    // 文字列をbyte型に直し、色を作成する
                    backgroundImage.color = new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]),
                                                    byte.Parse(ps[2]), byte.Parse(ps[3]));
                else
                    backgroundImage.color = new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]), byte.Parse(ps[2]), 255);
                break;
        }
    }

    //コマンドの呼び出し
    private void ReadCommand(string cmdLine)
    {
        // 最初の「!」を削除する
        cmdLine = cmdLine.Remove(0, 1);
        Queue<string> cmdQueue = SeparateString(cmdLine, SEPARATE_COMMAND);
        foreach (string cmd in cmdQueue)
        {
            // 「=」で分ける
            string[] cmds = cmd.Split(COMMAND_SEPARATE_PARAM);
            // もし背景コマンドの文字列が含まれていたら
            if (cmds[0].Contains(COMMAND_BACKGROUND))
                SetBackgroundImage(cmds[0], cmds[1]);
        }
    }


}

