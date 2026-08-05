Imports System.Data

Partial Public Class MainForm
    Private Sub ShowUnavailableSection(title As String)
        Dim section = CreateSectionLayout(title)
        section.Controls.Add(CreateEmptyStateLabel("No hay datos disponibles para esta seccion con la respuesta actual de la API."), 0, 1)
        SetExportData("", Nothing)
        SetContent(section)
    End Sub

    Private Sub ShowNoPolicySelected()
        Dim section = CreateSectionLayout("Poliza")
        section.Controls.Add(CreateEmptyStateLabel("Ingrese una poliza y presione OK para ver el menu lateral."), 0, 1)
        SetExportData("", Nothing)
        SetContent(section)
    End Sub

    Private Sub ShowGridSection(
        title As String,
        columns As String(),
        rows As List(Of String()),
        Optional footer As Control = Nothing)

        If rows.Count = 0 Then
            ShowUnavailableSection(title)
            Return
        End If

        Dim table = CreateExportTable(title, columns, rows)
        Dim section = CreateSectionLayout(title)

        Dim grid As New DataGridView With {
            .Dock = DockStyle.Fill,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .AutoGenerateColumns = True,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            .ReadOnly = True,
            .RowHeadersVisible = False,
            .MultiSelect = True,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .DataSource = table
        }

        For Each column As DataGridViewColumn In grid.Columns
            column.SortMode = DataGridViewColumnSortMode.NotSortable
        Next

        If footer IsNot Nothing Then
            section.RowCount = 3
            section.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 50))
            section.Controls.Add(footer, 0, 2)
        End If

        section.Controls.Add(grid, 0, 1)
        SetContent(section)
        grid.CurrentCell = Nothing
        grid.ClearSelection()
        SetExportData(title, table, grid)
    End Sub

    Private Shared Function CreateExportTable(
        title As String,
        columns As String(),
        rows As List(Of String())) As DataTable

        Dim table As New DataTable(title)

        For Each columnName In columns
            table.Columns.Add(columnName)
        Next

        For Each sourceRow In rows
            Dim values(sourceRow.Length - 1) As Object
            Array.Copy(sourceRow, values, sourceRow.Length)
            table.Rows.Add(values)
        Next

        Return table
    End Function

    Private Function CreateSectionLayout(title As String) As TableLayoutPanel
        Dim section As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Margin = New Padding(0),
            .BackColor = Color.White
        }

        section.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        section.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim header As New Label With {
            .Dock = DockStyle.Fill,
            .Text = title,
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.FromArgb(232, 236, 242),
            .ForeColor = Color.FromArgb(35, 42, 52),
            .Font = New Font(Font, FontStyle.Bold),
            .BorderStyle = BorderStyle.FixedSingle
        }

        section.Controls.Add(header, 0, 0)
        Return section
    End Function

    Private Sub AddDetailRow(layout As TableLayoutPanel, row As Integer, labelText As String, valueText As String)
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))

        Dim label As New Label With {
            .Text = labelText,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleRight,
            .Padding = New Padding(0, 0, 12, 0),
            .ForeColor = Color.FromArgb(0, 0, 0),
            .Font = New Font(Font, FontStyle.Bold)
        }

        Dim valueLabel As New Label With {
            .Text = If(valueText, ""),
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = Color.FromArgb(0, 0, 0),
            .AutoEllipsis = True
        }

        layout.Controls.Add(label, 0, row)
        layout.Controls.Add(valueLabel, 1, row)
    End Sub

    Private Shared Function CreateEmptyStateLabel(text As String) As Label
        Return New Label With {
            .Dock = DockStyle.Fill,
            .Text = text,
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = Color.DimGray,
            .BackColor = Color.White,
            .AutoEllipsis = True
        }
    End Function

    Private Sub SetContent(control As Control)
        For index = _contentPanel.Controls.Count - 1 To 0 Step -1
            Dim existingControl = _contentPanel.Controls(index)
            _contentPanel.Controls.RemoveAt(index)
            existingControl.Dispose()
        Next

        _contentPanel.Controls.Add(control)
    End Sub

    Private Sub SetBusy(isBusy As Boolean, Optional message As String = "")
        UseWaitCursor = isBusy
        _branchComboBox.Enabled = Not isBusy AndAlso _branchComboBox.Items.Count > 0
        _policyNumberTextBox.Enabled = Not isBusy
        _certificateNumeric.Enabled = Not isBusy
        _effectiveDatePicker.Enabled = Not isBusy
        _searchButton.Enabled = Not isBusy AndAlso _branchComboBox.Items.Count > 0
        _navigationTree.Enabled = Not isBusy AndAlso _currentPolicy IsNot Nothing
        SetExportButtonsEnabled(
            Not isBusy AndAlso
            _currentExportTable IsNot Nothing AndAlso
            _currentExportTable.Rows.Count > 0)

        If Not String.IsNullOrWhiteSpace(message) Then
            SetStatus(message)
        End If
    End Sub

    Private Sub SetStatus(message As String, Optional isError As Boolean = False)
        _statusLabel.Text = message
        _statusLabel.ForeColor = If(
            isError,
            Color.Firebrick,
            If(String.IsNullOrWhiteSpace(message), Color.DimGray, Color.DarkGreen))
    End Sub
End Class