using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //默认将日期选择为当前日期
            DateSelector.Text = DateTime.Today.ToLongDateString();
            
        }
        //初始化变量
        string HostNameDate = "";
        string SNDate = "";
        string SWVersionDate = "";
        string TestSiteTypeDate = "";
        string StartedDate = "";
        string StoppedDate = "";
        string StoppedDateF = "";

        bool flag = false;
        int SearchCount = 0;

        void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //日期改变后重置显示结果
            HostNameText.Text = "";            
            SNText.Text = "";
            SWVersionText.Text = "";            
            TestSiteTypeText.Text = "";            
            StartedText.Text = "";            
            StoppedText.Text = "";

            flag = false;
            SearchCount = 0;
        }

        
        void DataSearchBtn_Click(object sender, RoutedEventArgs e)
        {

            SearchCount++;
            //编写数据库连接串
            string connStr = "server=localhost;uid=DBACCOUNT;pwd=DBPASSWORD;database=DBNAME";
            //创建 SqlConnection的实例
            SqlConnection conn = null;
            //定义SqlDataReader类的对象
            SqlDataReader dr = null;

            try
            {
                conn = new SqlConnection(connStr);
                //打开数据库连接
                conn.Open();
                String DateSelectedStr = Convert.ToDateTime(DateSelector.Text).ToString("yyyy-MM-dd");
                //填充SQL语句
                string QuerySQL = string.Format("SELECT BSR_HOSTNAME, SERIAL_NUMBER, SOFTWARE_VERSION, TESTSITE_TYPE, CONVERT(varchar(100), TS_KDT_STARTED, 20), CONVERT(varchar(100), TS_KDT_STOPPED, 20) FROM TA_DB.TA_SYS.TESTSITE_RUN WHERE TESTRUN_STATUS='USER_ABORT' AND TESTRUN_TYPE='Endurance Test' AND CONVERT(varchar(100), TS_KDT_STOPPED, 23)='{0}'", DateSelectedStr);
                //创建SqlCommand对象
                SqlCommand QueryCmd = new SqlCommand(QuerySQL, conn);

                //执行Sql语句
                dr = QueryCmd.ExecuteReader();
                //判断SQL语句是否执行成功
                if (dr.Read()){
                    //读取指定数据
                    HostNameDate = dr[0].ToString();
                    SNDate = dr[1].ToString();
                    SWVersionDate = dr[2].ToString();
                    TestSiteTypeDate = dr[3].ToString();
                    StartedDate = dr[4].ToString();
                    StoppedDate = dr[5].ToString();
                    StoppedDateF = Convert.ToDateTime(dr[5]).ToString("yyyyMMddHHmmss");

                    //将值显示在标签上
                    HostNameText.Text = HostNameDate;
                    SNText.Text = SNDate;
                    SWVersionText.Text = SWVersionDate;
                    TestSiteTypeText.Text = TestSiteTypeDate;
                    StartedText.Text = StartedDate;
                    StoppedText.Text = StoppedDate;

                    flag = true;
                }
                else {
                    MessageBox.Show("No Data FOUND! Please check TA Web!");
                    flag = false;
                    HostNameDate = "";
                    SNDate = "";
                    SWVersionDate = "";
                    TestSiteTypeDate = "";
                    StartedDate = "";
                    StoppedDate = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search FAILED！" + ex.Message);
                HostNameDate = "";
                SNDate = "";
                SWVersionDate = "";
                TestSiteTypeDate = "";
                StartedDate = "";
                StoppedDate = "";
            }
            finally
            {
                if (dr != null)
                {
                    //判断dr不为空，关闭SqlDataReader对象
                    dr.Close();
                }
                if (conn != null)
                {
                    //关闭数据库连接
                    conn.Close();
                }
            }
        }


        void FileCreateBtn_Click(object sender, RoutedEventArgs e)
        {
            if(SearchCount != 0)
            {
                if (flag)
                {
                    //创建XML文件
                    XDocument document = new XDocument();
                    //XML文件内容
                    document.Declaration = new XDeclaration("1.0", "iso-8859-1", "");

                    XElement root = new XElement("root");

                    root.Add(
                        new XElement("test_data",
                            new XElement("system_hostname", HostNameDate),
                            new XElement("serial_number", SNDate),
                            new XElement("system_sw_version", SWVersionDate),
                            new XElement("system_model_type", TestSiteTypeDate),
                            new XElement("start_time", StartedDate),
                            new XElement("end_time", StoppedDate),
                            new XElement("test_starter", "RSServ"),
                            new XElement("test_stopper", "RSServ"),
                            new XElement("test_comment", "This is the Trigger XML file for evaluation of the corresponding Event Log Zip file."))
                            );
                    document.Add(root);

                    //XML文件名
                    document.Save("XA" + SNDate + "_" + StoppedDateF + "_TestData.xml");
                    //创建完成提示
                    MessageBox.Show("Trigger file CREATED!");
                }
                else
                {
                    MessageBox.Show("No Data FOUND! Please check TA Web!");
                }
            }
            else
            {
                MessageBox.Show("Before Create Trigger File, Please SEARCH and Check Data!");
            }
        }

    }
}
