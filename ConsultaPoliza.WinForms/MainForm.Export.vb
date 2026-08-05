Imports System.Data
Imports System.IO
Imports System.Linq
Imports ConsultaPoliza.WinForms.Exporting

Partial Public Class MainForm
    Private Sub CsvExportButton_Click(sender As Object, e As EventArgs)
        ExportCurrent(
            ".csv",
            "Archivo CSV (*.csv)|*.csv",
            AddressOf ExportService.ExportCsv)
    End Sub

    Private Sub ExcelExportButton_Click(sender As Object, e As EventArgs)
        ExportCurrent(
            ".xlsx",
            "Libro de Excel (*.xlsx)|*.xlsx",
            AddressOf ExportService.ExportExcel)
    End Sub

    Private Sub PdfExportButton_Click(sender As Object, e As EventArgs)
        ExportCurrent(
            ".pdf",
            "Documento PDF (*.pdf)|*.pdf",
            AddressOf ExportService.ExportPdf)
    End Sub

    Private Sub RtfExportButton_Click(sender As Object, e As EventArgs)
        ExportCurrent(
            ".rtf",
            "Documento RTF (*.rtf)|*.rtf",
            AddressOf ExportService.ExportRtf)
    End Sub

    Private Function CreateSelectedExportTable() As DataTable
        If _currentExportTable Is Nothing Then
            Return Nothing
        End If

        If _currentExportGrid Is Nothing Then
            Return _currentExportTable.Copy()
        End If

        If _currentExportGrid.SelectedRows.Count = 0 Then
            Return Nothing
        End If

        Dim selectedTable = _currentExportTable.Clone()

        Dim selectedRows = _currentExportGrid.SelectedRows.
            Cast(Of DataGridViewRow)().
            OrderBy(Function(row) row.Index).
            ToList()

        For Each gridRow In selectedRows
            Dim rowView = TryCast(gridRow.DataBoundItem, DataRowView)

            If rowView IsNot Nothing Then
                selectedTable.ImportRow(rowView.Row)
            End If
        Next

        Return selectedTable
    End Function

    Private Sub ExportCurrent(
        extension As String,
        filter As String,
        exporter As Action(Of String, DataTable, String))

        If _currentExportTable Is Nothing OrElse
        _currentExportTable.Rows.Count = 0 Then

            SetStatus("No hay datos visibles para exportar.", True)
            Return
        End If

        Dim tableToExport = CreateSelectedExportTable()

        If tableToExport Is Nothing OrElse tableToExport.Rows.Count = 0 Then
            SetStatus("Seleccione al menos una fila para exportar.", True)
            Return
        End If

        Using dialog As New SaveFileDialog With {
            .Filter = filter,
            .DefaultExt = extension.TrimStart("."c),
            .AddExtension = True,
            .FileName = BuildExportFileName(extension),
            .Title = "Exportar " & _currentExportTitle
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Try
                exporter(
                    _currentExportTitle,
                    tableToExport,
                    dialog.FileName)

                SetStatus(
                    "Exportacion creada: " &
                    Path.GetFileName(dialog.FileName))
            Catch ex As Exception
                SetStatus(
                    "No se pudo crear la exportacion: " & ex.Message,
                    True)
            End Try
        End Using
    End Sub

    Private Function BuildExportFileName(extension As String) As String
        Dim policyNumber = "sin_poliza"

        If _currentPolicy IsNot Nothing AndAlso
        Not String.IsNullOrWhiteSpace(_currentPolicy.NumeroPoliza) Then

            policyNumber = _currentPolicy.NumeroPoliza.Trim()
        End If

        Dim fileName =
            "Poliza_" &
            policyNumber &
            "_" &
            If(_currentExportTitle, "Datos") &
            "_" &
            DateTime.Now.ToString("yyyyMMdd_HHmmss") &
            extension

        For Each invalidCharacter In Path.GetInvalidFileNameChars()
            fileName = fileName.Replace(invalidCharacter, "_"c)
        Next

        Return fileName
    End Function

    Private Sub SetExportData(title As String, table As DataTable, Optional grid As DataGridView = Nothing)
        _currentExportTitle = If(title, "")
        _currentExportTable = table
        _currentExportGrid = grid

        SetExportButtonsEnabled(
            table IsNot Nothing AndAlso
            table.Columns.Count > 0 AndAlso
            table.Rows.Count > 0)
    End Sub

    Private Sub SetExportButtonsEnabled(enabled As Boolean)
        _csvExportButton.Enabled = enabled
        _excelExportButton.Enabled = enabled
        _pdfExportButton.Enabled = enabled
        _rtfExportButton.Enabled = enabled
    End Sub
End Class