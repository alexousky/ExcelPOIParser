﻿using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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


            var sheetsNumber = workbook.NumberOfSheets;
            for (var s = 0; s < sheetsNumber; s++)
            {
                var sheet = workbook.GetSheetAt(s);
                if (sheet != null)
                {
                    _treeRowsList = new List<TreeNode>();

                    var rowCount = sheet.LastRowNum; // This may not be valid row count.
                    // If first row is table head, i starts from 1
                    for (var rowNum = 1; rowNum <= rowCount; rowNum++)
                    {
                        _treeCellsList = new List<TreeNode>();

                        var curRow = sheet.GetRow(rowNum);
                        if (curRow == null)
                        {
                            rowCount = rowNum - 1;
                            break;
                        }

                        foreach (var cell in curRow.Cells)
                        {
                            if (cell != null)
                            {
                                // 1. AddCell
                                var cellNodesList = new List<TreeNode>();

                                cellNodesList.Add(new TreeNode(nameof(cell.RowIndex) + " = " + cell.RowIndex));
                                cellNodesList.Add(new TreeNode(nameof(cell.ColumnIndex) + " = " + cell.ColumnIndex));

                                cellNodesList.Add(
                                    new TreeNode(nameof(cell.CellStyle.Alignment) + " = " + cell.CellStyle.Alignment));

                                cellNodesList.Add(new TreeNode(nameof(cell.IsMergedCell) + " = " + cell.IsMergedCell));

                                var cellFont = cell.CellStyle.GetFont(workbook);
                                cellNodesList.Add(new TreeNode(nameof(cellFont.FontName) + " = " + cellFont.FontName));
                                cellNodesList.Add(new TreeNode(nameof(cellFont.Color) + " = " + cellFont.Color));
                                cellNodesList.Add(new TreeNode(nameof(cellFont.IsBold) + " = " + cellFont.IsBold));
                                cellNodesList.Add(new TreeNode(nameof(cellFont.IsItalic) + " = " + cellFont.IsItalic));
                                cellNodesList.Add(
                                    new TreeNode(nameof(cellFont.FontHeight) + " = " + cellFont.FontHeight));

                                if (cell.CellType == CellType.String)
                                {
                                    cellNodesList.Add(new TreeNode(
                                        nameof(cell.RichStringCellValue) + " = " + cell.RichStringCellValue));
                                    cellNodesList.Add(
                                        new TreeNode(nameof(cell.StringCellValue) + " = " + cell.StringCellValue));
                                }

                                if (cell.CellType == CellType.Formula)
                                {
                                    cellNodesList.Add(
                                        new TreeNode(nameof(cell.CellFormula) + " = " + cell.CellFormula));
                                }

                                cellNodesList.Add(new TreeNode(nameof(cell.CellComment) + " = " + cell.CellComment));

                                var styleNode = new TreeNode("Cell : " + cell.ColumnIndex, cellNodesList.ToArray());
                                _treeCellsList.Add(styleNode);
                            }
                        }

                        var treeNodeRow = new TreeNode("Row " + rowNum, _treeCellsList.ToArray());
                        _treeRowsList.Add(treeNodeRow);
                    }
                }

                // Ajout des feuilles à la listView
                var treeNode = new TreeNode("Sheet " + s, _treeRowsList.ToArray());
                treeView.Nodes.Add(treeNode);
            }

            return treeView.Nodes;
        }
    }
}