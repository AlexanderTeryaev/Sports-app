using iTextSharp.text;
using iTextSharp.text.pdf;
using Resources;

namespace CmsApp.Helpers
{
    public class PDFFooter : PdfPageEventHelper
    {
        PdfPTable _tabHeader;
        float _headerExtra;

        public PDFFooter(float headerExtra, PdfPTable tabHeader)
        {
            _tabHeader = tabHeader;
            _headerExtra = headerExtra;
        }

        // write on top of document
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            base.OnOpenDocument(writer, document);
        }

        // write on start of each page
        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
            _tabHeader.TotalWidth = document.PageSize.Width - 2 * document.Left;
            _tabHeader.WriteSelectedRows(0, -1, document.Left, document.Top + _headerExtra + 30, writer.DirectContent);
        }

        // write on end of each page
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

        }

        //write on close of document
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

        }

    }

}