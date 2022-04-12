using System;
using System.Data;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace CFW.Web
{
	/// <summary>
	/// GridView를 엑셀로 출력하는 클래스
	/// </summary>
	public class GridViewExportUtil
	{
		/// <summary>
		/// GridView를 지정한 파일명의 엑셀파일로 출력합니다
		/// 파일명엔 반드시 확장자를 붙여야 합니다(.xls)
		/// </summary>
		/// <param name="fileName">xxx.xls</param>
		/// <param name="gv">GridView</param>
        public static void Export(string fileName, GridView gv, GridView gv1, GridView gv2, GridView gv3)
		{
			HttpContext.Current.Response.Clear();
			HttpContext.Current.Response.AddHeader(
				"content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.Charset = "euc-kr";
			HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding(949);

           
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
			using (StringWriter sw = new StringWriter())
			{
				using (HtmlTextWriter htw = new HtmlTextWriter(sw))
				{
					//  Create a form to contain the grid
					Table table = new Table();

                    if (gv.Rows.Count > 0)
                    {
                        table.BorderStyle = BorderStyle.Solid;
                        table.BorderColor = System.Drawing.Color.Black;
                        table.GridLines = GridLines.Both;
                    }

					//  add the header row to the table
					if (gv.HeaderRow != null)
					{
						GridViewExportUtil.PrepareControlForExport(gv.HeaderRow);
						table.Rows.Add(gv.HeaderRow);
					}

					//  add each of the data rows to the table
					foreach (GridViewRow row in gv.Rows)
					{
						GridViewExportUtil.PrepareControlForExport(row);
						table.Rows.Add(row);
					}

					//  add the footer row to the table
					if (gv.FooterRow != null)
					{
						GridViewExportUtil.PrepareControlForExport(gv.FooterRow);
						table.Rows.Add(gv.FooterRow);
					}

					//  render the table into the htmlwriter
					table.RenderControl(htw);

                    Table emptyTable = new Table();
                    emptyTable.RenderControl(htw);


                    Table table1 = new Table();

                    if (gv1.Rows.Count > 0)
                    {
                        table1.BorderStyle = BorderStyle.Solid;
                        table1.BorderColor = System.Drawing.Color.Black;
                        table1.GridLines = GridLines.Both;
                    }

                    //  add the header row to the table
                    if (gv1.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv1.HeaderRow);
                        table1.Rows.Add(gv1.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row1 in gv1.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row1);
                        table1.Rows.Add(row1);
                    }

                    //  add the footer row to the table
                    if (gv1.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv1.FooterRow);
                        table1.Rows.Add(gv1.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table1.RenderControl(htw);

                    Table emptyTable1 = new Table();
                    emptyTable1.RenderControl(htw);

                    Table table2 = new Table();

                    if (gv2.Rows.Count > 0)
                    {
                        table2.BorderStyle = BorderStyle.Solid;
                        table2.BorderColor = System.Drawing.Color.Black;
                        table2.GridLines = GridLines.Both;
                    }


                    //  add the header row to the table
                    if (gv2.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv2.HeaderRow);
                        table2.Rows.Add(gv2.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row2 in gv2.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row2);
                        table2.Rows.Add(row2);
                    }

                    //  add the footer row to the table
                    if (gv2.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv2.FooterRow);
                        table2.Rows.Add(gv2.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table2.RenderControl(htw);

                    Table emptyTable2 = new Table();
                    emptyTable2.RenderControl(htw);
                    
                    Table table3 = new Table();

                    if (gv3.Rows.Count > 0)
                    {
                        table3.BorderStyle = BorderStyle.Solid;
                        table3.BorderColor = System.Drawing.Color.Black;
                        table3.GridLines = GridLines.Both;
                    }

                    //  add the header row to the table
                    if (gv3.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv3.HeaderRow);
                        table3.Rows.Add(gv3.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row3 in gv3.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row3);
                        table3.Rows.Add(row3);
                    }

                    //  add the footer row to the table
                    if (gv3.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv3.FooterRow);
                        table3.Rows.Add(gv3.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table3.RenderControl(htw);

					//  render the htmlwriter into the response
					HttpContext.Current.Response.Write(sw.ToString());
					HttpContext.Current.Response.End();
				}
			}
		}

        /// <summary>
        /// GridView를 지정한 파일명의 엑셀파일로 출력합니다
        /// 파일명엔 반드시 확장자를 붙여야 합니다(.xls)
        /// </summary>
        /// <param name="fileName">xxx.xls</param>
        /// <param name="gv">GridView</param>
        public static void Export2(string fileName, GridView gv, GridView gv1, GridView gv2, GridView gv3, GridView gv4)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.Charset = "euc-kr";
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding(949);


            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    //  Create a form to contain the grid
                    Table table = new Table();

                    if (gv.Rows.Count > 0)
                    {
                        table.BorderStyle = BorderStyle.Solid;
                        table.BorderColor = System.Drawing.Color.Black;
                        table.GridLines = GridLines.Both;
                    }

                    //  add the header row to the table
                    if (gv.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv.HeaderRow);
                        table.Rows.Add(gv.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row in gv.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row);
                        table.Rows.Add(row);
                    }

                    //  add the footer row to the table
                    if (gv.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv.FooterRow);
                        table.Rows.Add(gv.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table.RenderControl(htw);

                    Table emptyTable = new Table();
                    emptyTable.RenderControl(htw);


                    Table table1 = new Table();

                    if (gv1.Rows.Count > 0)
                    {
                        table1.BorderStyle = BorderStyle.Solid;
                        table1.BorderColor = System.Drawing.Color.Black;
                        table1.GridLines = GridLines.Both;
                    }

                    //  add the header row to the table
                    if (gv1.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv1.HeaderRow);
                        table1.Rows.Add(gv1.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row1 in gv1.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row1);
                        table1.Rows.Add(row1);
                    }

                    //  add the footer row to the table
                    if (gv1.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv1.FooterRow);
                        table1.Rows.Add(gv1.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table1.RenderControl(htw);

                    Table emptyTable1 = new Table();
                    emptyTable1.RenderControl(htw);

                    Table table2 = new Table();

                    if (gv2.Rows.Count > 0)
                    {
                        table2.BorderStyle = BorderStyle.Solid;
                        table2.BorderColor = System.Drawing.Color.Black;
                        table2.GridLines = GridLines.Both;
                    }


                    //  add the header row to the table
                    if (gv2.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv2.HeaderRow);
                        table2.Rows.Add(gv2.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row2 in gv2.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row2);
                        table2.Rows.Add(row2);
                    }

                    //  add the footer row to the table
                    if (gv2.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv2.FooterRow);
                        table2.Rows.Add(gv2.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table2.RenderControl(htw);

                    Table emptyTable2 = new Table();
                    emptyTable2.RenderControl(htw);

                    Table table3 = new Table();

                    if (gv3.Rows.Count > 0)
                    {
                        table3.BorderStyle = BorderStyle.Solid;
                        table3.BorderColor = System.Drawing.Color.Black;
                        table3.GridLines = GridLines.Both;
                    }

                    //  add the header row to the table
                    if (gv3.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv3.HeaderRow);
                        table3.Rows.Add(gv3.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row3 in gv3.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row3);
                        table3.Rows.Add(row3);
                    }

                    //  add the footer row to the table
                    if (gv3.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv3.FooterRow);
                        table3.Rows.Add(gv3.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table3.RenderControl(htw);

                    Table emptyTable3 = new Table();
                    emptyTable3.RenderControl(htw);

                    Table table4 = new Table();

                    if (gv4.Rows.Count > 0)
                    {
                        table4.BorderStyle = BorderStyle.Solid;
                        table4.BorderColor = System.Drawing.Color.Black;
                        table4.GridLines = GridLines.Both;
                    }

                    //  add the header row to the table
                    if (gv4.HeaderRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv4.HeaderRow);
                        table4.Rows.Add(gv4.HeaderRow);
                    }

                    //  add each of the data rows to the table
                    foreach (GridViewRow row4 in gv4.Rows)
                    {
                        GridViewExportUtil.PrepareControlForExport(row4);
                        table4.Rows.Add(row4);
                    }

                    //  add the footer row to the table
                    if (gv4.FooterRow != null)
                    {
                        GridViewExportUtil.PrepareControlForExport(gv4.FooterRow);
                        table4.Rows.Add(gv4.FooterRow);
                    }

                    //  render the table into the htmlwriter
                    table4.RenderControl(htw);

                    //  render the htmlwriter into the response
                    HttpContext.Current.Response.Write(sw.ToString());
                    HttpContext.Current.Response.End();
                }
            }
        }

		/// <summary>
		/// Replace any of the contained controls with literals
		/// </summary>
		/// <param name="control"></param>
		private static void PrepareControlForExport(System.Web.UI.Control control)
		{
			for (int i = 0; i < control.Controls.Count; i++)
			{
				System.Web.UI.Control current = control.Controls[i];
				if (current is LinkButton)
				{
					control.Controls.Remove(current);
					control.Controls.AddAt(i, new LiteralControl((current as LinkButton).Text));                    
				}
				else if (current is ImageButton)
				{
					control.Controls.Remove(current);
					control.Controls.AddAt(i, new LiteralControl((current as ImageButton).AlternateText));
				}
				else if (current is HyperLink)
				{
					control.Controls.Remove(current);
					control.Controls.AddAt(i, new LiteralControl((current as HyperLink).Text));
				}
				else if (current is DropDownList)
				{
					control.Controls.Remove(current);
					control.Controls.AddAt(i, new LiteralControl((current as DropDownList).SelectedItem.Text));
				}
				else if (current is CheckBox)
				{
					control.Controls.Remove(current);
					control.Controls.AddAt(i, new LiteralControl((current as CheckBox).Checked ? "True" : "False"));
				}

				if (current.HasControls())
				{
					GridViewExportUtil.PrepareControlForExport(current);
				}
			}
		}
	}
}
