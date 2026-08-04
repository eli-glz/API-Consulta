Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports ClosedXML.Excel
Imports MigraDoc.DocumentObjectModel
Imports MigraDoc.Rendering

Namespace Exporting
    Public Module ExportService
        Public Sub ExportCsv(title As String, table As DataTable, filePath As String)
            Using writer As New StreamWriter(filePath, False, New UTF8Encoding(True))
                writer.WriteLine(String.Join(
                    ";",
                    table.Columns.Cast(Of DataColumn)().
                        Select(Function(column) EscapeCsv(column.ColumnName))))

                For Each row As DataRow In table.Rows
                    writer.WriteLine(String.Join(
                        ";",
                        row.ItemArray.Select(Function(value) EscapeCsv(CellText(value)))))
                Next
            End Using
        End Sub

        Public Sub ExportExcel(title As String, table As DataTable, filePath As String)
            Using workbook As New XLWorkbook()
                Dim worksheet = workbook.Worksheets.Add(SafeWorksheetName(title))

                For columnIndex = 0 To table.Columns.Count - 1
                    worksheet.Cell(1, columnIndex + 1).Value =
                        table.Columns(columnIndex).ColumnName
                Next

                For rowIndex = 0 To table.Rows.Count - 1
                    For columnIndex = 0 To table.Columns.Count - 1
                        worksheet.Cell(rowIndex + 2, columnIndex + 1).Value =
                            CellText(table.Rows(rowIndex)(columnIndex))
                    Next
                Next

                Dim completeRange = worksheet.Range(
                    1,
                    1,
                    table.Rows.Count + 1,
                    table.Columns.Count)

                completeRange.CreateTable()
                worksheet.SheetView.FreezeRows(1)
                worksheet.Columns().AdjustToContents()

                For Each column In worksheet.ColumnsUsed()
                    If column.Width > 45 Then
                        column.Width = 45
                    End If
                Next

                workbook.SaveAs(filePath)
            End Using
        End Sub

        Public Sub ExportPdf(title As String, sourceTable As DataTable, filePath As String)
            Dim table = CreatePrintableTable(sourceTable)
            Dim document As New Document()

            If table.Columns.Count = 0 Then
                Throw New InvalidOperationException("No hay columnas para exportar.")
            End If

            document.Info.Title = title
            document.Styles("Normal").Font.Name = "Segoe UI"
            document.Styles("Normal").Font.Size = Unit.FromPoint(8)

            Dim section = document.AddSection()
            section.PageSetup.PageFormat = PageFormat.A4
            section.PageSetup.Orientation = Orientation.Landscape
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1)
            section.PageSetup.RightMargin = Unit.FromCentimeter(1)
            section.PageSetup.TopMargin = Unit.FromCentimeter(1)
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1)

            Dim titleParagraph = section.AddParagraph(title)
            titleParagraph.Format.Font.Bold = True
            titleParagraph.Format.Font.Size = Unit.FromPoint(14)
            titleParagraph.Format.SpaceAfter = Unit.FromCentimeter(0.4)

            Dim pdfTable = section.AddTable()
            pdfTable.Borders.Width = Unit.FromPoint(0.5)

            Dim columnWidth = 27.0R / table.Columns.Count
            For columnIndex = 0 To table.Columns.Count - 1
                pdfTable.AddColumn(Unit.FromCentimeter(columnWidth))
            Next

            Dim header = pdfTable.AddRow()
            header.HeadingFormat = True
            header.Format.Font.Bold = True
            header.Format.Font.Color = Colors.White

            For columnIndex = 0 To table.Columns.Count - 1
                header.Cells(columnIndex).Shading.Color = Colors.DimGray
                header.Cells(columnIndex).AddParagraph(
                    table.Columns(columnIndex).ColumnName)
            Next

            For Each dataRow As DataRow In table.Rows
                Dim pdfRow = pdfTable.AddRow()

                For columnIndex = 0 To table.Columns.Count - 1
                    pdfRow.Cells(columnIndex).AddParagraph(
                        CellText(dataRow(columnIndex)))
                Next
            Next

            Dim renderer As New PdfDocumentRenderer With {
                .Document = document
            }

            renderer.RenderDocument()
            renderer.PdfDocument.Save(filePath)
        End Sub

        Public Sub ExportRtf(title As String, sourceTable As DataTable, filePath As String)
            Dim table = CreatePrintableTable(sourceTable)
            Dim builder As New StringBuilder()

            builder.AppendLine(
                "{\rtf1\ansi\ansicpg1252\deff0" &
                "\landscape\paperw16840\paperh11907" &
                "\margl720\margr720\margt720\margb720")
            builder.AppendLine("{\fonttbl{\f0 Segoe UI;}}")
            builder.Append("\f0\fs20\b ")
            builder.Append(EscapeRtf(title))
            builder.AppendLine("\b0\par\par")

            AppendRtfRow(
                builder,
                table.Columns.Cast(Of DataColumn)().
                    Select(Function(column) column.ColumnName).ToArray(),
                True)

            For Each row As DataRow In table.Rows
                AppendRtfRow(
                    builder,
                    row.ItemArray.Select(Function(value) CellText(value)).ToArray(),
                    False)
            Next

            builder.AppendLine("}")
            File.WriteAllText(filePath, builder.ToString(), Encoding.ASCII)
        End Sub

        Private Function CreatePrintableTable(source As DataTable) As DataTable
            If source.Rows.Count <> 1 OrElse source.Columns.Count <= 8 Then
                Return source
            End If

            Dim result As New DataTable(source.TableName)
            result.Columns.Add("Campo")
            result.Columns.Add("Valor")

            For columnIndex = 0 To source.Columns.Count - 1
                result.Rows.Add(
                    source.Columns(columnIndex).ColumnName,
                    CellText(source.Rows(0)(columnIndex)))
            Next

            Return result
        End Function

        Private Sub AppendRtfRow(
            builder As StringBuilder,
            values As String(),
            isHeader As Boolean)
            If values.Length = 0 Then
                Return
            End If
            Dim cellWidth = 14000 \ values.Length

            builder.Append("\trowd\trgaph108")

            For index = 1 To values.Length
                builder.Append("\cellx")
                builder.Append(cellWidth * index)
            Next

            builder.AppendLine()

            For Each value In values
                builder.Append("\intbl ")

                If isHeader Then
                    builder.Append("\b ")
                End If

                builder.Append(EscapeRtf(value))

                If isHeader Then
                    builder.Append("\b0")
                End If

                builder.Append("\cell ")
            Next

            builder.AppendLine("\row")
        End Sub

        Private Function EscapeCsv(value As String) As String
            Dim text = If(value, "")

            If text.Contains(";"c) OrElse
               text.Contains(""""c) OrElse
               text.Contains(vbCr) OrElse
               text.Contains(vbLf) Then

                Return """" & text.Replace("""", """""") & """"
            End If

            Return text
        End Function

        Private Function EscapeRtf(value As String) As String
            Dim builder As New StringBuilder()

            For Each character In If(value, "")
                Select Case character
                    Case "\"c
                        builder.Append("\\")
                    Case "{"c
                        builder.Append("\{")
                    Case "}"c
                        builder.Append("\}")
                    Case ChrW(13)
                    Case ChrW(10)
                        builder.Append("\line ")
                    Case Else
                        Dim code = AscW(character)

                        If code < 32 OrElse code > 126 Then
                            builder.Append("\u")
                            builder.Append(code)
                            builder.Append("?")
                        Else
                            builder.Append(character)
                        End If
                End Select
            Next

            Return builder.ToString()
        End Function

        Private Function CellText(value As Object) As String
            If value Is Nothing OrElse value Is DBNull.Value Then
                Return ""
            End If

            Return Convert.ToString(
                value,
                CultureInfo.GetCultureInfo("es-AR"))
        End Function

        Private Function SafeWorksheetName(title As String) As String
            Dim result = If(title, "Datos")

            For Each character In New Char() {
                ":"c, "\"c, "/"c, "?"c, "*"c, "["c, "]"c
            }
                result = result.Replace(character, "_"c)
            Next

            If result.Length > 31 Then
                result = result.Substring(0, 31)
            End If

            Return If(String.IsNullOrWhiteSpace(result), "Datos", result)
        End Function
    End Module
End Namespace