using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dqe.ApplicationServices
{
    public interface IDocumentConverterService
    {
        byte[] ConvertHtmlToPdf(string html);
        byte[] ConvertUrlToPdf(string url);
    }
}
