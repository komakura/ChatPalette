using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

namespace ChatPalette
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EditLinesWindow EditWindow;

        //Dictionary<string, string> dicKeyPairs;
        //Lines辞書(変数を登録する辞書)を登録する辞書
        private Dictionary<string, Dictionary<string, string>> TabsDictionary;
        string strLastDirectoryPath;

        public MainWindow()
        {
            InitializeComponent();
            TabsDictionary = new Dictionary<string, Dictionary<string, string>>();
            lordInit();
        }

        // MainWindowのクローズ処理
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveInit();
            // EditWindowがクローズ出来てなかったなら、閉じる。
            if (EditWindow != null)
                EditWindow.Close();
        }

        #region Sava&Lord関係

        /// <summary>
        /// ファイルを開いて全文を取得する。
        /// </summary>
        /// <param name="strFilePath">開くファイルのパス</param>
        /// <param name="strReturn">ファイルの内容を返す</param>
        /// <returns>ファイルを開いて内容を取得できたか否か</returns>
        public bool loadFileAll(string strFilePath, out string strReturn) 
        {
            strReturn = null;
            try
            {
                //Fileがあるかチェック。
                FileInfo fInfo = new FileInfo(strFilePath);
                if (!fInfo.Exists)
                    return false;

                using (Hnx8.ReadJEnc.FileReader reader = new Hnx8.ReadJEnc.FileReader(fInfo)) 
                {
                    // 判別読み出し実行。判別結果はReadメソッドの戻り値で把握できます
                    Hnx8.ReadJEnc.CharCode c = reader.Read(fInfo);
                    // 戻り値のNameプロパティから文字コード名を取得できます
                    string name = c.Name;
                    // GetEncoding()を呼び出すと、エンコーディングを取得できます
                    System.Text.Encoding enc = c.GetEncoding();
                    // 実際に読み出したテキストは、Textプロパティから取得できます
                    // ※非テキストファイルの場合は、nullが設定されます
                    strReturn = reader.Text;
                }
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("LoadFileAll Error:01\n" + e.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 初期設定読み込み。起動時に呼び出し。
        /// </summary>
        public void lordInit()
        {
            //String.Join("\r\n", File.ReadAllLines(filePath));
            string strSettingAll=null, strFilePath=null;
            DirectoryInfo dirTemp;
            //fileがあるかのチェック
            try
            {
                //現在値から、設定用フォルダを検索&作製
                dirTemp = new DirectoryInfo(Directory.GetCurrentDirectory()+"/Setting");
                if (dirTemp.Exists)
                    strFilePath = dirTemp.FullName + "/Setting.txt";
                
            }
            catch (ArgumentException e) 
            {
                MessageBox.Show("loadInit Error:\n"+e.ToString());
            }

            // Load SettingFile
            if (loadFileAll(strFilePath, out strSettingAll)) // fileあり。
            {
                // 読み込み反映
                foreach (string strSettingLine in strSettingAll.Split("\r\n"))
                {
                    string strRight, strLeft;
                    if (strSettingLine.IndexOf('=') < 0) continue;
                    strLeft = strSettingLine.Substring(0, strSettingLine.IndexOf("="));
                    try
                    {
                        int intTemp;
                        strRight = strSettingLine.Substring(strSettingLine.IndexOf("="));
                        strRight = strRight.TrimStart('=');
                        switch (strLeft)
                        {
                            case "Height":
                                if (Int32.TryParse(strRight, out intTemp))
                                    this.Height = (double)intTemp;
                                break;
                            case "Width":
                                if (Int32.TryParse(strRight, out intTemp))
                                    this.Width = (double)intTemp;
                                break;
                            case "Top":
                                if (Int32.TryParse(strRight, out intTemp))
                                {
                                    if (intTemp < 0) intTemp = 0;
                                    else
                                    {
                                        int intHeightBuff = (int)(SystemParameters.WorkArea.Height - this.Height);
                                        if (intTemp > intHeightBuff) intTemp = intHeightBuff;
                                    }
                                    this.Top = (double)intTemp;
                                }
                                break;
                            case "Left":
                                if (Int32.TryParse(strRight, out intTemp))
                                {
                                    if (intTemp < 0) intTemp = 0;
                                    else
                                    {
                                        int intWidthBuff = (int)(SystemParameters.WorkArea.Width - this.Width);
                                        if (intTemp > intWidthBuff) intTemp = intWidthBuff;
                                    }
                                    this.Left = (double)intTemp;
                                }
                                break;
                            case "LastDirectroy":
                                strLastDirectoryPath = strRight;
                                break;
                        }
                    }
                    catch (ArgumentException e)
                    {
                        MessageBox.Show("LoadInit Error02:\n" + e.ToString());
                    }
                }
            }
            else 
            {
                strLastDirectoryPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }

        /// <summary>
        /// 初期設定を保存する。Close時に呼び出す。
        /// </summary>
        /// <returns></returns>
        public bool saveInit()
        {
            StreamWriter writeSetting=null;
            DirectoryInfo dirTemp; 

            // fold探索
            try
            {
                dirTemp = new DirectoryInfo(Directory.GetCurrentDirectory());
                dirTemp = new DirectoryInfo("Setting");
                if (!dirTemp.Exists)
                    dirTemp.Create();

                try
                {
                    writeSetting = new StreamWriter(dirTemp.FullName + "/Setting.txt", false, Encoding.Default);

                    writeSetting.WriteLine("[Setting]");
                    // 位置と大きさを保存
                    writeSetting.WriteLine("Height=" + this.Height.ToString());
                    writeSetting.WriteLine("Width=" + this.Width.ToString());
                    writeSetting.WriteLine("Top=" + this.Top.ToString());
                    writeSetting.WriteLine("Left=" + this.Left.ToString());

                    // チャットパレットのディレクトリのパスを
                    writeSetting.WriteLine("LastDirectroy=" + strLastDirectoryPath);
                }
                finally
                {
                    if (writeSetting != null)
                        writeSetting.Close();
                }
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("Erro SavaInit:\n" + e.ToString());
                return false;
            }
            return true;
        }

        #endregion

        #region ChatPalette関係
        public void lordChatPaletteFile()
        {
            string strFilePath = null;
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "テキストファイル(.txt)|*.txt|チャットパレットファイル(.json)|*.json|チャットパレットファイル(.cpd)|*.cpd|全てのファイル(*.*)|*.*";
            dlg.FilterIndex = 4;
            if (dlg.ShowDialog() == true)
            {
                this.IsEnabled = false;
                strFilePath = dlg.FileName;
                this.IsEnabled = true;
            }
            if (strFilePath != null)
                try
                {
                    switch (strFilePath.Substring(strFilePath.LastIndexOf('.')))
                    {
                        case ".txt":
                            setChatPaletteFromTextFile(strFilePath);
                            break;
                        case ".json":
                            setChatPaletteFromJsonFile(strFilePath);
                            break;
                        case ".cpd":
                            break;
                        default:
                            break;
                    }
                }
                catch (ArgumentException e)
                {
                    MessageBox.Show("Error LoadChatPaletteFile"+ e.ToString());
                }
        }

        public bool setChatPaletteFromTextFile(string strFilePath) 
        {
            string strFileAll = null;
            if (!loadFileAll(strFilePath, out strFileAll)) 
                return false;

            DeleteAllTabs();
            makeNewTabItem();
            makeLines(strFileAll);
            return true; 
        }

        /// <summary>
        /// Jsonファイルからチャットパレットを読み込む。Tekeyの形式を採用
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public bool setChatPaletteFromJsonFile(string strFilePath)
        {
            string strFileAll=null;
            if (!loadFileAll(strFilePath, out strFileAll))
            {
                MessageBox.Show(strFilePath + "\nFileを開けませんでした。");
                return false;
            }
            DeleteAllTabs();

            SaveData sd = new SaveData();
            sd = JsonSerializer.Deserialize<SaveData>(strFileAll);

            foreach (DataChatPalette varTemp in sd.palettes)  // 1タブの処理
            {
                makeNewTabItem();
                makeLines(varTemp.stock);
                changeLinesDictionaryTabName(getSelectedTabName(), varTemp.label);
                setSelectedTabName(varTemp.label);
            }
            return true;
        }


        public void saveChatPalette()
        {
            string strFilePath = null,strWrite;
            //タブ数を数える。
            int intTabCount= TabControl.Items.Count;
            if (intTabCount == 0) return;
            //タブ内容の容器を作る。
            SaveData sd = new SaveData();
            sd.palettes  = new DataChatPalette[intTabCount];

            // 各タブのデータを格納する。
            for (int i = 0; i < intTabCount; i++) 
            {
                sd.palettes[i] = new DataChatPalette();
                TabItem tabTemp = (TabItem)TabControl.Items[i];
                sd.palettes[i].paletteID = i + 1;
                sd.palettes[i].order = i;
                sd.palettes[i].label = tabTemp.Header.ToString();
                sd.palettes[i].stock = composeLines(i);
                sd.palettes[i].textColor = "#000000";
            }

            var options = new JsonSerializerOptions() {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, //.Create(UnicodeRanges.All), 
                WriteIndented = true
            };
            strWrite = JsonSerializer.Serialize(sd,options);
            strWrite = strWrite.Replace("\u3000", "　");//Encoredで"　"は"\u3000"にエスケープされるため修正。
            
            // ファイルセーブポイント設定
            var dlgSave = new Microsoft.Win32.SaveFileDialog();
            try
            {
                dlgSave.DefaultExt = ".json";
                dlgSave.Filter = "チャットパレットファイル(.json)|*.json|全てのファイル(*.*)|*.*";
                dlgSave.FilterIndex = 1;
                if (dlgSave.ShowDialog() == true)
                {
                    this.IsEnabled = false;
                    strFilePath = dlgSave.FileName;
                    this.IsEnabled = true;
                }
            }
            catch (ArgumentException e) 
            {
                MessageBox.Show("Error SavaChatPalette :\n" + e.ToString());
            }
            // ファイル書き込み
            if (strFilePath == "" || strFilePath == null) return;
         
            using (StreamWriter writer = new StreamWriter(strFilePath, false, Encoding.GetEncoding("utf-8"))) 
            {
                writer.Write(strWrite);
            }
                return;
        }

        #endregion


        #region Getter Setter

        /// <summary>
        /// 選択されているタブの名前を変更する。
        /// タブ名の変更に合わせて辞書名の変更も行え。Call ChangeLinesDictyonary
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        public int setSelectedTabName(string setName)
        {
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0)
            {
                TabItem tab = (TabItem)TabControl.Items[TabControl.SelectedIndex];
                if (tab.Header.ToString() != setName)
                {
                    tab.Header = setName;
                }
            }
            return 0;
        }

        /// <summary>
        /// 選択されているタブの名前を取得する関数。
        /// </summary>
        /// <returns>string 選択されているタブの名前を返す</returns>
        public string getSelectedTabName()
        {
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0)
            {
                TabItem tab = (TabItem)TabControl.Items[TabControl.SelectedIndex];
                return tab.Header.ToString();
            }
            return "";
        }


        /// <summary>
        /// 文章をtextBoxに代入する。
        /// 変数処理関数を呼び出している。
        /// </summary>
        private void setToTextBox(string strSetSentence)
        {
            strSetSentence = tryReplase(strSetSentence);
            textbox.Text = strSetSentence;
        }

        #endregion

        #region Dictionary関係

        /// <summary>
        /// 変数用の辞書を作成する。タブが作成されたときに使う。
        /// </summary>
        /// <param name="strTabName">作成するタブ名</param>
        public void makeNewLinesDictionary(string strTabName)
        {
            try
            {
                TabsDictionary.Add(strTabName, new Dictionary<string, string>());
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("新しい辞書名がNullです。");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("新しい辞書を登録できませんでした。");
            }
        }

        /// <summary>
        /// TabsDictionaryにstrChackNameがあるかどうか確認する。
        /// </summary>
        /// <param name="strChackName"></param>
        /// <returns></returns>
        public bool checkTabsDictionaryName(string strChackName)
        {
            if (TabsDictionary.ContainsKey(strChackName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tas辞書に登録されている辞書を消す。タブを消すときに使用する。
        /// </summary>
        /// <param name="strClearLinesDictionaryName">Tabs辞書に登録されているLines辞書の名前</param>
        /// <returns></returns>
        public bool ClearLinesDictionary(string strClearLinesDictionaryName)
        {
            Dictionary<string, string> dicTemp;
            try
            {
                if (TabsDictionary.TryGetValue(strClearLinesDictionaryName, out dicTemp))
                {
                    dicTemp.Clear();
                    return true;
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("削除を試みた辞書" + strClearLinesDictionaryName + "は見つかりませんでした。");
            }
            return false;
        }

        /// <summary>
        /// 変数を辞書から調べる。
        /// </summary>
        /// <param name="strTabName">調べるタブ名</param>
        /// <param name="searchKey">調べる変数名</param>
        /// <returns>変数の変換後の文章</returns>
        public string getValueFromLinesDictionary(string strTabName, string searchKey)
        {
            string strReturn = null;
            Dictionary<string, string> dicTemp;
            try
            {
                if (TabsDictionary.TryGetValue(strTabName, out dicTemp))
                {
                    if (dicTemp.TryGetValue(searchKey, out strReturn))
                    {
                        return strReturn;
                    }
                    MessageBox.Show("TabNameは一致した。変数変換は一致しなかった");
                }
                MessageBox.Show("TabNameは一致しなかった。");
                return strReturn;
            }
            catch (ArgumentException)
            {
                // strTabName == null
                return null;
            }
        }

        /// <summary>
        /// 変数を辞書に登録する。
        /// </summary>
        /// <param name="strTabName">登録するタブ名</param>
        /// <param name="strSetKey">登録する変数名</param>
        /// <param name="strSetValue">登録する変換後名</param>
        /// <returns></returns>
        public bool setWordToLinesDictionary(string strTabName, string strSetKey, string strSetValue)
        {
            Dictionary<string, string> dicTemp;
            try
            {
                if (TabsDictionary.TryGetValue(strTabName, out dicTemp))
                {
                    try
                    {
                        dicTemp.Add(strSetKey, strSetValue);
                        return true;
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("登録不可:" + strSetKey + "=" + strSetValue + "は登録できません。");
                        return false;
                    }
                }
                MessageBox.Show("タブ名が合致しませんでした。");
                return false;
            }
            catch (ArgumentException)
            {
                //strTabName == null
                return false;
            }
        }

        /// <summary>
        /// Tabs辞書に登録されている変数辞書の名前を変更する。
        /// </summary>
        /// <param name="strOldTabName">変更前の辞書名</param>
        /// <param name="strNewTabName">変更後の辞書名</param>
        /// <returns></returns>
        public bool changeLinesDictionaryTabName(string strOldTabName, string strNewTabName)
        {
            Dictionary<string, string> tempDictionary;
            try
            {
                if (TabsDictionary.TryGetValue(strOldTabName, out tempDictionary))
                {
                    try
                    {
                        TabsDictionary.Add(strNewTabName, tempDictionary);
                        TabsDictionary.Remove(strOldTabName);
                        return true;
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show("重複登録:" + strNewTabName + "は登録できません。");
                        return false;
                    }
                }
                MessageBox.Show("変えようとしたタブ" + strOldTabName + "は見つかりません。");
                return false;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("変えようとしたタブ" + strOldTabName + "はNullでした。");
                return false;
            }
        }

        /// <summary>
        /// Tabs辞書に登録されているLines辞書を削除する。タブを削除するときに使う。
        /// </summary>
        public void deleteLinesDictionary(string strDeleteLinesDictionaryName)
        {
            Dictionary<string, string> tempDictionary;
            try
            {
                if (TabsDictionary.TryGetValue(strDeleteLinesDictionaryName, out tempDictionary))
                {
                    try
                    {
                        if (!TabsDictionary.Remove(strDeleteLinesDictionaryName))
                            MessageBox.Show(strDeleteLinesDictionaryName + "タブは削除できませんでした。");
                    }
                    catch (ArgumentNullException)
                    {
                        MessageBox.Show("削除するタブ名を入れてください。");
                    }
                }
            }
            catch (ArgumentException) { MessageBox.Show(strDeleteLinesDictionaryName + "タブは削除できませんでした。"); }
        }

        #endregion


        #region TabItems関係

        /// <summary>
        /// 新しいItemtabを生成。Tabs辞書に新しく作成した辞書を登録する。
        /// ItemTabにScrollViewerを追加。
        /// ScrollViewerにStackPanelを追加。
        /// </summary>
        private void makeNewTabItem()
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            

            scrollViewer.Content = new StackPanel();
            string newTabName = "Tab";

            // 新しいTab名を決定
            for (int i = 0; ; i++)
                if (!TabsDictionary.ContainsKey("Tab" + i.ToString()))
                {
                    newTabName = "Tab" + i.ToString();
                    break;
                }
            TabItem tab = new TabItem()
            {
                Header = newTabName,
                Content = scrollViewer
            };
            TabControl.Items.Add(tab);
            tab.IsSelected = true;
            makeNewLinesDictionary(tab.Header.ToString());
        }

        /// <summary>
        /// 　現在選択しているタブを削除する
        /// </summary>
        private void DeleteSelectedTab()
        {
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0)
            {
                TabItem tab = (TabItem)TabControl.Items[TabControl.SelectedIndex];
                deleteLinesDictionary(tab.Header.ToString());
                TabControl.Items.RemoveAt(TabControl.SelectedIndex);
            }
        }

        /// <summary>
        /// 全てのタブを削除する。Lines辞書も全て削除する。
        /// </summary>
        public void DeleteAllTabs() 
        {
                int intCountToDelete = TabControl.Items.Count;
                for (int i = 0; i < intCountToDelete; i++)
                {
                    TabItem tabTemp = (TabItem)TabControl.Items[0];
                    deleteLinesDictionary(tabTemp.Header.ToString());
                    TabControl.Items.RemoveAt(0);
                }
        } 

        #endregion


        /// <summary>
        /// 一行を辞書登録のKeyとValueにパースする。
        /// </summary>
        /// <param name="strRagis">パースする文章。//"Key"="value"</param>
        /// <param name="key">変数を格納する</param>
        /// <param name="value">値を格納する</param>
        public void parseString(string strRagis, ref string key, ref string value)
        {
            try
            {
                key = strRagis.Substring(2);
                key = key.Substring(0, key.IndexOf("="));
                value = strRagis.Substring(strRagis.IndexOf("="));
                value = value.TrimStart('=');
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("ParseString：" + e.ToString());
            }
        }


        /// <summary>
        /// Linesをマウスでクリックして離した時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _double_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock;
            if (!(sender is TextBlock)) return;
            textBlock = (TextBlock)sender;
            textbox.Text = textBlock.Text;
        }


        /// <summary>
        /// 変数があるか調べ、ある場合は
        /// </summary>
        /// <param name="strOrg"></param>
        /// <param name="strCheckWord"></param>
        /// <returns></returns>
        public string tryReplase(string strOldSentence)
        {
            Dictionary<string, string> tempDic;
            string strTabName, strNewSentence = null, strReword, strDictionaryKey, strDictionaryValue=null;
            int intIndexBegin, intIndexEnd;
            bool isLoopTimes;

            strTabName = getSelectedTabName();
            do
            {
                isLoopTimes = false;
                if (strOldSentence == "" || strOldSentence == null)
                    break;

                //{変数}はあるか？
                intIndexBegin = strOldSentence.IndexOf("{");
                if (intIndexBegin >= 0)
                {
                    intIndexEnd = strOldSentence.IndexOf("}");
                    if (intIndexBegin < intIndexEnd) // intIndexが0以上であるかを確認する必要はない。intIndexStartがこの時点で0以上であるため。 
                    {
                        strReword = strOldSentence.Substring(intIndexBegin, intIndexEnd + 1 - intIndexBegin);
                        strDictionaryKey = strReword.Substring(1, strReword.IndexOf("}") - 1);
                        if (strReword.IndexOf("@") >= 0)
                        {
                            strTabName = strReword.Substring(strReword.IndexOf("@"));
                            strTabName = strTabName.TrimStart('@');
                            strTabName = strTabName.TrimEnd('}');
                            strDictionaryKey = strReword.Substring(1, strReword.IndexOf("@") - 1);
                        }

                        // 変数変換可能か
                        try
                        {
                            bool isExchange = TabsDictionary.TryGetValue(strTabName, out tempDic);
                            if (isExchange) // Lines辞書が見つからない場合、変数探索は行わない。
                                isExchange = tempDic.TryGetValue(strDictionaryKey, out strDictionaryValue);
                            if (isExchange)
                            {
                                strNewSentence += strOldSentence.Substring(0, intIndexEnd + 1); // {までを切り渡す
                                strNewSentence = strNewSentence.TrimEnd('\r');
                                strOldSentence = strOldSentence.Substring(intIndexEnd + 1); // {までを切り捨てる
                                strNewSentence = strNewSentence.Replace(strReword, strDictionaryValue);
                                isLoopTimes = true;
                            }
                            else
                            {
                                // MessageBox.Show("変数変換出来ませんでした。");
                                strNewSentence += strOldSentence.Substring(0, intIndexEnd + 1); // {までを切り渡す
                                strOldSentence = strOldSentence.Substring(intIndexEnd + 1); // {までを切り捨てる
                                isLoopTimes = true;
                            }

                        }
                        catch (ArgumentNullException e)
                        {
                            break;
                        }
                        catch (ArgumentException e)
                        {
                            break;
                            MessageBox.Show("Error");
                        }

                    }
                    else // {はあったけど、｝はなかった
                    {
                        strNewSentence += strOldSentence.Substring(0, intIndexBegin + 1);
                        strOldSentence = strOldSentence.Substring(intIndexBegin + 1);
                        isLoopTimes = true;
                    }
                }
                else //{はなかった。
                {
                    strNewSentence += strOldSentence;
                    break;
                }
            } while (isLoopTimes);
            return strNewSentence;
        }


        /// <summary>
        /// クリップボードにTextBoxのTextを張り付ける。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            string text = textbox.Text; // テキストボックスに入力されている値を取得する。
            if (text != "")
            {
                Clipboard.SetData(DataFormats.Text, text);
            }
        }

        /// <summary>
        /// 現在選択されているタブのStackPanelにLineを作る。
        /// </summary>
        /// <param name="strLines"></param>
        /// <returns></returns>
        public int makeLines(string strLines)
        {
            string[] anyStrLines;
            // 現在選択しているTabItemのStackPaneまでのリンクを取得する。
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0 && strLines != null)
            {
                var selectedTabItem = (TabItem)TabControl.Items[TabControl.SelectedIndex];
                var tempScroll = (ScrollViewer)selectedTabItem.Content;
                var tempStack = (StackPanel)tempScroll.Content;

                deleteAllLines();
                ClearLinesDictionary(selectedTabItem.Header.ToString());

                // 文章を分解して、一行ごとのLineを作成する。
                anyStrLines = strLines.Split("\n");
                for (int i = 0; i < anyStrLines.Length; i++)
                {
                    string strTemp = anyStrLines[i];
                    if (strTemp.EndsWith("\r"))
                        strTemp = strTemp.TrimEnd('\r');
                    // 追加するLinesの作成
                    Button btnNew = new Button();
                    btnNew.Content = strTemp;
                    btnNew.Click += btnCopyToClip;
                    tempStack.Children.Add(btnNew);

                    // 変数登録処理
                    if (strTemp.StartsWith("//"))
                    {
                        string key = null, value = null;
                        parseString(strTemp, ref key, ref value);
                        setWordToLinesDictionary(selectedTabItem.Header.ToString(), key, value);
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 選択されているTabitem内のLinesを全てを削除する。
        /// 選択されているタブのスタックパネルのボタンを数え、その回数だけ削除する。
        /// </summary>
        public void deleteAllLines()
        {
            // 現在選択しているTabItemのStackPaneまでのリンク
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0)
            {
                var selectedTabItem = (TabItem)TabControl.Items[TabControl.SelectedIndex];
                var tempScroll = (ScrollViewer)selectedTabItem.Content;
                var tempStack = (StackPanel)tempScroll.Content;
                int intCount = tempStack.Children.Count;
                if (intCount != 0)
                    for (int i = 0; i < intCount; i++)
                        tempStack.Children.RemoveAt(0);
            }
        }


        /// <summary>
        /// Linesの文章を取得、一つの文章にして返す。
        /// </summary>
        /// <param name="intTabIndex">取得したいタブのインデックス</param>
        /// <returns></returns>
        public string composeLines(int intTabIndex)
        {
            string strLines = null;

            //現在選択中のタブのStackPanelを取得する。
            if (TabControl.Items.Count > 0 && TabControl.Items.Count >= intTabIndex && intTabIndex >= 0 )
            {
                var selectedTabItem = (TabItem)TabControl.Items[intTabIndex];
                var tempScroll = (ScrollViewer)selectedTabItem.Content;
                var tempStack = (StackPanel)tempScroll.Content;


                for (int i = 0; i < tempStack.Children.Count; i++)
                {
                    Button tempBtn = (Button)tempStack.Children[i];
                    strLines += tempBtn.Content.ToString();

                    if (i != tempStack.Children.Count - 1) strLines += "\n";
                }
            }
            return strLines;
        }

        #region Button関係

        /// <summary>
        /// Linesに設定するボタン操作。テキストボックスに名前を移す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopyToClip(object sender, RoutedEventArgs e)
        {
            Button tempBtn = (Button)sender;
            setToTextBox(tempBtn.Content.ToString());
        }


        /// <summary>
        /// Lineを編集する画面を出す。
        /// 編集画面に必要な要素を渡す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenEditWindow(object sender, RoutedEventArgs e)
        {
            // タブがなければ編集画面は出さない。    
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0)
            {
                EditWindow = new EditLinesWindow(this);
                // 編集画面の表示位置を現在のウィンドウのx,y座標に合わせる。
                EditWindow.Top = this.Top;
                EditWindow.Left = this.Left;
                EditWindow.Height = this.Height;
                EditWindow.Width = this.Width;
                EditWindow.setEditedTabName(getSelectedTabName());
                EditWindow.setEditLines(composeLines(TabControl.SelectedIndex));
                EditWindow.Show();
                this.Hide();
            }
        }


        /// <summary>
        /// セーブボタンの関数。saveChatPalette関数を呼び出すだけ。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveChatPalette(object sender, RoutedEventArgs e)
        {
            saveChatPalette();
        }

        private void btnLoadChatPalette(object sender, RoutedEventArgs e)
        {
            lordChatPaletteFile();
        }

        /// <summary>
        /// タブを追加するボタンの処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddNewTab(object sender, RoutedEventArgs e)
        {
            makeNewTabItem();
        }

        /// <summary>
        /// タブを削除するボタンの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteSelectedTab(object sender, RoutedEventArgs e)
        {
            DeleteSelectedTab();
        }

        #endregion

        /// <summary>
        /// 最前面表示の切り替え。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeTopmost(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }
    }
}
