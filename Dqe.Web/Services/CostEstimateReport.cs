using System;
using System.Collections.Generic;
using System.IO;
using Dqe.ApplicationServices.Reports;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.Core.Helper.HtmlToPdf;
using PdfRpt.FluentInterface;

namespace Dqe.Web.Services
{
    public class CostEstimateReport
    {
        public IPdfReportData CreatePayItemReport(List<CostEstimate> itemEntities)
        {
            var pdfReport = new PdfReport().DocumentPreferences(doc =>
            {
                doc.RunDirection(PdfRunDirection.LeftToRight);
                doc.Orientation(PageOrientation.Landscape);
                doc.PageSize(PdfPageSize.Legal);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "Kevin",
                    Application = "PdfRpt",
                    Keywords = "IList Rpt.",
                    Subject = "Test Rpt",
                    Title = "Test Item Average Report"
                });
            })
                .DefaultFonts(fonts =>
                {
                    fonts.Path(Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\arial.ttf",
                        Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\verdana.ttf");
                })
                .PagesFooter(footer =>
                {
                    footer.DefaultFooter(DateTime.Now.ToString("MM/dd/yyyy"));
                })
                .PagesHeader(header =>
                {
                    //header.DefaultHeader(defaultHeader =>
                    //{
                    //    defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                    //    //defaultHeader.ImagePath(AppPath.ApplicationPath + "\\Images\\01.png");
                    //    defaultHeader.Message("Florida Department of Transportation\n Item Average Unit Cost\n From 01/01/2014 to 12/12/2014");
                    //});
                    header.XHtmlHeader(rptHeader =>
                    {
                        rptHeader.PageHeaderProperties(new XHeaderBasicProperties
                        {
                            RunDirection = PdfRunDirection.LeftToRight,
                            ShowBorder = true
                        });
                        rptHeader.AddPageHeader(pageHeader =>
                        {
                            //var message = "Grouping employees by department and age. <hr size='1' width='90%' align='center' />";
                            var data = pageHeader.NewGroupInfo;
                            var proposal = data.GetSafeStringValueOf<CostEstimate>(x => x.ProposalNumber);
                            var photo = Path.Combine(AppPath.ApplicationPath, "Images\\01.png");
                            var image = string.Format("<img src='{0}' />", photo);
                            return string.Format(@"<table style='width: 100%;font-size:9pt;font-family:tahoma;'>
										            <tr>
                                                        <td align='left'>PPDEPSL</td>
                                                        <td align='center'>Florida Department of Transportation</td>
                                                        <td align='right'></td>
                                                    </tr>
										            <tr>
											            <td colspan='3' align='center'>PROPOSAL SUMMARY LOG FOR PROPOSAL {0}</td>
										            </tr>
								                </table>", proposal);
                        });

                        rptHeader.GroupHeaderProperties(new XHeaderBasicProperties
                        {
                            RunDirection = PdfRunDirection.LeftToRight,
                            ShowBorder = true,
                            SpacingBeforeTable = 10f
                        });
                        rptHeader.AddGroupHeader(groupHeader =>
                        {
                            var data = groupHeader.NewGroupInfo;
                            var category = data.GetSafeStringValueOf<CostEstimate>(x => x.Category);
                            var categoryDescription = data.GetSafeStringValueOf<CostEstimate>(x => x.CategoryDescription);
                            var fundingSources = data.GetSafeStringValueOf<CostEstimate>(x => x.FundingSource);
                            var constructionClass = data.GetSafeStringValueOf<CostEstimate>(x => x.ConstructionClass);
                            var bridgeId = data.GetSafeStringValueOf<CostEstimate>(x => x.BridgeId);
                            var bridgeType = data.GetSafeStringValueOf<CostEstimate>(x => x.BridgeType);
                            var maintenanceActivity = data.GetSafeStringValueOf<CostEstimate>(x => x.MaintenanceActivity);
                            var spans = data.GetSafeStringValueOf<CostEstimate>(x => x.BridgeSpans);
                            var workType = data.GetSafeStringValueOf<CostEstimate>(x => x.WorkType);
                            var bridgeLength = data.GetSafeStringValueOf<CostEstimate>(x => x.BridgeLength);
                            var catLength = data.GetSafeStringValueOf<CostEstimate>(x => x.CategoryLength);
                            var roadSection = data.GetSafeStringValueOf<CostEstimate>(x => x.RoadSectionNumber);
                            var bridgeWidth = data.GetSafeStringValueOf<CostEstimate>(x => x.BridgeWidth);
                            var catWidth = data.GetSafeStringValueOf<CostEstimate>(x => x.CategoryWidth);
                            var structureWorkClass = data.GetSafeStringValueOf<CostEstimate>(x => x.StructureWorkClass);
                            // http://demo.itextsupport.com/xmlworker/itextdoc/CSS-conformance-list.htm
                            return string.Format(@"<table style='width: 100%; font-size:9pt;font-family:tahoma;'>
										                     <tr>
                                                                <td style='width:10%'></td>
													            <td style='width:5%'>Category:</td>
													            <td style='width:20%'>{0}   {1}</td> 
                                                                <td style='width:10%'></td> 
                                                                <td style='width:50%'></td> 
                                                            </tr>
                                                            <tr style='width:100%'>
                                                                <td colspan='2'></td> 
                                                                <td>Funding Source(s) and Participation:</td>
													            <td>{2}</td>       
                                                                <td></td>                                       
												            </tr>	
                                                  </table>
                                                  <table style='width: 100%; font-size:9pt;font-family:tahoma;'>
										                    <tr>
                                                                <td style='width:10%'>Construction Class:</td>
                                                                <td style='width:30%'>{3}</td> 
                                                                <td style='width:10%'>Bridge Id:</td>
													            <td style='width:10%'>{4}</td> 
                                                                <td style='width:10%'>Bridge Type:</td>
													            <td style='width:10%'>{5}</td>  
                                                                <td style='width:20%'></td>                                                  
												            </tr>
                                                            <tr>
                                                                <td>Maintenance Activity:</td>
													            <td>{6}</td>
                                                                <td># of Spans:</td>
													            <td>{7}</td> 
                                                                <td>Work Type:</td>
													            <td colspan='2'>{8}</td> 
                                                            </tr>
												            <tr>
													            <td>Road Section Number:</td>
													            <td>{9}</td>
                                                                <td>Bridge Length:</td> 
                                                                <td>{10}</td>  
                                                                <td>Cat Length:</td> 
                                                                <td colspan='2'>{11}</td>  
												            </tr>	
												            <tr>
													            <td>Structure Work Class:</td>
													            <td>{12}</td>
                                                                <td>Bridge Width:</td> 
                                                                <td>{13}</td>  
                                                                <td>Cat Width:</td> 
                                                                <td colspan='2'>{14}</td>  
												            </tr>
                                                  </table>",
                                category, categoryDescription, fundingSources, constructionClass, bridgeId, bridgeType,
                                maintenanceActivity, spans, workType, roadSection, bridgeLength, catLength, structureWorkClass,
                                bridgeWidth, catWidth);
                        });
                    });
                })
                .MainTableTemplate(template =>
                {
                    template.BasicTemplate(BasicTemplate.ClassicTemplate);
                })
                .MainTablePreferences(table =>
                {
                    table.GroupsPreferences(new GroupsPreferences
                    {
                        //GroupType = GroupType.HideGroupingColumns,
                        RepeatHeaderRowPerGroup = true,
                        ShowOneGroupPerPage = true
                    });
                    //table.KeepTogether(true);

                    table.ColumnsWidthsType(TableColumnWidthType.Relative);
                    //table.NumberOfDataRowsPerPage(5);
                })
                .MainTableDataSource(dataSource =>
                {
                    dataSource.StronglyTypedList(itemEntities);
                })
                .MainTableSummarySettings(summarySettings =>
                {
                    //summarySettings.AllGroupsSummarySettings("Fin Proj Total");
                    //summarySettings.OverallSummarySettings("Category Total");
                    //summarySettings.PreviousPageSummarySettings("Previous Page Summary");
                    //summarySettings.PageSummarySettings("Page Summary");
                })
                .MainTableColumns(columns =>
                {
                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.Category);
                        column.IsVisible(false);
                        column.Group(
                            (val1, val2) =>
                            {
                                return val1.ToString() == val2.ToString();
                            });
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.LineNumber);
                        column.IsRowNumber(true);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(0);
                        column.Width(1);
                        column.HeaderCell("Line Number");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.ItemNumber);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(1);
                        column.Width(1);
                        column.HeaderCell("Item Number");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.ItemDescription);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(2);
                        column.Width(3);
                        column.HeaderCell("Item Description");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.EstimatedQuantity);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(3);
                        column.Width(2);
                        column.HeaderCell("Estimated Quantity");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.ItemUnit);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(4);
                        column.Width(1);
                        column.HeaderCell("Item Unit");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.UnitPrice);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(5);
                        column.Width(2);
                        column.HeaderCell("Unit Price");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CostEstimate>(x => x.Amount);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(6);
                        column.Width(2);
                        column.HeaderCell("Amount");
                        column.ColumnItemsTemplate(template =>
                        {
                            template.TextBlock();
                            template.DisplayFormatFormula(
                                obj => obj == null ? string.Empty : string.Format("{0:n0}", obj));
                        });
                        column.AggregateFunction(aggregateFunction =>
                        {
                            aggregateFunction.NumericAggregateFunction(AggregateFunction.Sum);
                            aggregateFunction.DisplayFormatFormula(
                                obj => obj == null ? string.Empty : string.Format("{0:n0}", obj));
                        });
                    });

                })
                .MainTableEvents(events =>
                {
                    //events.RowAddedInjectCustomRows(new Action<InjectCustomRowsBuilder>(builder => builder.AddRow()));
                    events.DataSourceIsEmpty(message: "There is no data available to display.");
                    events.GroupAdded(args =>
                    {
                        //args.PdfDoc.Add(new Phrase("\nGroup added event."));

                        /*var data = args.ColumnCellsSummaryData
                            .Where(data => data.CellData.PropertyName.Equals("propertyName")
                                   && data.GroupNumber == 1);*/
                        var data = args.PreviousTableRowData;
                        var table = new PdfGrid(1)
                        {
                            RunDirection = (int)PdfRunDirection.LeftToRight,
                            WidthPercentage = args.PageSetup.MainTablePreferences.WidthPercentage
                        };
                        var htmlCell = new XmlWorkerHelper
                        {
                            // the registered fonts (DefaultFonts section) should be specified here
                            Html = string.Format(@"<table style='width: 100%;font-size:9pt;font-family:tahoma;'>
										            <tr>
                                                        <td style='width:10%'></td>
                                                        <td style='width:64%'></td>
                                                        <td style='width:17%'></td>
                                                        <td style='width:9%'></td>
                                                    </tr>
										            <tr>
											            <td></td>
                                                        <td>X Excluded From Category Total</td>
                                                        <td colspan='2'></td>
										            </tr>
										            <tr>
											            <td></td>
                                                        <td>( ) Will Be Bid As Lump Sum</td>
                                                        <td colspan='2'></td>
										            </tr>
										            <tr>
											            <td colspan='2'></td>
                                                        <td>{0}  Category Total</td>
                                                        <td>{1}</td>
										            </tr>
								                </table>", data.GetSafeStringValueOf<CostEstimate>(x => x.Category), data.GetSafeStringValueOf<CostEstimate>(x => x.GroupAmount)),
                            RunDirection = PdfRunDirection.LeftToRight,
                            CssFilesPath = null, // optional
                            ImagesPath = null, // optional
                            InlineCss = null, // optional
                            DefaultFont = args.PdfFont.Fonts[1] // verdana
                        }.RenderHtml();
                        htmlCell.Border = 0;
                        table.AddCell(htmlCell);
                        table.SpacingBefore = args.PageSetup.MainTablePreferences.SpacingBefore;

                        args.PdfDoc.Add(table);
                    });
                })
                .Export(export =>
                {
                    export.ToExcel();
                    export.ToCsv();
                    export.ToXml();
                })
                .Generate(data => data.FlushInBrowser());

            return pdfReport;
        }
    }
}