using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

/// <summary>
/// 수정 사항 
/// 1. 저장되는거 엑셀 한파일에 sheet단위로 변경  ok 동적 할당 하는 법 확인
/// 2. 저장되는 과정 표시  ok 근데 속도가 느리네
/// 3. 파일 열때 에러 처리  ok
/// 4. 파일 오픈 하고 그래프 출력되는지 확인 되네? ㅋㅋ 라인이랑 차트가됨 ok
/// 5. 소스 정리를 어떻게 하는지 확인 필요 
/// 6. 모든 프로그램 사용시 로그인 화면 
/// 7. 그래프 그리고 파일 내용 리스트로 바꾸기  ok
/// 
/// 내용
/// 1. 그래프 사용 
/// 2. 파일한줄씩 읽어 파싱 부분 
/// 3. 엑셀 저장 
/// 4. 리스트 동적 배열 사용(1차, 2차)
/// </summary>

namespace GetLpcData
{
    public partial class Form1 : Form
    {
        struct PANELINFO
        {
            public string toolname;
            public int nDataCnt;
        };

        struct SAVEDATAINFO
        {
            public string toolname;
            public int value;
            public int refvalue;
        }
        static string OpenedFilePath;
        static string SavedFilePath;

        //로딩된 판낼 카운트 
        static int nTotalPanelCount;

        //선택된 판낼 카운트 
        static int nSelectedCount;

        //선택된 판낼 번호 
        static int nSelectedToolNo;


        List<PANELINFO> toolList = new List<PANELINFO>();//1차워 동적 배열 할당 
        List<List<SAVEDATAINFO>> ExcelSaveData = new List<List<SAVEDATAINFO>>(); // 2차워 동적 배열 할당 
        List<int> GraphData = new List<int>();  // 그래프에 저장할 데이터 

        static Excel.Application excelApp = null;
        static Excel.Workbook workBook = null;
        // List<Excel.Worksheet> workSheet = new List<Excel.Worksheet>();//1차워 동적 배열 할당 


        // 출입 함수

        private void button1_Click(object sender, EventArgs e)
        {
            SetProgressBar(0);
            InitInternalvalue();
            textBox_filename.Clear();

            OpenedFilePath = null;
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName == "")
                    return;

                OpenedFilePath = openFileDialog1.FileName;
                textBox_filename.Text = OpenedFilePath.Split('\\')[OpenedFilePath.Split('\\').Length - 1];

                ShowPanelLsit();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            SetProgressBar(0);


            SavedFilePath = null;
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName == "")
                    return;

                SavedFilePath = saveFileDialog1.FileName;
                SaveExcelFile();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listBox_panel_SelectedIndexChanged(object sender, EventArgs e)
        {
            nSelectedCount = listBox_panel.SelectedIndex;
            ShowPanelInformation();
        }

        private void btn_show_Click(object sender, EventArgs e)
        {
            nSelectedToolNo = listBox_ToolInfo.SelectedIndex;
            ShowToolInformation();
        }

        private void listBox_ToolInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            nSelectedToolNo = listBox_ToolInfo.SelectedIndex;
            ShowToolInformation();
        }
    }
}
