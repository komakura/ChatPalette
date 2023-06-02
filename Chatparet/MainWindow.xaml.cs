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
        string strLastDirectoryPath;

        public MainWindow()
        {
            InitializeComponent();
            lordInit();
            setWindowTitle("Chat Palette");
        }

        // MainWindowのクローズ処理
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveInit();
        }

        /// <summary>
        /// ウィンドウのタイトルを設定する。ファイルを読み込んだ時に呼び出す。
        /// </summary>
        /// <param name="strNewTitle">新しいタイトル</param>
        public void setWindowTitle(string strNewTitle)
        {
            top.Title = strNewTitle;
        }

        #region Sava&Lord関係

        /// <summary>
        /// ファイルを開いて全文を取得する。
        /// </summary>
        /// <param name="strFilePath">開くファイルのパス</param>
        /// <returns>ファイルの中身。失敗したらnullを返す</returns>
        public string loadFile(string strFilePath) 
        {
            string strReturn = null;
            try
            {
                //Fileがあるかチェック。
                FileInfo fInfo = new FileInfo(strFilePath);
                if (!fInfo.Exists)
                    return strReturn;

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
                return strReturn;
            }
            return strReturn;
        }

        /// <summary>
        /// 初期設定読み込み。起動時に呼び出し。
        /// </summary>
        public void lordInit()
        {
            //String.Join("\r\n", File.ReadAllLines(filePath));
            string strSettingContents=null, strFilePath=null;
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
            strSettingContents = loadFile(strFilePath);
            if (strSettingContents != null) // fileあり。
            {
                // 読み込み反映
                foreach (string strSettingLine in strSettingContents.Split("\r\n"))
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
            //初期1tabを作成
            makeNewTab("Tab",null);
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
        public bool lordChatPaletteFile()
        {
            string strFilePath = null;
            string strFileContents = null;

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

            //ファイルを読み取る
            strFileContents = loadFile(strFilePath);
            if (strFileContents == null)
            {
                setWindowTitle("ファイルを読み込めませんでした。");
                return false;
            }
            //拡張子に応じた処理
            if (strFilePath != null)
                try
                {
                    switch (strFilePath.Substring(strFilePath.LastIndexOf('.')))
                    {
                        case ".txt":
                            setChatPaletteFromTextFile(strFileContents);
                            break;
                        case ".json":
                            setChatPaletteFromJsonFile(strFileContents);
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
            setWindowTitle(dlg.SafeFileName);
            ((TabItem)TabControl.Items[0]).IsSelected = true;
            return true;
        }

        public bool setChatPaletteFromTextFile(string strFileContents) 
        {
            DeleteAllTabs();
            makeNewTab("tab", strFileContents);
            return true; 
        }

        /// <summary>
        /// Jsonファイルからチャットパレットを読み込む。Tekeyの形式を採用
        /// </summary>
        /// <param name="strFileContents">読み込んだファイルの中身</param>
        /// <returns></returns>
        public bool setChatPaletteFromJsonFile(string strFileContents)
        {
            DeleteAllTabs();
            try
            {
                SaveData sd = new SaveData();
                sd = JsonSerializer.Deserialize<SaveData>(strFileContents);

                foreach (DataChatPalette varTemp in sd.palettes)  //各タブの処理
                {
                    makeNewTab(varTemp.label, varTemp.stock);
                }
            }
            catch
            {
                MessageBox.Show("ファイルの中身が見つかりませんでした。");
                makeNewTab(null, null);
                return false;
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
                //TabItem tabTemp = (TabItem)TabControl.Items[i];
                //TextBox tempTextBox = ((TextBox)tabTemp.Content);
                sd.palettes[i].paletteID = i + 1;
                sd.palettes[i].order = i;
                sd.palettes[i].label = ((TabItem)TabControl.Items[i]).Header.ToString();
                sd.palettes[i].stock = ((TextBox)((TabItem)TabControl.Items[i]).Content).Text;
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
                return ((TabItem)TabControl.Items[TabControl.SelectedIndex]).Header.ToString();
            }
            return "";
        }

        #endregion


        #region TabItems関係

        /// <summary>
        /// 新しいタブを作成。
        /// </summary>
        /// <param name="strTabName">tab名。無ければ新しいtabの数字が入る</param>
        /// <param name="strSentence">tabの本文。無ければ空欄</param>
        public void makeNewTab(string strTabName, string strSentence)
        {
            string strNewTabName = strTabName;
            TextBox tempTextBox = new TextBox()
            {
                Text = strSentence,
                AcceptsReturn = true,
                AcceptsTab = true
            };
            // 新しいTab名を決定
            //strTabNameが空欄だったら。
            if (strNewTabName == null || strNewTabName == "") 
            {
                strNewTabName = Convert.ToString(TabControl.Items.Count+1);
            }

            TabItem tab = new TabItem()
            {
                Header = strNewTabName,
                Content = tempTextBox
            };
            TabControl.Items.Add(tab);
            tab.IsSelected = true;
        }


        /// <summary>
        /// 　現在選択しているタブを削除する
        /// </summary>
        private void DeleteSelectedTab()
        {
            if (TabControl.Items.Count > 0 && TabControl.SelectedIndex >= 0)
            {
                TabItem tab = (TabItem)TabControl.Items[TabControl.SelectedIndex];
                TabControl.Items.RemoveAt(TabControl.SelectedIndex);
            }
        }

        /// <summary>
        /// 全てのタブを削除する。
        /// </summary>
        public void DeleteAllTabs() 
        {
                int intCountToDelete = TabControl.Items.Count;
                for (int i = 0; i < intCountToDelete; i++)
                {
                    TabItem tabTemp = (TabItem)TabControl.Items[0];
                    TabControl.Items.RemoveAt(0);
                }
        } 

        #endregion


        /// <summary>
        /// 新しいタブを作成し
        /// </summary>
        /// <param name="strSentence"></param>
        /// <returns></returns>
        public int makeTextEditter(string strSentence) 
        {

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

        #region Button関係

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
            makeNewTab(null, null);
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
