using System;
using System.Collections.Generic;
using Dqe.ApplicationServices.Reports;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;

namespace Dqe.Web.Services
{
    public class CategoryFundingSummaryReport
    {
        public IPdfReportData CreatePayItemReport(List<CategoryFundingSummary> itemEntities)
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
                            return string.Format(@"<table style='width: 100%;font-size:9pt;font-family:tahoma;'>
										            <tr>
                                                        <td align='left'></td>
                                                        <td align='center'>Florida Department of Transportation</td>
                                                        <td align='right'></td>
                                                    </tr>
										            <tr>
											            <td colspan='3' align='center'>Category Funding Summary By Fin Proj Fund</td>
										            </tr>
								                </table>");
                        });

                        rptHeader.GroupHeaderProperties(new XHeaderBasicProperties
                        {
                            RunDirection = PdfRunDirection.LeftToRight,
                            ShowBorder = false,
                            SpacingBeforeTable = 10f
                        });
                        rptHeader.AddGroupHeader(groupHeader =>
                        {
                            var data = groupHeader.NewGroupInfo;
                            var financialProject = data.GetSafeStringValueOf<CategoryFundingSummary>(x => x.FinancialProjectNumber);
                            var federalAidNumber = data.GetSafeStringValueOf<CategoryFundingSummary>(x => x.FederalAidNumber);
                            var proposal = data.GetSafeStringValueOf<CategoryFundingSummary>(x => x.ProposalNumber);
                            var fundClass = data.GetSafeStringValueOf<CategoryFundingSummary>(x => x.FundClass);
                            return string.Format(@"<table style='width: 100%;font-size:9pt;font-family:tahoma;'>
										            <tr>
                                                        <td style='width:10%'>Fin Proj #</td>
                                                        <td style='width:30%'>{0}</td>
                                                        <td style='width:10%'>Federal Aid #</td>
                                                        <td>{1}</td>
                                                    </tr>
										            <tr>
											            <td>Proposal #</td>
                                                        <td>{2}</td>
                                                        <td colspan='2'></td>
										            </tr>
										            <tr>
											            <td>Fund Class</td>
                                                        <td>{3}</td>
                                                        <td colspan='2'></td>
										            </tr>
                                                  </table>", financialProject, federalAidNumber, proposal, fundClass);
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
                        ShowOneGroupPerPage = false
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
                        column.PropertyName<CategoryFundingSummary>(x => x.FundClass);
                        column.IsVisible(false);
                        column.Group(
                            (val1, val2) =>
                            {
                                return val1.ToString() == val2.ToString();
                            });
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CategoryFundingSummary>(x => x.Category);
                        column.IsRowNumber(true);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(0);
                        column.Width(1);
                        column.HeaderCell("Category");
                    });

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CategoryFundingSummary>(x => x.Cost);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(1);
                        column.Width(2);
                        column.HeaderCell("Cost");
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

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CategoryFundingSummary>(x => x.ConstEngr);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(2);
                        column.Width(2);
                        column.HeaderCell("Const Engr");
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

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CategoryFundingSummary>(x => x.Total);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(3);
                        column.Width(2);
                        column.HeaderCell("Total");
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

                    columns.AddColumn(column =>
                    {
                        column.PropertyName<CategoryFundingSummary>(x => x.Funding);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(4);
                        column.Width(2);
                        column.HeaderCell("Funding");
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