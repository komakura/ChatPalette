using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatPalette
{
    /// <summary>
    /// EditLinesWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditLinesWindow : Window
    {
        MainWindow mainWindow;
        public EditLinesWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        /// <summary>
        /// クローズ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.Show();
        }

        /// <summary>
        /// 編集画面の位置座標と大きさをメインウィンドウに合わせる。
        /// </summary>
        /// <returns></returns>
        public int setPintSize()
        {
            return 0;
        }

        /// <summary>
        /// Tab名を入れるTextBoxのTextにsetNameを代入する。
        /// 編集画面を開いたとき、変更するタブ名を設定する。
        /// </summary>
        /// <param name="setName"></param>
        /// <returns></returns>
        public int setEditedTabName(string setName) 
        {
            EditedTabName.Text = setName;
            return 0;
        }

        /// <summary>
        /// 編集したタブ名を取得する 
        /// </summary>
        /// <returns></returns>
        public string getEditedTabName()
        {
            return EditedTabName.Text;
        }

        /// <summary>
        /// Linesの内容を作るTextBoxの初期を取得。
        /// </summary>
        /// <param name="strSetLines"></param>
        /// <returns></returns>
        public void setEditLines(string strSetLines)
        {
            // Linesの一行一行を+stringしつつ、TextBoxに全部入れる。
            EditedLines.Text = strSetLines;
        }

        /// <summary>
        /// EditlinesのTextを取得する。
        /// </summary>
        /// <returns></returns>
        public string getEditLines()
        {
            return EditedLines.Text;
        }

        /// <summary>
        /// Edit Linesの編集を完了する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoneEdit(object sender, RoutedEventArgs e)
        {
            bool boolCheckName;
            if (EditedLines.Text == null || EditedLines.Text == "")
            {
                MessageBox.Show("タブ名かテキスト内容が空欄です。何か入れてください");
            }
            else
            {
                boolCheckName = mainWindow.checkTabsDictionaryName(getEditedTabName());
                if (!boolCheckName) // 名前かぶりなし。
                {
                    mainWindow.makeLines(EditedLines.Text);             // SetSelectedTabNameの前に宣言しないと、Tab名が先に変更されて、削除処理がおかしくなる。
                    //Lines辞書名の変更とTab名の変更
                    mainWindow.changeLinesDictionaryTabName(mainWindow.getSelectedTabName(), getEditedTabName());
                    mainWindow.setSelectedTabName(getEditedTabName());
                    //this.Hide();
                    mainWindow.Show();
                    this.Close();
                    
                }
                else if (getEditedTabName() == mainWindow.getSelectedTabName()) //名前かぶりありかつ名前変更なし
                {

                    mainWindow.makeLines(EditedLines.Text);// SetSelectedTabNameの前に宣言しないと、Tab名が先に変更されて、削除処理がおかしくなる。
                    //this.Hide();
                    mainWindow.Show();
                    this.Close();
                }
                else // 名前かぶりあり、名前変更有
                {
                    MessageBox.Show("そのタブ名は既に他のタブで使われています。");
                }
            }
        }

        /// <summary>
        /// Edit Linesの編集を止める。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelEdit(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            this.Close();
            mainWindow.Show();
        }
    }

}
