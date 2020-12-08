using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

///



namespace GetLpcData
{ 
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitInternalvalue();
        }

        /// <summary>
        /// 한줄씩 읽어 판낼이 몇개인지 리스트에 뿌려준다. 
        /// 
        /// </summary>
        void ShowPanelLsit()
        {
            listBox_panel.Items.Clear();

            string[] lines = File.ReadAllLines(OpenedFilePath);

            foreach (string lead in lines)
            {
                if (lead.Contains("Project"))
                {
                    string strInsertList = GetListBoxString(lead);

                    listBox_panel.Items.Add(strInsertList);
                    nTotalPanelCount++;
                }
            }
        }

        void ShowToolInformation()
        {
            GraphData.Clear();
            int nSelected = nSelectedCount;
            int nSelectedTool = nSelectedToolNo;


            int nCurrentPanelCnt = 0;
            int nCurrentToolCnt = -1;

            bool saved = false;

            string[] lines = File.ReadAllLines(OpenedFilePath);

            string strOldToolName = "empty";

            string strToolName = null;
            int nRefValue = 0;

            foreach (string lead in lines)
            {
                if (lead.Contains("Project"))
                {
                    saved = false;
                    if (nSelected == nCurrentPanelCnt)
                    {
                        saved = true;
                    }

                    nCurrentPanelCnt++;
                    continue;
                }

                if(lead.Contains("-> Value"))
                {
                    continue;
                }

                if (saved)
                {
                    string tooltag = lead.Split('\t')[0];
                    string toolname = tooltag.Split(' ')[tooltag.Split(' ').Length - 2] + " " + tooltag.Split(' ')[tooltag.Split(' ').Length - 1];

                    if (strOldToolName == "empty" || strOldToolName != toolname)
                    {
                        nCurrentToolCnt++;
                        strOldToolName = toolname;
                    }

                    if (nCurrentToolCnt == nSelectedTool)
                    {
                        string strRefValue = lead.Split(' ')[lead.Split(' ').Length - 1];
                        string strvalue = lead.Split('\t')[lead.Split('\t').Length - 2];
                        strvalue = strvalue.Replace("Value ", "");

                        GraphData.Add(Convert.ToInt32(strvalue));
                        nRefValue = Convert.ToInt32(strRefValue);
                        strToolName = toolname;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if(strToolName != null)
                ShowGraphData(strToolName, nRefValue);

        }

        void ShowGraphData(string strToolName, int nRefValue)
        {
            // 여기 데이터 다 담겨있다
            // 차트 데이터 편집

            int nMinValue = GraphData.Min();
            int nMaxValue = GraphData.Max();
            int nStandValue = nRefValue;

            if (nMinValue == nMaxValue)
            {
                MessageBox.Show("Graph Show Fail");
                return;
            }
            chart1.Series[0].Points.Clear();

            chart1.ChartAreas[0].AxisY.Minimum = nMinValue;
            chart1.ChartAreas[0].AxisY.Maximum = nMaxValue;
            chart1.ChartAreas[0].AxisY.Interval = (nMaxValue - nMinValue) / 5;
            chart1.Series[0].LegendText = "RefValue: " + nStandValue.ToString();


            foreach (int nData in GraphData)
            {
                
                chart1.Series[0].Points.Add(nData);
            }

            // 값 표시
            double dPercentMin = (double)(nMinValue * 100) / (double)nStandValue;
            double dPercentMax = (double)(nMaxValue * 100) / (double)nStandValue;



            string strTemp;
            textBox_info.Clear();
            StringBuilder sb = new StringBuilder();

            strTemp = string.Format("Ref Value: {0}", nStandValue);
            sb.AppendLine(strTemp);

            strTemp = string.Format("Min: {0} {1: 0.00}%, Max: {2} {3: 0.00}%", nMinValue, dPercentMin, nMaxValue, dPercentMax);
            sb.AppendLine(strTemp);


            double dMaxGab = 0;

            if(Math.Abs(100 - dPercentMax) > Math.Abs(100 - dPercentMin) )
                dMaxGab = dPercentMax;
            else
                dMaxGab = dPercentMin;

            dMaxGab = Math.Abs(100 - dMaxGab);

            strTemp = string.Format("MaxGab: {0: 0.00}%", dMaxGab);
            sb.AppendLine(strTemp);

            textBox_info.Text = sb.ToString();

        }

        void ShowPanelInformation()
        {
            toolList.Clear();

            int nSelected = nSelectedCount;
            int nCurrentCnt = 0;

            int nEachIndexCnt = 0;
            string strcuttenttoolinfo = "empty";
            string strSubToonInformation = "";

            string[] lines = File.ReadAllLines(OpenedFilePath);

            bool saved = false;
            foreach (string lead in lines)
            {
                if (lead.Contains("Project"))
                {
                    // 마지막 남은거 저장 
                    if ((nEachIndexCnt != 0) && saved)
                    {
                        SaveToolList(strcuttenttoolinfo + strSubToonInformation, nEachIndexCnt);
                        nEachIndexCnt = 0;
                    }

                    saved = false;
                    if (nSelected == nCurrentCnt)
                        saved = true;

                    nCurrentCnt++;
                    continue;
                }

                if (lead.Contains("-> Value"))
                {
                    continue;
                }

                if (saved)
                {
                    string tooltag = lead.Split('\t')[0];
                    string toolname = tooltag.Split(' ')[tooltag.Split(' ').Length - 2] + " " +tooltag.Split(' ')[tooltag.Split(' ').Length - 1];

                    string strWvalue = lead.Split('\t')[1];
                    strWvalue = strWvalue.Replace("WValue ", "");

                    string strFreq = lead.Split('\t')[2];
                    strFreq = strFreq.Replace("Freq ", "");

                    string strCurrentA = lead.Split('\t')[3];
                    strCurrentA = strCurrentA.Replace("CurrentA ", "");

                    strSubToonInformation = string.Format(" W{0} F{1} A{2}", strWvalue, Convert.ToInt32(strFreq)/1000, strCurrentA);

                    if (strcuttenttoolinfo != toolname && strcuttenttoolinfo != "empty")
                    {
                        SaveToolList(strcuttenttoolinfo + strSubToonInformation, nEachIndexCnt);
                        nEachIndexCnt = 0;
                    }
                    strcuttenttoolinfo = toolname;
                    nEachIndexCnt++; // 저장 카운트 1 증가 
                }
            }

            // 맨 마지막거 저장
            if ((nEachIndexCnt != 0) && saved)
            {
                SaveToolList(strcuttenttoolinfo + strSubToonInformation, nEachIndexCnt);
                nEachIndexCnt = 0;
            }

            DisplayToolInfo();
        }


        void DisplayToolInfo()
        {
            int nTotalCnt = 0;

            listBox_ToolInfo.Items.Clear();
            foreach (PANELINFO temp in toolList)
            {
                string insertstring = string.Format("{0}: Cnt{1} ", temp.toolname, temp.nDataCnt);
                listBox_ToolInfo.Items.Add(insertstring);

                nTotalCnt += temp.nDataCnt;
            }

            
            textBox_info.Clear();
            textBox_info.Text = "Total Cnt: " + nTotalCnt.ToString();
        }

        void SaveToolList(string strtag, int nSumcnt)
        {
            PANELINFO Panelinfo;
            Panelinfo.toolname = strtag;
            Panelinfo.nDataCnt = nSumcnt;
            toolList.Add(Panelinfo);
        }


        string GetListBoxString(string insertstr)
        {
            string strRet = null;

            // split로 문자열 쪼개서 가져오는 방법 
            //split[] 안에 쪼개어진 것들이 다 들어가있따..와... 

            string strPanelNo =  " " + insertstr.Split(' ')[insertstr.Split(' ').Length - 1];
            string strPrjName = insertstr.Split(' ')[insertstr.Split(' ').Length - 3];

            strPrjName = strPrjName.Split('\\')[strPrjName.Split('\\').Length - 1];
            strRet = strPrjName + strPanelNo;

            return strRet;
        }

        void InitInternalvalue()
        {
            nTotalPanelCount = 0;
            nSelectedCount = 0;
            nSelectedToolNo = 0;
        }


        void SaveExcelFile()
        {
            ExcelSaveData.Clear();
           

            int nSelected = nSelectedCount;
            int nCurrentCnt = 0;

            int nEachIndexCnt = 0;
            string strcuttenttoolinfo = "empty";

            int nExcelSaveCount = 0;

            string[] lines = File.ReadAllLines(OpenedFilePath);

            bool saved = false;

            SAVEDATAINFO SaveTempdata;

            foreach (string lead in lines)
            {
                if (lead.Contains("Project"))
                {

                    // 마지막 남은거 저장 
                    if ((nEachIndexCnt != 0) && saved)
                    {
                        nEachIndexCnt = 0;
                    }

                    saved = false;
                    if (nSelected == nCurrentCnt)
                    {
                        saved = true;
                        ExcelSaveData.Add(new List<SAVEDATAINFO>());// 첫번째 배열 초기화 
                    }

                    nCurrentCnt++;
                    continue;
                }
                if (lead.Contains("-> Value"))
                {
                    continue;
                }

                if (saved)
                {
                    string tooltag = lead.Split('\t')[0];
                    string toolname = tooltag.Split(' ')[tooltag.Split(' ').Length - 2] + " " + tooltag.Split(' ')[tooltag.Split(' ').Length - 1];

                    string strRefvalue = lead.Split(' ')[lead.Split(' ').Length - 1];
                    string strvalue = lead.Split('\t')[lead.Split('\t').Length -2];
                    strvalue = strvalue.Replace("Value ", "");

                    if (strcuttenttoolinfo != toolname && strcuttenttoolinfo != "empty")
                    {
                        ExcelSaveData.Add(new List<SAVEDATAINFO>());// 첫번째 배열 초기화 
                        nEachIndexCnt = 0;
                        nExcelSaveCount++;
                    }
                    SaveTempdata.toolname = toolname;
                    SaveTempdata.value = Convert.ToInt32(strvalue);
                    SaveTempdata.refvalue = Convert.ToInt32(strRefvalue);

                    ExcelSaveData[nExcelSaveCount].Add(SaveTempdata);

  
                    strcuttenttoolinfo = toolname;
                    nEachIndexCnt++;
                }
            }

            // 맨 마지막거 저장
            if ((nEachIndexCnt != 0) && saved)
            {
                nEachIndexCnt = 0;

            }

            SaveToFile();

        }



       
        void SaveToFile()
        {
            SetProgressBar(20); // 로딩 하는게 20프로정도 
            

            int nCurrentProgressPercent = 0; 

            bool bisoneSheet = true;

            if (bisoneSheet)
            {
                // 이건 한 시트에 다 넣는 방식
                Excel.Worksheet[] workSheet = new Excel.Worksheet[ExcelSaveData.Count]; // 이거 동적 할당 어떻게 하냐?

                try
                {
                    string path = SavedFilePath;
                    excelApp = new Excel.Application(); //엑셀을 실행 
                    workBook = excelApp.Workbooks.Add(); // 엑셀의 기본 틀 생성 

                    int nRefValue = 0;
                    for (int i = 0; i < ExcelSaveData.Count; i++)
                    {
                        workSheet[i] = workBook.Worksheets.Add(Type.Missing, workBook.Worksheets[i + 1]); //워크 스트를 하나씩 생성한다.
                        workSheet[i].Name = ExcelSaveData[i][0].toolname; ;

                        workSheet[i].Cells[1, 1] = "value";
                        workSheet[i].Cells[1, 2] = "RefValue";

                        for (int j = 0; j < ExcelSaveData[i].Count; j++)
                        {
                            if (j == 0)
                            {
                                workSheet[i].Cells[j + 2, 1] = ExcelSaveData[i][j].value;
                                nRefValue = ExcelSaveData[i][j].refvalue;
                            }
                            else
                            {
                                workSheet[i].Cells[j + 2, 1] = ExcelSaveData[i][j].value;
                                nRefValue = ExcelSaveData[i][j].refvalue;
                            }
                        }
                        workSheet[i].Cells[2, 2] = nRefValue;
                        //workSheet[i].Columns.AutoFit(); 이건 없는게 나을듯

                        nCurrentProgressPercent = (i * ExcelSaveData.Count / 100) + 20;
                        SetProgressBar(nCurrentProgressPercent);
                    }


                    workBook.SaveAs(path, Excel.XlFileFormat.xlWorkbookDefault);
                    workBook.Close(true);
                    excelApp.Quit();

                   

                }
                finally
                {
                    for (int i = 0; i < ExcelSaveData.Count; i++)
                        ReleaseObject(workSheet[i]);

                    ReleaseObject(workBook);
                    ReleaseObject(excelApp);
                }

                SetProgressBar(100);
            }
            else
            {

                Excel.Worksheet workSheet = null;


                for (int i = 0; i < ExcelSaveData.Count; i++)
                {
                    try
                    {
                        string path = SavedFilePath + ExcelSaveData[i][0].toolname;

                        excelApp = new Excel.Application();
                        workBook = excelApp.Workbooks.Add();
                        workSheet = workBook.Worksheets.get_Item(1) as Excel.Worksheet; // 매시트 저장이 다름 


                        workSheet.Cells[1, 1] = "value";
                        workSheet.Cells[1, 2] = "RefValue";

                        int nRefValue = 0;

                        for (int j = 0; j < ExcelSaveData[i].Count; j++)
                        {
                            // 출력 ExcelSaveData[i][j];

                            if (j == 0)
                            {
                                workSheet.Cells[j + 2, 1] = ExcelSaveData[i][j].value;
                                nRefValue = ExcelSaveData[i][j].refvalue;
                            }
                            else
                            {
                                workSheet.Cells[j + 2, 1] = ExcelSaveData[i][j].value;
                                nRefValue = ExcelSaveData[i][j].refvalue;
                            }
                        }
                        workSheet.Cells[2, 1] = nRefValue;
                        // workSheet.Columns.AutoFit();

                        workBook.SaveAs(path, Excel.XlFileFormat.xlWorkbookDefault);
                        workBook.Close(true);
                        excelApp.Quit();


                        nCurrentProgressPercent = (i * ExcelSaveData.Count / 100) + 20;
                        SetProgressBar(nCurrentProgressPercent);

                    }
                    finally
                    {
                        ReleaseObject(workSheet);
                        ReleaseObject(workBook);
                        ReleaseObject(excelApp);

                    }

                }

                SetProgressBar(100);
            }
        }

        static void ReleaseObject(object obj)
        {
            try
            {
                if (obj != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                    obj = null;
                }

            }
            catch (Exception ex)
            {
                obj = null;
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
        }

        void SetProgressBar(int nvalue)
        {
            progressBar1.Value = nvalue;
            progressBar1.Update();
           
        }


    }
}
