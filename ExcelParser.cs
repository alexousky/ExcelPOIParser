﻿using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TestExcelParser.Model;
using CellType = NPOI.SS.UserModel.CellType;
using File = System.IO.File;

namespace TestExcelParser
{
    public class ExcelParser
    {
        private static List<TreeNode> _treeNodesList;
        private static List<TreeNode> _treeRowsList;
        private static List<TreeNode> _treeCellsList;

        public TreeNodeCollection Parse(string filePath)
        {
            var treeView = new TreeView();

            IWorkbook workbook = null;
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (filePath.IndexOf(".xlsx") > 0)
            {
                workbook = new XSSFWorkbook(fs);
            }
            else if (filePath.IndexOf(".xls") > 0)
            {
                workbook = new HSSFWorkbook(fs);
            }

            var formatter = new DataFormatter();

            var cellsList = new List<Cell>();

            var sheetsNumber = workbook.NumberOfSheets;
            sheetsNumber = 2;

            var linesList = new List<Line>();
            for (var sheetIndex = 0; sheetIndex < sheetsNumber; sheetIndex++)
            {
                var sheet = workbook.GetSheetAt(sheetIndex);
                if (sheet != null)
                {
                    _treeRowsList = new List<TreeNode>();

                    var rowCount = sheet.LastRowNum; // This may not be valid row count.


                    // If first row is table head, i starts from 1
                    for (var rowNum = 0; rowNum < rowCount; rowNum++)
                    {
                        _treeCellsList = new List<TreeNode>();

                        var curRow = sheet.GetRow(rowNum);
                        if (curRow == null)
                        {
                            rowCount = rowNum - 1;
                            break;
                        }

                        var elementsList = new List<Element>();
                        foreach (var cell in curRow.Cells)
                        {
                            if (cell != null)
                            {
                                // 1. AddCell

                                var cellFont = cell.CellStyle.GetFont(workbook);

                                var cellObject = new Cell
                                {
                                    column = cell.ColumnIndex,
                                    row = cell.RowIndex,
                                    sheet = sheetIndex,
                                    style = new Style
                                    {
                                        alignment = cell.CellStyle.Alignment,
                                        backgroundColor = null,
                                        font = cellFont.FontName,
                                        fontSize = cellFont.FontHeight,
                                        foregroundColor = null,
                                        isBold = cellFont.IsBold,
                                        isItalic = cellFont.IsItalic,
                                        isUnderline = cellFont.Underline != null ? true : false,
                                        isStrikeout = cellFont.IsStrikeout,
                                        verticalAlignment = cell.CellStyle.VerticalAlignment
                                    }
                                };


                                //var cellNodesList = new List<TreeNode>();

                                //cellNodesList.Add(new TreeNode(nameof(cell.RowIndex) + " = " + cell.RowIndex));
                                //cellNodesList.Add(new TreeNode(nameof(cell.ColumnIndex) + " = " + cell.ColumnIndex));

                                //cellNodesList.Add(
                                //    new TreeNode(nameof(cell.CellStyle.Alignment) + " = " + cell.CellStyle.Alignment));

                                //cellNodesList.Add(new TreeNode(nameof(cell.IsMergedCell) + " = " + cell.IsMergedCell));


                                //cellNodesList.Add(new TreeNode(nameof(cellFont.FontName) + " = " + cellFont.FontName));
                                //cellNodesList.Add(new TreeNode(nameof(cellFont.Color) + " = " + cellFont.Color));
                                //cellNodesList.Add(new TreeNode(nameof(cellFont.IsBold) + " = " + cellFont.IsBold));
                                //cellNodesList.Add(new TreeNode(nameof(cellFont.IsItalic) + " = " + cellFont.IsItalic));
                                //cellNodesList.Add(
                                //    new TreeNode(nameof(cellFont.FontHeight) + " = " + cellFont.FontHeight));


                                var formatCellValue = formatter.FormatCellValue(cell);
                                //cellNodesList.Add(new TreeNode(
                                //    nameof(formatCellValue) + " = " + formatCellValue));
                                cellObject.displayContent = formatCellValue;
                                string cellContent = null;
                                if (cell.CellType == CellType.String)
                                {
                                    //cellNodesList.Add(new TreeNode(
                                    //    nameof(cell.RichStringCellValue) + " = " + cell.RichStringCellValue));
                                    //cellNodesList.Add(
                                    //    new TreeNode(nameof(cell.StringCellValue) + " = " + cell.StringCellValue));

                                    cellContent = cell.RichStringCellValue.ToString();
                                    cellObject.cellType = Model.CellType.String;
                                }
                                else if (cell.CellType == CellType.Numeric)
                                {
                                    var myCell = cell;
                                    //cellNodesList.Add(new TreeNode(
                                    //    nameof(cell.NumericCellValue) + " = " + cell.NumericCellValue));
                                    cellContent = cell.NumericCellValue.ToString();
                                    cellObject.cellType = Model.CellType.Numeric;
                                }
                                else if (cell.CellType == CellType.Formula)
                                {
                                    //cellNodesList.Add(
                                    //    new TreeNode(nameof(cell.CellFormula) + " = " + cell.CellFormula));
                                    cellContent = cell.CellFormula;
                                    cellObject.cellType = Model.CellType.Formula;
                                }
                                else if (cell.CellType == CellType.Boolean)
                                {
                                    //cellNodesList.Add(
                                    //    new TreeNode(nameof(cell.BooleanCellValue) + " = " + cell.BooleanCellValue));
                                    cellContent = cell.BooleanCellValue.ToString();
                                    cellObject.cellType = Model.CellType.Boolean;
                                }

                                cellObject.content = cellContent;

                                //cellNodesList.Add(new TreeNode(nameof(cell.CellComment) + " = " + cell.CellComment));

                                //var styleNode = new TreeNode("Cell : " + cell.ColumnIndex, cellNodesList.ToArray());
                                //_treeCellsList.Add(styleNode);


                                var element = new Element
                                {
                                    cell = cellObject
                                };


                                elementsList.Add(element);

                                cellsList.Add(cellObject);
                            }
                        }

                        var line = new Line
                        {
                            ElementGroupsList = new List<ElementGroup>
                            {
                                new ElementGroup
                                {
                                    elementsList = elementsList
                                }
                            }
                        };

                        linesList.Add(line);
                    }

                    SerializeObject(linesList);

                    //var treeNodeRow = new TreeNode("Row " + rowNum, _treeCellsList.ToArray());
                    //_treeRowsList.Add(treeNodeRow);
                }

                // Ajout des feuilles à la listView
                var treeNode = new TreeNode("Sheet " + sheetIndex, _treeRowsList.ToArray());
                treeView.Nodes.Add(treeNode);
            }

            return treeView.Nodes;
        }

        private static void SerializeObject(List<Line> cellsList)
        {
            var writer =
                new XmlSerializer(typeof(List<Line>));

            //var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "//SerializationOverview.xml";
            var path = @"D:\tests\Extractions NPOI\" + "Extraction.xml";
            var file = File.Create(path);

            writer.Serialize(file, cellsList);
            file.Close();
        }
    }
}