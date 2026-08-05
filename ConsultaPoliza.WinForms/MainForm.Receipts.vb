Imports System.Globalization
Imports System.Linq

Partial Public Class MainForm
    Private Sub ShowReceiptsSection(
        Optional selectedReceiptIndex As Integer? = Nothing)

        If _currentPolicy.Recibos Is Nothing OrElse
        _currentPolicy.Recibos.Count = 0 Then

            ShowUnavailableSection("Recibos")
            Return
        End If

        Dim totalItems = _currentPolicy.Recibos.Count
        Dim totalPages =
            CInt(Math.Ceiling(totalItems / CDbl(ReceiptsPageSize)))

        If selectedReceiptIndex.HasValue Then
            _receiptsPageIndex =
                selectedReceiptIndex.Value \ ReceiptsPageSize
        End If

        _receiptsPageIndex =
            Math.Max(0, Math.Min(_receiptsPageIndex, totalPages - 1))

        Dim pageReceipts = _currentPolicy.Recibos.
            Skip(_receiptsPageIndex * ReceiptsPageSize).
            Take(ReceiptsPageSize).
            ToList()

        Dim rows As New List(Of String())()

        For Each receipt In pageReceipts
            rows.Add(New String() {
                receipt.NumeroRecibo.ToString(CultureInfo.InvariantCulture),
                FormatDate(receipt.FechaEmision),
                FormatDate(receipt.InicioVigencia),
                FormatDate(receipt.FinVigencia),
                FormatDate(receipt.FechaUltimoPago),
                FormatDate(receipt.FechaVencimiento),
                If(receipt.Moneda, "").Trim(),
                FormatAmount(receipt.Premio),
                FormatAmount(receipt.Saldo),
                If(receipt.Estado, "").Trim(),
                FormatDate(receipt.FechaAnulacion),
                If(receipt.CodigoAnulacion, "").Trim(),
                If(receipt.Situacion, "").Trim(),
                FormatDate(receipt.FechaSegundoVencimiento)
            })
        Next

        ShowGridSection(
            "Recibos",
            New String() {
                "Recibo", "Emision", "Inicio vig.", "Fin vig.",
                "Ultimo pago", "Fec. Vto.", "Moneda", "Premio",
                "Saldo", "Estado", "Fecha anulacion",
                "Codigo anulacion", "Situacion del Recibo",
                "Fec. 2do Vto."
            },
            rows,
            CreateReceiptsPager(totalItems, totalPages))

        If _currentExportGrid IsNot Nothing Then
            _currentExportGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells

            For Each column As DataGridViewColumn In _currentExportGrid.Columns
                column.MinimumWidth = 90
            Next
        End If

        If selectedReceiptIndex.HasValue AndAlso
        _currentExportGrid IsNot Nothing Then

            Dim localIndex =
                selectedReceiptIndex.Value Mod ReceiptsPageSize

            If localIndex >= 0 AndAlso
            localIndex < _currentExportGrid.Rows.Count Then

                _currentExportGrid.ClearSelection()
                _currentExportGrid.Rows(localIndex).Selected = True
                _currentExportGrid.CurrentCell =
                    _currentExportGrid.Rows(localIndex).Cells(0)
            End If
        End If
    End Sub

    Private Function CreateReceiptsPager(
        totalItems As Integer,
        totalPages As Integer) As Control

        Dim pager As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .AutoScroll = True,
            .Padding = New Padding(6, 5, 6, 3),
            .BackColor = Color.White
        }

        pager.Controls.Add(New Label With {
            .AutoSize = True,
            .Text =
                "Pagina " & (_receiptsPageIndex + 1).ToString() &
                " de " & totalPages.ToString() &
                " (" & totalItems.ToString() & " elementos)",
            .Margin = New Padding(0, 9, 10, 0)
        })

        Dim previousButton As New Button With {
            .Text = "<",
            .Width = 32,
            .Height = 36,
            .Enabled = _receiptsPageIndex > 0,
            .Margin = New Padding(2, 0, 2, 0)
        }

        AddHandler previousButton.Click,
            Sub(sender, e)
                ChangeReceiptsPage(_receiptsPageIndex - 1)
            End Sub

        pager.Controls.Add(previousButton)

        For pageNumber = 1 To totalPages
            Dim targetPage = pageNumber - 1

            Dim pageButton As New Button With {
                .Text = pageNumber.ToString(),
                .Width = 34,
                .Height = 36,
                .Enabled = targetPage <> _receiptsPageIndex,
                .Margin = New Padding(2, 0, 2, 0)
            }

            AddHandler pageButton.Click,
                Sub(sender, e)
                    ChangeReceiptsPage(targetPage)
                End Sub

            pager.Controls.Add(pageButton)
        Next

        Dim nextButton As New Button With {
            .Text = ">",
            .Width = 32,
            .Height = 36,
            .Enabled = _receiptsPageIndex < totalPages - 1,
            .Margin = New Padding(2, 0, 2, 0)
        }

        AddHandler nextButton.Click,
            Sub(sender, e)
                ChangeReceiptsPage(_receiptsPageIndex + 1)
            End Sub

        pager.Controls.Add(nextButton)
        Return pager
    End Function

    Private Sub ChangeReceiptsPage(pageIndex As Integer)
        _receiptsPageIndex = pageIndex
        ShowReceiptsSection()
    End Sub
End Class