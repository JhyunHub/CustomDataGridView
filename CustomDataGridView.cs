public class CustomDataGridView : DataGridView
{
    private DataTable _dataTable;
    private Dictionary<(int row, int col), Font> _cellFonts = new Dictionary<(int, int), Font>();
    private Dictionary<(int row, int col), Color> _cellColors = new Dictionary<(int, int), Color>();

    private int _hoveredRow = -1;
    private bool _sortAscending = true;
    private int _sortedColumn = -1;

    public CustomDataGridView()
    {
        // 깜빡임 제거
        typeof(DataGridView)
        .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        .SetValue(this, true, null);


        // 기본 스타일
        this.EnableHeadersVisualStyles = false;
        this.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
        this.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        this.ColumnHeadersDefaultCellStyle.Font = new Font("맑은 고딕", 11, FontStyle.Bold);
        this.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


        this.DefaultCellStyle.BackColor = Color.White;
        this.DefaultCellStyle.ForeColor = Color.Black;
        this.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 144, 255);
        this.DefaultCellStyle.SelectionForeColor = Color.White;
        this.DefaultCellStyle.Font = new Font("맑은 고딕", 10);


        this.RowTemplate.Height = 28;
        this.AllowUserToAddRows = false;
        this.AllowUserToResizeRows = false;
        this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.BackgroundColor = Color.White;
        this.BorderStyle = BorderStyle.None;


        // 행 번호
        this.RowHeadersWidth = 50;
        this.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


        // VirtualMode
        this.VirtualMode = true;
        this.CellValueNeeded += CustomDataGridView_CellValueNeeded;
        this.CellFormatting += CustomDataGridView_CellFormatting;


        // Hover/UX
        this.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) { _hoveredRow = e.RowIndex; this.InvalidateRow(e.RowIndex); } };
        this.CellMouseLeave += (s, e) => { if (_hoveredRow >= 0) { this.InvalidateRow(_hoveredRow); _hoveredRow = -1; } };


        // 컬럼 클릭 정렬
        this.ColumnHeaderMouseClick += CustomDataGridView_ColumnHeaderMouseClick;
    }

    public void SetDataTable(DataTable dt)
    {
        _dataTable = dt;
        this.RowCount = dt.Rows.Count;
        this.ColumnCount = dt.Columns.Count;

        for (int i = 0; i < dt.Columns.Count; i++)
        {
            this.Columns[i].Name = dt.Columns[i].ColumnName;
        }
    }

    private void CustomDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
        if (_dataTable != null && e.RowIndex < _dataTable.Rows.Count)
        {
            e.Value = _dataTable.Rows[e.RowIndex][e.ColumnIndex];
        }
    }

    private void CustomDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_cellFonts.TryGetValue((e.RowIndex, e.ColumnIndex), out Font f))
            e.CellStyle.Font = f;
        if (_cellColors.TryGetValue((e.RowIndex, e.ColumnIndex), out Color c))
            e.CellStyle.ForeColor = c;


        if (_hoveredRow == e.RowIndex)
            e.CellStyle.BackColor = Color.LightBlue;
    }


    public void SetCellFont(int rowIndex, int colIndex, Font font)
    {
        _cellFonts[(rowIndex, colIndex)] = font;
        this.InvalidateCell(colIndex, rowIndex);
    }


    public void SetCellForeColor(int rowIndex, int colIndex, Color color)
    {
        _cellColors[(rowIndex, colIndex)] = color;
        this.InvalidateCell(colIndex, rowIndex);
    }


    protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
    {
        base.OnRowPostPaint(e);
        string rowIdx = (e.RowIndex + 1).ToString();
        var center = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        Rectangle rect = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, this.RowHeadersWidth, e.RowBounds.Height);
        e.Graphics.DrawString(rowIdx, this.RowHeadersDefaultCellStyle.Font, Brushes.Black, rect, center);
    }


    private void CustomDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (_dataTable == null) return;

        string colName = this.Columns[e.ColumnIndex].Name;

        // 정렬 방향 토글: 같은 컬럼이면 방향 바꾸기, 아니면 기본 오름차순
        if (_sortedColumn == e.ColumnIndex)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortAscending = true;
            _sortedColumn = e.ColumnIndex;
        }

        var sortedRows = _sortAscending ?
            _dataTable.AsEnumerable().OrderBy(r => r[colName]) :
            _dataTable.AsEnumerable().OrderByDescending(r => r[colName]);

        _dataTable = sortedRows.CopyToDataTable();
        this.RowCount = _dataTable.Rows.Count;
        this.Invalidate();
    }

    public void ExportCsv(string path)
    {
        if (_dataTable == null) return;
        using (var sw = new StreamWriter(path))
        {
            // Header
            foreach (DataRow row in _dataTable.Rows)
            {
                sw.WriteLine(string.Join(",", row.ItemArray));
            }
        }
    }
}